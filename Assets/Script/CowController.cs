
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class CowController : MonoBehaviour
{
    [Header("Thông tin")]
    public string cowName = "Bò sữa";
    public float maxHealth = 5f;
    public float currentHealth;
    public float animatorDieTime = 3f;
    private Slider healthSlider => GetComponentInChildren<Slider>();

    [Header("Chuyển động")]
    public float moveSpeed = 2f;
    public float wanderRadius = 5f;
    public float idleTimeMin = 1f;
    public float idleTimeMax = 3f;
    private Animator animator => GetComponent<Animator>();

    private Coroutine wanderRoutine;

    [Header("Giới hạn vùng di chuyển")]
    public EdgeCollider2D worldBoundary;

    private Rigidbody2D rb;
    private Vector2 startPos;
    private Vector2 targetPos;
    private bool isMoving = false;
    private bool canMove = true;
    private bool waitingAfterCollision = false;
    public bool Die = false;

    private Spawn_Cow spawner; // Tham chiếu đến script spawn

    void Start()
    {

        spawner = GetComponentInParent<Spawn_Cow>();
        currentHealth = maxHealth;
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;

        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        startPos = transform.position;
        wanderRoutine = StartCoroutine(WanderRoutine());
    }

    void Update()
    {
        healthSlider.value = currentHealth;
        if (!isMoving)
            rb.linearVelocity = Vector2.zero;
        animator.SetBool("Walk", false);

        // Lật hướng sprite
        if (isMoving)
        {
            if (rb.linearVelocity.x > 0.01f)
                transform.localScale = new Vector3(-1, 1, 1);
            else if (rb.linearVelocity.x < -0.01f)
                transform.localScale = new Vector3(1, 1, 1);
            animator.SetBool("Walk", true);
        }
    }

    public void TakeHit()
    {
        currentHealth -= 1f;
        animator.SetTrigger("Hit");
        Debug.Log(cowName + " bị trúng đòn! Máu còn: " + currentHealth);
        StartCoroutine(BostSpeed());
        isMoving = true;

        if (currentHealth <= 0 && Die == false)
        {
            Die = true;
            animator.SetBool("Dead", true);
            if (wanderRoutine != null)
                StopCoroutine(wanderRoutine);
            canMove = false;
            isMoving = false;
            rb.linearVelocity = Vector2.zero;
            if (spawner != null)
            {
                spawner.OnCowDied();
                Debug.Log(cowName + " đã chết!");
            }
            Destroy(gameObject, animatorDieTime);
        }
    }
    IEnumerator BostSpeed()
    {
        float originalSpeed = moveSpeed;
        moveSpeed *= 2;
        yield return new WaitForSeconds(2f);
        moveSpeed = originalSpeed;
    }

    IEnumerator WanderRoutine()
    {
        while (canMove)
        {
            isMoving = false;
            rb.linearVelocity = Vector2.zero;
            yield return new WaitForSeconds(Random.Range(idleTimeMin, idleTimeMax));

            targetPos = GetValidRandomPoint();
            isMoving = true;

            while (Vector2.Distance(transform.position, targetPos) > 0.1f && !waitingAfterCollision)
            {
                Vector2 dir = ((Vector2)targetPos - (Vector2)transform.position).normalized;
                rb.linearVelocity = dir * moveSpeed;
                yield return null;
            }

            rb.linearVelocity = Vector2.zero;
            isMoving = false;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isMoving && !waitingAfterCollision)
            StartCoroutine(HandleCollisionReaction());
    }

    IEnumerator HandleCollisionReaction()
    {
        waitingAfterCollision = true;
        isMoving = false;
        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(Random.Range(idleTimeMin, idleTimeMax));
        targetPos = GetValidRandomPoint();
        waitingAfterCollision = false;
    }

    Vector2 GetValidRandomPoint()
    {
        for (int i = 0; i < 50; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * wanderRadius;
            Vector2 candidate = startPos + randomOffset;

            if (worldBoundary == null || IsPointInsideBoundary(candidate))
                return candidate;
        }
        return transform.position;
    }

    bool IsPointInsideBoundary(Vector2 point)
    {
        if (worldBoundary == null) return true;

        Vector2[] points = worldBoundary.points;
        int n = points.Length;
        bool inside = false;

        Vector2 offset = (Vector2)worldBoundary.transform.position + worldBoundary.offset;

        for (int i = 0, j = n - 1; i < n; j = i++)
        {
            Vector2 pi = points[i] + offset;
            Vector2 pj = points[j] + offset;

            if (((pi.y > point.y) != (pj.y > point.y)) &&
                (point.x < (pj.x - pi.x) * (point.y - pi.y) / (pj.y - pi.y) + pi.x))
                inside = !inside;
        }
        return inside;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, wanderRadius);
    }
}
