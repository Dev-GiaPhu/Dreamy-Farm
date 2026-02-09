using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class HedgehogController : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip spawnSound;
    [Header("Thông tin")]
    public string hedgehogName = "Nhím";
    public float maxHealth = 3f;
    public float currentHealth;
    public float dieTime = 3f;
    private Slider healthSlider => GetComponentInChildren<Slider>();

    [Header("Tấn công")]
    public CapsuleCollider2D attackCollider;
    public float attackDamage = 1f;

    [Header("Di chuyển")]
    public float moveSpeed = 2.5f;
    public float wanderRadius = 4f;

    [Header("Giới hạn vùng")]
    public EdgeCollider2D worldBoundary;

    private Rigidbody2D rb;
    private Animator animator;

    private Vector2 startPos;
    private Vector2 targetPos;

    private bool canMove = true;
    private bool waitingAfterCollision = false;
    private bool isAttacking = false;
    public bool Die = false;

    void Start()
    {
        audioSource.PlayOneShot(spawnSound);
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        rb.gravityScale = 0;
        rb.freezeRotation = true;

        currentHealth = maxHealth;
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;

        if (attackCollider != null)
            attackCollider.enabled = false;

        startPos = transform.position;
        targetPos = GetValidRandomPoint();
    }

    void Update()
    {
        healthSlider.value = currentHealth;

        if (!canMove || Die) return;

        Move();
    }

    // ================== MOVE ==================

    void Move()
    {
        Vector2 dir = (targetPos - (Vector2)transform.position).normalized;
        rb.linearVelocity = dir * moveSpeed;

        // flip theo hướng di chuyển
        if (rb.linearVelocity.x > 0.01f)
            transform.localScale = new Vector3(1, 1, 1);
        else if (rb.linearVelocity.x < -0.01f)
            transform.localScale = new Vector3(-1, 1, 1);

        if (Vector2.Distance(transform.position, targetPos) < 0.2f)
            targetPos = GetValidRandomPoint();
    }

    // ================== HIT / ATTACK ==================

    public void TakeHit()
    {
        if (Die) return;

        currentHealth -= 1f;

        animator.SetTrigger("Hit");
        animator.SetBool("Attacking", false);

        StartCoroutine(AttackAfterHit());

        if (currentHealth <= 0)
            DieAction();
    }

    IEnumerator AttackAfterHit()
    {
        canMove = false;
        rb.linearVelocity = Vector2.zero;

        // chờ animation Hit
        yield return new WaitForSeconds(0.4f);

        animator.SetBool("Attacking", true);
        isAttacking = true;
        if (attackCollider != null)
            attackCollider.enabled = true;

        // thời gian tấn công
        yield return new WaitForSeconds(1f);

        animator.SetBool("Attacking", false);
        isAttacking = false;
        if (attackCollider != null)
            attackCollider.enabled = false;

        canMove = true;
    }


    // ================== DIE ==================

    void DieAction()
    {
        Die = true;
        canMove = false;

        rb.linearVelocity = Vector2.zero;
        animator.SetBool("Dead", true);

        if (attackCollider != null)
            attackCollider.enabled = false;
    }

    public void OnDeathAnimationComplete()
    {
        Destroy(gameObject);
    }

    // ================== COLLISION ==================

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (waitingAfterCollision || Die) return;
        StartCoroutine(ChangeDirection());
    }

    IEnumerator ChangeDirection()
    {
        waitingAfterCollision = true;
        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(0.2f);

        targetPos = GetValidRandomPoint();
        waitingAfterCollision = false;
    }

    // ================== RANDOM POINT ==================

    Vector2 GetValidRandomPoint()
    {
        for (int i = 0; i < 50; i++)
        {
            Vector2 p = startPos + Random.insideUnitCircle * wanderRadius;
            if (worldBoundary == null || IsInside(p))
                return p;
        }
        return transform.position;
    }

    bool IsInside(Vector2 point)
    {
        Vector2[] pts = worldBoundary.points;
        Vector2 offset = (Vector2)worldBoundary.transform.position + worldBoundary.offset;
        bool inside = false;

        for (int i = 0, j = pts.Length - 1; i < pts.Length; j = i++)
        {
            Vector2 a = pts[i] + offset;
            Vector2 b = pts[j] + offset;

            if ((a.y > point.y) != (b.y > point.y) &&
                point.x < (b.x - a.x) * (point.y - a.y) / (b.y - a.y) + a.x)
                inside = !inside;
        }
        return inside;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, wanderRadius);
    }
}
