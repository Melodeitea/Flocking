using UnityEngine;

public class RandomScaleAtStart : MonoBehaviour
{
    [SerializeField]
    private float minScale = 0.1f;

    [SerializeField]
    private float maxScale = 2.0f;

    void Start()
    {
        transform.localScale = Vector3.one * Mathf.Lerp(minScale, maxScale, Random.value);
    }
}
