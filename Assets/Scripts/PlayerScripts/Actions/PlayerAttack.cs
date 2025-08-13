using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Атака")]
    public float handAttackRange = 1f;          
    public float handAttackDamage = 1f;           
    public float handAttackCooldown = 1f;      
    public LayerMask enemyLayer;                

    private bool canAttack = true;              
    private float currentAttackRange;
    private float currentAttackDamage;
    private float currentAttackCooldown;

    [Header("Анимация удара")]
    [SerializeField] private Animator animator;
    [SerializeField] private AnimationClip attackClip;
    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && canAttack)
        {
            animator.ResetTrigger("Attack");
            animator.SetTrigger("Attack");
            StartCoroutine(PerformAttack());
        }
    }

    private void OnEnable()
    {
        if (EquipmentManager.Instance != null)
            EquipmentManager.Instance.onEquipmentChanged += OnEquipmentChanged;
    }

    private void OnDisable()
    {
        if (EquipmentManager.Instance != null)
            EquipmentManager.Instance.onEquipmentChanged -= OnEquipmentChanged;
    }

    private void Start()
    {
        if (EquipmentManager.Instance != null)
            EquipmentManager.Instance.onEquipmentChanged += OnEquipmentChanged;

        OnEquipmentChanged(
            EquipmentManager.Instance?.equippedWeaponItem,
            EquipmentManager.Instance?.equippedArmorItem
        );
    }

    private void OnEquipmentChanged(ItemData newWeapon, ItemData newArmor)
    {
        if (newWeapon != null && newWeapon.weaponStats != null)
        {
            currentAttackDamage = newWeapon.weaponStats.damage;
            currentAttackRange = newWeapon.weaponStats.range;
            float attackSpeed     = newWeapon.weaponStats.attackSpeed;
            currentAttackCooldown = 1f / attackSpeed;
            if (animator != null && attackClip != null)
            {
                float speedMultiplier = attackClip.length / currentAttackCooldown;
                animator.SetFloat("AttackSpeed", speedMultiplier);
            }
        }
        else
        {
            currentAttackDamage = handAttackDamage;
            currentAttackRange = handAttackRange;
            currentAttackCooldown = handAttackCooldown;

            if (animator != null && attackClip != null)
            {
                float speedMultiplier = attackClip.length / currentAttackCooldown;
                animator.SetFloat("AttackSpeed", speedMultiplier);
            }
        }
    }

    private IEnumerator PerformAttack()
    {
        canAttack = false;

        Ray ray = Camera.main.ScreenPointToRay(new Vector2(Screen.width / 2f, Screen.height / 2f));

        if (Physics.Raycast(ray, out RaycastHit hit, currentAttackRange, enemyLayer))
        {
            Debug.Log("Попал во врага");
            EnemyHealthSystem enemyHealth;

            if (hit.collider.name == "BackKillPoint")
            {
                enemyHealth = hit.collider.transform.parent.gameObject.GetComponent<EnemyHealthSystem>();
                enemyHealth.TakeDamage(enemyHealth.currentHealth);
            }
            else
            {
                Debug.Log(hit.collider.name);
                enemyHealth = hit.collider.transform.gameObject.GetComponent<EnemyHealthSystem>();
                if (enemyHealth != null) enemyHealth.TakeDamage(currentAttackDamage);
            }
        }

        yield return new WaitForSeconds(currentAttackCooldown);

        canAttack = true;
    }

    private void OnDrawGizmosSelected()
    {
        if (Camera.main == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * currentAttackRange);
    }
}
