using UnityEngine;

public enum ObjectiveType { FetchItem, KillEnemy, DeliverItem }

[System.Serializable]
public class QuestObjectiveData
{
    public ObjectiveType type;

    [Tooltip("ID предмета: для FetchItem и DeliverItem")]
    public string targetId;

    [Tooltip("ID NPC: только для DeliverItem")]
    public string targetNPCId;

    [Tooltip("Сколько предметов нужно отдать/принести/убить")]
    public int requiredAmount;

    [TextArea] 
    public string description;
}
