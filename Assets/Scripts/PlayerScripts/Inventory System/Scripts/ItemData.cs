using UnityEngine;
using System;

public enum ItemType
{
    Weapon,
    Armor,
    Heal,
    Mana
}
public enum EquipmentSlotType
{
    Weapon,
    Armor,
    None
}

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public ItemType itemType;
    public EquipmentSlotType slotType;

    [Header("Basic Info")]
    public string itemName;
    public string itemId;
    public Sprite icon;
    public bool isStackable = true;
    public int maxStackSize = 1;
    [Multiline]
    public string lore;

    [Header("World Model")]
    public GameObject worldModelPrefab;
    public GameObject equipPrefab;
    public WeaponStats weaponStats;
    public ArmorStats  armorStats;
}

[Serializable]
public class WeaponStats
{
    public int   damage;
    public float attackSpeed;
    public float range;
}

[Serializable]
public class ArmorStats
{
    public int   defense;
    public float weight;
    public float durability;
}
