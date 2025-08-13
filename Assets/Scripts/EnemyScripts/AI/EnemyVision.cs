using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class EnemyVision : MonoBehaviour
{
    public float viewRadius = 10f;
    public float viewAngle = 90f;
    public Transform visionPoint;
    public LayerMask targetLayer;
    public float heightLimit = 2f;
    public float updateInterval = 0.2f;

    public Transform DetectedPlayer; 
    public float detectionTime = 3f;
    public float detectionProgress = 0f;
    public VisibilitySystem visibilitySystem;

    public bool targetLocked = false;
    private float forgetTimer = 2f;
    public float forgetCountdown = 0f;

    private void Start()
    {
        StartCoroutine(UpdateVision());
    }

    private IEnumerator UpdateVision()
    {
        while (true)
        {
            UpdateDetection();
            yield return new WaitForSeconds(updateInterval);
        }
    }

    private void UpdateDetection()
    {
        Transform targetPlayer = null;

        Collider[] targetsInViewRadius = Physics.OverlapSphere(visionPoint.position, viewRadius, targetLayer);

        foreach (Collider target in targetsInViewRadius)
        {
            Transform targetTransform = target.transform;
            Vector3 directionToTarget = (targetTransform.position - visionPoint.position).normalized;
            float verticalDifference = Mathf.Abs(targetTransform.position.y - visionPoint.position.y);

            if (verticalDifference > heightLimit)
                continue;

            if (Vector3.Angle(visionPoint.forward, directionToTarget) < viewAngle / 2)
            {
                float distanceToTarget = Vector3.Distance(visionPoint.position, targetTransform.position);

                if (Physics.Raycast(visionPoint.position, directionToTarget, out RaycastHit hit, distanceToTarget))
                {
                    if (hit.collider.transform == targetTransform)
                    {
                        targetPlayer = targetTransform;

                        float detectionRate = Mathf.Lerp(1f, 0.1f, distanceToTarget / viewRadius);
                        detectionProgress += detectionRate * (updateInterval / detectionTime) * (visibilitySystem.visibility + 1f);
                        detectionProgress = Mathf.Clamp01(detectionProgress);

                        if (detectionProgress >= 1f)
                        {
                            targetLocked = true;
                            DetectedPlayer = targetPlayer;
                            forgetCountdown = forgetTimer;
                        }

                        break;
                    }
                }
            }
        }

        if (targetPlayer == null)
        {
            if (targetLocked)
            {
                forgetCountdown -= updateInterval;

                if (forgetCountdown <= 0f)
                {
                    targetLocked = false;
                    DetectedPlayer = null;
                    detectionProgress = 0f;
                }
            }
            else
            {
                detectionProgress -= (updateInterval / detectionTime) * (visibilitySystem.visibility + 0.4f);
                detectionProgress = Mathf.Clamp01(detectionProgress);

                if (detectionProgress < 0.5f)
                    DetectedPlayer = null;
            }
        }
    }


    private void OnDrawGizmos()
    {
        if (visionPoint == null) return;


        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
        Gizmos.DrawWireSphere(visionPoint.position, viewRadius);


        Vector3 leftBoundary = DirectionFromAngle(-viewAngle / 2, false);
        Vector3 rightBoundary = DirectionFromAngle(viewAngle / 2, false);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(visionPoint.position, visionPoint.position + leftBoundary * viewRadius);
        Gizmos.DrawLine(visionPoint.position, visionPoint.position + rightBoundary * viewRadius);


        if (DetectedPlayer != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(visionPoint.position, DetectedPlayer.position);
        }
    }

    private Vector3 DirectionFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += visionPoint.eulerAngles.y;
        }
        float angleInRadians = angleInDegrees * Mathf.Deg2Rad;
        return new Vector3(Mathf.Sin(angleInRadians), 0, Mathf.Cos(angleInRadians));
    }
}
