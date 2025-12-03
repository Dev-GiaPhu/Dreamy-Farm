using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

// Giả định class PlayerController có sẵn trong dự án của bạn
// và có hàm public bool IsAnyActionInProgress()
// class PlayerController : MonoBehaviour { public bool IsAnyActionInProgress() { ... } }

public enum ItemType
{
    Null,
    Sword,
    Axe,
    Pickaxe,
    Shovel,
    CanWater,
    FishingRod,
}

// Implement các interface kéo thả
public class InventorySlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    // Cần GÁN component Image của Icon CON vào biến này trong Inspector
    public ItemType type;
    public Image icon; 
    public TMP_Text amountText; 
    
    // Index CỐ ĐỊNH của Slot này trong List Inventory.Items
    public int inventoryIndex; 
    
    // Đảm bảo là static để chỉ có MỘT icon ảo được kéo
    private static GameObject dragIconTemp = null; 
    private static InventorySlotUI sourceSlotUI = null;

    // Biến cache PlayerController để tránh tìm kiếm nhiều lần
    private PlayerController _playerControllerCache;
    
    private void Awake()
    {
        // 1. Nếu biến 'icon' chưa được gán, tự tìm trong con
        if (icon == null)
            icon = GetComponentInChildren<Image>();
            
        // KHẮC PHỤC LỖI RAYCAST:
        // Icon item PHẢI TẮT Raycast Target để OnDrop ở slot cha hoạt động
        if (icon != null)
        {
            icon.raycastTarget = false; // TẮT Raycast Target trên Image Icon item
            icon.enabled = false; 
            icon.color = new Color(1f, 1f, 1f, 1f); 
        }

        // 2. Lấy Text số lượng
        if (amountText == null)
            amountText = GetComponentInChildren<TMP_Text>();
            
        // 3. Cảnh báo nếu Slot cha không có Image/Raycast Target
        Image slotImage = GetComponent<Image>();
        if (slotImage != null && !slotImage.raycastTarget)
        {
            Debug.LogWarning("Slot '" + name + "' Image component must have Raycast Target = TRUE in Editor to receive OnDrop events fully.");
        }
    }
    
    public void SetItem(ItemData data, string nameitem, int count)
    {
        if (icon == null || amountText == null)
        {
            Debug.LogError("MISSING UI REFERENCES IN: " + name);
            return;
        }

        string typeit = nameitem.ToString();
        switch (typeit)
        {
            case "Sword":
                type = ItemType.Sword; break;
            case "Axe":
                type = ItemType.Axe; break;
            case "Pickaxe":
                type = ItemType.Pickaxe; break;
            case "Shovel":
                type = ItemType.Shovel; break;
            case "CanWater":
                type = ItemType.CanWater; break;
            case "FishingRod":
                type = ItemType.FishingRod; break;
            default:
                type = ItemType.Null; break;
        }
        icon.sprite = data.icon;
        icon.enabled = true;
        icon.color = new Color(1f, 1f, 1f, 1f);

        amountText.text = (count > 1) ? "x" + count : "";
    }

    public void ClearSlot()
    {
        if (icon == null || amountText == null)
        {
            Debug.LogError("MISSING UI REFERENCES IN: " + name);
            return;
        }

        icon.sprite = null;
        icon.enabled = false;
        amountText.text = "";
    }
    
    // --- Kéo thả ---
    public void OnBeginDrag(PointerEventData eventData)
    {
        // >> THAY ĐỔI: Sử dụng FindFirstObjectByType thay cho FindObjectOfType (loại bỏ cảnh báo lỗi thời) <<
        
        // 1. Tìm và cache PlayerController (chỉ tìm lần đầu)
        if (_playerControllerCache == null)
        {
            // Cập nhật: FindObjectOfType đã bị cảnh báo, thay bằng FindFirstObjectByType
            #if UNITY_2023_1_OR_NEWER
                _playerControllerCache = FindFirstObjectByType<PlayerController>();
            #else
                _playerControllerCache = FindObjectOfType<PlayerController>();
            #endif
        }

        // 2. Kiểm tra trạng thái hành động
        if (_playerControllerCache != null && _playerControllerCache.IsAnyActionInProgress())
        {
            Debug.Log("Không thể kéo thả khi đang thực hiện hành động.");
            eventData.pointerDrag = null;
            return;
        }
        
        // 3. Kiểm tra Slot có trống không
        ListItem itemData = Inventory.Instance.Items[inventoryIndex];
        if (itemData.IsEmpty) 
        {
            eventData.pointerDrag = null; 
            return; 
        }

        // 4. KHỞI TẠO HOẶC CHỈ CẬP NHẬT Drag Icon
        Image dragImage;
        if (dragIconTemp == null)
        {
            // TẠO MỚI (Chỉ một lần)
            dragIconTemp = new GameObject("DragIconTemp");
            dragImage = dragIconTemp.AddComponent<Image>();
            dragIconTemp.AddComponent<RectTransform>(); 
            
            Canvas parentCanvas = GetComponentInParent<Canvas>();
            dragIconTemp.transform.SetParent(parentCanvas.transform);
            dragImage.raycastTarget = false; 
        }
        else
        {
            // ĐÃ TỒN TẠI, CHỈ CẦN LẤY COMPONENT (Khắc phục lỗi Can't Add Component)
            dragImage = dragIconTemp.GetComponent<Image>();
        }
        
        // 5. Cập nhật nội dung và kích hoạt 
        dragImage.sprite = itemData.itemData.icon;
        dragImage.color = new Color(1, 1, 1, 1);
        dragImage.rectTransform.sizeDelta = GetComponent<RectTransform>().sizeDelta;

        dragIconTemp.SetActive(true);
        
        // 6. Thiết lập Slot nguồn
        icon.color = new Color(1f, 1f, 1f, 0.4f); // Làm mờ Icon gốc
        sourceSlotUI = this; 
        
        dragImage.raycastTarget = false; 
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragIconTemp != null && dragIconTemp.activeSelf)
        {
            // Thiết lập vị trí theo chuột
            dragIconTemp.transform.position = eventData.position; 
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 1. Vô hiệu hóa Icon ảo
        if (dragIconTemp != null)
        {
            dragIconTemp.SetActive(false);
        }
        
        // 2. Reset Icon gốc (Chỉ nếu có slot nguồn)
        if (sourceSlotUI != null && sourceSlotUI.icon != null && sourceSlotUI.icon.enabled) 
        {
            sourceSlotUI.icon.color = new Color(1f, 1f, 1f, 1f); 
        }

        // 3. Reset slot nguồn
        sourceSlotUI = null;
    }
    
    public void OnDrop(PointerEventData eventData)
    {
        if (sourceSlotUI == null) return; 
        if (sourceSlotUI == this) return;

        int sourceIndex = sourceSlotUI.inventoryIndex;
        int targetIndex = this.inventoryIndex;

        // Gọi hàm Swap Dữ liệu
        Inventory.Instance.SwapSlots(sourceIndex, targetIndex);
    }
}