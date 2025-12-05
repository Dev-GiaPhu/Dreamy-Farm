using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }

    public int slotHotbar = 6;
    public int slotInv = 24;

    // List chứa tất cả các slot, bao gồm cả slot trống
    public List<ListItem> Items = new List<ListItem>(); 

    // UI
    public InventorySlotUI[] hotbarSlots;
    public InventorySlotUI[] inventorySlots;

    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    private void Start()
    {
        // KHỞI TẠO TẤT CẢ CÁC SLOT TRONG INVENTORY
        int totalSlots = slotHotbar + slotInv;
        for (int i = 0; i < totalSlots; i++)
        {
            // new ListItem() được sử dụng an toàn nhờ constructor không tham số
            ListItem newSlot = new ListItem(); 
            newSlot.SetSlotIndex(i);
            Items.Add(newSlot);
        }

        foreach (var item in Items)
        {
            // The fixed index of the item in the Items List
            int dataIndex = item.slotIndex; 
            
            // Determine the ItemType
            ItemType determinedType = item.itemData != null ? item.itemData.itemType : ItemType.Null;

            // --- A. Handle Hotbar (Index 0 up to slotHotbar - 1) ---
            if (dataIndex < slotHotbar)
            {
                // Check the bounds of the UI array to prevent IndexOutOfRangeException
                if (dataIndex < hotbarSlots.Length)
                {
                    // The UI index is the same as the data index for the hotbar
                    hotbarSlots[dataIndex].type = determinedType;
                }
            }
            // --- B. Handle Main Inventory (Index slotHotbar onwards) ---
            else
            {
                // Calculate the relative index for the inventorySlots UI array (e.g., Data Index 6 -> UI Index 0)
                int invIndex = dataIndex - slotHotbar;
                
                // Check the bounds of the UI array
                if (invIndex >= 0 && invIndex < inventorySlots.Length)
                {
                    inventorySlots[invIndex].type = determinedType;
                }
            }
        }
        UpdateUI();
    }

    // Thêm item vào inventory
    public int AddItem(ItemData data, string nameitem, int amount)
    {
        int remaining = amount;
        
        // --- 1. Ưu tiên stack vào slot tồn tại ---
        for (int i = 0; i < Items.Count; i++)
        {
            var item = Items[i];
            if (item.itemData == data && item.count < data.maxStack)
            {
                int space = data.maxStack - item.count;

                int add = Mathf.Min(space, remaining);
                item.count += add;
                remaining -= add;

                if (remaining <= 0)
                {
                    UpdateUI();
                    return 0; // ADD HẾT
                }
            }
        }

        // --- 2. Nếu còn dư → điền vào slot trống đầu tiên ---
        if (remaining > 0)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                var item = Items[i];
                if (item.IsEmpty)
                {
                    int add = Mathf.Min(data.maxStack, remaining);
                    
                    // Gán dữ liệu cho slot trống
                    item.itemData = data;
                    item.count = add;
                    item.itname = data.itemName; // Cập nhật tên item
                    
                    remaining -= add;
                    
                    if (remaining <= 0)
                    {
                        UpdateUI();
                        return 0;
                    }
                }
            }
        }

        // Nếu List đã full và không thể stack (dù đã khởi tạo hết slot)
        if (remaining > 0)
        {
            UpdateUI();
            return remaining;
        }
        
        UpdateUI();
        return 0; 
    }


    // HÀM HOÁN ĐỔI ĐÃ SỬA LỖI NRE (chỉ hoán đổi thuộc tính)
    public void SwapSlots(int index1, int index2)
    {
        // Kiểm tra index hợp lệ
        if (index1 < 0 || index1 >= Items.Count || index2 < 0 || index2 >= Items.Count)
        {
            Debug.LogError($"Swap index out of bounds. Index1: {index1}, Index2: {index2}, Total Items: {Items.Count}");
            return;
        }

        // Lấy dữ liệu hai slot (đối tượng ListItem)
        ListItem item1 = Items[index1];
        ListItem item2 = Items[index2];

        // --- Bắt đầu Hoán đổi Dữ liệu ---

        // 1. Lưu tạm thời dữ liệu item1
        ItemData tempItemData = item1.itemData;
        int tempCount = item1.count;
        
        // 2. Gán dữ liệu item2 cho vị trí 1
        item1.itemData = item2.itemData;
        item1.count = item2.count;
        item1.itname = (item2.itemData != null) ? item2.itemData.itemName : "";


        // 3. Gán dữ liệu item1 (temp) cho vị trí 2
        item2.itemData = tempItemData;
        item2.count = tempCount;
        item2.itname = (tempItemData != null) ? tempItemData.itemName : "";
        
        // --- Kết thúc Hoán đổi Dữ liệu ---
        
        UpdateUI();
    }


    public void UpdateUI()
    {
        // 1. Clear toàn bộ UI trước
        foreach (var slot in hotbarSlots)
            slot.ClearSlot();

        foreach (var slot in inventorySlots)
            slot.ClearSlot();

        // 2. Render item list (Dùng foreach và item.slotIndex để gán chính xác)
        foreach (var item in Items)
        {
            // Chỉ render nếu item KHÔNG trống
            if (item.IsEmpty) continue;

            // Chỉ mục cố định mà Item này phải hiển thị
            int targetIndex = item.slotIndex;

            // --- A. Hotbar slots ---
            if (targetIndex < slotHotbar)
            {
                // Truy cập mảng UI bằng targetIndex (item.slotIndex)
                if (targetIndex >= 0 && targetIndex < hotbarSlots.Length)
                {
                    hotbarSlots[targetIndex].SetItem(item.itemData, item.itname, item.count);
                }
            }
            // --- B. Inventory slots ---
            else
            {
                // Tính chỉ mục tương đối
                int invIndex = targetIndex - slotHotbar;
                if (invIndex >= 0 && invIndex < inventorySlots.Length)
                {
                    inventorySlots[invIndex].SetItem(item.itemData, item.itname, item.count);
                }
            }
        }
    }
}