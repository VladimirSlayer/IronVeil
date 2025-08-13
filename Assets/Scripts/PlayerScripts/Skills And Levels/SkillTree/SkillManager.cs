using System.Collections.Generic;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    private Dictionary<string, Skill> skills = new();
    public static SkillManager Instance { get; private set; }
    public int skillPoints = 1;
    [System.Serializable]
    public class SkillConfig
    {
        public string id;
        public string displayName;
        public int maxLevel;
        public float effectPerLevel;
    }

    [Header("Настройка скиллов")]
    public List<SkillConfig> skillConfigs = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        if (skillConfigs.Count == 0)
        {
            AddDefaultSkills();
        }

        foreach (var config in skillConfigs)
        {
            if (!skills.ContainsKey(config.id))
                skills.Add(config.id, new Skill(config.displayName, config.maxLevel, config.effectPerLevel));
        }
    }

    private void AddDefaultSkills()
    {
        skillConfigs = new List<SkillConfig>
        {
            new SkillConfig { id = "crouchSpeed", displayName = "Крадущаяся Тишина", maxLevel = 3, effectPerLevel = 1.5f },
            new SkillConfig { id = "jumpHeight", displayName = "Гравитационный Толчок", maxLevel = 3, effectPerLevel = 1f },
            new SkillConfig { id = "playerStats", displayName = "Живая Сталь", maxLevel = 4, effectPerLevel = 25f },
            new SkillConfig { id = "objectDragging", displayName = "Кинетический Привод", maxLevel = 3, effectPerLevel = 2.5f },
            new SkillConfig { id = "blinkAbility", displayName = "Катушка Искажения", maxLevel = 3, effectPerLevel = 5f },
            new SkillConfig { id = "eyeAbility", displayName = "Эфирный Сканер", maxLevel = 3, effectPerLevel = 7.5f },
            new SkillConfig { id = "lockpicking", displayName = "Взлом Замков", maxLevel = 5, effectPerLevel = 10f },
            new SkillConfig { id = "crouchHeight", displayName = "Пластичность", maxLevel = 2, effectPerLevel = 0.5f }
        };
    }

    public void LevelUpSkill(string skillName)
    {
        if (!skills.ContainsKey(skillName) || skillPoints<=0) return;
        skillPoints--;

        var skill = skills[skillName];
        if (skill.LevelUp())
            Debug.Log($"Навык '{skill.Name}' прокачан! Уровень: {skill.CurrentLevel}");
        else
            Debug.Log($"Навык '{skill.Name}' уже на максимуме.");
    }

    public float GetSkillEffect(string skillName)
    {
        return skills.ContainsKey(skillName) ? skills[skillName].GetCurrentEffect() : 0f;
    }

    public Dictionary<string, Skill> GetAllSkills() => skills;
}
