using System.Collections.Generic;

// QUAN TRỌNG: Lớp này phải là [System.Serializable]
[System.Serializable]
public class InventoryListWrapper
{
    // Tên biến NÀY (ItemsToSave) phải trùng với tên List bạn muốn lưu (List<ListItem> Items)
    // Tên biến này sẽ được dùng làm key trong JSON
    public List<ListItem> ItemsToSave;

    public InventoryListWrapper(List<ListItem> items)
    {
        this.ItemsToSave = items;
    }
}