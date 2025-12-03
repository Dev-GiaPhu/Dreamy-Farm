using UnityEngine;

[System.Serializable]
public class ListItem
{
    public ItemData itemData;
    public string itname;
    public int count;
    public int slotIndex;

    public ListItem()
    {
    }

    public ListItem(ItemData itemData, int count, int slotIndex)
    {
        this.itemData = itemData;
        this.count = count;
        this.slotIndex = slotIndex;
        // KHẮC PHỤC LỖI NRE: Đảm bảo itemData không phải là null trước khi truy cập itemName
        this.itname = itemData.itemName;
    }

    // Thuộc tính tiện ích để kiểm tra slot trống
    public bool IsEmpty => itemData == null || count <= 0;

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