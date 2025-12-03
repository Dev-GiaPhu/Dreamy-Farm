using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;
using Unity.VisualScripting;

/// <summary>
/// PlayerController — quản lý di chuyển, hành động (sword/pickaxe/axe/fishing/jump) và UI slot.
/// Fishing: tự gán collider con nếu chưa gán, check Tilemap water bằng Tilemap API (không cần collider trên tilemap).
/// Inventory: dùng InventoryPanel + ItemUI
/// </summary>
public class PlayerController : MonoBehaviour
{
    #region Player state & components
    [Header("Player Stats")]
    public ItemType it;
    public int handItem = 0;
    public float Speed = 3f;
    public float Heath = 100f;
    // trạng thái hành động
    public bool isJumping = false;
    public bool isAxeing = false;
    public bool isSwording = false;
    public bool isPickaxing = false;
    public bool isFishing = false;
    private Coroutine fishingCoroutine;

    public bool isShoveling = false;
    public bool isWaterCaning = false;
    public bool OpenPackBack = false;

    public bool isRight;

    [Tooltip("Balo của người chơi (gán trong Inspector)")]
    public GameObject PackBackUI;

    public GameObject SelectIcon;

    [Header("Slot Hot Bar")]
    public GameObject Slot1;
    public GameObject Slot2;
    public GameObject Slot3;
    public GameObject Slot4;
    public GameObject Slot5;
    public GameObject Slot6;

    private Rigidbody2D rb;
    private Vector2 movement;
    private Animator animator;
    #endregion

    #region Fishing references
    [Header("Fishing Area")]
    [Tooltip("Collider khu vực câu cá bên phải (có thể gán thủ công, nếu null script sẽ tìm child tên RightAreaFish)")]
    public CircleCollider2D RightAreaFish;
    [Tooltip("Collider khu vực câu cá bên trái (có thể gán thủ công, nếu null script sẽ tìm child tên LeftAreaFish)")]
    public CircleCollider2D LeftAreaFish;

    private static readonly Color[] MajorFishColors = new Color[]
    {
        Color.red,      // Đỏ
        Color.yellow,   // Vàng
        Color.green,    // Xanh lá
        Color.blue,     // Xanh dương
        Color.magenta,  // Hồng/Tím (Pink)
        Color.white,    // Trắng
        Color.cyan,     // Xanh lơ (Cyan)
        new Color(1f, 0.5f, 0f), // Cam (Orange)
        new Color(0.5f, 0f, 0.5f), // Tím đậm (Purple) - vẫn nổi bật
    };

    [Header("Icon")]
    [Tooltip("Sprite object hiển thị 'ex fish'")]
    public GameObject ExFish;

    [Header("Fish")]
    public ItemData[] fish;

    [Header("ItemCollect")]
    public Sprite collectedSprite;
    public GameObject conllectItem;
    public AnimationItemCollect timeCollectitonItem;

    private SpriteRenderer exFishRenderer;

    [Header("Tilemap Water")]
    public Tilemap waterTilemap;
    #endregion

    #region Camera Settings
    [Header("Camera Settings")]
    public Camera mainCamera;
    public float cameraOffsetX = -4.5f;
    public float cameraMoveSpeed = 3f;

    private Vector3 cameraOriginalPos;
    #endregion

    #region Unity callbacks
    void Awake()
    {
        if (RightAreaFish == null)
        {
            Transform t = transform.Find("RightAreaFish");
            if (t != null) RightAreaFish = t.GetComponent<CircleCollider2D>();
        }
        if (LeftAreaFish == null)
        {
            Transform t = transform.Find("LeftAreaFish");
            if (t != null) LeftAreaFish = t.GetComponent<CircleCollider2D>();
        }

        if (ExFish != null) exFishRenderer = ExFish.GetComponent<SpriteRenderer>();

        if (exFishRenderer != null) exFishRenderer.enabled = false;

        timeCollectitonItem = conllectItem.GetComponent<AnimationItemCollect>();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Camera
        if (mainCamera == null)
            mainCamera = Camera.main;
        if (mainCamera != null)
            cameraOriginalPos = mainCamera.transform.position;
    }

    void Update()
    {
        HandleSlotSelection();
        HandleActions();
        HandleMovement();
    }

