using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FBossController : MonoBehaviour, IDamageable
{

    [Header("Boss Health System")]
    [SerializeField] private int maxHealth = 100;
    private int currenthealth;

    [Space(5)]
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float detectionRange = 10f;
    private bool isFollowingPlayer = false;

    [Space(5)]
    [Header("Chasing Settings")]
    [SerializeField] private Transform player;
    private bool isAlive = true;

    [Space(5)]
    [Header("Attack Settings")]
    [SerializeField] private int attackDamage = 15;
    [SerializeField] private float attackRange = 1;
    [SerializeField] private float attackCooldown = 3f;
    private bool canAttack = true;

    [Space(5)]
    [Header("2 Phase Settings")]
    [SerializeField] private float phase2SpeedMultiplier = 1.5f;
    private bool hasEnteredPhase2 = false;
    private bool isStunned = false;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private Vector2 lastMoveDirection;

    private void Start()
    {
        currenthealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (isAlive)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            if (distanceToPlayer <= detectionRange)
            {
                isFollowingPlayer = true;
            }
            else
            {
                isFollowingPlayer = false;
            }

            if(isFollowingPlayer && !isStunned)
            {
                FollowPlayer();

                if (distanceToPlayer <= attackRange && canAttack)
                {
                    StartCoroutine(AttackPlayer());
                }
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
                animator.SetBool("isWalking", false);
            }
        }
    }

    // ---- Attack Logic ----

    private IEnumerator AttackPlayer()
    {
        canAttack = false;

        string attackAnim = GetAttackAnimation(lastMoveDirection);
        animator.SetTrigger(attackAnim);

        yield return new WaitForSeconds(0.3f);

        if (player.TryGetComponent(out PlayerController playerController))
        {
            playerController.TakeDamage(attackDamage);
            Debug.Log("Boss Attacked the player and dealt damage!");
        }

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    private string GetAttackAnimation(Vector2 direction)
    {
        if (direction.y > 0.5f) return "Attack_Up";
        if (direction.y < -0.5f) return "Attack_Down";
        if (direction.x > 0.5f) return "Attack_Right";
        if (direction.x < -0.5f) return "Attack_Left";

        return "Attack_Down";
    }

    // ---- Movement Logic ----

    private void FollowPlayer()
    {
        if (player != null)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            rb.linearVelocity = direction * moveSpeed;

            animator.SetBool("isWalking", true);
            animator.SetFloat("InputX", direction.x);
            animator.SetFloat("InputY", direction.y);

            lastMoveDirection = direction;

            if (direction.x > 0)
            {
                spriteRenderer.flipX = false;
            }
            else if (direction.x < 0)
            {
                spriteRenderer.flipX = true;
            }
        }
    }

    // ---- Health Logic ----

    public void TakeDamage(int damage)
    {
        currenthealth -= damage;
        animator.SetTrigger("TakeDamage");

        Debug.Log($"Boss took {damage} damage! Current HP: {currenthealth}");

        if (currenthealth <= 0)
        {
            Die();
        }
        else if (currenthealth == 50 && !hasEnteredPhase2)
        {
            hasEnteredPhase2 = true;
            StartCoroutine(EnterPhase2());
        }
    }

    private IEnumerator EnterPhase2()
    {
        Debug.Log("Boss is entering Phase 2!");

        isStunned = true;
        rb.linearVelocity = Vector2.zero;
        animator.SetBool("isWalking", false);

        yield return new WaitForSeconds(2f);

        moveSpeed *= phase2SpeedMultiplier;
        isStunned = false;

        Debug.Log($"Boss resumed movement with increased speed: {moveSpeed}");
    }

    private void Die()
    {
        isAlive = false;
        rb.linearVelocity = Vector2.zero;
        animator.SetBool("isWalking", false);
        Debug.Log("Boss has died.");

        this.enabled = false;
        Destroy(gameObject, 1f); 
    }

    // ---- Gizmos ----
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
