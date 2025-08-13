using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
    public List<InventorySlot> inventory = new List<InventorySlot>();
    public int maxSlots = 20;
    public float dropDistance = 2f;

    private string equippedWeaponSlotID;
    private string equippedArmorSlotID;

    public delegate void OnInventoryChanged();
    public event OnInventoryChanged onInventoryChangedCallback;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddItem(ItemData item)
    {
        int limit = item.isStackable
            ? item.maxStackSize
            : 1;

        if (item.isStackable)
        {
            var existingSlot = inventory
                .Find(s => s.item == item && s.count < limit);
            if (existingSlot != null)
            {
                existingSlot.count++;
                onInventoryChangedCallback?.Invoke();
                return;
            }
        }

        if (inventory.Count < maxSlots)
        {
            inventory.Add(new InventorySlot(item));
            onInventoryChangedCallback?.Invoke();
        }
        else
        {
            Debug.Log("Инвентарь переполнен!");
        }
    }

    public void RemoveItemBySlotID(string slotID)
    {
        var slot = inventory.Find(s => s.id == slotID);
        if (slot == null) return;

        slot.count--;
        if (slot.count <= 0)
            inventory.Remove(slot);

        onInventoryChangedCallback?.Invoke();
    }

    public void UseItem(string slotID)
    {
        var slot = inventory.Find(s => s.id == slotID);
        if (slot == null) return;

        switch (slot.item.itemType)
        {
            case ItemType.Heal:
                PlayerStats.Instance.Heal(20);
                RemoveItemBySlotID(slotID);
                break;

            case ItemType.Armor or ItemType.Weapon:
                EquipmentManager.Instance.Equip(slot.item);
                break;

            case ItemType.Mana:
                PlayerStats.Instance.AddMana(20);
                RemoveItemBySlotID(slotID);
                break;
        }

        switch (slot.item.slotType)
        {
            case EquipmentSlotType.Weapon:
                equippedWeaponSlotID = slotID;
                break;
            case EquipmentSlotType.Armor:
                equippedArmorSlotID = slotID;
                break;
        }

        onInventoryChangedCallback?.Invoke();
    }

    public bool IsSlotEquipped(string slotID)
    {
        return slotID == equippedWeaponSlotID
            || slotID == equippedArmorSlotID;
    }

    public void DropItem(string slotID)
    {
        var slot = inventory.Find(s => s.id == slotID);
        if (slot == null) return;

        if (IsSlotEquipped(slotID))
        {
            EquipmentManager.Instance.Unequip(slot.item);

            if (slot.item.slotType == EquipmentSlotType.Weapon)
                equippedWeaponSlotID = null;
            else
                equippedArmorSlotID = null;
        }
        RemoveItemBySlotID(slotID);

        Camera cam = Camera.main;
        if (slot.item.worldModelPrefab != null && cam != null)
        {
            Vector3 pos = cam.transform.position + cam.transform.forward * dropDistance;
            Instantiate(slot.item.worldModelPrefab, pos, Quaternion.identity);
        }

        onInventoryChangedCallback?.Invoke();
    }
}
