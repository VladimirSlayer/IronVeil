using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class DialogueOption
{
    public string text;
    public string nextNode;

    public string questIdCondition;
    public bool hideIfQuestIssuedOrCompleted;
}

[Serializable]
public class DialogueNode
{
    public string id;
    public string text;
    public DialogueOption[] options;

    public bool givesQuest;
    public QuestData questAsset;
}

[Serializable]
public class DialogueData
{
    public string dialogueId;
    public DialogueNode[] nodes;
}

public static class DialogueLoader
{
    public static Dictionary<string, DialogueQuestData> QuestDataByNodeId { get; private set; } = new Dictionary<string, DialogueQuestData>();

    [Serializable]
    class DialogueJson
    {
        public string dialogueId;
        public NodeJson[] nodes;
    }

    [Serializable]
    class NodeJson
    {
        public string id;
        public string text;
        public DialogueOption[] options;
        public bool givesQuest;
        public DialogueQuestData questData;  
    }

    public static DialogueData LoadFromJson(TextAsset jsonFile)
    {
        QuestDataByNodeId.Clear();
        var dj = JsonUtility.FromJson<DialogueJson>(jsonFile.text);

        QuestDataByNodeId = new Dictionary<string, DialogueQuestData>();

        var dd = new DialogueData
        {
            dialogueId = dj.dialogueId,
            nodes = new DialogueNode[dj.nodes.Length]
        };

        for (int i = 0; i < dj.nodes.Length; i++)
        {
            var nj = dj.nodes[i];
            var dn = new DialogueNode
            {
                id = nj.id,
                text = nj.text,
                options = nj.options,
                givesQuest = nj.givesQuest,
                questAsset = null  
            };
            dd.nodes[i] = dn;

            if (nj.givesQuest && nj.questData != null)
                QuestDataByNodeId[nj.id] = nj.questData;
        }

        return dd;
    }
}


[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue System/Dialogue Asset")]
public class DialogueAsset : ScriptableObject
{
    public string dialogueId;
    public List<DialogueNode> nodes = new List<DialogueNode>();
}

[Serializable]
public class DialogueQuestData
{
    public string questId;
    public string title;
    public string description;
    public QuestObjectiveData[] objectives;
}

