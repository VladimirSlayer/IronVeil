using UnityEngine;

public class PlayerParameterController : MonoBehaviour
{
    private PlayerStats stats;
    private PlayerProgression progression;

    private void Start()
    {
        stats = GetComponent<PlayerStats>();
        progression = GetComponent<PlayerProgression>();

        progression.OnLevelUp += OnLevelUp;
    }

    void OnLevelUp(int newLevel)
    {
        Debug.Log($"Поздравляем! Вы достигли уровня {newLevel}");
    }
}
