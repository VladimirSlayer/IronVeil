using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlotUI : MonoBehaviour
{
    public Image iconImage;
    public TMP_Text nameText;
    public TMP_Text countText;
    public Button dropButton;
    public Button useButton;
    public TMP_Text descriptionText;

    private InventorySlot currentSlot;

    public void SetSlot(InventorySlot slot)
    {
        currentSlot = slot;
        iconImage.sprite = slot.item.icon;
        iconImage.enabled = true;
        nameText.text = slot.item.itemName;
        countText.text = (slot.item.isStackable && slot.count > 1)
                            ? slot.count.ToString()
                            : "";
        switch (slot.item.itemType)
        {
            case ItemType.Weapon:
                descriptionText.text = $"Характеристики:\nУрон: {slot.item.weaponStats.damage}\nCкорость аттаки: {slot.item.weaponStats.attackSpeed}\nРадиус: {slot.item.weaponStats.range}\nОписание:\n{slot.item.lore}";
                break;
            case ItemType.Armor:
                descriptionText.text = $"Характеристики:\nЗащита: {slot.item.armorStats.defense}\nВес: {slot.item.armorStats.weight}\nПрочность: {slot.item.armorStats.durability}\nОписание:\n{slot.item.lore}";
                break;
            default:
                descriptionText.text = $"Описание:\n{slot.item.lore}";
                break;
        }
        dropButton.onClick.RemoveAllListeners();
        dropButton.onClick.AddListener(() =>
            InventoryManager.Instance.DropItem(currentSlot.id)
        );
        dropButton.gameObject.SetActive(true);

        bool equipped = InventoryManager.Instance.IsSlotEquipped(currentSlot.id);
        useButton.onClick.RemoveAllListeners();
        useButton.onClick.AddListener(() =>
            InventoryManager.Instance.UseItem(currentSlot.id)
        );
        useButton.interactable = !equipped;
        useButton.gameObject.SetActive(true);
    }

    public void ClearSlot()
    {
        currentSlot = null;
        iconImage.sprite = null;
        iconImage.enabled = false;
        nameText.text = "";
        countText.text = "";

        dropButton.onClick.RemoveAllListeners();
        dropButton.gameObject.SetActive(false);

        useButton.onClick.RemoveAllListeners();
        useButton.gameObject.SetActive(false);
    }
}
