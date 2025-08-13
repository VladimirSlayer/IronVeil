using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private enum PlayerState { Idle, Walking, Running, Crouching }

    private PlayerState currentState = PlayerState.Idle;
    public string CurrentState => currentState.ToString();

    private CharacterController characterController;
    private PlayerStats playerStats;
    private Transform cameraTransform;

    public ItemData testItem;

    private Vector3 velocity;
    public Vector3 Velocity { get => velocity; set => velocity = value; }

    private float gravity = -9.81f;
    public float Gravity
    {
        get { return gravity; }
        set { gravity = value; }
    }
    public bool isGrounded;
    private bool isLeaning = false;

    private float xRotation = 0f;
    private float cameraBobTimer = 0f;
    private Vector3 originalCameraPosition;

    [Header("Base Movement Settings")]
    public float baseWalkSpeed = 5f;
    public float baseRunSpeed = 8f;
    public float baseCrouchSpeed = 2f;
    public float baseJumpHeight = 2f;

    [Header("Lean Settings")]
    public float leanAngle = 15f;
    public float leanSpeed = 5f;
    private float currentLeanAngle = 0f;

    [Header("Falling Damage")]
    public float fallDamageCoef = 1f;

    [Header("Camera Bob")]
    public float cameraBobFrequency = 2f;
    public float cameraBobAmplitude = 0.1f;

    private float stepTimer = 0f;
    private float stepInterval = 0.5f; 
    private int stepIndex = 0;
    private readonly string[] stepClips = { "step1", "step2", "step3", "step4" };


    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerStats = GetComponent<PlayerStats>();
        cameraTransform = Camera.main.transform;

        originalCameraPosition = cameraTransform.localPosition;

        RecalculateStatsFromSkills();
    }

    private void Update()
    {
        MovePlayer();
        UpdateState();
        HandleLeaning();
        HandleCameraRotation();
        RestrictCapsuleRotation();
        HandleCameraBobbing();
    }



    private void MovePlayer()
    {
        bool wasGrounded = isGrounded;
        isGrounded = characterController.isGrounded;

        if (isGrounded)
        {
            if (!wasGrounded && velocity.y < -12f)
                ApplyFallDamage((Mathf.Abs(velocity.y) - 12f) * fallDamageCoef);

            if (velocity.y < 0f) velocity.y = -2f;
        }

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        float speed = currentState switch
        {
            PlayerState.Walking => baseWalkSpeed + SkillModifier.Instance.GetModifier("walkSpeed"),
            PlayerState.Running => baseRunSpeed + SkillModifier.Instance.GetModifier("runSpeed"),
            PlayerState.Crouching => baseCrouchSpeed + SkillModifier.Instance.GetModifier("crouchSpeed"),
            _ => 0f
        };

        
        if (currentState == PlayerState.Walking || currentState == PlayerState.Running || currentState == PlayerState.Crouching)
        {
            bool isMoving = Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0;

            if (isMoving)
            {
                float currentInterval = currentState switch
                {
                    PlayerState.Running => 0.25f,
                    PlayerState.Walking => 0.5f,
                    PlayerState.Crouching => 0.7f,
                    _ => 0.5f
                };

                if (stepTimer <= 0f)
                {
                    string clipName = stepClips[stepIndex];
                    SoundManager.Instance.PlaySFXAt(clipName, transform.position);

                    stepIndex = (stepIndex + 1) % stepClips.Length; 
                    stepTimer = currentInterval;
                }
                else
                {
                    stepTimer -= Time.deltaTime;
                }
            }
            else
            {
                stepTimer = 0f; 
            }
        }
        else
        {
            stepTimer = 0f; 
        }

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        characterController.Move(move * speed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded && currentState != PlayerState.Crouching)
        {
            float jumpBoost = SkillModifier.Instance.GetModifier("jumpHeight");
            velocity.y = Mathf.Sqrt((baseJumpHeight + jumpBoost) * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    private void UpdateState()
    {
        bool isMoving = Input.GetAxis("Vertical") != 0 || Input.GetAxis("Horizontal") != 0;

        if (Input.GetKey(KeyCode.LeftControl))
            currentState = PlayerState.Crouching;
        else if (Input.GetKey(KeyCode.LeftShift) && isMoving)
            currentState = PlayerState.Running;
        else if (isMoving)
            currentState = PlayerState.Walking;
        else
            currentState = PlayerState.Idle;

        characterController.height = (currentState == PlayerState.Crouching) ? (1.5f - SkillModifier.Instance.GetModifier("crouchHeight")) : 2f;
    }

    private void HandleLeaning()
    {
        float targetLeanAngle = 0f;

        if (Input.GetKeyDown(KeyCode.P))
        {
            InventoryManager.Instance.AddItem(testItem); 
        }


        if (Input.GetKey(KeyCode.Q) && currentState != PlayerState.Running)
        {
            targetLeanAngle = leanAngle;
            isLeaning = true;
        }
        else if (Input.GetKey(KeyCode.E) && currentState != PlayerState.Running)
        {
            targetLeanAngle = -leanAngle;
            isLeaning = true;
        }
        else
        {
            isLeaning = false;
        }

        currentLeanAngle = Mathf.Lerp(currentLeanAngle, targetLeanAngle, Time.deltaTime * leanSpeed);
        transform.localRotation = Quaternion.Euler(0f, transform.localRotation.eulerAngles.y, currentLeanAngle);
        cameraTransform.localRotation = Quaternion.Euler(cameraTransform.localRotation.eulerAngles.x,
                                                         cameraTransform.localRotation.eulerAngles.y,
                                                        -currentLeanAngle);
    }

    private void HandleCameraRotation()
    {
        if (isLeaning) return;
    }

    private void RestrictCapsuleRotation()
    {
        transform.localRotation = Quaternion.Euler(0f, transform.localRotation.eulerAngles.y, transform.localRotation.eulerAngles.z);
    }

    private void HandleCameraBobbing()
    {
        if (currentState == PlayerState.Running && isGrounded)
        {
            cameraBobTimer += Time.deltaTime * cameraBobFrequency;
            float offsetY = Mathf.Sin(cameraBobTimer) * cameraBobAmplitude;
            cameraTransform.localPosition = originalCameraPosition + new Vector3(0, offsetY, 0);
        }
        else
        {
            cameraBobTimer = 0f;
            cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, originalCameraPosition, Time.deltaTime * 10f);
        }
    }

    private void ApplyFallDamage(float damage)
    {
        playerStats.TakeDamage(damage);
    }

    public void RecalculateStatsFromSkills()
    {
    }
}
