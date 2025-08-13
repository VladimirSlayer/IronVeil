using UnityEngine;

public class SkillDebugInput : MonoBehaviour
{
    private SkillManager skillManager;
    private string[] skillIDs;

    private void Start()
    {
        skillManager = GetComponent<SkillManager>();
        skillIDs = new string[skillManager.GetAllSkills().Count];
        int index = 0;
        foreach (var skill in skillManager.GetAllSkills())
        {
            skillIDs[index++] = skill.Key;
        }
    }

    private void Update()
    {
        for (int i = 0; i < skillIDs.Length; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                skillManager.LevelUpSkill(skillIDs[i]);
                Debug.Log($"{skillIDs[i]} → {skillManager.GetSkillEffect(skillIDs[i])}");
            }
        }
    }
}
