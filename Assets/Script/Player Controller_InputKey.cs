using UnityEngine;

public class PlayerController_InputKey : MonoBehaviour
{
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

        if (Input.GetKey(KeyCode.W))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y + speed).normalized;
            Debug.Log("Up key pressed");
        }
        if (Input.GetKey(KeyCode.S))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y - speed).normalized;
            Debug.Log("Down key pressed");
        }
        if (Input.GetKey(KeyCode.A))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x - speed, rb.linearVelocity.y).normalized;
            Debug.Log("Left key pressed");
            transform.localScale = new Vector3(-1, 1, 1);
        }
        if (Input.GetKey(KeyCode.D))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x + speed, rb.linearVelocity.y).normalized;
            Debug.Log("Right key pressed");
            transform.localScale = new Vector3(1, 1, 1);
        }
    }
}
