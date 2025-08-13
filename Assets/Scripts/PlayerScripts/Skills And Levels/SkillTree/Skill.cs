using UnityEngine;

public class Skill
{
    public string Name { get; private set; }           
    public int CurrentLevel { get; private set; }       
    public int MaxLevel { get; private set; }           
    public float EffectPerLevel { get; private set; }   

    public Skill(string name, int maxLevel, float effectPerLevel)
    {
        Name = name;
        MaxLevel = maxLevel;
        EffectPerLevel = effectPerLevel;
        CurrentLevel = 0;
    }

    public bool LevelUp()
    {
        if (CurrentLevel < MaxLevel)
        {
            CurrentLevel++;
            return true;
        }
        return false;
    }

    public float GetCurrentEffect()
    {
        return CurrentLevel * EffectPerLevel;
    }
}
