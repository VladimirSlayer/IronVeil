using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ItemData))]
public class ItemDataEditor : Editor
{
    SerializedProperty
        pItemType,
        pEquipmentSlotType,
        pItemName,
        pItemId,
        pIcon,
        pLore,
        pIsStackable,
        pMaxStackSize,
        pWorldModel,
        pEquipModel,
        pWeaponStats,
        pArmorStats;

    void OnEnable()
    {
        pItemType = serializedObject.FindProperty("itemType");
        pEquipmentSlotType = serializedObject.FindProperty("slotType");
        pItemName = serializedObject.FindProperty("itemName");
        pItemId = serializedObject.FindProperty("itemId");
        pIcon = serializedObject.FindProperty("icon");
        pLore = serializedObject.FindProperty("lore");
        pIsStackable = serializedObject.FindProperty("isStackable");
        pMaxStackSize = serializedObject.FindProperty("maxStackSize");
        pWorldModel = serializedObject.FindProperty("worldModelPrefab");
        pEquipModel = serializedObject.FindProperty("equipPrefab");
        pWeaponStats = serializedObject.FindProperty("weaponStats");
        pArmorStats = serializedObject.FindProperty("armorStats");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(pItemType);
        EditorGUILayout.PropertyField(pEquipmentSlotType);
        EditorGUILayout.PropertyField(pItemName);
        EditorGUILayout.PropertyField(pItemId);
        EditorGUILayout.PropertyField(pIcon);
        EditorGUILayout.PropertyField(pLore);
        EditorGUILayout.PropertyField(pIsStackable);
        EditorGUILayout.PropertyField(pMaxStackSize);
        EditorGUILayout.PropertyField(pWorldModel);
        EditorGUILayout.PropertyField(pEquipModel);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Stats", EditorStyles.boldLabel);

        var type = (ItemType)pItemType.enumValueIndex;
        switch (type)
        {
            case ItemType.Weapon:
                EditorGUILayout.PropertyField(pWeaponStats, new GUIContent("Weapon Stats"), true);
                break;
            case ItemType.Armor:
                EditorGUILayout.PropertyField(pArmorStats, new GUIContent("Armor Stats"), true);
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
