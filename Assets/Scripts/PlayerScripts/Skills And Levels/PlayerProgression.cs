using UnityEngine;

public class PlayerProgression : MonoBehaviour
{
    public int level = 1;
    public int xp = 0;
    public int[] xpToNextLevel = { 100, 250, 500, 1000 }; 
    public delegate void LevelUpDelegate(int newLevel);
    public event LevelUpDelegate OnLevelUp;
    public CharacterSkillsUI characterSkillsUI;

    public void AddXP(int amount)
    {
        xp += amount;
        CheckLevelUp();
    }

    private void CheckLevelUp()
    {
        while (level - 1 < xpToNextLevel.Length && xp >= xpToNextLevel[level - 1])
        {
            xp -= xpToNextLevel[level - 1];
            level++;
            SkillManager.Instance.skillPoints++;
            OnLevelUp?.Invoke(level);
            characterSkillsUI.UpdateSkillPointsCounter();
        }
    }
}