    void FixedUpdate()
    {
        if (rb != null && isWaterCaning) rb.linearVelocity = movement * (Speed*0.5f);
        else if (rb != null && !isWaterCaning) rb.linearVelocity = movement * Speed;
    }
    #endregion

    #region Input & handlers

    public void OnclickSlot1(){handItem = 0;}
    public void OnclickSlot2(){handItem = 1;}
    public void OnclickSlot3(){handItem = 2;}
    public void OnclickSlot4(){handItem = 3;}
    public void OnclickSlot5(){handItem = 4;}
    public void OnclickSlot6(){handItem = 5;}
    
    void HandleSlotSelection()
    {
        if (isFishing || isAxeing || isSwording || isPickaxing || isJumping || isShoveling || isWaterCaning)
        {
            return;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll < 0f) handItem = (handItem + 1) % 6;
        else if (scroll > 0f) handItem = (handItem - 1 + 6) % 6;

        if (Input.GetKeyDown(KeyCode.Alpha1)) handItem = 0;
        else if (Input.GetKeyDown(KeyCode.Alpha2)) handItem = 1;
        else if (Input.GetKeyDown(KeyCode.Alpha3)) handItem = 2;
        else if (Input.GetKeyDown(KeyCode.Alpha4)) handItem = 3;
        else if (Input.GetKeyDown(KeyCode.Alpha5)) handItem = 4;
        else if (Input.GetKeyDown(KeyCode.Alpha6)) handItem = 5;

        switch (handItem)
        {
            case 0:
                SelectIcon.transform.position = Slot1.GetComponent<RectTransform>().position;
                it = Slot1.GetComponent<InventorySlotUI>().type;
                break;
            case 1:
                SelectIcon.transform.position = Slot2.GetComponent<RectTransform>().position;
                it = Slot2.GetComponent<InventorySlotUI>().type;
                break;
            case 2:
                SelectIcon.transform.position = Slot3.GetComponent<RectTransform>().position;
                it = Slot3.GetComponent<InventorySlotUI>().type;
                break;
            case 3:
                SelectIcon.transform.position = Slot4.GetComponent<RectTransform>().position;
                it = Slot4.GetComponent<InventorySlotUI>().type;
                break;
            case 4:
                SelectIcon.transform.position = Slot5.GetComponent<RectTransform>().position;
                it = Slot5.GetComponent<InventorySlotUI>().type;
                break;
            case 5:
                SelectIcon.transform.position = Slot6.GetComponent<RectTransform>().position;
                it = Slot6.GetComponent<InventorySlotUI>().type;
                break;
            default:
                it = ItemType.Null;
                break;
        }
    }

    void HandleMovement()
    {
        if (!isAxeing && !isSwording && !isPickaxing && !isFishing && !isShoveling)
        {
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveY = Input.GetAxisRaw("Vertical");
            movement = new Vector2(moveX, moveY).normalized;

            if (animator != null) animator.SetBool("Walk", moveX != 0 || moveY != 0);

            if (moveX > 0) transform.localScale = new Vector3(1, 1, 1);
            else if (moveX < 0) transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            movement = Vector2.zero;
            if (animator != null) animator.SetBool("Walk", false);
        }
    }

