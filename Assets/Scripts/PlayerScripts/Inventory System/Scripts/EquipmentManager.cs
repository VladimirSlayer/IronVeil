using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
  public static EquipmentManager Instance { get; private set; }

  public delegate void OnEquipmentChanged(ItemData newWeapon, ItemData newArmor);
  public event OnEquipmentChanged onEquipmentChanged;

  [Header("Equip Points (child transforms)")]
  public Transform weaponSlot; 
  public Transform armorSlot; 

  private GameObject equippedWeaponGO;
  private GameObject equippedArmorGO;
  public ItemData equippedWeaponItem { get; private set; }
  public ItemData equippedArmorItem { get; private set; }

  private void Awake()
  {
    if (Instance == null) Instance = this;
    else Destroy(gameObject);
  }

  public void Equip(ItemData item)
  {
    if (item.worldModelPrefab == null)
    {
      return;
    }

    switch (item.slotType)
    {
      case EquipmentSlotType.Weapon:
        if (equippedWeaponGO != null)
          Destroy(equippedWeaponGO);
        equippedWeaponGO = Instantiate(
            item.equipPrefab,
            weaponSlot.position,
            weaponSlot.rotation,
            weaponSlot
        );
        equippedWeaponItem = item;
        break;

      case EquipmentSlotType.Armor:
        if (equippedArmorGO != null)
          Destroy(equippedArmorGO);
        equippedArmorGO = Instantiate(
            item.equipPrefab,
            armorSlot.position,
            armorSlot.rotation,
            armorSlot
        );
        equippedArmorItem = item;
        break;
    }
    onEquipmentChanged?.Invoke(equippedWeaponItem, equippedArmorItem);
  }

  public void Unequip(ItemData item)
  {
    switch (item.slotType)
    {
      case EquipmentSlotType.Weapon:
        if (equippedWeaponItem == item && equippedWeaponGO != null)
        {
          Destroy(equippedWeaponGO);
          equippedWeaponGO = null;
          equippedWeaponItem = null;
        }
        break;

      case EquipmentSlotType.Armor:
        if (equippedArmorItem == item && equippedArmorGO != null)
        {
          Destroy(equippedArmorGO);
          equippedArmorGO = null;
          equippedArmorItem = null;
        }
        break;
    }
    onEquipmentChanged?.Invoke(equippedWeaponItem, equippedArmorItem);
    Debug.Log($"Unequipped {item.itemName}");
  }

  public bool IsEquipped(ItemData item)
  {
    return (item.slotType == EquipmentSlotType.Weapon && equippedWeaponItem == item)
        || (item.slotType == EquipmentSlotType.Armor && equippedArmorItem == item);
  }
}
