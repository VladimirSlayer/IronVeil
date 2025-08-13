using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using Unity.AI.Navigation;

public class EnemyMovement : MonoBehaviour, IPausable
{
    public enum EnemyState
    {
        Patrol,
        Chase,
        GoToLastKnownPosition,
        RotateAndSearch,
        Idle,
        InvestigateSound,
        Attacking
    }

    [Header("Настройки")]
    public Light eyeLight;
    public float hearRadius = 5f;
    public LayerMask playerLayer;
    public Transform[] patrolPoints;

    [Header("Скорости и тайминги")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public float searchDuration = 5f;
    public float stopChaseDistance = 15f;
    public float investigationSoundTime = 5f;
    private float attackDistance = 3f;
    private float attackCooldown = 2.2f;

    [Header("Атака")]
    public int attackDamage = 10;

    [HideInInspector] public EnemyState currentState = EnemyState.Patrol;

    private NavMeshAgent navMeshAgent;
    private EnemyVision enemyVision;
    private int currentPatrolIndex = 0;
    private Vector3 lastKnownPosition;
    private Coroutine patrolCoroutine;
    private Coroutine searchCoroutine;
    private bool canAttack = true;
    private bool isPaused = false;

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        enemyVision = GetComponent<EnemyVision>();

        navMeshAgent.updatePosition = true;
        navMeshAgent.updateRotation = true;

        Animator anim = GetComponent<Animator>();
        if (anim != null) anim.applyRootMotion = false;

        if (enemyVision == null || navMeshAgent == null)
            Debug.LogError("NavMeshAgent или EnemyVision не найдены!");

        SetState(EnemyState.Patrol);
    }


    void Update()
    {
        if (isPaused) return;

        Physics.SyncTransforms();

        if (currentState == EnemyState.Chase || currentState == EnemyState.Attacking)
            ChaseUpdate();

        UpdateEyeLightColor();
        CheckForPlayerSound();
    }

    public void Pause()
    {
        isPaused = true;
        navMeshAgent.isStopped = true;
    }

    public void Resume()
    {
        isPaused = false;
        if (currentState != EnemyState.Attacking)
            navMeshAgent.isStopped = false;
    }

    private void SetState(EnemyState newState)
    {
        if (patrolCoroutine != null) StopCoroutine(patrolCoroutine);
        if (searchCoroutine != null) StopCoroutine(searchCoroutine);
        currentState = newState;

        switch (newState)
        {
            case EnemyState.Patrol:
                patrolCoroutine = StartCoroutine(PatrolCoroutine());
                break;

            case EnemyState.Chase:
                StartChasing();
                break;

            case EnemyState.GoToLastKnownPosition:
                searchCoroutine = StartCoroutine(GoToLastKnownPositionCoroutine());
                break;

            case EnemyState.RotateAndSearch:
                searchCoroutine = StartCoroutine(RotateAndSearchCoroutine());
                break;

            case EnemyState.Idle:
                Transform point = patrolPoints[currentPatrolIndex];
                float wait = point.GetComponent<PatrolPoint>()?.waitTime ?? 0f;
                patrolCoroutine = StartCoroutine(IdleCoroutine(wait));
                break;
        }
    }

    private IEnumerator PatrolCoroutine()
    {
        if (patrolPoints.Length == 0) yield break;

        while (true)
        {
            Transform point = patrolPoints[currentPatrolIndex];

            if (Vector3.Distance(transform.position, point.position) < 0.5f)
            {
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                point = patrolPoints[currentPatrolIndex];
            }

            navMeshAgent.speed = patrolSpeed;
            navMeshAgent.isStopped = false;

            bool set = navMeshAgent.SetDestination(point.position);

            if (!set) yield break;

            yield return new WaitUntil(() => !navMeshAgent.pathPending && navMeshAgent.hasPath);

            while (navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance)
            {
                if (isPaused) { yield return null; continue; }

                if (enemyVision.DetectedPlayer != null)
                {
                    SetState(EnemyState.Chase);
                    yield break;
                }

                yield return null;
            }

            SetState(EnemyState.Idle);
            yield break;
        }
    }



