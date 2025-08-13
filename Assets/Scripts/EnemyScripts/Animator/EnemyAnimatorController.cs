using UnityEngine;

public class EnemyAnimatorController : MonoBehaviour
{
    private Animator animator;
    private EnemyMovement enemyMovement;

    private void Start()
    {
        animator = GetComponent<Animator>();
        enemyMovement = GetComponent<EnemyMovement>();

        if (animator == null)
            Debug.LogError("Animator не найден!");

        if (enemyMovement == null)
            Debug.LogError("EnemyMovement не найден!");
    }

    private void Update()
    {
        UpdateAnimationState();
    }

    private void UpdateAnimationState()
    {
        SetAllBools(false);

        switch (enemyMovement.currentState)
        {
            case EnemyMovement.EnemyState.Patrol:
                animator.SetBool("IsWalking", true);
                break;

            case EnemyMovement.EnemyState.Idle:
                animator.SetBool("IsIdle", true);
                break;

            case EnemyMovement.EnemyState.Chase:
                animator.SetBool("IsWalking", true);
                animator.SetBool("IsChasing", true);
                break;

            case EnemyMovement.EnemyState.GoToLastKnownPosition:
                animator.SetBool("IsChasing", true);
                break;

            case EnemyMovement.EnemyState.RotateAndSearch:
                animator.SetBool("IsLookingAround", true);
                break;

            case EnemyMovement.EnemyState.Attacking:
                animator.SetBool("IsAttacking", true);
                break;

            default:
                animator.SetBool("IsIdle", true);
                break;
        }
    }


    private void SetAllBools(bool state)
    {
        animator.SetBool("IsWalking", state);
        animator.SetBool("IsIdle", state);
        animator.SetBool("IsLookingAround", state);
        animator.SetBool("IsChasing", state);
        animator.SetBool("IsAttacking", state);
    }
}
