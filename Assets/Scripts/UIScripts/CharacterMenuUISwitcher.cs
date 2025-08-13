using UnityEngine;

public class CharacterMenuUISwitcher : MonoBehaviour
{
    public GameObject statsEquipMenu;
    public GameObject skillsMenu;
    public GameObject unusedSkillPoints;
    public GameObject questJournal;

    public void DisableAllViews()
    {
        statsEquipMenu.SetActive(false);
        skillsMenu.SetActive(false);
        unusedSkillPoints.SetActive(false);
        questJournal.SetActive(false);
    }

    public void ShowStatsView()
    {
        DisableAllViews();
        statsEquipMenu.SetActive(true);
        statsEquipMenu.GetComponent<CharacterEquipAndStatsUI>().RefreshUI();
    }
    public void ShowSkillView()
    {
        DisableAllViews();
        skillsMenu.SetActive(true);
        unusedSkillPoints.SetActive(true);
    }
        public void ShowQuestJournal()
    {
        DisableAllViews();
        questJournal.SetActive(true);
    }
}
