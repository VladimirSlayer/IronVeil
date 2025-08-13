using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI npcText;
    public Transform optionsRoot;
    public Button optionButtonPrefab;
    public bool isDialogueActive = false;

    private List<Button> spawnedButtons = new List<Button>();

    void Awake()
    {
        dialoguePanel.SetActive(false);
    }

    void OnEnable()
    {
        DialogueManager.OnDialogueStart  += ShowPanel;
        DialogueManager.OnDialogueEnd    += HidePanel;
        DialogueManager.OnShowText       += UpdateText;
        DialogueManager.OnShowOptions    += PopulateOptions;
    }

    void OnDisable()
    {
        DialogueManager.OnDialogueStart  -= ShowPanel;
        DialogueManager.OnDialogueEnd    -= HidePanel;
        DialogueManager.OnShowText       -= UpdateText;
        DialogueManager.OnShowOptions    -= PopulateOptions;
    }

    private void ShowPanel()
    {
        isDialogueActive = true;
        dialoguePanel.SetActive(true);
        Camera.main.GetComponent<PlayerCamController>().camEnabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void HidePanel()
    {
        isDialogueActive = false;
        dialoguePanel.SetActive(false);
        ClearOptions();
        Camera.main.GetComponent<PlayerCamController>().camEnabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UpdateText(string text)
    {
        npcText.text = text;
    }

    private void PopulateOptions(List<DialogueOption> options)
    {
        ClearOptions();
        for (int i = 0; i < options.Count; i++)
        {
            var opt = options[i];
            var btn = Instantiate(optionButtonPrefab, optionsRoot);
            btn.GetComponentInChildren<TextMeshProUGUI>().text = opt.text;
            int index = i; 
            btn.onClick.AddListener(() => {
                DialogueManager.Instance.ChooseOption(index);
            });
            spawnedButtons.Add(btn);
        }
    }

    private void ClearOptions()
    {
        foreach (var b in spawnedButtons)
            if (b) Destroy(b.gameObject);
        spawnedButtons.Clear();
    }
}
