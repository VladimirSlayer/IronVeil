using UnityEngine;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    public GameObject slotContainer;
    public InventorySlotUI slotPrefab;

    private List<InventorySlotUI> slotUIs = new List<InventorySlotUI>();

    private void OnEnable()
    {
        InventoryManager.Instance.onInventoryChangedCallback += RefreshUI;
        RefreshUI();
    }

    private void OnDisable()
    {
        InventoryManager.Instance.onInventoryChangedCallback -= RefreshUI;
    }

    void RefreshUI()
    {
        foreach (Transform t in slotContainer.transform) Destroy(t.gameObject);
        slotUIs.Clear();

        foreach (var slot in InventoryManager.Instance.inventory)
        {
            var ui = Instantiate(slotPrefab, slotContainer.transform);
            ui.SetSlot(slot);    
            slotUIs.Add(ui);
        }
    }
}
