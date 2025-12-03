using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Định nghĩa cấu trúc Item rơi, cho phép gán trong Inspector
// Chứa trực tiếp Prefab của Item (ví dụ: WoodItemPrefab, StoneItemPrefab)
[System.Serializable]
public struct ResourceDrop 
{
    [Tooltip("Prefab của vật phẩm (đã gán sẵn ItemData trong ItemPickup.cs)")]
    public GameObject itemPrefab; // THAY THẾ ItemData bằng GameObject Prefab

    [Range(1, 50)]
    [Tooltip("Số lượng tối thiểu rơi ra.")]
    public int minAmount;
    
    [Range(1, 50)]
    [Tooltip("Số lượng tối đa rơi ra.")]
    public int maxAmount;
}

// ResourceNode là component chung cho mọi đối tượng tài nguyên có thể bị phá hủy
public class ResourceNode : MonoBehaviour 
{
    [Header("Node Stats")]
    [Tooltip("Tên loại node (ví dụ: Tree, Rock, Iron Ore)")]
    public string nodeType = "Resource Node";
    public int maxHealth = 3;
    public int currentHealth;
    public float respawnTime = 5f;

    [Header("Item Drop Settings")]
    [Tooltip("Danh sách các Prefab vật phẩm có thể rơi ra từ node này.")]
    public ResourceDrop[] dropList;   
    [Tooltip("Vật phẩm do node này sinh ra có rơi (fall animation) không?")]
    public bool shouldDropItems = true; 

    [Header("Drop Physics")]
    public float ellipseRadiusX = 1.5f;
    public float ellipseRadiusY = 1.0f;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Collider2D coll;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        coll = GetComponent<Collider2D>();
    }

    public void TakeHit()
    {
        if (isDead) return;
        Debug.Log($"{nodeType} bị trúng đòn!"); // Dùng nodeType thay cho "Cây"
        currentHealth -= 1;

        if (animator != null)
            animator.SetTrigger("Hit");

        if (currentHealth <= 0)
            StartCoroutine(HandleNodeDeath());
    }

    IEnumerator HandleNodeDeath()
    {
        isDead = true;
        DropItems();
        if (animator != null)
            animator.SetBool("Dead", true);

        if (coll != null)
            coll.enabled = false;

        yield return new WaitForSeconds(respawnTime);

        currentHealth = maxHealth;
        isDead = false;
        if (animator != null)
        {
            animator.SetBool("Dead", false);
        }

        if (coll != null)
            coll.enabled = true;
    }
    
    void DropItems()
    {
        // 1. Kiểm tra danh sách drop có hợp lệ không
        if (dropList == null || dropList.Length == 0)
        {
            Debug.LogWarning($"{nodeType} không có item drop.");
            return;
        }

        Vector2 nodePosition = transform.position;
        
        // 2. CHỌN NGẪU NHIÊN 1 LOẠI VẬT PHẨM TỪ DANH SÁCH
        int randomIndex = Random.Range(0, dropList.Length);
        ResourceDrop selectedDrop = dropList[randomIndex];

        // 3. Kiểm tra Prefab của vật phẩm đã chọn có hợp lệ không
        if (selectedDrop.itemPrefab == null)
        {
            Debug.LogError($"Item Drop tại index {randomIndex} của {nodeType} thiếu Prefab.");
            return;
        }

        // 4. Tính toán số lượng vật phẩm (từ 1 đến Max)
        int dropAmount = Random.Range(selectedDrop.minAmount, selectedDrop.maxAmount + 1); 

        if (dropAmount <= 0)
        {
            Debug.Log($"Node {nodeType} đã không drop vật phẩm nào (Drop Amount = 0).");
            return;
        }
        
        // 5. Sinh ra từng đơn vị vật phẩm
        for (int i = 0; i < dropAmount; i++)
        {
            // 5a. Tính toán vị trí mục tiêu (dropPosition) ngẫu nhiên theo elip
            Vector3 dropPosition = Vector3.zero;
            
            if (shouldDropItems)
            {
                float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float randomRadiusFactor = Random.Range(0.5f, 1f); 

                float offsetX = Mathf.Cos(randomAngle) * ellipseRadiusX * randomRadiusFactor;
                float offsetY = Mathf.Sin(randomAngle) * ellipseRadiusY * randomRadiusFactor;
                
                dropPosition = new Vector3(
                    nodePosition.x + offsetX, 
                    nodePosition.y + offsetY, 
                    transform.position.z 
                );
            }

            // 5b. Tạo vật phẩm, sử dụng Prefab CỤ THỂ đã được chọn ngẫu nhiên
            GameObject itemSpawn = Instantiate(selectedDrop.itemPrefab, transform.position, Quaternion.identity);
            
            // 5c. Gán thuộc tính vật lý và gọi UpdateUI().
            ItemPickup itemPickupScript = itemSpawn.GetComponent<ItemPickup>();
            if (itemPickupScript != null)
            {
                // Quy định dạng item (Rơi/Tĩnh)
                itemPickupScript.isDroppedItem = shouldDropItems; 

                // Gán VỊ TRÍ MỤC TIÊU
                itemPickupScript.targetDropPosition = dropPosition; 
                
                // Cập nhật UI (với dữ liệu đã gán sẵn trong Prefab)
                itemPickupScript.UpdateUI();
            }
            else
            {
                Debug.LogError("Prefab trong dropList thiếu ItemPickup script!");
                Destroy(itemSpawn);
            }
        }
        Debug.Log($"Node {nodeType} đã drop {dropAmount} đơn vị vật phẩm {selectedDrop.itemPrefab.name}.");
    }
    
    // Hàm vẽ Gizmos để hiển thị đường viền elip trong Editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        
        Vector3 center = transform.position;
        int segments = 60;
        Vector3 previousPoint = Vector3.zero;

        for (int i = 0; i <= segments; i++)
        {
            float angle = (float)i / segments * 360f;
            float radians = angle * Mathf.Deg2Rad;

            float x = center.x + Mathf.Cos(radians) * ellipseRadiusX;
            float y = center.y + Mathf.Sin(radians) * ellipseRadiusY;
            Vector3 currentPoint = new Vector3(x, y, center.z);

            if (i > 0)
            {
                Gizmos.DrawLine(previousPoint, currentPoint);
            }

            previousPoint = currentPoint;
        }
    }
}