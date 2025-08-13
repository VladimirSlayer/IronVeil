using UnityEngine;
using System;
using System.Collections.Generic;

public class JournalManager : MonoBehaviour
{
    public static JournalManager Instance { get; private set; }
    private List<JournalEntry> entries = new List<JournalEntry>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void RecordDialogue(string dialogueId, string nodeId)
    {
        var e = new JournalEntry {
            Type = JournalEntry.EntryType.Dialogue,
            sourceId = dialogueId,
            nodeId = nodeId,
            timeStamp = DateTime.Now,
            description = $"Разговор {dialogueId}, узел {nodeId}"
        };
        entries.Add(e);
    }

    public void RecordQuest(string questId, string questTitle)
    {
        Debug.Log($"Квест принят: {questTitle}");
        var e = new JournalEntry {
            Type = JournalEntry.EntryType.QuestAccepted,
            sourceId = questId,
            timeStamp = DateTime.Now,
            description = $"Квест принят: {questTitle}"
        };
        entries.Add(e);
    }

    public IReadOnlyList<JournalEntry> GetEntries() => entries;
}
