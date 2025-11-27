using UnityEngine;

/* <summary>
    ScriptableObject that stores parameters for a fish type.
    Create assets via Assets -> Create -> Fish -> Fish Data.
    </summary> */
[CreateAssetMenu(fileName = "FishData", menuName = "Fish/Fish Data")]
public class FishData : ScriptableObject
{
    [Header("Movement")]
    // Maximum speed of the fish (units per second).
    [Range(0f, 10f)] public float maxSpeed = 1f;

    // Maximum steering force applied when changing direction.
    [Range(0.01f, 1f)] public float maxForce = 0.03f;

    [Header("Boid Radii")]
    // Radius used to find neighbors for alignment and cohesion.
    [Range(0.1f, 20f)] public float neighborhoodRadius = 3f;

    // Radius used to consider nearby boids for separation (avoidance).
    [Range(0.05f, 10f)] public float separationRadius = 1f;

    [Header("Behavior weights")]
    // Weight for separation steering component.
    [Range(0f, 3f)] public float separationAmount = 1f;

    // Weight for cohesion steering component.
    [Range(0f, 3f)] public float cohesionAmount = 1f;

    // Weight for alignment steering component.
    [Range(0f, 3f)] public float alignmentAmount = 1f;

    [Header("Visual / Prefab")]
    // Prefab to instantiate for this fish type. The prefab should contain a Fish component on the root.
    public GameObject prefab;
}