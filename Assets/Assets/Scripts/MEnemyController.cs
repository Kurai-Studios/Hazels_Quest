using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class MEnemyController : MonoBehaviour, IDamageable
{
    [Header("Enemy Stats")]
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    [Space(5)]
    [Header("Patrol Settings")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float patrolSpeed = 2f;
    private int currentPatrolIndex = 0;

    [Space(5)]
    [Header("Chase Settings")]
    [SerializeField] private float chaseSpeed = 3f;
    [SerializeField] private float chaseRange = 5f;
    [SerializeField] private Transform player;
    private bool hasDetectedPlayer = false;

    [Space(5)]
    [Header("Attack Settings")]
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private float attackCooldown = 1f;
    private bool canAttack = true;

    private enum EnemyState { Patrolling, Chasing, Attacking }
    private EnemyState currentState = EnemyState.Patrolling;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;


    void Start()
    {
        currentHealth = maxHealth;
        rb= GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            if (distanceToPlayer < chaseRange)
            {
                hasDetectedPlayer = true;
            }

            if (hasDetectedPlayer)
            {
                if (distanceToPlayer <= attackRange)
                {
                    currentState = EnemyState.Attacking;
                }
                else
                {
                    currentState = EnemyState.Chasing;
                }
            }
            else
            {
                currentState = EnemyState.Patrolling;
            }
            
        }


         switch (currentState)
        {
            case EnemyState.Patrolling:
                Patrol();
                break;

            case EnemyState.Chasing:
                ChasePlayer();
                break;

            case EnemyState.Attacking:
                AttackPlayer();
                break;
        }
    }


    // ---- State Logics ----
    private void Patrol()
    {
        if (patrolPoints.Length == 0) return;

        Transform targetPoint = patrolPoints[currentPatrolIndex];
        MoveTowards(targetPoint.position, patrolSpeed);

        if (Vector2.Distance(transform.position, targetPoint.position) < 0.2f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }
    }

    private void ChasePlayer()
    {
        if (player == null) return;

        MoveTowards(player.position, chaseSpeed);
    }

    private void AttackPlayer()
    {

        if (!canAttack) return;

        rb.linearVelocity = Vector2.zero;
        animator.SetTrigger("Attack");

        canAttack = false;
        StartCoroutine(AttackLogic());
    }

    private IEnumerator AttackLogic()
    {
        yield return new WaitForSeconds(1.06f);

        if (player.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(attackDamage);
            Debug.Log("Enemy dealt damage after attack animation delay!");
        }

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    private void MoveTowards(Vector2 targetPosition, float speed)
    {
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;  
        rb.linearVelocity = direction * speed;

        if (direction.x > 0)
        {
            spriteRenderer.flipX = false;
        }
        else if (direction.x < 0)
        {
            spriteRenderer.flipX = true;
        }
    }


    // ---- Health Logic ----
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"Enemy took {damage} damage! Current HP: {currentHealth}");

        animator.SetTrigger("TakeDamage");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Enemy has died");

        animator.SetTrigger("Die");
        rb.linearVelocity = Vector2.zero;
        this.enabled = false;

        Destroy(gameObject, 1f);
    }

    // ---- Gizmos ----

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
