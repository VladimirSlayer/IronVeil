using UnityEngine;

public class NPCInteraction : MonoBehaviour
{
    [Tooltip("Максимальная дистанция взаимодействия")]
    public float interactDistance = 2f;
    [Tooltip("Слой, на котором лежат NPC")]
    public LayerMask npcLayer;
    public GameObject textGO;

    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
        if (mainCam == null)
            Debug.LogWarning("NPCInteraction: нет камеры с тегом MainCamera!");
    }

    void Update()
    {
        CheckForNPC();
        if (Input.GetKeyDown(KeyCode.F))
        {
            TryStartDialogue();
        }
    }

    private void TryStartDialogue()
    {
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        Ray ray = mainCam.ScreenPointToRay(screenCenter);

        Debug.DrawRay(ray.origin, ray.direction * interactDistance, Color.green, 1f);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, npcLayer))
        {
            var npc = hit.collider.GetComponent<NPCDialogue>();
            if (npc != null && npc.dialogueJson != null)
            {
                DialogueManager.Instance.StartDialogue(npc.dialogueJson);
            }
        }
    }

    private void CheckForNPC()
    {
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        Ray ray = mainCam.ScreenPointToRay(screenCenter);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, npcLayer))
        {
            textGO.SetActive(true);
        }
        else
        {
            textGO.SetActive(false);
        }
    }
}
