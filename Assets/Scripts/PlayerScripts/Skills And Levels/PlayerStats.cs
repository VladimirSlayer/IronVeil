using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }
    public event Action OnStatsChanged;

    [Header("Base Stats")]
    public float baseHealth = 100f;
    public float baseMana = 100f;

    [Header("Current Stats")]
    public float maxHealth;
    public float currentHealth;

    public float maxMana;
    public float currentMana;

    public float manaRegenRate = 5f;
    public float manaRegenLimit = 20f;

    private float manaRegenAvailable = 0f;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        RecalculateStatsFromSkills();

        currentHealth = maxHealth;
        currentMana = maxMana;
    }

    private void Update()
    {
        RegenerateMana();
    }

    public void Die()
    {
        SceneManager.UnloadSceneAsync(1);
        SceneManager.LoadScene(0);
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth < 0f) Die();
        SoundManager.Instance.PlaySFXAt("player-damage", transform.position);
        OnStatsChanged?.Invoke();
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        OnStatsChanged?.Invoke();
    }

    public void AddMana(float amount)
    {
        currentMana += amount;
        if (currentMana > maxMana) currentMana = maxMana;
        OnStatsChanged?.Invoke();
    }

    public void UseMana(float amount)
    {
        currentMana -= amount;
        if (currentMana < 0f) currentMana = 0f;

        manaRegenAvailable = manaRegenLimit;
        OnStatsChanged?.Invoke();
    }

    private void RegenerateMana()
    {
        if (manaRegenAvailable <= 0f) return;

        float regenAmount = manaRegenRate * Time.deltaTime;
        regenAmount = Mathf.Min(regenAmount, manaRegenAvailable); 

        currentMana += regenAmount;
        if (currentMana > maxMana) currentMana = maxMana;

        manaRegenAvailable -= regenAmount;
        OnStatsChanged?.Invoke();
    }

    public void RecalculateStatsFromSkills()
    {
        float healthBonus = SkillModifier.Instance.GetModifier("maxHealth");
        float manaBonus = SkillModifier.Instance.GetModifier("maxMana");

        maxHealth = baseHealth + healthBonus;
        maxMana = baseMana + manaBonus;

        OnStatsChanged?.Invoke();
        Debug.Log($"[PlayerStats] Статы обновлены: HP={maxHealth}, MP={maxMana}");
    }

    public float GetCurrentHealth() => currentHealth;
    public float GetCurrentMana() => currentMana;
}
