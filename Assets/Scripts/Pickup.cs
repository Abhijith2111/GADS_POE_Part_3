using UnityEngine;

public enum PickupType { Clothing, Dish, Food }

public class Pickup : MonoBehaviour
{
    public PickupType pickupType = PickupType.Clothing;
    public int amount = 1;               // how many units this pickup counts for the objective
    [Tooltip("Seconds before this type respawns after being collected (can be overridden in SpawnManager)")]
    public float respawnDelay = 5f;

    // --- These are set by the SpawnManager when the pickup is instantiated ---
    [HideInInspector] public int prefabIndex = -1;      // index into SpawnManager.pickupPrefabs
    [HideInInspector] public int spawnPointIndex = -1;  // which spawn point this instance occupies

    [SerializeField] AudioSource pickedFX;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            pickedFX.Play();
            // Register collection for objectives
            ObjectiveManager.Instance?.RegisterPickupCollected(pickupType, amount);

            // Tell SpawnManager to free the spawn point and respawn this prefab
            if (SpawnManager.Instance != null && prefabIndex >= 0)
            {
                SpawnManager.Instance.HandlePickupCollected(prefabIndex, spawnPointIndex, respawnDelay);
            }

            // optional: play SFX/VFX here

            Destroy(gameObject);
        }
    }
}
