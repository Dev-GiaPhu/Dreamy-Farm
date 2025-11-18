using UnityEngine;

[System.Serializable]
public class ListItem
{
    public ItemData itemData;
    public int count;
    public int slotIndex;

    public ListItem(ItemData itemData, int count, int slotIndex)
    {
        this.itemData = itemData;
        this.count = count;
        this.slotIndex = slotIndex;
    }

    public void AddCount(int count)
    {
        this.count += count;
    }

    public void RemoveCount(int count)
    {
        this.count -= count;
    }

    public void SetCount(int count)
    {
        this.count = count;
    }

    public void SetSlotIndex(int slotIndex)
    {
        this.slotIndex = slotIndex;
    }
}