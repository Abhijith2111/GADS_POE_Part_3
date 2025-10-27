using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;

public class CollectibleManager : MonoBehaviour
{
    [System.Serializable]
    public class CollectibleTypeData
    {
        public CollectibleType type;
        public GameObject prefab;
        public int requiredAmount = 5;
        public string completionMessage;
        public Color messageColor = Color.white;
        public int currentAmount = 0;
        [HideInInspector] public bool isCompleted = false;
    }

    [Header("Collectible Settings")]
    public List<CollectibleTypeData> collectibleTypes = new List<CollectibleTypeData>();
    public List<Transform> spawnPoints = new List<Transform>();

    [Header("UI Settings")]
    public Text counterText;
    public GameObject collectionListUI;
    public Text messageText;
    public float messageDisplayTime = 3f;

    [Header("Spawn Settings")]
    public bool spawnRandomTypes = true;

    private List<GameObject> currentCollectibles = new List<GameObject>();
    private int totalCollectionsCompleted = 0;

    void Start()
    {
        UpdateCounter();
        SpawnCollectibles();

        if (messageText != null)
        {
            messageText.gameObject.SetActive(false);
        }
    }

    public void CollectItem(Collectible collectedItem)
    {
        CollectibleTypeData typeData = GetTypeData(collectedItem.type);

        if (typeData != null && !typeData.isCompleted)
        {
            typeData.currentAmount++;
            UpdateCounter();

            if (typeData.currentAmount >= typeData.requiredAmount && !typeData.isCompleted)
            {
                typeData.isCompleted = true;
                totalCollectionsCompleted++;
                ShowCompletionMessage(typeData);

                if (AllCollectionsComplete())
                {
                    CompleteAllCollections();
                }
            }
        }
    }

    CollectibleTypeData GetTypeData(CollectibleType type)
    {
        foreach (CollectibleTypeData data in collectibleTypes)
        {
            if (data.type == type)
            {
                return data;
            }
        }
        return null;
    }

    void UpdateCounter()
    {
        if (counterText != null)
        {
            string counterString = "";
            foreach (CollectibleTypeData typeData in collectibleTypes)
            {
                string status = typeData.isCompleted ? "?" : $"{typeData.currentAmount}/{typeData.requiredAmount}";
                counterString += $"{typeData.type}: {status}\n";
            }
            counterText.text = counterString;
        }
    }

    void SpawnCollectibles()
    {
        foreach (GameObject collectible in currentCollectibles)
        {
            if (collectible != null)
                Destroy(collectible);
        }
        currentCollectibles.Clear();

        int totalCollectiblesNeeded = 0;
        foreach (CollectibleTypeData typeData in collectibleTypes)
        {
            totalCollectiblesNeeded += typeData.requiredAmount;
        }

        List<Transform> availableSpawns = new List<Transform>(spawnPoints);

        for (int i = 0; i < totalCollectiblesNeeded && availableSpawns.Count > 0; i++)
        {
            int randomSpawnIndex = Random.Range(0, availableSpawns.Count);
            Transform spawnPoint = availableSpawns[randomSpawnIndex];
            availableSpawns.RemoveAt(randomSpawnIndex);

            CollectibleType typeToSpawn = GetRandomTypeToSpawn();
            CollectibleTypeData typeData = GetTypeData(typeToSpawn);

            if (typeData != null && typeData.prefab != null)
            {
                GameObject newCollectible = Instantiate(typeData.prefab, spawnPoint.position, Quaternion.identity);
                Collectible collectibleScript = newCollectible.GetComponent<Collectible>();

                if (collectibleScript != null)
                {
                    collectibleScript.manager = this;
                    collectibleScript.type = typeToSpawn;
                }

                currentCollectibles.Add(newCollectible);
            }
        }
    }

    CollectibleType GetRandomTypeToSpawn()
    {
        if (!spawnRandomTypes)
        {
            int totalSpawned = currentCollectibles.Count;
            return collectibleTypes[totalSpawned % collectibleTypes.Count].type;
        }

        List<CollectibleType> availableTypes = new List<CollectibleType>();

        foreach (CollectibleTypeData typeData in collectibleTypes)
        {
            int spawnedOfThisType = CountSpawnedOfType(typeData.type);
            if (spawnedOfThisType < typeData.requiredAmount)
            {
                availableTypes.Add(typeData.type);
            }
        }

        if (availableTypes.Count == 0)
            return collectibleTypes[0].type;

        return availableTypes[Random.Range(0, availableTypes.Count)];
    }

    int CountSpawnedOfType(CollectibleType type)
    {
        int count = 0;
        foreach (GameObject collectible in currentCollectibles)
        {
            Collectible colScript = collectible.GetComponent<Collectible>();
            if (colScript != null && colScript.type == type)
            {
                count++;
            }
        }
        return count;
    }

    void ShowCompletionMessage(CollectibleTypeData completedType)
    {
        if (messageText != null && !string.IsNullOrEmpty(completedType.completionMessage))
        {
            StopAllCoroutines();
            StartCoroutine(DisplayMessage(completedType.completionMessage, completedType.messageColor));
        }

        Debug.Log($"Completed {completedType.type} collection: {completedType.completionMessage}");
    }

    IEnumerator DisplayMessage(string message, Color color)
    {
        messageText.gameObject.SetActive(true);
        messageText.text = message;
        messageText.color = color;

        yield return new WaitForSeconds(messageDisplayTime);

        messageText.gameObject.SetActive(false);
    }

    bool AllCollectionsComplete()
    {
        foreach (CollectibleTypeData typeData in collectibleTypes)
        {
            if (!typeData.isCompleted)
                return false;
        }
        return true;
    }

    void CompleteAllCollections()
    {
        if (collectionListUI != null)
        {
            collectionListUI.SetActive(false);
        }

        if (messageText != null)
        {
            StopAllCoroutines();
            StartCoroutine(DisplayMessage("All collections complete! Congratulations!", Color.yellow));
        }

        Debug.Log("All collectible types completed!");
    }

    [ContextMenu("Add Default Types")]
    void AddDefaultTypes()
    {
        collectibleTypes.Clear();

        collectibleTypes.Add(new CollectibleTypeData
        {
            type = CollectibleType.Dish,
            requiredAmount = 5,
            completionMessage = "I hope he does not hit me with these.",
            messageColor = Color.yellow
        });

        collectibleTypes.Add(new CollectibleTypeData
        {
            type = CollectibleType.Clothing,
            requiredAmount = 3,
            completionMessage = "I hope he is pleased with the condition",
            messageColor = Color.cyan
        });

        collectibleTypes.Add(new CollectibleTypeData
        {
            type = CollectibleType.Food,
            requiredAmount = 4,
            completionMessage = "I hope he does not spit it out.",
            messageColor = Color.white
        });
    }
}