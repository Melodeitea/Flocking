using System.Collections.Generic;
using UnityEngine;

	/* <summary>
	 Fish behavior implementing simple boids (alignment, cohesion, separation).
	 Parameters are driven by a FishData ScriptableObject assigned at runtime via Initialize.
	 </summary> */
[RequireComponent(typeof(Collider2D))] // Collider2D helps ensure this GameObject can be detected by OverlapCircleAll.
public class Fish : MonoBehaviour
{
	// Data asset that controls behavior. Hidden in inspector because we assign it via Initialize.
	[HideInInspector]
	public FishData data;

	// Runtime state
	public Vector2 acceleration;
	public Vector2 velocity;

	/* <summary>
	 Public initializer. Call this immediately after instantiating the fish prefab so it gets its FishData.
	 Sets a random rotation and initial velocity.
	 </summary> */
	public void Initialize(FishData fishData)
	{
		data = fishData;

		// Random orientation around Z.
		float angle = Random.Range(0f, 2f * Mathf.PI);
		transform.rotation = Quaternion.Euler(0f, 0f, angle);

		// Start at a random fraction of max speed so not all fish behave identically at spawn.
		float startSpeed = Random.Range(0.5f, 1f) * (data != null ? data.maxSpeed : 1f);
		velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * startSpeed;
	}

	private void Start()
	{
		/* If the fish was placed in the scene directly (not spawned) and Initialize wasn't called,
		   set a safe default so the fish still behaves.*/
		if (data == null)
		{
			if (Application.isPlaying)
				Debug.LogWarning($"Fish '{name}' has no FishData assigned. Assign via FishTank or call Initialize(FishData).");
			float angle = Random.Range(0f, 2f * Mathf.PI);
			transform.rotation = Quaternion.Euler(0f, 0f, angle);
			velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
		}
	}

	private void Update()
	{
		// Without data we cannot calculate behavior.
		if (data == null) return;

		// Find nearby colliders within neighborhood radius.
		var colliders = Physics2D.OverlapCircleAll(transform.position, data.neighborhoodRadius);

		// Build a list of other Fish components found.
		var boids = new List<Fish>(colliders.Length);
		foreach (var c in colliders)
		{
			if (c == null) continue;
			var f = c.GetComponent<Fish>();
			if (f != null && f != this) boids.Add(f);
		}

		// Compute steering forces and update motion.
		ComputeAcceleration(boids);
		UpdateVelocity();
		UpdatePosition();
		UpdateRotation();
	}

	/* <summary>
	 Combine alignment, cohesion and separation using the weights from FishData.
	 </summary> */
	private void ComputeAcceleration(IEnumerable<Fish> boids)
	{
		var alignment = Alignment(boids);
		var separation = Separation(boids);
		var cohesion = Cohesion(boids);

		// Weighted sum of steering components.
		acceleration = data.alignmentAmount * alignment + data.cohesionAmount * cohesion + data.separationAmount * separation;
	}

	/* <summary>
	 Apply acceleration to velocity and clamp speed to maxSpeed.
	The original code added acceleration directly (frame dependent). This preserves that behavior.
	</summary> */
	public void UpdateVelocity()
	{
		velocity += acceleration;
		velocity = LimitMagnitude(velocity, data.maxSpeed);
	}

	/* <summary>
	 Move the transform in world space according to velocity.
	 </summary> */
	private void UpdatePosition()
	{
		transform.Translate(velocity * Time.deltaTime, Space.World);
	}

	/* <summary>
	Rotate the fish to face the direction of movement.
	</summary> */
	private void UpdateRotation()
	{
		if (velocity.sqrMagnitude > 0.0001f)
		{
			float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
			transform.rotation = Quaternion.Euler(0f, 0f, angle);
		}
	}

	/* <summary>
	Alignment: steer toward average heading of neighbors.
	Returns a steering vector (desired - current velocity) limited by maxForce.
	</summary> */
	private Vector2 Alignment(IEnumerable<Fish> boids)
	{
		Vector2 sum = Vector2.zero;
		int count = 0;
		foreach (var b in boids)
		{
			sum += b.velocity;
			count++;
		}

		if (count == 0) return Vector2.zero;

		sum /= count; // average velocity
		return Steer(sum.normalized * data.maxSpeed);
	}

	/* <summary>
	Cohesion: steer toward the center of mass of neighbors.
	</summary> */
	private Vector2 Cohesion(IEnumerable<Fish> boids)
	{
		Vector2 sum = Vector2.zero;
		int count = 0;
		foreach (var b in boids)
		{
			sum += (Vector2)b.transform.position;
			count++;
		}

		if (count == 0) return Vector2.zero;

		Vector2 average = sum / count;
		Vector2 direction = average - (Vector2)transform.position;
		return Steer(direction.normalized * data.maxSpeed);
	}

	/* <summary>
	 Separation: steer away from close neighbors within separationRadius.
	 </summary> */
	private Vector2 Separation(IEnumerable<Fish> boids)
	{
		Vector2 direction = Vector2.zero;
		int count = 0;
		foreach (var b in boids)
		{
			float dist = Vector2.Distance(transform.position, b.transform.position);
			if (dist <= data.separationRadius && dist > 0f)
			{
				// Sum normalized vectors pointing away from neighbor.
				Vector2 diff = (Vector2)transform.position - (Vector2)b.transform.position;
				direction += diff.normalized;
				count++;
			}
		}

		if (count == 0) return Vector2.zero;

		direction /= count;
		return Steer(direction.normalized * data.maxSpeed);
	}

	/* <summary>
	Compute steering vector to move toward desired velocity and limit by maxForce.
	</summary> */
	private Vector2 Steer(Vector2 desired)
	{
		Vector2 steer = desired - velocity;
		steer = LimitMagnitude(steer, data.maxForce);
		return steer;
	}

	/* <summary>
	 Limit a vector's magnitude to max.
	 </summary> */
	private Vector2 LimitMagnitude(Vector2 v, float max)
	{
		if (v.sqrMagnitude > max * max) return v.normalized * max;
		return v;
	}

	/*<summary>
	Draw debug gizmos for neighborhood and separation radii.
	If no data is assigned, draw default values to help debugging.
	</summary> */
	private void OnDrawGizmosSelected()
	{
		if (data == null)
		{
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(transform.position, 3f);

			Gizmos.color = new Color(1f, 0.5f, 0.5f); // approximate salmon color
			Gizmos.DrawWireSphere(transform.position, 1f);
			return;
		}

		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(transform.position, data.neighborhoodRadius);

		Gizmos.color = new Color(1f, 0.5f, 0.5f);
		Gizmos.DrawWireSphere(transform.position, data.separationRadius);
	}
}