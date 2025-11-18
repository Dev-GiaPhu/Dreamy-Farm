using UnityEngine;

/// <summary>
/// PlayerHitBox — quản lý collider đánh của người chơi.
/// Collider luôn tồn tại trên player, chỉ enable khi swing để tránh trừ nhiều lần.
/// </summary>

public class PlayerHitBox : MonoBehaviour
{
    private BoxCollider2D hitCollider;
    public bool hasHitThisSwing = false; // Kiểm tra đã trúng 1 lần trong swing chưa
    public PlayerController player => GetComponentInParent<PlayerController>();

    void Awake()
    {
        // Lấy collider và tắt nó lúc bắt đầu
        hitCollider = GetComponent<BoxCollider2D>();
        hitCollider.enabled = false;
    }

    /// <summary>
    /// Bật collider khi bắt đầu swing, reset flag
    /// </summary>
    public void EnableHitBox()
    {
        hitCollider.enabled = true;
        hasHitThisSwing = false;
    }

    /// <summary>
    /// Tắt collider khi swing kết thúc
    /// </summary>
    public void DisableHitBox()
    {
        hitCollider.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D trigger)
    {
        if (player == null) return;

        // Nếu đã trúng 1 lần trong swing hiện tại → bỏ qua
        if (hasHitThisSwing) return;

        // Swing Axe (handItem == 2) đánh Tree
        if (player.handItem == 2 && trigger.CompareTag("Tree"))
        {
            Tree tree = trigger.GetComponentInParent<Tree>();
            if (tree != null)
            {
                tree.TakeHit();
                Debug.Log("Cây bị chặt!");
                hasHitThisSwing = true;
            }
        }

        // Swing Sword (handItem == 0) đánh Cow
        if (player.handItem == 0 && trigger.CompareTag("Cow"))
        {
            CowController cow = trigger.GetComponentInParent<CowController>();
            if (cow != null)
            {
                cow.TakeHit();
                Debug.Log("Con bò bị đánh!");
                hasHitThisSwing = true;
            }
        }
    }
}
