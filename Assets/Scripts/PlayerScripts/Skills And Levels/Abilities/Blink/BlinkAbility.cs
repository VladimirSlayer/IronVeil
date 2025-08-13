using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;


public class BlinkAbility : MonoBehaviour
{
    [Header("Настройки телепорта")]
    public float blinkRange;              
    public LayerMask blinkLayerMask;             
    public float groundLevel = 0f;              
    public float cooldownTime = 2f;             
    public float travelTime = 0.5f;              
    public Color gizmoColor = Color.cyan;       
    public float manaCost = 20f;               

    [Header("Замедление времени")]
    public float timeSlowFactor = 0.5f;
    private bool isTimeSlowed = false;

    [Header("UI")]
    public GameObject cancelAbilityText;        

    private Transform playerTransform;
    private Camera playerCamera;
    private PlayerStats playerStats;
    private PlayerMovement playerMovement;

    private bool isOnCooldown = false;
    private Vector3? targetPosition = null;
    private bool isSelecting = false;
    private bool isCancelled = false;

    private GameObject gizmoMarker;
    private float basicBlinkRadius = 4f;

    void Start()
    {
        blinkRange = basicBlinkRadius;
        playerTransform = transform;
        playerCamera = Camera.main;
        playerStats = GetComponent<PlayerStats>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if (IngameUIController.Instance.currentState == GameUIState.Gameplay)
        {
            blinkRange = basicBlinkRadius + SkillModifier.Instance.GetModifier("blinkAbility");
            HandleBlinkAbility();
        }
    }

    void HandleBlinkAbility()
    {
        bool rightMouseHeld = Input.GetMouseButton(1);

        if (rightMouseHeld && !isOnCooldown && !isCancelled && playerStats.currentMana >= manaCost)
        {
            cancelAbilityText.SetActive(true);
            if (!isTimeSlowed) SlowTime();

            isSelecting = true;
            UpdateTargetPosition();
        }

        if (isSelecting && Input.GetKeyDown(KeyCode.C))
        {
            cancelAbilityText.SetActive(false);
            CancelBlink();
            return;
        }

        if (Input.GetMouseButtonUp(1) && isSelecting && !isCancelled)
        {
            cancelAbilityText.SetActive(false);

            if (targetPosition.HasValue)
            {
                StartCoroutine(PerformBlink(targetPosition.Value));
                playerStats.UseMana(manaCost);
            }

            ResetTime();
            isSelecting = false;

            if (gizmoMarker)
            {
                Destroy(gizmoMarker);
                gizmoMarker = null;
            }
        }

        if (Input.GetMouseButtonUp(1))
        {
            isCancelled = false;
        }
    }

    void UpdateTargetPosition()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, blinkRange, blinkLayerMask))
        {
            targetPosition = hit.point;
        }
        else
        {
            Vector3 point = playerCamera.transform.position + playerCamera.transform.forward * blinkRange;
            targetPosition = new Vector3(point.x, Mathf.Max(point.y, groundLevel), point.z);
        }

        if (gizmoMarker == null)
        {
            gizmoMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            gizmoMarker.GetComponent<Collider>().enabled = false;
            gizmoMarker.GetComponent<Renderer>().material.color = gizmoColor;
            gizmoMarker.transform.localScale = Vector3.one * 0.5f;
        }

        gizmoMarker.transform.position = targetPosition.Value;
    }

    IEnumerator PerformBlink(Vector3 target)
    {
        StartCoroutine(StartCooldown());

        CharacterController controller = GetComponent<CharacterController>();

        if (Vector3.Distance(playerTransform.position, target) > blinkRange)
        {
            target = playerTransform.position + (target - playerTransform.position).normalized * blinkRange;
        }

        Vector3 start = transform.position;
        float elapsed = 0f;

        SoundManager.Instance.PlaySFXAt("tp", transform.position);

        while (elapsed < travelTime)
        {
            playerMovement.Velocity = new Vector3(playerMovement.Velocity.x, 0, playerMovement.Velocity.z);
            transform.position = Vector3.Lerp(start, target, elapsed / travelTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = target;
    }

    void CancelBlink()
    {

        ResetTime();

        if (gizmoMarker)
        {
            Destroy(gizmoMarker);
            gizmoMarker = null;
        }

        isSelecting = false;
        isCancelled = true;
        targetPosition = null;
    }

    void SlowTime()
    {
        Time.timeScale = timeSlowFactor;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
        isTimeSlowed = true;
    }

    void ResetTime()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        isTimeSlowed = false;
    }

    IEnumerator StartCooldown()
    {
        isOnCooldown = true;
        yield return new WaitForSecondsRealtime(cooldownTime);
        isOnCooldown = false;
    }
}
