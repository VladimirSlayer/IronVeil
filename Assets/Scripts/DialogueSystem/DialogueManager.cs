using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class DialogueManager : MonoBehaviour
{
  public static DialogueManager Instance { get; private set; }

  public static event Action OnDialogueStart;
  public static event Action OnDialogueEnd;
  public static event Action<string> OnShowText;
  public static event Action<List<DialogueOption>> OnShowOptions;

  private DialogueData dialogueData;
  private Dictionary<string, DialogueNode> nodeLookup;
  private DialogueNode currentNode;

  void Awake()
  {
    if (Instance == null) Instance = this;
    else Destroy(gameObject);
  }

  public void StartDialogue(TextAsset dialogueJson)
  {
    dialogueData = DialogueLoader.LoadFromJson(dialogueJson);
    nodeLookup = dialogueData.nodes.ToDictionary(n => n.id);

    OnDialogueStart?.Invoke();

    currentNode = nodeLookup["start"];
    ShowCurrent();
  }

  private void ShowCurrent()
  {

    OnShowText?.Invoke(currentNode.text);

    var filteredOptions = currentNode.options
        .Where(opt =>
        {

          if (opt.hideIfQuestIssuedOrCompleted
           && !string.IsNullOrEmpty(opt.questIdCondition)
           && QuestManager.Instance.IsQuestIssuedOrCompleted(opt.questIdCondition))
          {
            return false;
          }
          return true;
        })
        .ToList();

    OnShowOptions?.Invoke(filteredOptions);

    JournalManager.Instance.RecordDialogue(dialogueData.dialogueId, currentNode.id);
  }



  public void ChooseOption(int index)
  {
    var opt = currentNode.options[index];
    string next = opt.nextNode?.Trim();

    if (string.IsNullOrEmpty(next))
    {
      EndDialogue();
      return;
    }

    if (!nodeLookup.TryGetValue(next, out var nextNode))
    {
      Debug.LogWarning($"DialogueManager: узел с id '{next}' не найден.");
      EndDialogue();
      return;
    }

    currentNode = nextNode;

    if (currentNode.givesQuest)
    {
      if (DialogueLoader.QuestDataByNodeId != null
       && DialogueLoader.QuestDataByNodeId.TryGetValue(currentNode.id, out var qd)
       && qd != null)
      {
        QuestManager.Instance.AcceptQuestFromJson(qd);
      }
      else if (currentNode.questAsset != null)
      {
        QuestManager.Instance.AcceptQuest(currentNode.questAsset);
      }
      else
      {
        Debug.LogWarning($"Для узла '{currentNode.id}' помечено givesQuest, но ни JSON‑данных, ни questAsset нет.");
      }
    }


    ShowCurrent();
  }


  private void EndDialogue()
  {
    OnShowText?.Invoke(string.Empty);
    OnShowOptions?.Invoke(new List<DialogueOption>());

    OnDialogueEnd?.Invoke();
  }
}
