using UnityEngine;

[ExecuteAlways]
public class Banking : MonoBehaviour
{
    public float Speed = 45f;
    public float Scale = 10f;
    public Transform Pivot;

    private Vector3 baseRotation;
    private Vector3 previousDirection;

    private void Start()
    {
        previousDirection = transform.right;
        baseRotation = Pivot.localRotation.eulerAngles;
    }

    private void Update()
    {
        // We rotate our previous/lagging direction toward our current direction base on Speed.
        previousDirection = Vector3.RotateTowards(previousDirection, transform.right, Speed * Mathf.Deg2Rad * Time.deltaTime, 0f);

        // We compute the angle between our previous rotation and current rotation.
        float angle = Vector3.SignedAngle(previousDirection, transform.right, transform.forward);

        // We rotate our pivot based on the computed angle.
        Pivot.localRotation = Quaternion.Euler(baseRotation.x, baseRotation.y, baseRotation.z + angle * Scale);
    }
}
