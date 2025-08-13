using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class CharacterEquipAndStatsUI : MonoBehaviour
{
  public Image armorSlot;
  public Image weaponSlot;

  public void RefreshUI()
  {
    if (EquipmentManager.Instance.equippedArmorItem != null)
    {
      Color color = new Color(1, 1, 1);
      color.a = 1;
      armorSlot.color = color;
      armorSlot.sprite = EquipmentManager.Instance.equippedArmorItem.icon;
    }
    else
    {
      armorSlot.sprite = null;
      Color color = new Color(1, 1, 1);
      color.a = 0;
      armorSlot.color = color;
    }
    if (EquipmentManager.Instance.equippedWeaponItem != null)
    {
      Color color = new Color(1, 1, 1);
      color.a = 1;
      weaponSlot.color = color;
      weaponSlot.sprite = EquipmentManager.Instance.equippedWeaponItem.icon;
    }
    else
    {
      weaponSlot.sprite = null;
      Color color = new Color(1, 1, 1);
      color.a = 0;
      weaponSlot.color = color;
    }
  }
}