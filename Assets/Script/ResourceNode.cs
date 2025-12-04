using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Định nghĩa cấu trúc Item rơi. Chỉ còn Trọng số (Weight)
[System.Serializable]
public struct ResourceDrop 
{
    [Tooltip("Prefab của vật phẩm (đã gán sẵn ItemData trong ItemPickup.cs)")]
    public GameObject itemPrefab; 
    
    [Header("Drop Probability")]
    [Range(0f, 100f)]
    [Tooltip("Trọng số xác suất rơi. Số càng cao, tỉ lệ rơi càng lớn so với các item khác.")]
    public float weight; 
    
    // Đã loại bỏ minAmount/maxAmount vì Node sẽ xác định TỔNG số lượng drops
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

    [Header("Drop Count Settings")]
    [Range(1, 10)]
    [Tooltip("Số lượng vật phẩm TỐI THIỂU (đơn vị) sẽ rơi ra.")]
    public int minDrops = 1;
    [Range(1, 10)]
    [Tooltip("Số lượng vật phẩm TỐI ĐA (đơn vị) sẽ rơi ra.")]
    public int maxDrops = 3;

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
        Debug.Log($"{nodeType} bị trúng đòn!"); 
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
    
    // Thuật toán chọn ngẫu nhiên dựa trên trọng số (Weighted Random Selection)
    // Trả về ResourceDrop được chọn
    private ResourceDrop GetWeightedRandomDrop()
    {
        // 1. Tính tổng trọng số
        float totalWeight = 0f;
        foreach (var drop in dropList)
        {
            totalWeight += drop.weight;
        }

        // Nếu tổng trọng số = 0, trả về drop đầu tiên (trường hợp lỗi)
        if (totalWeight <= 0f)
        {
            Debug.LogWarning("Total drop weight is 0. Returning first item.");
            return dropList[0];
        }

        // 2. Chọn một số ngẫu nhiên từ 0 đến tổng trọng số (độc quyền)
        float randomNumber = Random.Range(0f, totalWeight);
        
        // 3. Lặp qua danh sách để tìm item tương ứng
        float currentWeight = 0f;
        foreach (var drop in dropList)
        {
            currentWeight += drop.weight;
            
            // Nếu số ngẫu nhiên rơi vào khoảng trọng số của item này, chọn nó
            if (randomNumber < currentWeight)
            {
                return drop;
            }
        }

        // Fallback (chỉ xảy ra nếu có lỗi tính toán, trả về item cuối cùng)
        return dropList[dropList.Length - 1];
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
        
        // 2. TÍNH TỔNG SỐ LƯỢNG ĐƠN VỊ VẬT PHẨM SẼ RƠI (Min-Max của Node)
        int totalDrops = Random.Range(minDrops, maxDrops + 1); 

        if (totalDrops <= 0)
        {
            Debug.Log($"Node {nodeType} đã không drop vật phẩm nào (Total Drops = 0).");
            return;
        }

        Debug.Log($"Node {nodeType} sẽ drop tổng cộng {totalDrops} đơn vị vật phẩm.");
        
        // 3. Vòng lặp: DROP MỖI ĐƠN VỊ VẬT PHẨM RIÊNG LẺ
        for (int i = 0; i < totalDrops; i++)
        {
            // 3a. CHỌN LOẠI VẬT PHẨM DỰA TRÊN TRỌNG SỐ
            ResourceDrop selectedDrop = GetWeightedRandomDrop();

            if (selectedDrop.itemPrefab == null)
            {
                Debug.LogError($"Item Drop được chọn thiếu Prefab tại vòng lặp {i}.");
                continue;
            }
            
            // 3b. Tính toán vị trí mục tiêu (dropPosition) ngẫu nhiên theo elip
            Vector3 dropPosition = transform.position; // Default to node position
            
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

            // 3c. Tạo vật phẩm (luôn là 1 đơn vị)
            // LƯU Ý: ItemPickup.amount mặc định là 1. Chúng ta không cần thay đổi.
            GameObject itemSpawn = Instantiate(selectedDrop.itemPrefab, transform.position, Quaternion.identity);
            
            // 3d. Gán thuộc tính vật lý
            ItemPickup itemPickupScript = itemSpawn.GetComponent<ItemPickup>();
            if (itemPickupScript != null)
            {
                itemPickupScript.isDroppedItem = shouldDropItems; 
                itemPickupScript.targetDropPosition = dropPosition; 
                
                // Đảm bảo số lượng là 1 cho mỗi lần sinh (mặc dù Prefab đã gán, nhưng kiểm tra lại)
                itemPickupScript.amount = 1; 
                
                itemPickupScript.UpdateUI();
            }
            else
            {
                Debug.LogError($"Prefab '{selectedDrop.itemPrefab.name}' thiếu ItemPickup script!");
                Destroy(itemSpawn);
            }
            
            Debug.Log($"Đơn vị drop thứ {i+1}: {selectedDrop.itemPrefab.name} (Weight: {selectedDrop.weight})");
        }
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