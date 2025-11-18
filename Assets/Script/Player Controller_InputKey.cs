using UnityEngine;

/// <summary>
/// PlayerController — quản lý di chuyển, hành động (sword/pickaxe/axe/fishing/jump) và UI slot.
/// Fishing: tự gán collider con nếu chưa gán, check Tilemap water bằng Tilemap API (không cần collider trên tilemap).
/// </summary>
public class PlayerController_InputKey : MonoBehaviour
{
    public KeyCode moveUpKey = KeyCode.W;
    public KeyCode moveDownKey = KeyCode.S;
    public KeyCode moveLeftKey = KeyCode.A;
    public KeyCode moveRightKey = KeyCode.D;

    public Animator animator;

    public float speed = 3f;

    private Rigidbody2D rb => GetComponent<Rigidbody2D>();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            animator.SetTrigger("Jump");
        }

        rb.linearVelocity = Vector2.zero;

        if (Input.GetKey(moveUpKey))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y + speed).normalized;
            Debug.Log("Up key pressed");
        }
        if (Input.GetKey(moveDownKey))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y - speed).normalized;
            Debug.Log("Down key pressed");
        }
        if (Input.GetKey(moveLeftKey))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x - speed, rb.linearVelocity.y).normalized;
            Debug.Log("Left key pressed");
            transform.localScale = new Vector3(-1, 1, 1); // Flip sprite sang trái
        }
        if (Input.GetKey(moveRightKey))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x + speed, rb.linearVelocity.y).normalized;
            Debug.Log("Right key pressed");
            transform.localScale = new Vector3(1, 1, 1); // Flip sprite sang phải
        }
    }
}
