using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;


public class EnemyAI : MonoBehaviour
{
    [Header("Зрение")]
    public float viewRadius = 10f;                    
    public float viewAngle = 90f;                    
    public Transform visionPoint;                     
    public LayerMask targetLayer;                    
    public float updateInterval = 0.2f;                 
    public Color visionGizmoColor = new Color(1f, 0f, 0f, 0.2f);

    [Header("Погоня и поиск")]
    public float stopChaseDistance = 15f;        
    public float searchDuration = 5f;                   

    private Transform player;                          
    private List<Transform> visibleTargets = new();   
    private Vector3 lastKnownPosition;                 
    private bool isChasing = false;
    private bool isSearching = false;
    private Coroutine visionCoroutine;
    private NavMeshAgent navMeshAgent;
    private float searchTimer = 0f;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogError("Игрок с тегом 'Player' не найден!");
        }

        navMeshAgent = GetComponent<NavMeshAgent>();
        if (navMeshAgent == null)
        {
            Debug.LogError("NavMeshAgent не найден на враге!");
        }

        visionCoroutine = StartCoroutine(UpdateVision());
    }

    void Update()
    {
        if (isChasing)
        {
            ChasePlayer();
        }
        else if (isSearching)
        {
            SearchLastKnownPosition();
        }
        else
        {
            Patrol();
        }
    }

    IEnumerator UpdateVision()
    {
        while (true)
        {
            FindVisibleTargets();
            yield return new WaitForSeconds(updateInterval);
        }
    }

    void FindVisibleTargets()
    {
        visibleTargets.Clear();

        Collider[] targetsInRange = Physics.OverlapSphere(visionPoint.position, viewRadius, targetLayer);

        foreach (Collider target in targetsInRange)
        {
            Transform t = target.transform;
            Vector3 dirToTarget = (t.position - visionPoint.position).normalized;

            if (Vector3.Angle(visionPoint.forward, dirToTarget) < viewAngle / 2)
            {
                float dist = Vector3.Distance(visionPoint.position, t.position);

                if (Physics.Raycast(visionPoint.position, dirToTarget, out RaycastHit hit, dist))
                {
                    if (hit.transform == t)
                    {
                        visibleTargets.Add(t);

                        if (t.CompareTag("Player"))
                        {
                            isChasing = true;
                            isSearching = false;
                            lastKnownPosition = t.position;
                        }
                    }
                }
            }
        }
    }


    void Patrol()
    {
        navMeshAgent.isStopped = true;
        transform.Rotate(0f, 50f * Time.deltaTime, 0f);
    }


    void ChasePlayer()
    {
        if (player == null || navMeshAgent == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        Vector3 dirToPlayer = (player.position - visionPoint.position).normalized;

        bool canSeePlayer = Physics.Raycast(visionPoint.position, dirToPlayer, out RaycastHit hit, viewRadius) &&
                            hit.collider.transform == player;

        if (!canSeePlayer || distanceToPlayer > stopChaseDistance)
        {
            isChasing = false;
            isSearching = true;
            searchTimer = 0f;
            return;
        }

        navMeshAgent.isStopped = false;
        navMeshAgent.SetDestination(player.position);
        lastKnownPosition = player.position;
    }


    void SearchLastKnownPosition()
    {
        if (navMeshAgent.remainingDistance < 0.5f)
        {
            searchTimer += Time.deltaTime;

            if (searchTimer >= searchDuration)
            {
                isSearching = false;
                return;
            }

            transform.Rotate(0f, 50f * Time.deltaTime, 0f);
        }
        else
        {
            navMeshAgent.isStopped = false;
            navMeshAgent.SetDestination(lastKnownPosition);
        }
    }


    void OnDrawGizmosSelected()
    {
        if (visionPoint == null) return;

        Gizmos.color = visionGizmoColor;
        Gizmos.DrawWireSphere(visionPoint.position, viewRadius);

        Vector3 left = DirectionFromAngle(-viewAngle / 2, false);
        Vector3 right = DirectionFromAngle(viewAngle / 2, false);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(visionPoint.position, visionPoint.position + left * viewRadius);
        Gizmos.DrawLine(visionPoint.position, visionPoint.position + right * viewRadius);

        Gizmos.color = Color.red;
        foreach (Transform target in visibleTargets)
        {
            Gizmos.DrawLine(visionPoint.position, target.position);
        }
    }

    Vector3 DirectionFromAngle(float angleDegrees, bool global)
    {
        if (!global)
            angleDegrees += visionPoint.eulerAngles.y;

        float rad = angleDegrees * Mathf.Deg2Rad;
        return new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad));
    }


    void OnDisable()
    {
        if (visionCoroutine != null)
        {
            StopCoroutine(visionCoroutine);
        }
    }
}
