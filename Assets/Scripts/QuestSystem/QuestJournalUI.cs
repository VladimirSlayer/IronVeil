using UnityEngine;
using TMPro;
using System.Linq;

public class QuestJournalUI : MonoBehaviour
{
  [Header("UI References")]
  public RectTransform contentRoot;    
  public GameObject entryPrefab;        

  void OnEnable()
  {
    QuestManager.Instance.OnQuestAccepted += Rebuild;
    QuestManager.Instance.OnQuestUpdated += Rebuild;
    QuestManager.Instance.OnQuestCompleted += Rebuild;
    Rebuild(null);
  }

  void OnDisable()
  {
    QuestManager.Instance.OnQuestAccepted -= Rebuild;
    QuestManager.Instance.OnQuestUpdated -= Rebuild;
    QuestManager.Instance.OnQuestCompleted -= Rebuild;
  }

  void Rebuild(Quest _)
  {
    foreach (Transform t in contentRoot) Destroy(t.gameObject);

    foreach (var q in QuestManager.Instance.GetActiveQuests())
    {
      var go = Instantiate(entryPrefab, contentRoot);
      var txt = go.GetComponentInChildren<TextMeshProUGUI>();

      string progress = q.Status == QuestStatus.Completed
          ? "<color=green>✓ Завершён</color>"
          : string.Join("\n",
              q.Objectives
               .Select(o => $"{o.Description}: {o.CurrentAmount}/{o.RequiredAmount}"));

      txt.text = $"<b>{q.Title}</b>\n{q.Description}\n{progress}";
    }
  }
}
