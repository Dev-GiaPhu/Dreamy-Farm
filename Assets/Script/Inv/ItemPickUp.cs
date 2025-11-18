using UnityEngine;
using TMPro;

public class ItemPickup : MonoBehaviour
{
    public ItemData itemData;          // ItemData gốc
    public int amount = 1;             // Số lượng hiện tại

    public SpriteRenderer spriteRenderer;  // SpriteRenderer hiển thị icon
    public TextMeshProUGUI countText;     // Text hiển thị số lượng

    public float pickupRadius = 2f;       // Khoảng cách bắt đầu hút
    public float followDuration = 0.3f;

    private Transform player;
    private bool isFollowing = false;
    private Vector3 startPosition;
    private float followTime = 0f;

    void Start()
    {
        UpdateUI();
    }

    // Cập nhật sprite và text dựa trên itemData + amount
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
        if (player == null)
        {
            GameObject go = GameObject.FindGameObjectWithTag("Player");
            if (go != null)
                player = go.transform;
        }

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
    }

    private bool pickedUp = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (pickedUp) return;   // đã nhặt → không nhặt lại
        if (!other.CompareTag("Player")) return;

        pickedUp = true; // đánh dấu

        int leftover = Inventory.Instance.AddItem(itemData, amount);

        if (leftover <= 0)
            Destroy(gameObject);
        else
        {
            amount = leftover;
            UpdateUI();
            pickedUp = false; // nếu còn dư → cho nhặt tiếp
        }
    }

}
