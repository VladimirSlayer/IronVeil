using UnityEngine;
using UnityEngine.AI;


public class EnemyHealthSystem : MonoBehaviour
{
    [Header("Параметры здоровья")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("VFX при попадании")]
    [Tooltip("Префаб ParticleSystem, который должен проиграться один раз при получении урона")]
    public ParticleSystem hitEffectPrefab;

    private Animator animator;
    private Rigidbody[] ragdollBodies;
    private Collider[] ragdollColliders;

    private void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();

        ragdollBodies = GetComponentsInChildren<Rigidbody>();
        ragdollColliders = GetComponentsInChildren<Collider>();

        DisableRagdoll();
    }

    public void TakeDamage(float damage)
    {
        if (hitEffectPrefab != null)
        {

            var vfx = Instantiate(
                hitEffectPrefab,
                transform.position + Vector3.up * 1f, 
                Quaternion.identity
            );
            vfx.Play();
            Destroy(vfx.gameObject, 
                vfx.main.duration + vfx.main.startLifetime.constantMax);
        }
        currentHealth -= damage;
        Debug.Log($"Врагу нанесено: {damage}, осталось здоровья: {currentHealth}");
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }

    private void Die()
    {
        if (TryGetComponent<EnemyMovement>(out var enemyMove) && enemyMove.eyeLight != null)
        {
            enemyMove.eyeLight.intensity = 0;
            enemyMove.StopAllCoroutines();
        }

        if (TryGetComponent<EnemyXPReward>(out var xpReward))
        {
            PlayerExperience.Instance.AddXP(xpReward.xpAmount);
        }

        foreach (var script in GetComponents<MonoBehaviour>())
        {
            script.enabled = false;
        }

        if (animator != null) animator.enabled = false;
        if (TryGetComponent<NavMeshAgent>(out var agent)) agent.enabled = false;

        EnableRagdoll();
    }

    private void EnableRagdoll()
    {
        if (animator != null) animator.enabled = false;

        foreach (var body in ragdollBodies)
        {
            body.isKinematic = false;
        }

        foreach (var col in ragdollColliders)
        {
            if (col.name == "BackKillPoint" || col.name == "Cube.004")
            {
                col.enabled = false;
                Debug.Log($"{col.name} отключен");
            }
            else
            {
                col.enabled = true;
            }
        }

        if (TryGetComponent<Collider>(out var mainCol)) mainCol.enabled = false;
        if (TryGetComponent<Rigidbody>(out var mainRb)) mainRb.isKinematic = true;
    }

    private void DisableRagdoll()
    {
        foreach (var body in ragdollBodies)
        {
            body.isKinematic = true;
        }

        foreach (var col in ragdollColliders)
        {
            if (col.name == "BackKillPoint" || col.name == "Cube.004")
            {
                col.enabled = true; 
            }
            else
            {
                col.enabled = false;
            }
        }

        if (TryGetComponent<Collider>(out var mainCol)) mainCol.enabled = true;
        if (TryGetComponent<Rigidbody>(out var mainRb)) mainRb.isKinematic = false;
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }
}
