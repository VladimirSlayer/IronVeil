using UnityEngine;

public abstract class QuestObjective
{
    public string TargetId        { get; }
    public string TargetNPCId     { get; }  
    public int    RequiredAmount  { get; }
    public int    CurrentAmount   { get; protected set; }
    public bool   IsComplete      => CurrentAmount >= RequiredAmount;
    public string Description     { get; }

    protected QuestObjective(QuestObjectiveData data)
    {
        TargetId       = data.targetId;
        TargetNPCId    = data.targetNPCId;    
        RequiredAmount = data.requiredAmount;
        Description    = data.description;
    }

    public abstract void AddProgress(string subjectId, int amount, string npcId = null);

    public static QuestObjective Create(QuestObjectiveData data)
    {
        switch (data.type)
        {
            case ObjectiveType.FetchItem:
                return new FetchItemObjective(data);
            case ObjectiveType.KillEnemy:
                return new KillEnemyObjective(data);
            case ObjectiveType.DeliverItem:
                return new DeliverItemObjective(data);
            default:
                throw new System.ArgumentException("Unknown type");
        }
    }
}

public class FetchItemObjective : QuestObjective
{
    public FetchItemObjective(QuestObjectiveData d) : base(d) { }

    public override void AddProgress(string subjectId, int amount = 1, string npcId = null)
    {
        Debug.Log($"subjectId: {subjectId} || TargetId: {TargetId}");
        if (IsComplete) return;
        if (subjectId == TargetId)
            CurrentAmount = Mathf.Min(CurrentAmount + amount, RequiredAmount);
    }
}

public class KillEnemyObjective : QuestObjective
{
    public KillEnemyObjective(QuestObjectiveData d) : base(d) { }

    public override void AddProgress(string subjectId, int amount = 1, string npcId = null)
    {
        if (IsComplete) return;
        if (subjectId == TargetId)
            CurrentAmount = Mathf.Min(CurrentAmount + amount, RequiredAmount);
    }
}

public class DeliverItemObjective : QuestObjective
{
    public DeliverItemObjective(QuestObjectiveData d) : base(d) { }

    public override void AddProgress(string subjectId, int amount = 1, string npcId = null)
    {
        if (IsComplete) return;
        if (subjectId == TargetId && npcId == TargetNPCId)
            CurrentAmount = Mathf.Min(CurrentAmount + amount, RequiredAmount);
    }
}

