using System.Linq;

public class Quest
{
  public string QuestId { get; }
  public string Title { get; }
  public string Description { get; }
  public QuestStatus Status { get; private set; }
  public QuestObjective[] Objectives => _objectives;

  private QuestObjective[] _objectives;

  public Quest(QuestData data)
  {
    QuestId = data.questId;
    Title = data.title;
    Description = data.description;
    Status = QuestStatus.Active;

    _objectives = data.objectives
                      .Select(o => QuestObjective.Create(o))
                      .ToArray();
  }

  public Quest(string questId, string title, string description, QuestObjectiveData[] objectivesData)
  {
    QuestId = questId;
    Title = title;
    Description = description;
    Status = QuestStatus.Active;

    _objectives = objectivesData
                      .Select(o => QuestObjective.Create(o))
                      .ToArray();
  }

  public void ReportProgress(string subjectId, string npcId = null, int amount = 1)
  {
    foreach (var obj in _objectives)
      obj.AddProgress(subjectId, amount, npcId);

    if (_objectives.All(o => o.IsComplete))
      Complete();
  }

  private void Complete()
  {
    Status = QuestStatus.Completed;
    QuestManager.Instance.HandleQuestCompleted(this);
  }
}
