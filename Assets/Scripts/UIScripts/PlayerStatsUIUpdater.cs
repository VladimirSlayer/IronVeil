using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerStatsUIUpdater : MonoBehaviour
{
    public Slider hpSlider;
    public Slider manaSlider;

    private void Start()
    {
        StartCoroutine(InitAfterDelay());
    }

    private IEnumerator InitAfterDelay()
    {
        yield return null;

        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.OnStatsChanged += UpdateUI;
            UpdateUI();
        }
    }

    private void OnDestroy()
    {
        if (PlayerStats.Instance != null)
            PlayerStats.Instance.OnStatsChanged -= UpdateUI;
    }

    private void UpdateUI()
    {
        if (PlayerStats.Instance == null) return;

        hpSlider.maxValue = PlayerStats.Instance.maxHealth;
        hpSlider.value = PlayerStats.Instance.GetCurrentHealth();
        manaSlider.maxValue = PlayerStats.Instance.maxMana;
        manaSlider.value = PlayerStats.Instance.GetCurrentMana();
    }
}
