using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class PickupObjective
{
    public PickupType type;
    public string label = "Clothes"; // shown before numbers (e.g. "Clothes")
    public int requiredCount = 5;
    [TextArea] public string completionMessage = "You collected all clothes!";
    public Text uiText; // drag the UI Text here

    [HideInInspector] public int currentCount = 0;
    public bool IsComplete => currentCount >= requiredCount;
}

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance { get; private set; }

    public List<PickupObjective> objectives = new List<PickupObjective>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        RefreshUI();
    }

    public void RegisterPickupCollected(PickupType type, int amount = 1)
    {
        var obj = objectives.Find(o => o.type == type);
        if (obj == null) return;

        if (!obj.IsComplete)
        {
            obj.currentCount += amount;
            if (obj.currentCount > obj.requiredCount) obj.currentCount = obj.requiredCount;
            RefreshObjectiveUI(obj);
        }
    }

    private void RefreshUI()
    {
        foreach (var obj in objectives) RefreshObjectiveUI(obj);
    }

    private void RefreshObjectiveUI(PickupObjective obj)
    {
        if (obj.uiText == null) return;

        if (obj.IsComplete)
        {
            obj.uiText.text = obj.completionMessage;
        }
        else
        {
            obj.uiText.text = $"{obj.label}: {obj.currentCount}/{obj.requiredCount}";
        }
    }

    // optional: if you want to reset objectives for a new level
    public void ResetObjectives()
    {
        foreach (var o in objectives)
        {
            o.currentCount = 0;
        }
        RefreshUI();
    }

    public bool IsObjectiveComplete(PickupType type)
    {
        var obj = objectives.Find(o => o.type == type);
        if (obj == null) return false;
        return obj.IsComplete;
    }
    
    public bool AreAllObjectivesComplete()
    {
        if (objectives == null || objectives.Count == 0) return false;
        foreach (var o in objectives)
        {
            if (!o.IsComplete) return false;
        }
        return true;
    }
}
