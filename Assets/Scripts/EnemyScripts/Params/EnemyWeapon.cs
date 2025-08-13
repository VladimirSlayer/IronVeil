using UnityEngine;

public class EnemyWeapon : MonoBehaviour
{
     public int damage = 10;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) 
        {
            PlayerStats playerHealth = other.GetComponent<PlayerStats>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log("Игрок получил урон от оружия врага!");
            }
        }
    }
}
