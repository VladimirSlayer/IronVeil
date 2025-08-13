using Unity.VisualScripting;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    private Animator doorAnimator;
    public bool isLocked = false;
    public bool isLockedOut = false; 
    private bool isOpen = false;

    private void Start()
    {
        doorAnimator = GetComponent<Animator>();
        if (doorAnimator == null)
        {
            Debug.LogError("Animator not assigned!");
        }
    }

    public void OpenDoor()
    {
        if (!isLocked || isLockedOut)
        {
            isOpen = true;
            doorAnimator.SetBool("IsOpen", true);
        }
        else
        {
            Debug.Log("Дверь заперта! Попробуйте взломать её.");
        }
    }

    public void CloseDoor()
    {
        isOpen = false;
        doorAnimator.SetBool("IsOpen", false);
    }

    public void TryToUnlockDoor()
    {
        if (isLocked && !isLockedOut)
        {
            var chance = SkillModifier.Instance.GetModifier("lockpicking") + Random.Range(0f, 60f);
            Debug.Log($"Your chance is: {chance}");
            if(chance>50f)
            {
                isLockedOut = true;
                isLocked = false;
            }
        }
        else if (!isLocked)
        {
            Debug.Log("Дверь не заперта.");
        }
        else
        {
            Debug.Log("Вы уже взломали эту дверь.");
        }
    }

    public void LockDoor()
    {
        isLocked = true;
        isLockedOut = false;
        Debug.Log("Дверь заперта.");
    }

    public void ToggleDoor()
    {
        if (isOpen)
        {
            CloseDoor();
        }
        else
        {
            TryToUnlockDoor();
            OpenDoor();
        }
    }
}