    void HandleActions()
    {
        if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.J)) && !EventSystem.current.IsPointerOverGameObject())
        {
            if (!isFishing && !isJumping && !isAxeing && !isSwording && !isPickaxing && !isWaterCaning)
            {
                switch (it)
                {
                    case ItemType.Sword: StartCoroutine(DoSword()); break;
                    case ItemType.Pickaxe: StartCoroutine(DoPickaxe()); break;
                    case ItemType.Axe: StartCoroutine(DoAxe()); break;
                    case ItemType.Shovel: StartCoroutine(DoShovel()); break;
                    case ItemType.CanWater:
                        if (animator != null && !isWaterCaning)
                        {
                            animator.SetTrigger("Can Water");
                            isWaterCaning = true;
                        }
                        break;
                }
            }
        }

        if (Input.GetMouseButtonUp(0) || Input.GetKeyUp(KeyCode.J) || it != ItemType.CanWater)
        {
            if (isWaterCaning)
            {
                isWaterCaning = false;
                if (animator != null) animator.SetTrigger("Stop Water");
            }
        }

        // BỔ SUNG: Logic dừng Fishing khi đổi item
        // Nếu đang câu cá VÀ item hiện tại không phải là Cần câu
        if (isFishing && it != ItemType.FishingRod)
        {
            StopFishingImmediately();
        }

        if ((Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.J)) && !EventSystem.current.IsPointerOverGameObject())
        {
            if (!isFishing && !isJumping && !isAxeing && !isSwording && !isPickaxing)
            {
                if (it == ItemType.FishingRod)
                    // LƯU COROUTINE để có thể dừng sau này
                    fishingCoroutine = StartCoroutine(DoFishing());
            }
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (!OpenPackBack)
            {
                PackBackUI.transform.position = new Vector3(410, 540, 0);
                OpenPackBack = true;
            }
            else
            {
                PackBackUI.transform.position = new Vector3(-9340, 540, 0);
                OpenPackBack = false;
            }
        }

        if (Input.GetKey(KeyCode.Space) && !isJumping && !isAxeing && !isSwording && !isPickaxing && !isFishing)
        {
            StartCoroutine(Jump());
        }
    }
    #endregion

    #region Coroutines (actions)
    IEnumerator MoveCameraSmooth(Vector3 targetPos)
    {
        if (mainCamera == null) yield break;
        Vector3 startPos = mainCamera.transform.position;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * cameraMoveSpeed;
            mainCamera.transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }
    }

    IEnumerator DoSword()
    {
        isSwording = true;
        if (animator != null) animator.SetTrigger("Sword");
        var hitbox = GetComponentInChildren<PlayerHitBox>();
        if (hitbox != null)
            hitbox.EnableHitBox();
        yield return new WaitForSeconds(1f);
        isSwording = false;
        Debug.Log("Attack Done.");
    }

    IEnumerator DoPickaxe()
    {
        isPickaxing = true;
        Debug.Log("Pickaxe use.");
        if (animator != null) animator.SetTrigger("Pickaxe");
        var hitbox = GetComponentInChildren<PlayerHitBox>();
        if(hitbox == null)
            Debug.Log("hitbox is null");
        if (hitbox != null)
        {
            hitbox.EnableHitBox();
            Debug.Log("hitbox is'not null");
        }
        yield return new WaitForSeconds(0.8f);
        if (hitbox != null)
            hitbox.DisableHitBox();
        isPickaxing = false;
    }

    IEnumerator DoAxe()
    {
        isAxeing = true;
        if (animator != null) animator.SetTrigger("Axe");
        var hitbox = GetComponentInChildren<PlayerHitBox>();
        if(hitbox = null)
            Debug.Log("hitbox is null");
        if (hitbox != null)
            hitbox.EnableHitBox();
        Debug.Log("Pickaxe use.");

        yield return new WaitForSeconds(0.8f);
        if (hitbox != null)
            hitbox.DisableHitBox();
        isAxeing = false;
    }

    IEnumerator DoFishing()
    {
        CircleCollider2D fishingArea = transform.localScale.x > 0 ? RightAreaFish : LeftAreaFish;
        isRight = transform.localScale.x > 0;

        if (!IsColliderOverWater(fishingArea))
        {
            isFishing = true;
            if (animator != null) animator.SetTrigger("Can'tFish");
            yield return new WaitForSeconds(0.7f);
            Debug.Log("Không có tile Water trong vùng, không thể câu cá!");
            isFishing = false;
            fishingCoroutine = null; // Đặt lại sau khi Coroutine kết thúc
            yield break;
        }

        Debug.Log("Bắt đầu câu cá...");
        isFishing = true;

        if (animator != null) animator.SetTrigger("Fishing");
        yield return new WaitForSeconds(0.5f);

        float fishingDuration = Random.Range(3f, 10f);
        float timer = 0f;
        Debug.Log("Cho trong khoảng " + fishingDuration.ToString("F1") + " giây để cá cắn...");
        while (timer < fishingDuration)
        {
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.J))
            {
                Debug.Log("😢 Bạn thu cần quá sớm!");
                // Sử dụng hàm dọn dẹp để xử lý thu cần và chờ animation
                yield return StartCoroutine(FinishFishingCleanup());
                fishingCoroutine = null; // Đặt lại sau khi Coroutine kết thúc
                yield break;
            }
            timer += Time.deltaTime;
            yield return null;
        }

        // CHỌN MỘT MÀU NỔI BẬT VÀ KHÁC BIỆT TỪ DANH SÁCH ĐÃ ĐỊNH NGHĨA
        Color fishColor = MajorFishColors[Random.Range(0, MajorFishColors.Length)];

        if (exFishRenderer != null)
        {
            exFishRenderer.color = fishColor;
            exFishRenderer.enabled = true;
        }

        float reactionTime = 2f;
        bool caught = false;
        float reactionTimer = 0f;
        while (reactionTimer < reactionTime)
        {
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.J))
            {
                caught = true;
                break;
            }
            reactionTimer += Time.deltaTime;
            yield return null;
        }

        if (exFishRenderer != null) exFishRenderer.enabled = false;

        if (animator != null) animator.SetTrigger("FishingUp");

        if (caught)
        {
            int valuefish = Random.Range(0,15);
            var _fish = fish[valuefish];
            yield return new WaitForSeconds(Random.Range(0.5f, 3f));
            
            // Sử dụng hàm dọn dẹp để xử lý thu cần và chờ animation
            yield return StartCoroutine(FinishFishingCleanup());
            
            Debug.Log($"🎉 Bạn đã câu được {_fish}");
            AnimationItemCollect.Instance.TriggerAnimation(_fish.icon);
            yield return new WaitForSeconds(timeCollectitonItem.lifetime + timeCollectitonItem.durationGrow + (timeCollectitonItem.durationGrow *0.5f) + timeCollectitonItem.durationShrink);
            Inventory.Instance.AddItem(_fish, _fish.itemName, 1);
        }
        else
        {
            // Sử dụng hàm dọn dẹp để xử lý thu cần và chờ animation
            yield return StartCoroutine(FinishFishingCleanup());
            Debug.Log($"😢 Bạn đã để tuột mất!");
        }
        
        fishingCoroutine = null; // Đặt lại sau khi Coroutine kết thúc
    }

    public void StopFishingImmediately()
    {
        if (fishingCoroutine != null)
        {
            StopCoroutine(fishingCoroutine);
            fishingCoroutine = null;
        }

        if (isFishing)
        {
            // THU CẦN VÀ CHỜ ANIMATION KẾT THÚC
            StartCoroutine(FinishFishingCleanup());
        }
    }

    IEnumerator FinishFishingCleanup()
    {
        Debug.Log("Fishing stopped by external event (item change). Cleaning up...");
        
        // Vô hiệu hóa Icon cá nếu đang hiển thị
        if (exFishRenderer != null) exFishRenderer.enabled = false;
        
        // Kích hoạt animation thu cần (FishingDone)
        if (animator != null) animator.SetTrigger("FishingDone");
        
        // Chờ animation kết thúc (1 giây như bạn đề xuất)
        yield return new WaitForSeconds(1f);
        
        // Đặt lại trạng thái
        isFishing = false;
    }

    IEnumerator DoShovel()
    {
        isShoveling = true;
        if (animator != null) animator.SetTrigger("Shovel");
        yield return new WaitForSeconds(1.2f);
        isShoveling = false;
    }

    IEnumerator Jump()
    {
        isJumping = true;
        if (animator != null) animator.SetTrigger("Jump");
        yield return new WaitForSeconds(1f);
        isJumping = false;
    }
    
    #endregion

    #region Helpers
    private bool IsColliderOverWater(CircleCollider2D areaCollider)
    {
        if (areaCollider == null || waterTilemap == null) return false;

        Bounds bounds = areaCollider.bounds;
        Vector3Int min = waterTilemap.WorldToCell(bounds.min);
        Vector3Int max = waterTilemap.WorldToCell(bounds.max);

        for (int x = min.x; x <= max.x; x++)
        {
            for (int y = min.y; y <= max.y; y++)
            {
                Vector3Int cellPos = new Vector3Int(x, y, 0);
                TileBase tile = waterTilemap.GetTile(cellPos);
                if (tile != null) return true;
            }
        }
        return false;
    }
    public bool IsAnyActionInProgress()
    {
        return isAxeing || isSwording || isPickaxing || isFishing || isShoveling || isWaterCaning || isJumping;
    }
#endregion
}