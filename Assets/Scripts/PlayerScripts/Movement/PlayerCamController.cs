using UnityEngine;

public class PlayerCamController : MonoBehaviour
{
    public bool camEnabled = true; 

    public enum CameraState
    {
        Normal,  
        Zooming  
    }

    [Header("Поворот камеры")]
    public float lookSpeed = 2f; 

    [Header("Настройки зума")]
    public float normalFOV = 60f;
    public float zoomFOV = 30f;  
    public float zoomSpeed = 5f; 

    private Transform playerBody;       
    private float xRotation = 0f;         
    private Camera playerCamera;        
    private CameraState currentState = CameraState.Normal; 

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; 

        playerBody = transform.parent;         
        playerCamera = GetComponent<Camera>();  

        playerCamera.fieldOfView = normalFOV;  
    }

    void Update()
    {
        if (!camEnabled) return;

        LookAround();      
        HandleZoomInput(); 
        UpdateCameraState(); 
    }

    void LookAround()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * lookSpeed;

        playerBody.Rotate(Vector3.up * mouseX);

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localEulerAngles = new Vector3(xRotation, 0f, 0f);
    }

    void HandleZoomInput()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            currentState = (currentState == CameraState.Normal) ? CameraState.Zooming : CameraState.Normal;
        }
    }

    void UpdateCameraState()
    {
        float targetFOV = (currentState == CameraState.Zooming) ? zoomFOV : normalFOV;
        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * zoomSpeed);
    }
}
