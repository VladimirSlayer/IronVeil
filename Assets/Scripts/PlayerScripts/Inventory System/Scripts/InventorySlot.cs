using System;

[System.Serializable]
public class InventorySlot
{
    public ItemData item;
    public int count;
    public string id;

    public InventorySlot(ItemData item, int count = 1)
    {
        this.item = item;
        this.count = count;
        this.id = Guid.NewGuid().ToString();
    }
}
