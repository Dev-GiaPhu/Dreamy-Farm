using UnityEngine;
using System.Collections;

public class PlayerHitBox : MonoBehaviour
{
    private BoxCollider2D hitCollider;
    public bool hasHitThisSwing = false; // Kiểm tra đã trúng 1 lần trong swing chưa
    public PlayerController player => GetComponentInParent<PlayerController>();

    void Awake()
    {
        hitCollider = GetComponent<BoxCollider2D>();
        hitCollider.enabled = false;
    }

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
        if (player.it == ItemType.Axe && trigger.CompareTag("Tree"))
        {
            Tree tree = trigger.GetComponentInParent<Tree>();
            if (tree != null)
            {
                tree.TakeHit();
                Debug.Log("Cây bị chặt!");
                StartCoroutine(WaitOff());
            }
        }

        // Swing Sword (handItem == 0) đánh Cow
        if (player.it == ItemType.Sword && trigger.CompareTag("Cow"))
        {
            CowController cow = trigger.GetComponentInParent<CowController>();
            if (cow != null)
            {
                cow.TakeHit();
                Debug.Log("Con bò bị đánh!");
                StartCoroutine(WaitOff());
            }
        }
    }
    IEnumerator WaitOff()
    {
        hasHitThisSwing = true;
        yield return new WaitForSeconds(1f);
        hasHitThisSwing = false;
    }
}
