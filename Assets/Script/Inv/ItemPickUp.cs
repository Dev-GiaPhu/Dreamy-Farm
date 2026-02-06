using UnityEngine;
using TMPro;
using System.Collections;

public class ItemPickup : MonoBehaviour
{
    // Tham chiếu Manager. Tự động được gán trong Awake()
    private Gold_Iron_vv_Manager icm;
    
    [Header("Item Behaviour")]
    [Tooltip("Nếu TRUE, item sẽ rơi từ vị trí sinh ra đến vị trí mục tiêu và có delay nhặt.")]
    public bool isDroppedItem = false; // <-- BIẾN QUY ĐỊNH DẠNG ITEM (RƠI/TĨNH)
    private bool isDropping = false; // Trạng thái: Item đang rơi (chưa thể nhặt)

    [Header("Item Data")]
    public ItemData itemData;          // <-- GIỮ NGUYÊN ItemData
    public string nameitem;
    public int amount = 1;             

    public SpriteRenderer spriteRenderer;  
    public TextMeshProUGUI countText;     

    [Header("Pickup Settings")]
    public float pickupRadius = 2f;       
    public float followDuration = 0.3f;
    
    [Header("Drop/Fall Settings")]
    [HideInInspector] 
    // Vị trí mục tiêu. Nếu Tree gán nó, nó sẽ khác Vector3.zero.
    public Vector3 targetDropPosition = Vector3.zero; 
    public float dropSpeed = 5f; 
    
    private Transform player;
    private bool isFollowing = false;
    private Vector3 startPosition;
    private float followTime = 0f;
    
    private int leftover = 0;
    private bool pickedUp = false;

    void Awake()
    {
        // 1. Tìm Player
        GameObject go = GameObject.FindGameObjectWithTag("Player");
        if (go != null)
            player = go.transform;
        
        // 2. Tìm Manager (ICM) bằng Tag
        GameObject icmGo = GameObject.FindGameObjectWithTag("ICM");
        if (icmGo != null)
        {
            icm = icmGo.GetComponent<Gold_Iron_vv_Manager>();
            if (icm == null)
            {
                Debug.Log("<color=red>FATAL ERROR:</color> GameObject with tag 'ICM' does not have the Gold_Iron_vv_Manager script.");
            }
        }
        else
        {
            Debug.Log("<color=red>CRITICAL ERROR:</color> GameObject with tag 'ICM' not found! Please tag your manager object.");
        }
    }

    void Start()
    {
        UpdateUI();
        
        // CHỈ BẮT ĐẦU COROUTINE RƠI NẾU isDroppedItem = true
        if (isDroppedItem)
        {
            isDropping = true; // Kích hoạt trạng thái đang rơi

            // Nếu targetDropPosition không được gán, item rơi đến vị trí hiện tại của nó.
            if (targetDropPosition == Vector3.zero)
            {
                 targetDropPosition = transform.position;
            }
            
            StartCoroutine(FallToTargetPosition());
        }
        // Nếu isDroppedItem là false, item là tĩnh và isDropping giữ nguyên false, cho phép nhặt ngay.
    }

    // Coroutine quản lý quá trình item rơi
    IEnumerator FallToTargetPosition()
    {
        Vector3 startPos = transform.position;
        float startTime = Time.time;
        float distance = Vector3.Distance(startPos, targetDropPosition);
        
        float duration = distance > 0.01f ? distance / dropSpeed : 0.05f; 

        while (Time.time < startTime + duration)
        {
            float t = (Time.time - startTime) / duration;
            transform.position = Vector3.Lerp(startPos, targetDropPosition, t);
            yield return null;
        }

        // Đã đến vị trí mục tiêu
        transform.position = targetDropPosition;

        yield return new WaitForSeconds(1f);
        isDropping = false; // CHỈ CHO PHÉP NHẶT SAU KHI RƠI XONG
        Debug.Log("Item drop complete. Ready for pickup.");
    }

