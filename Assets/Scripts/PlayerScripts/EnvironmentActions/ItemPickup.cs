using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Tooltip("Данные предмета, из которых берём icon, name и логику")]
    public ItemData itemData;
    public int amount = 1;
}
