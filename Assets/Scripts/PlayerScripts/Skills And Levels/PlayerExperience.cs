using UnityEngine;

public class PlayerExperience : MonoBehaviour
{
    public static PlayerExperience Instance { get; private set; }

    private PlayerProgression progression;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    private void Start()
    {
        progression = GetComponent<PlayerProgression>();
    }

    public void AddXP(int amount)
    {
        if (progression != null)
        {
            progression.AddXP(amount);
            Debug.Log($"Игрок получил {amount} XP");
        }
    }
}