    private IEnumerator IdleCoroutine(float waitTime)
    {
        float elapsed = 0f;
        navMeshAgent.isStopped = true;

        while (elapsed < waitTime)
        {
            if (isPaused) { yield return null; continue; }

            if (enemyVision.DetectedPlayer != null)
            {
                SetState(EnemyState.Chase);
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        SetState(EnemyState.Patrol);
    }


    private void StartChasing()
    {
        navMeshAgent.speed = chaseSpeed;
    }

    private void ChaseUpdate()
    {
        if (isPaused) return;

        Transform player = enemyVision.DetectedPlayer;

        if (player == null)
        {
            lastKnownPosition = navMeshAgent.destination;
            SetState(EnemyState.GoToLastKnownPosition);
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > stopChaseDistance)
        {
            lastKnownPosition = player.position;
            SetState(EnemyState.GoToLastKnownPosition);
            return;
        }

        if (currentState == EnemyState.Attacking && Vector3.Distance(transform.position, player.position) > attackDistance)
        {
            SetState(EnemyState.Chase);
            return;
        }
        Debug.Log($"{distance <= attackDistance} distance <= attackDistance");
        if (distance <= attackDistance)
        {
            navMeshAgent.isStopped = true;
            SetState(EnemyState.Attacking);
            LookAtPlayer(player);
            AttackPlayer();
        }
        else
        {
            navMeshAgent.isStopped = false;
            navMeshAgent.SetDestination(player.position);
        }
    }

    private IEnumerator GoToLastKnownPositionCoroutine()
    {
        navMeshAgent.isStopped = false;
        navMeshAgent.speed = patrolSpeed;
        navMeshAgent.SetDestination(lastKnownPosition);

        while (navMeshAgent.pathPending || navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance)
        {
            if (isPaused) { yield return null; continue; }

            if (enemyVision.DetectedPlayer != null)
            {
                SetState(EnemyState.Chase);
                yield break;
            }

            yield return null;
        }

        SetState(EnemyState.RotateAndSearch);
    }

    private IEnumerator RotateAndSearchCoroutine()
    {
        navMeshAgent.isStopped = true;
        float elapsed = 0f;

        while (elapsed < searchDuration)
        {
            if (isPaused) { yield return null; continue; }

            if (enemyVision.DetectedPlayer != null)
            {
                SetState(EnemyState.Chase);
                yield break;
            }

            transform.Rotate(0f, 50f * Time.deltaTime, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        navMeshAgent.isStopped = false;
        SetState(EnemyState.Patrol);
    }

    private IEnumerator InvestigateSoundCoroutine(Vector3 position)
    {
        currentState = EnemyState.InvestigateSound;
        navMeshAgent.isStopped = true;
        float elapsed = 0f;

        while (elapsed < investigationSoundTime)
        {
            if (isPaused) { yield return null; continue; }

            if (enemyVision.DetectedPlayer != null)
            {
                SetState(EnemyState.Chase);
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        navMeshAgent.isStopped = false;
        SetState(EnemyState.Patrol);
    }

    private void AttackPlayer()
    {
        if (!canAttack) return;

        Transform target = enemyVision.DetectedPlayer;
        if (target == null)
        {
            SetState(EnemyState.GoToLastKnownPosition);
            return;
        }

        PlayerStats stats = target.GetComponent<PlayerStats>();
        if (stats != null)
        {
            stats.TakeDamage(attackDamage);
        }

        StartCoroutine(AttackCooldown());
    }

    private IEnumerator AttackCooldown()
    {
        canAttack = false;
        Debug.Log($"Enemy Attack On Cooldown {canAttack}");
        float elapsed = 0f;

        while (elapsed < attackCooldown)
        {
            if (isPaused) { yield return null; continue; }
            elapsed += Time.deltaTime;
            yield return null;
        }
        canAttack = true;
        Debug.Log($"Enemy Attack Cooldowned {canAttack}");
    }

    private void LookAtPlayer(Transform player)
    {
        Vector3 dir = (player.position - transform.position).normalized;
        Quaternion lookRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 5f);
    }

    private void CheckForPlayerSound()
    {
        if (isPaused) return;

        Collider[] players = Physics.OverlapSphere(transform.position, hearRadius, playerLayer);

        foreach (var playerCol in players)
        {
            var move = playerCol.GetComponent<PlayerMovement>();
            if (move != null && (move.CurrentState == "Running" || move.CurrentState == "Walking"))
            {
                Vector3 soundPos = playerCol.transform.position;

                if (enemyVision.detectionProgress > 0.8f)
                {
                    lastKnownPosition = soundPos;
                    SetState(EnemyState.GoToLastKnownPosition);
                }
                else if (currentState != EnemyState.Chase && currentState != EnemyState.GoToLastKnownPosition)
                {
                    StopAllCoroutines();
                    LookAtPlayer(playerCol.transform);
                    StartCoroutine(InvestigateSoundCoroutine(soundPos));
                }

                return;
            }
        }
    }

    private void UpdateEyeLightColor()
    {
        if (enemyVision == null || eyeLight == null) return;

        Color target = Color.Lerp(Color.green, Color.red, enemyVision.detectionProgress);
        eyeLight.color = target;
    }

    private void OnDrawGizmos()
    {
        if (patrolPoints.Length > 1)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                Transform current = patrolPoints[i];
                Transform next = patrolPoints[(i + 1) % patrolPoints.Length];
                if (current && next)
                    Gizmos.DrawLine(current.position, next.position);
            }
        }

        foreach (var point in patrolPoints)
        {
            if (point)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(point.position, 0.3f);
            }
        }
    }
}
