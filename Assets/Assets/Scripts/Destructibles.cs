
using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;

public class Destructibles : MonoBehaviour, IDamageable
{
    private int maxHealth = 3;
    private int currentHealth;

    [SerializeField] private int staminaRestore = 15;
    [SerializeField] private int healthRestore = 20;
    [SerializeField, Range(0f, 1f)] private float staminaDropChance = 0.5f;
    [SerializeField, Range(0f, 1f)] private float healthDropChance = 0.5f;

    [SerializeField] private Transform player;
    private SpriteRenderer spriterenderer;

    void Start()
    {
        currentHealth = maxHealth;
        spriterenderer = GetComponent<SpriteRenderer>();

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("Object Destroyed! ");

        if (currentHealth <= 0)
        {
            HandleDrops();
            Die();
        }
    }
    private void HandleDrops()
    {
        if (player.TryGetComponent<PlayerController>(out PlayerController playerController))
        {

            // Chance to restore stamina
            if (Random.value <= staminaDropChance)
            {
                Debug.Log("Stamina restored");
                playerController.RestoreStamina(staminaRestore);
            }

            // Chance to restore health
            if (Random.value <= healthDropChance)
            {
                Debug.Log("Healht Restored");
                playerController.RestoreHealth(healthRestore);
            }
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}
