using UnityEngine;

    /* <summary>
     Spawns a set of fish inside a rectangular tank and handles wrapping (teleport from one side to the opposite).
     Uses a FishData ScriptableObject to choose the prefab and behavior parameters (fallback prefab supported).
     </summary> */
public class FishTank : MonoBehaviour
{
    // Size of the tank (width, height).
    [SerializeField]
    private Vector2 Size = new Vector2(16f, 9f);

    [Tooltip("FishData asset that defines the fish type. If assigned, its prefab will be used.")]
    [SerializeField]
    private FishData fishData = null;

    [Tooltip("Fallback prefab to use if fishData.prefab is not assigned.")]
    [SerializeField]
    private GameObject fallbackFishPrefab = null;

    // How many fish to spawn.
    [Range(0, 300)]
    [SerializeField]
    private int SpawningCount = 10;

    // Array holding references to spawned fish.
    private Fish[] fishes = null;

    private void Start()
    {
        fishes = new Fish[SpawningCount];

        // Prefer prefab referenced in FishData, otherwise use fallback.
        GameObject prefabToUse = null;
        if (fishData != null && fishData.prefab != null) prefabToUse = fishData.prefab;
        if (prefabToUse == null) prefabToUse = fallbackFishPrefab;

        if (prefabToUse == null)
        {
            Debug.LogError("FishTank: No fish prefab assigned (neither FishData.prefab nor fallback).");
            return;
        }

        // Instantiate fish and initialize them with FishData.
        for (int i = 0; i < SpawningCount; i++)
        {
            GameObject fishInstance = Instantiate(prefabToUse, transform);
            fishInstance.name = $"Fish {System.Guid.NewGuid()}";

            // Place fish at a random local position within the tank extents.
            var localPos = new Vector3(
                Random.Range(-Size.x * 0.5f, Size.x * 0.5f),
                Random.Range(-Size.y * 0.5f, Size.y * 0.5f),
                0f);
            fishInstance.transform.localPosition = localPos;

            var fish = fishInstance.GetComponent<Fish>();
            if (fish != null)
            {
                // Initialize with data (can be null — fish handles fallback).
                fish.Initialize(fishData);
                fishes[i] = fish;
            }
            else
            {
                Debug.LogWarning("Spawned prefab does not contain a Fish component.");
            }
        }
    }

    private void LateUpdate()
    {
        // Skip if nothing spawned.
        if (fishes == null) return;

        // Wrap fish positions around tank boundaries (toroidal wrapping).
        int fishesCount = fishes.Length;
        for (int i = 0; i < fishesCount; i++)
        {
            var fish = fishes[i];
            if (fish == null) continue;

            Vector3 position = fish.transform.localPosition;

            // Horizontal wrapping
            if (position.x < -Size.x * 0.5f) position.x += Size.x;
            else if (position.x > Size.x * 0.5f) position.x -= Size.x;

            // Vertical wrapping
            if (position.y > Size.y * 0.5f) position.y -= Size.y;
            if (position.y < -Size.y * 0.5f) position.y += Size.y;

            fish.transform.localPosition = position;
        }
    }

    // Draw the tank bounds in the Scene view when the object is selected.
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, Size);
    }
}