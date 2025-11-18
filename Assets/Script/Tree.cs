using UnityEngine;
using System.Collections;

public class Tree : MonoBehaviour
{
    public int maxHealth = 3;
    public int currentHealth;
    public float respawnTime = 5f;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Collider2D coll;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        coll = GetComponent<Collider2D>();
    }

    public void TakeHit()
    {
        if (isDead) return;
        Debug.Log("Cây bị trúng đòn!");
        currentHealth -= 1;

        if (animator != null)
            animator.SetTrigger("Hit");

        if (currentHealth <= 0)
            StartCoroutine(HandleTreeDeath());
    }

    IEnumerator HandleTreeDeath()
    {
        isDead = true;
        if (animator != null)
            animator.SetBool("Dead", true);

        if (coll != null)
            coll.enabled = false;

        yield return new WaitForSeconds(respawnTime);

        currentHealth = maxHealth;
        isDead = false;
        if (animator != null)
        {
            animator.SetBool("Dead", false);
        }

        if (coll != null)
            coll.enabled = true;
    }
}
