using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    [Tooltip("Points where pickups may spawn")]
    public Transform[] spawnPoints;

    [Tooltip("Pickup prefabs to choose from. Make sure each prefab has the Pickup component")]
    public GameObject[] pickupPrefabs;

    [Range(0f, 1f), Tooltip("Chance that a spawn point will actually spawn something on Start")]
    public float spawnChancePerPoint = 1f;

    [Tooltip("If true, initial spawning occurs on Start")]
    public bool spawnOnStart = true;

    // Tracks whether a spawn point is currently occupied by a pickup instance
    private bool[] occupied;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (spawnPoints == null) spawnPoints = new Transform[0];
        occupied = new bool[spawnPoints.Length];

        if (spawnOnStart)
            SpawnAll();
    }

    // Spawn at each spawn point (randomly choose prefab or none)
    public void SpawnAll()
    {
        // If all objectives are complete, stop spawning entirely
        if (ObjectiveManager.Instance != null && ObjectiveManager.Instance.AreAllObjectivesComplete())
            return;

        if (spawnPoints == null || spawnPoints.Length == 0 || pickupPrefabs == null || pickupPrefabs.Length == 0)
            return;

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            // check global completion mid-loop
            if (ObjectiveManager.Instance != null && ObjectiveManager.Instance.AreAllObjectivesComplete())
                break;

            Transform p = spawnPoints[i];
            if (p == null) continue;
            if (Random.value > spawnChancePerPoint) continue;
            if (occupied[i]) continue;

            // Pick a random allowed prefab index
            int prefabIndex = GetRandomAllowedPrefabIndex();
            if (prefabIndex < 0) continue; // nothing allowed to spawn

            GameObject instance = InstantiatePickupAt(prefabIndex, i);
            if (instance != null)
            {
                occupied[i] = true;
            }
        }
    }

    // Attempts to pick a random prefab index whose PickupType's objective is not complete.
    // Returns -1 if none available.
    private int GetRandomAllowedPrefabIndex()
    {
        if (pickupPrefabs == null || pickupPrefabs.Length == 0) return -1;

        // Quick check: if all objectives complete, return -1
        if (ObjectiveManager.Instance != null && ObjectiveManager.Instance.AreAllObjectivesComplete())
            return -1;

        // Build list of allowed indices (prefabs whose type is not complete)
        List<int> allowed = new List<int>();
        for (int i = 0; i < pickupPrefabs.Length; i++)
        {
            var pref = pickupPrefabs[i];
            if (pref == null) continue;
            var pickupComp = pref.GetComponent<Pickup>();
            if (pickupComp == null) continue;

            if (ObjectiveManager.Instance != null)
            {
                if (ObjectiveManager.Instance.IsObjectiveComplete(pickupComp.pickupType))
                    continue; // skip completed types
            }
            allowed.Add(i);
        }

        if (allowed.Count == 0) return -1;

        // pick randomly from allowed list
        return allowed[Random.Range(0, allowed.Count)];
    }

    // Creates an instance of the prefab at spawnPoints[spawnIndex] and sets fields on the pickup
    private GameObject InstantiatePickupAt(int prefabIndex, int spawnIndex)
    {
        // Double-check global completion before instantiating
        if (ObjectiveManager.Instance != null && ObjectiveManager.Instance.AreAllObjectivesComplete())
            return null;

        if (prefabIndex < 0 || prefabIndex >= pickupPrefabs.Length) return null;
        if (spawnIndex < 0 || spawnIndex >= spawnPoints.Length) return null;
        var prefab = pickupPrefabs[prefabIndex];
        if (prefab == null) return null;

        // Don’t spawn if the objective for this type is already complete
        var pickupCompPrefab = prefab.GetComponent<Pickup>();
        if (pickupCompPrefab != null)
        {
            if (ObjectiveManager.Instance != null && ObjectiveManager.Instance.IsObjectiveComplete(pickupCompPrefab.pickupType))
                return null;
        }

        GameObject go = Instantiate(prefab, spawnPoints[spawnIndex].position, Quaternion.identity, transform);
        var pickupComp = go.GetComponent<Pickup>();
        if (pickupComp != null)
        {
            // set these so pickup can report back to the SpawnManager (but we won't force same-type respawn)
            pickupComp.prefabIndex = prefabIndex;
            pickupComp.spawnPointIndex = spawnIndex;
        }
        else
        {
            Debug.LogWarning($"Spawned prefab {prefab.name} does not have a Pickup component.");
        }

        return go;
    }

    // Called by Pickup when collected. Frees the original spawn point and queues a respawn.
    // IMPORTANT: we ignore the collected prefabIndex here and pick a random allowed prefab at respawn time.
    public void HandlePickupCollected(int prefabIndexIgnored, int freedSpawnPointIndex, float respawnDelay)
    {
        // Free the slot immediately
        if (freedSpawnPointIndex >= 0 && freedSpawnPointIndex < occupied.Length)
            occupied[freedSpawnPointIndex] = false;

        // If all objectives are complete, don't queue respawn
        if (ObjectiveManager.Instance != null && ObjectiveManager.Instance.AreAllObjectivesComplete())
            return;

        StartCoroutine(RespawnCoroutineAtRandomPrefab(freedSpawnPointIndex, respawnDelay));
    }

    // This coroutine picks a random allowed prefab and a free spawn point (or any if none free),
    // then instantiates that prefab.
    private IEnumerator RespawnCoroutineAtRandomPrefab(int freedSpawnPointIndex, float delay)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        // If everything is complete, abort
        if (ObjectiveManager.Instance != null && ObjectiveManager.Instance.AreAllObjectivesComplete())
            yield break;

        // Pick an allowed prefab index (ignore the original collected prefab)
        int chosenPrefabIndex = GetRandomAllowedPrefabIndex();
        if (chosenPrefabIndex < 0)
            yield break; // nothing allowed to spawn

        // Still check global completion again
        if (ObjectiveManager.Instance != null && ObjectiveManager.Instance.AreAllObjectivesComplete())
            yield break;

        // find free spawn points
        List<int> freeIndices = new List<int>();
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (!occupied[i])
                freeIndices.Add(i);
        }

        int chosenIndex;
        if (freeIndices.Count == 0)
        {
            // no free points: prefer the freed index if valid, otherwise allow any point
            if (freedSpawnPointIndex >= 0 && freedSpawnPointIndex < spawnPoints.Length)
                chosenIndex = freedSpawnPointIndex;
            else
                chosenIndex = Random.Range(0, spawnPoints.Length);
        }
        else
        {
            // pick a random free spawn point
            // prefer the freed index if it's free (so it respawns near where the pickup was)
            if (freedSpawnPointIndex >= 0 && freeIndices.Contains(freedSpawnPointIndex))
                chosenIndex = freedSpawnPointIndex;
            else
                chosenIndex = freeIndices[Random.Range(0, freeIndices.Count)];
        }

        // Final check before spawning
        if (ObjectiveManager.Instance != null && ObjectiveManager.Instance.AreAllObjectivesComplete())
            yield break;

        GameObject instance = InstantiatePickupAt(chosenPrefabIndex, chosenIndex);
        if (instance != null)
            occupied[chosenIndex] = true;
    }

    // Optional helper: force spawn manually
    public GameObject ForceSpawnAt(int prefabIndex, int spawnPointIndex)
    {
        // If all objectives are complete, refuse to spawn
        if (ObjectiveManager.Instance != null && ObjectiveManager.Instance.AreAllObjectivesComplete())
            return null;

        if (prefabIndex < 0 || prefabIndex >= pickupPrefabs.Length) return null;
        if (spawnPointIndex < 0 || spawnPointIndex >= spawnPoints.Length) return null;

        var prefab = pickupPrefabs[prefabIndex];
        if (prefab == null) return null;

        var pickupType = prefab.GetComponent<Pickup>()?.pickupType ?? default(PickupType);
        if (ObjectiveManager.Instance != null && ObjectiveManager.Instance.IsObjectiveComplete(pickupType))
            return null;

        GameObject instance = InstantiatePickupAt(prefabIndex, spawnPointIndex);
        if (instance != null) occupied[spawnPointIndex] = true;
        return instance;
    }
}