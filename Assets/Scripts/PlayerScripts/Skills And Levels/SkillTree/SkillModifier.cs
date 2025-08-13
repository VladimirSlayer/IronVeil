using System.Collections.Generic;
using UnityEngine;

public class SkillModifier : MonoBehaviour
{
    public static SkillModifier Instance { get; private set; }

    private SkillManager skillManager;
    private Dictionary<string, float> modifiers = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        skillManager = GetComponent<SkillManager>();
    }

    private void Update()
    {
        if (skillManager == null) return;

        foreach (var pair in skillManager.GetAllSkills())
        {
            modifiers[pair.Key] = pair.Value.GetCurrentEffect();
        }
    }

    public float GetModifier(string key) => modifiers.TryGetValue(key, out var value) ? value : 0f;

    public void Upgrade(string key, float amount)
    {
        if (!modifiers.ContainsKey(key)) modifiers[key] = 0f;
        modifiers[key] += amount;
    }
}