    // Cập nhật sprite và text dựa trên itemData
    public void UpdateUI()
    {
        if (itemData != null)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = itemData.icon;
                spriteRenderer.enabled = true;
            }

            if (countText != null)
            {
                countText.text = amount > 1 ? $"x{amount}" : "";
            }
        }
    }

    void Update()
    {
        // Item đang rơi (isDropping = true) thì KHÔNG thực hiện hút
        if (isDropping) return; 

        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        // Bắt đầu hút item khi player gần
        if (!isFollowing && distance <= pickupRadius)
        {
            isFollowing = true;
            startPosition = transform.position;
            followTime = 0f;
        }

        if (isFollowing)
        {
            followTime += Time.deltaTime;
            float t = Mathf.Clamp01(followTime / followDuration);
            transform.position = Vector3.Lerp(startPosition, player.position, t);
        }

        if(pickedUp == true && transform.position == player.position)
            StartCoroutine(DestroyItem());
        
        if(pickedUp == false && transform.position == player.position)
        {
            pickedUp = true;
            switch(nameitem)
            {
                case "Wood":
                    if (icm != null)
                    {
                        icm.Wood += amount;
                        Debug.Log($"Wood picked up: {amount}. Current Wood Count: {icm.Wood}");
                        Destroy(gameObject);
                    }
                    break;

                case "Iron":
                    if (icm != null)
                    {
                        icm.Iron += amount;
                        Debug.Log($"Iron picked up: {amount}. Current Iron Count: {icm.Iron}");
                        Destroy(gameObject);
                    }
                    break;

                default:
                    // Xử lý các item khác (vào Inventory)
                    if (Inventory.Instance == null)
                    {
                        Debug.LogError("Inventory.Instance is NULL! Cannot add item to inventory.");
                        Destroy(gameObject);
                        return;
                    }
                    
                    Debug.Log("Item picked up: " + nameitem);
                    
                    // Sử dụng ItemData cho Inventory
                    leftover = Inventory.Instance.AddItem(itemData, nameitem, amount);

                    if (leftover <= 0)
                        Destroy(gameObject);
                    else
                    {
                        amount = leftover;
                        UpdateUI();
                        pickedUp = false; 
                    }
                    break;
            }
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // CHỈ CHO PHÉP NHẶT KHI isDropping = false
        if (isDropping || pickedUp) return;
        if (!other.CompareTag("Player")) return;

        pickedUp = true; // đánh dấu

        // --- Logic Xử lý Item ---
        
        if(other.CompareTag("Player"))
        {
            switch(nameitem)
            {
                case "Wood":
                    if (icm != null)
                    {
                        icm.Wood += amount;
                        Debug.Log($"Wood picked up: {amount}. Current Wood Count: {icm.Wood}");
                        Destroy(gameObject);
                    }
                    break;

                case "Iron":
                    if (icm != null)
                    {
                        icm.Iron += amount;
                        Debug.Log($"Iron picked up: {amount}. Current Iron Count: {icm.Iron}");
                        Destroy(gameObject);
                    }
                    break;

                default:
                    // Xử lý các item khác (vào Inventory)
                    if (Inventory.Instance == null)
                    {
                        Debug.LogError("Inventory.Instance is NULL! Cannot add item to inventory.");
                        Destroy(gameObject);
                        return;
                    }
                    
                    Debug.Log("Item picked up: " + nameitem);
                    
                    // Sử dụng ItemData cho Inventory
                    leftover = Inventory.Instance.AddItem(itemData, nameitem, amount);

                    if (leftover <= 0)
                        Destroy(gameObject);
                    else
                    {
                        amount = leftover;
                        UpdateUI();
                        pickedUp = false; 
                    }
                    break;
            }
            Destroy(gameObject);
        }
    }
    IEnumerator DestroyItem()
    {
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }
}