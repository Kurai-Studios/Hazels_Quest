using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, IDamageable
{
    [Header ("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private SpriteRenderer spriteRenderer;

    private Animator animator;

    [Space(5)]
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;
    public HealthBar healthBar;

    [Space(5)]
    [Header("Stamina Settings")]
    public int maxStamina = 75;
    public int currentStamina;
    public StaminaBar staminaBar;

    [Space(5)]
    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 1f;
    [SerializeField] private float dashCD = 2f;
    [SerializeField] private int dashCost = 25;

    private bool canDash = true;
    private bool isDashing = false;

    private PlayerInput playerInput;

    [Space(5)]
    [Header("Attack Settings")]
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private LayerMask enemyLayer;

    private bool canAttack = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        // ---- Health ----

        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);

        // ---- Stamina ----

        currentStamina = maxStamina;
        staminaBar.SetMaxstamina(maxStamina);
        
    }


    void Update()
    {
        // ---- Movement ---- [linearVelocity = velocity]

        if (!isDashing)
        {
            rb.linearVelocity = moveInput * moveSpeed;
        }

        // ---- Flip Sprite ----

        if (moveInput.x > 0)
        {
            spriteRenderer.flipX = false;
        }
        else if (moveInput.x < 0)
        {
            spriteRenderer.flipX = true;
        }
    }

    // ---- Dash Logic ----
   public void Dash(InputAction.CallbackContext context)
    {
        if (context.started && canDash && currentStamina >= dashCost)
        {
            Debug.Log("Dash Key pressed!");
            StartCoroutine(DashCoroutine());
        }
    }

    public void RestoreStamina(int amount)
    {
        currentStamina = Mathf.Min(currentStamina + amount, maxStamina);
        staminaBar.SetStamina(currentStamina);
        Debug.Log($"Stamina Restored by {amount}. Current stamina {currentStamina}");
    }
    public void RestoreHealth(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        healthBar.SetHealth(currentHealth);
        Debug.Log($"Health Restored by {amount}. Current helath {currentHealth}");
    }

    private IEnumerator DashCoroutine()
    {
        isDashing = true;
        canDash = false;
        currentStamina -= dashCost;
        staminaBar.SetStamina(currentStamina);

        animator.SetTrigger("isRolling");

        Vector2 dashDirection = moveInput.normalized;
        float elapsedTime = 0f;

        while (elapsedTime < dashDuration)
        {
            rb.linearVelocity = dashDirection * dashSpeed;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(0.1f);
        animator.ResetTrigger("isRolling");
        isDashing = false;

        yield return new WaitForSeconds(dashCD);

        canDash = true;
        
    }

    // ---- Movement Logic ----
    public void Move (InputAction.CallbackContext context)
    {
        animator.SetBool("isWalking", true);

        if (context.canceled)
        {
            animator.SetBool("isWalking", false);
            animator.SetFloat("LastInputX", moveInput.x);
            animator.SetFloat("LastInputY", moveInput.y);
        }

        moveInput = context.ReadValue<Vector2>();
        animator.SetFloat("InputX", moveInput.x);
        animator.SetFloat("InputY", moveInput.y);
    }

    // ---- Attack Logic ----

    public void Attack(InputAction.CallbackContext context)
    {
        if (context.started && canAttack)
        {
            string attackAnimation = GetAttackAnimation();
            animator.SetTrigger(attackAnimation);
            DealDamage();

            canAttack = false;
            StartCoroutine(ResetAttackCooldown());
        }
    }

    private void DealDamage()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.TryGetComponent<IDamageable>(out IDamageable damageable))
            {
                damageable.TakeDamage(attackDamage);
            }
        }
    }

    private string GetAttackAnimation()
    {
        if (moveInput.y > 0) return "Attack_Up";
        if (moveInput.y < 0) return "Attack_Down";
        if (moveInput.x > 0) return "Attack_Right";
        if (moveInput.x < 0) return "Attack_Left";

        return "Attack_Down";
    }

    private IEnumerator ResetAttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    // ---- Health Logic ----
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        healthBar.SetHealth(currentHealth);
        animator.SetTrigger("TakeDamage");
        Debug.Log($"Player took {damage} damage! Current HP: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Player has died");
        Destroy(gameObject, 2f);
    }

    // ---- Gizmos ----
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
        {
            return;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
