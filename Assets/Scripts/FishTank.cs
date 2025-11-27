using UnityEngine;
using UnityEngine.Serialization;
using System.Collections.Generic;

/// <summary>
/// Spawns a set of fish inside a rectangular tank and handles wrapping (teleport from one side to the opposite).
/// Uses a FishData ScriptableObject to choose the prefab and behavior parameters (fallback prefab supported).
/// </summary>
public class FishTank : MonoBehaviour
{
    // Size of the tank (width, height).
    [SerializeField]
    private Vector2 Size = new Vector2(16f, 9f);

    [Tooltip("FishData asset that defines the fish type. If assigned, its prefab will be used.")]
    [SerializeField]
    private FishData fishData = null;

    [Tooltip("Fallback prefab to use if fishData.prefab is not assigned.")]
    // Renamed to avoid shadowing local variables; keep serialized data if it was previously assigned.
    [FormerlySerializedAs("fish")]
    [SerializeField]
    private GameObject fallbackPrefab = null;

    // How many fish to spawn.
    [Range(0, 300)]
    [SerializeField]
    private int SpawningCount = 10;

    [SerializeField]
    private Camera myCamera;

    // List holding references to spawned fish.
    private List<Fish> fishes = new List<Fish>();

    private void Start()
    {
        // Choose prefab: prefer FishData.prefab, otherwise fallback.
        GameObject prefabToUse = fishData != null && fishData.prefab != null ? fishData.prefab : fallbackPrefab;

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

            var fishComp = fishInstance.GetComponent<Fish>();
            if (fishComp != null)
            {
                // Initialize with data (can be null — Fish should handle its fallback).
                fishComp.Initialize(fishData);
                // Add to list (was previously using index assignment into an empty list causing the ArgumentOutOfRangeException).
                fishes.Add(fishComp);
            }
            else
            {
                Debug.LogWarning("Spawned prefab does not contain a Fish component.");
                // Optionally destroy the instance if it's useless:
                // Destroy(fishInstance);
            }
        }
    }

    private void CreateFish(Vector3 worldPosition)
    {
        // Choose prefab again (prefer FishData.prefab, otherwise fallback).
        GameObject prefabToUse = fishData != null && fishData.prefab != null ? fishData.prefab : fallbackPrefab;
        if (prefabToUse == null)
        {
            Debug.LogError("CreateFish: No fish prefab available to instantiate.");
            return;
        }

        GameObject fishInstance = Instantiate(prefabToUse, transform);
        fishInstance.name = $"Fish {System.Guid.NewGuid()}";
        // Convert world position to local position so fish sits correctly under this transform.
        fishInstance.transform.position = worldPosition;
        var fishComp = fishInstance.GetComponent<Fish>();
        if (fishComp != null)
        {
            fishComp.Initialize(fishData);
            fishes.Add(fishComp);
        }
        else
        {
            Debug.LogWarning("CreateFish instantiated prefab without a Fish component.");
        }
    }

    private void LateUpdate()
    {
        // Player input: destroy fish under mouse click.
        if (Input.GetMouseButtonDown(0))
        {
            if (myCamera == null)
            {
                Debug.LogWarning("FishTank: myCamera is not assigned; cannot convert mouse position to world.");
            }
            else
            {
                Vector3 mousePosition = Input.mousePosition;
                // Ensure we set a Z that places the ScreenToWorldPoint result at Z = 0 in world space.
                // This works for typical 2D setups where the camera is at negative Z looking toward +Z.
                mousePosition.z = -myCamera.transform.position.z;
                Vector3 worldPos = myCamera.ScreenToWorldPoint(mousePosition);

                // Find colliders around the clicked point.
                Collider2D[] hits = Physics2D.OverlapCircleAll(worldPos, 1f);
                for (int i = 0; i < hits.Length; i++)
                {
                    if (hits[i] == null) continue;

                    // Try to get a Fish component on the collider's GameObject or its parents.
                    Fish fishComp = hits[i].GetComponent<Fish>() ?? hits[i].GetComponentInParent<Fish>() ?? hits[i].GetComponentInChildren<Fish>();
                    if (fishComp != null)
                    {
                        // Remove from tracking list and destroy the fish GameObject.
                        fishes.Remove(fishComp);
                        Destroy(fishComp.gameObject);
                    }
                    else
                    {
                        // If there's no Fish component, we can still destroy the whole GameObject if desired:
                        // Destroy(hits[i].gameObject);
                    }
                }
            }
        }

        // Skip if nothing spawned.
        if (fishes == null || fishes.Count == 0) return;

        // Wrap fish positions around tank boundaries (toroidal wrapping).
        int fishesCount = fishes.Count;
        for (int i = 0; i < fishesCount; i++)
        {
            Fish fish = fishes[i];
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