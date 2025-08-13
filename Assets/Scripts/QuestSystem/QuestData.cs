using UnityEngine;

public enum QuestStatus { Inactive, Active, Completed, Failed }

[CreateAssetMenu(fileName = "NewQuest", menuName = "Quest System/Quest Data")]
public class QuestData : ScriptableObject
{
    public string questId;
    public string title;
    [TextArea] public string description;
    public QuestObjectiveData[] objectives;
}
