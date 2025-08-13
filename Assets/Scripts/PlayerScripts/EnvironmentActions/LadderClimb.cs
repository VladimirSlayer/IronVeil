using UnityEngine;

public class LadderClimbController : MonoBehaviour
{
    public float climbSpeed = 3f;
    private bool isClimbing = false;
    private CharacterController controller;
    private PlayerMovement pMovement;
    private Vector3 moveDirection;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        pMovement = GetComponent<PlayerMovement>();

    }

    void Update()
    {
        if (isClimbing)
        {
            float verticalInput = Input.GetAxis("Vertical");
            moveDirection = new Vector3(0, verticalInput * climbSpeed, 0);
            pMovement.Gravity = 0f;
            controller.Move(moveDirection * Time.deltaTime);
            Debug.Log("isClimbing");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ladder"))
        {
            isClimbing = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ladder"))
        {
            isClimbing = false;

            pMovement.Gravity = -9.81f;
            pMovement.Velocity = new Vector3(pMovement.Velocity.x, -2f, pMovement.Velocity.z);
        }
    }
}
