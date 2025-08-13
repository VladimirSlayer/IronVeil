using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class CharacterSkillsUI : MonoBehaviour
{
    public GameObject skillEntryPrefab; 
    public Transform skillListParent;   

    private SkillManager skillManager;
    private Dictionary<string, SkillUIEntry> skillUIEntries = new();

    void Start()
    {
        skillManager = FindAnyObjectByType<SkillManager>();
        if (skillManager == null)
        {
            Debug.LogError("SkillManager не найден в сцене!");
            return;
        }
        UpdateSkillPointsCounter();
    }

    public void InitializeUI()
    {
        if (skillManager == null)
        {
            skillManager = FindAnyObjectByType<SkillManager>();
            if (skillManager == null)
            {
                Debug.LogError("SkillManager не найден при попытке инициализировать UI!");
                return;
            }
        }

        foreach (Transform child in skillListParent)
            Destroy(child.gameObject);

        skillUIEntries.Clear();

        foreach (var skillPair in skillManager.GetAllSkills())
        {
            string skillId = skillPair.Key;
            Skill skill = skillPair.Value;

            GameObject entryObj = Instantiate(skillEntryPrefab, skillListParent);
            SkillUIEntry entry = entryObj.GetComponent<SkillUIEntry>();

            if (entry == null)
            {
                Debug.LogError("SkillUIEntry компонент не найден на префабе!");
                continue;
            }

            entry.nameText.text = skill.Name;
            entry.levelText.text = $"Ур. {skill.CurrentLevel}/{skill.MaxLevel}";
            entry.upgradeButton.interactable = skill.CurrentLevel < skill.MaxLevel;

            entry.upgradeButton.onClick.AddListener(() =>
            {
                skillManager.LevelUpSkill(skillId);
                UpdateSkillUI(skillId);
                UpdateSkillPointsCounter();
            });

            skillUIEntries.Add(skillId, entry);
        }
    }

    private void UpdateSkillUI(string skillId)
    {
        if (!skillUIEntries.ContainsKey(skillId)) return;
        Skill skill = skillManager.GetAllSkills()[skillId];
        SkillUIEntry entry = skillUIEntries[skillId];
        entry.levelText.text = $"Ур. {skill.CurrentLevel}/{skill.MaxLevel}";
        entry.upgradeButton.interactable = skill.CurrentLevel < skill.MaxLevel;
    }

    public void UpdateSkillPointsCounter()
    {
        transform.parent.GetComponent<CharacterMenuUISwitcher>().unusedSkillPoints.GetComponent<TMP_Text>().text = $"Непотраченные очки навыков: {skillManager.skillPoints}";
    }
}
