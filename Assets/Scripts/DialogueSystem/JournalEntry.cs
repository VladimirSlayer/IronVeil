using UnityEngine;
using System;
using System.Collections.Generic;

public class JournalEntry
{
    public enum EntryType { Dialogue, QuestAccepted }
    public EntryType Type;
    public string sourceId;    
    public string nodeId;     
    public DateTime timeStamp;
    public string description;  
}