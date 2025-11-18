using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }

    public int slotHotbar = 6;
    public int slotInv = 24;

    public List<ListItem> Items = new List<ListItem>();

    // UI
    public InventorySlotUI[] hotbarSlots;
    public InventorySlotUI[] inventorySlots;

    private void Awake()
    {
        Instance = this;
    }

    // Thêm item vào inventory
    public int AddItem(ItemData data, string nameitem, int amount)
    {
        int remaining = amount;
        string iname = nameitem;

        // --- Ưu tiên stack vào slot tồn tại ---
        foreach (var item in Items)
        {
            if (item.itemData == data)
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

        // --- Nếu còn dư → tạo slot mới ---
        while (remaining > 0)
        {
            int add = Mathf.Min(data.maxStack, remaining);

            // Nếu inventory full thì return số dư
            if (Items.Count >= slotInv + slotHotbar)
            {
                UpdateUI();
                return remaining;
            }   

            Items.Add(new ListItem(data, add, Items.Count));

            remaining -= add;
        }
        UpdateUI();
        return 0; // ADD HẾT
    }

        public void UpdateUI()
    {
        // --- Clear toàn bộ UI trước ---
        foreach (var slot in hotbarSlots)
            slot.ClearSlot();

        foreach (var slot in inventorySlots)
            slot.ClearSlot();


        // --- Render item list ---
        for (int i = 0; i < Items.Count; i++)
        {
            var item = Items[i];

            // Slot hotbar trước
            if (i < slotHotbar)
            {
                hotbarSlots[i].SetItem(item.itemData, Items[i].itname, item.count);
            }
            else // Sau đó inventory
            {
                int invIndex = i - slotHotbar;
                if (invIndex < inventorySlots.Length)
                {
                    inventorySlots[invIndex].SetItem(item.itemData, Items[i].itname,  item.count);
                }
            }
        }
    }

}