using UnityEngine;

public class DeathZone : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "DeathZone")
        {
            GetComponent<PlayerStats>().Die();
            Debug.Log("DEATH");
        }
    }
}
