using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;

/// <summary>
/// PlayerController — quản lý di chuyển, hành động (sword/pickaxe/axe/fishing/jump) và UI slot.
/// Fishing: tự gán collider con nếu chưa gán, check Tilemap water bằng Tilemap API (không cần collider trên tilemap).
/// Inventory: dùng InventoryPanel + ItemUI
/// </summary>
public class PlayerController : MonoBehaviour
{
    #region Player state & components
    [Header("Player Stats")]
    public int handItem = 0;
    public float Speed = 3f;
    public float Heath = 100f;
    // trạng thái hành động
    public bool isJumping = false;
    public bool isAxeing = false;
    public bool isSwording = false;
    public bool isPickaxing = false;
    public bool isFishing = false;
    public bool isShoveling = false;
    public bool isWaterCaning = false;
    public bool OpenPackBack = false;
    public bool Backpack = false;

    [Tooltip("Balo của người chơi (gán trong Inspector)")]
    public GameObject PackBackUI;

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

    [Header("Vùng câu cá")]
    [Tooltip("Sprite object hiển thị 'ex fish' bên phải")]
    public GameObject ExFishRight;
    [Tooltip("Sprite object hiển thị 'ex fish' bên trái")]
    public GameObject ExFishLeft;

    private SpriteRenderer exFishRightRenderer;
    private SpriteRenderer exFishLeftRenderer;

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

        if (ExFishRight != null) exFishRightRenderer = ExFishRight.GetComponent<SpriteRenderer>();
        if (ExFishLeft != null) exFishLeftRenderer = ExFishLeft.GetComponent<SpriteRenderer>();

        if (exFishRightRenderer != null) exFishRightRenderer.enabled = false;
        if (exFishLeftRenderer != null) exFishLeftRenderer.enabled = false;
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
        HandleMovement();
        HandleActions();
    }

    void FixedUpdate()
    {
        if (rb != null) rb.linearVelocity = movement * Speed;
    }
    #endregion

    #region Input & handlers
    void HandleSlotSelection()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll < 0f) handItem = (handItem + 1) % 6;
        else if (scroll > 0f) handItem = (handItem - 1 + 6) % 6;

        if (Input.GetKeyDown(KeyCode.Alpha1)) handItem = 0;
        else if (Input.GetKeyDown(KeyCode.Alpha2)) handItem = 1;
        else if (Input.GetKeyDown(KeyCode.Alpha3)) handItem = 2;
        else if (Input.GetKeyDown(KeyCode.Alpha4)) handItem = 3;
        else if (Input.GetKeyDown(KeyCode.Alpha5)) handItem = 4;
        else if (Input.GetKeyDown(KeyCode.Alpha6)) handItem = 5;
    }

    public void OnClickSlot1() { handItem = 0; }
    public void OnClickSlot2() { handItem = 1; }
    public void OnClickSlot3() { handItem = 2; }
    public void OnClickSlot4() { handItem = 3; }
    public void OnClickSlot5() { handItem = 4; }
    public void OnClickSlot6() { handItem = 5; }

    public void OnClickBackpack()
    {

    }

    void HandleMovement()
    {
        if (!isAxeing && !isSwording && !isPickaxing && !isFishing && !isShoveling && !isWaterCaning)
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
            if (!isFishing && !isJumping && !isAxeing && !isSwording && !isPickaxing && !OpenPackBack && !isWaterCaning)
            {
                switch (handItem)
                {
                    case 0: StartCoroutine(DoSword()); break;
                    case 1: StartCoroutine(DoPickaxe()); break;
                    case 2: StartCoroutine(DoAxe()); break;
                    case 4: StartCoroutine(DoShovel()); break;
                    case 5:
                        if (animator != null && !isWaterCaning)
                        {
                            animator.SetTrigger("Can Water");
                            isWaterCaning = true;
                        }
                        break;
                }
            }
        }

        if (Input.GetMouseButtonUp(0) || Input.GetKeyUp(KeyCode.J) || handItem != 5)
        {
            if (isWaterCaning)
            {
                isWaterCaning = false;
                if (animator != null) animator.SetTrigger("Stop Water");
            }
        }

        if ((Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.J)) && !EventSystem.current.IsPointerOverGameObject())
        {
            if (!isFishing && !isJumping && !isAxeing && !isSwording && !isPickaxing && !OpenPackBack)
            {
                if (handItem == 3)
                    StartCoroutine(DoFishing());
            }
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            OnClickBackpack();
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
        yield return new WaitForSeconds(0.6f);
        if (hitbox != null)
            hitbox.DisableHitBox();
        isSwording = false;
    }

    IEnumerator DoPickaxe()
    {
        isPickaxing = true;
        if (animator != null) animator.SetTrigger("Pickaxe");
        var hitbox = GetComponentInChildren<PlayerHitBox>();
        if (hitbox != null)
            hitbox.EnableHitBox();
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
        if (hitbox != null)
            hitbox.EnableHitBox();

        yield return new WaitForSeconds(0.8f);
        if (hitbox != null)
            hitbox.DisableHitBox();
        isAxeing = false;
    }

    IEnumerator DoFishing()
    {
        // --- giữ nguyên toàn bộ logic fishing ---
        CircleCollider2D fishingArea = transform.localScale.x > 0 ? RightAreaFish : LeftAreaFish;
        bool isRight = transform.localScale.x > 0;

        if (!IsColliderOverWater(fishingArea))
        {
            Debug.Log("Không có tile Water trong vùng, không thể câu cá!");
            yield break;
        }

        Debug.Log("Bắt đầu câu cá...");
        isFishing = true;

        if (animator != null) animator.SetTrigger("Fishing");
        yield return new WaitForSeconds(0.5f);

        float fishingDuration = Random.Range(6f, 20f);
        float timer = 0f;
        Debug.Log("Cho trong khoảng " + fishingDuration.ToString("F1") + " giây để cá cắn...");
        while (timer < fishingDuration)
        {
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.J))
            {
                Debug.Log("😢 Bạn thu cần quá sớm!");
                if (animator != null) animator.SetTrigger("FishingDone");
                yield return new WaitForSeconds(1f);
                isFishing = false;
                yield break;
            }
            timer += Time.deltaTime;
            yield return null;
        }

        Color fishColor = Color.black;
        while (fishColor == Color.black)
        {
            fishColor = Random.ColorHSV(0.5f, 1f, 0.5f, 1f, 0.5f, 1f);
        }

        if (isRight && exFishRightRenderer != null)
        {
            exFishRightRenderer.color = fishColor;
            exFishRightRenderer.enabled = true;
        }
        else if (!isRight && exFishLeftRenderer != null)
        {
            exFishLeftRenderer.color = fishColor;
            exFishLeftRenderer.enabled = true;
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

        if (exFishRightRenderer != null) exFishRightRenderer.enabled = false;
        if (exFishLeftRenderer != null) exFishLeftRenderer.enabled = false;

        if (animator != null) animator.SetTrigger("FishingUp");

        if (caught)
        {
            yield return new WaitForSeconds(Random.Range(0.5f, 3f));
            if (animator != null) animator.SetTrigger("FishingDone");
            Debug.Log($"🎉 Bạn đã câu được");
        }
        else
        {
            if (animator != null) animator.SetTrigger("FishingDone");
            Debug.Log($"😢 Bạn đã để tuột mất!");
        }

        yield return new WaitForSeconds(0.7f);
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
    #endregion
}
