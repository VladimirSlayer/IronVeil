using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ObjectDragging : MonoBehaviour
{
    [Header("Settings")]
    public float interactionDistance = 5f; 
    public LayerMask draggableLayer;       
    public Transform holdPoint;     
    public float holdRadius = 3f;   
    public float smoothSpeed = 10f;    

    [Header("Throwing")]
    public float baseThrowPower = 20f;     
    private float currentThrowPower;     

    [Header("Pickup UI")]
    public TMP_Text pickupPromptText;
    public float pickupDistance = 3f;

    private Camera playerCamera;         
    private GameObject highlightedObject; 
    private GameObject heldObject;    
    private Rigidbody heldRigidbody;   
    private Material originalMaterial;  
    private Vector3 moveVelocity = Vector3.zero; 
    private ItemPickup lookedPickup;

    void Start()
    {
        playerCamera = Camera.main;
        UpdateThrowPower(); 
    }

    void Update()
    {
        UpdateThrowPower(); 

        if (heldObject) 
        {
            DragObject(); 
            if (Input.GetMouseButtonDown(0)) ReleaseObject(true); 
            AdjustHoldDistance();
        }
        else 
        {
            CheckForObject(); 
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (!heldObject && highlightedObject) PickUpObject();
            else if (heldObject) ReleaseObject(false);
            else if (lookedPickup != null)
            {
                InventoryManager.Instance.AddItem(lookedPickup.itemData);
                QuestManager.Instance.ReportItemPickup(lookedPickup.itemData.itemId);
                if (lookedPickup.transform.parent != null)
                    Destroy(lookedPickup.transform.parent.gameObject);
                else
                    Destroy(lookedPickup.transform.gameObject);
            }

        }
    }

    void FixedUpdate()
    {
        if (heldObject) DragObject();
    }

    private void CheckForPickup()
    {
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        if (Physics.Raycast(ray, out RaycastHit hit, 3f, LayerMask.GetMask("Item")))
        {
            var pickup = hit.collider.GetComponent<ItemPickup>();
            Debug.Log(pickup.itemData.itemName);
            if (pickup == null) return;
            lookedPickup = pickup;
            pickupPromptText.text = $"F – поднять «{pickup.itemData.itemName}»";
            pickupPromptText.gameObject.SetActive(true);
        }
        else
        {
            lookedPickup = null;
            pickupPromptText.gameObject.SetActive(false);
        }
    }

    private void CheckDoor()
    {
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        if (Physics.Raycast(ray, out RaycastHit doorHit, 3f, LayerMask.GetMask("Door")))
        {
            pickupPromptText.text = "Открыть дверь";
            pickupPromptText.gameObject.SetActive(true);
            if (Input.GetKeyDown(KeyCode.F))
            {
                doorHit.transform.parent.GetComponent<DoorController>().ToggleDoor();
            }
        }
        else
        {
            pickupPromptText.gameObject.SetActive(false);
        }
    }

    void UpdateThrowPower()
    {
        if (SkillModifier.Instance != null)
        {
            float modifier = SkillModifier.Instance.GetModifier("objectDragging"); 
            currentThrowPower = baseThrowPower + modifier;
        }
        else
        {
            currentThrowPower = baseThrowPower;
        }
    }

    void CheckForObject()
    {
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, draggableLayer))
        {
            if (highlightedObject != hit.collider.gameObject)
                HighlightObject(hit.collider.gameObject); 
        }
        else if (Physics.Raycast(ray, out RaycastHit doorHit, 3f, LayerMask.GetMask("Door"))){
            pickupPromptText.text = "Открыть дверь";
            pickupPromptText.gameObject.SetActive(true);
            if (Input.GetKeyDown(KeyCode.F))
            {
                doorHit.transform.parent.GetComponent<DoorController>().ToggleDoor();
            }
        }
        else if (Physics.Raycast(ray, out RaycastHit itemHit, 3f, LayerMask.GetMask("Item"))){
            var pickup = itemHit.collider.GetComponent<ItemPickup>();
            Debug.Log(pickup.itemData.itemName);
            if (pickup == null) return;
            lookedPickup = pickup;
            pickupPromptText.text = $"F – поднять «{pickup.itemData.itemName}»";
            pickupPromptText.gameObject.SetActive(true);
        }
        else
        {
            lookedPickup = null;
            pickupPromptText.gameObject.SetActive(false);
            UnhighlightObject(); 
        }
    }

    void HighlightObject(GameObject obj)
    {
        UnhighlightObject(); 
        highlightedObject = obj;

        if (TryGetRenderer(highlightedObject, out Renderer rend))
        {
            originalMaterial = rend.material;
            rend.material.color = Color.yellow;
        }
    }

    void UnhighlightObject()
    {
        if (highlightedObject && TryGetRenderer(highlightedObject, out Renderer rend) && originalMaterial)
        {
            rend.material = originalMaterial;
        }
        highlightedObject = null;
    }

    void PickUpObject()
    {
        heldObject = highlightedObject;
        heldRigidbody = heldObject.GetComponent<Rigidbody>();

        if (heldRigidbody)
        {
            heldRigidbody.useGravity = false;
            heldRigidbody.linearDamping = 10f;
        }

        UnhighlightObject();
    }

    void ReleaseObject(bool isThrow)
    {
        if (heldRigidbody)
        {
            heldRigidbody.useGravity = true;
            heldRigidbody.linearDamping = 0f;

            if (isThrow)
                heldRigidbody.AddForce(playerCamera.transform.forward * currentThrowPower, ForceMode.Impulse);
        }

        heldObject = null;
        heldRigidbody = null;
    }

    void DragObject()
    {
        if (!heldObject || !heldRigidbody) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit[] hits = Physics.RaycastAll(ray, holdRadius);

        foreach (var hit in hits)
        {

            if (heldObject && hit.collider.gameObject == heldObject) continue;

            Vector3 safePoint = hit.point + hit.normal * 0.5f;

            Vector3 movePos = Vector3.SmoothDamp(
                heldRigidbody.position,
                safePoint,
                ref moveVelocity,
                0.05f
            );

            heldRigidbody.MovePosition(movePos);
            return;
        }

        Vector3 defaultPoint = playerCamera.transform.position + playerCamera.transform.forward * holdRadius;
        Vector3 fallback = Vector3.SmoothDamp(heldRigidbody.position, defaultPoint, ref moveVelocity, 0.05f);
        heldRigidbody.MovePosition(fallback);
    }

    void AdjustHoldDistance()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
            holdRadius = Mathf.Clamp(holdRadius + scroll * 2f, 2f, 5f);
    }

    bool TryGetRenderer(GameObject obj, out Renderer renderer)
    {
        renderer = obj.GetComponent<Renderer>();
        return renderer != null;
    }
}
