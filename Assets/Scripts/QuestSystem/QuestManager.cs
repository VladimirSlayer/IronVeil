using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class QuestManager : MonoBehaviour
{
  public static QuestManager Instance { get; private set; }
  private Dictionary<string, Quest> _activeQuests = new();

  public event Action<Quest> OnQuestAccepted;
  public event Action<Quest> OnQuestCompleted;
  public event Action<Quest> OnQuestUpdated;

  void Awake()
  {
    if (Instance == null) Instance = this; else Destroy(gameObject);
  }

  public void AcceptQuest(QuestData data)
  {
    if (_activeQuests.ContainsKey(data.questId)) return;
    var quest = new Quest(data: data);
    _activeQuests[data.questId] = quest;
    OnQuestAccepted?.Invoke(quest);
    JournalManager.Instance.RecordQuest(data.questId, data.title);
  }

  public void AcceptQuestFromJson(DialogueQuestData qd)
  {
    if (_activeQuests.ContainsKey(qd.questId)) return;
    var quest = new Quest(qd.questId, qd.title, qd.description, qd.objectives);
    _activeQuests[quest.QuestId] = quest;
    OnQuestAccepted?.Invoke(quest);
    JournalManager.Instance.RecordQuest(quest.QuestId, quest.Title);
  }

  public bool IsQuestActive(string questId)
  {
    return _activeQuests.TryGetValue(questId, out var q)
        && q.Status == QuestStatus.Active;
  }

  public bool IsQuestCompleted(string questId)
  {
    return _activeQuests.TryGetValue(questId, out var q)
        && q.Status == QuestStatus.Completed;
  }

  public bool IsQuestIssuedOrCompleted(string questId)
  {
    return IsQuestActive(questId) || IsQuestCompleted(questId);
  }

  public void ReportItemDelivery(string npcId, string itemId, int amount = 1)
  {
    ReportProgressAll(itemId, npcId, amount);
  }

  public void ReportKill(string enemyId)
      => ReportProgressAll(enemyId, null, 1);

  public void ReportItemPickup(string itemId, int amount = 1)
  {
    ReportProgressAll(itemId, null, amount);
  }

  private void ReportProgressAll(string subjectId, string npcId, int amount)
  {
    foreach (var quest in _activeQuests.Values)
    {
      if (quest.Status != QuestStatus.Active) continue;
      quest.ReportProgress(subjectId, npcId, amount);
      Debug.Log($"subjectId: {subjectId}");
      OnQuestUpdated?.Invoke(quest);
    }
  }

  internal void HandleQuestCompleted(Quest quest)
  {
    OnQuestCompleted?.Invoke(quest);
  }

  public IReadOnlyList<Quest> GetActiveQuests() => _activeQuests.Values.ToList();
}
