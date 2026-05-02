using UnityEngine;
using UnityEngine.AI;

public class MonsterBlock : MonoBehaviour
{
    public int health = 50;
    public int damageToPlayer = 10;
    public int coinsToDrop = 20;
    public float attackRange = 1.5f;
    public float attackCooldown = 1.0f;

    private NavMeshAgent agent;
    private Transform playerTransform;
    private PlayerStats playerStats;
    private float lastAttackTime;
    private Animator animator;
    private Monster monsterScript;
    private bool isDead = false;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        monsterScript = GetComponent<Monster>();
        
        GameObject player = GameObject.Find("Complete XR Origin Set Up Variant");
        if (player != null)
        {
            playerTransform = player.transform;
            playerStats = player.GetComponent<PlayerStats>();
        }
    }

    private void Update()
    {
        if (isDead) return;

        if (playerTransform != null && agent != null && agent.isOnNavMesh)
        {
            agent.SetDestination(playerTransform.position);

            // Update Animator if it exists
            if (animator != null)
            {
                animator.SetFloat("Speed", agent.velocity.magnitude);
            }

            // Distance-based attack
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            if (distance <= attackRange)
            {
                if (Time.time - lastAttackTime >= attackCooldown)
                {
                    AttackPlayer();
                    lastAttackTime = Time.time;
                }
            }
        }
    }

    private void AttackPlayer()
    {
        if (playerStats != null)
        {
            playerStats.TakeDamage(damageToPlayer);
            Debug.Log("Monster attacked player for " + damageToPlayer + " damage!");
            
            // Trigger attack animation if possible
            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isDead) return;
        HandleWeaponHit(collision.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDead) return;
        HandleWeaponHit(other.gameObject);
    }

    private void HandleWeaponHit(GameObject obj)
    {
        // Check if it's a weapon
        if (obj.CompareTag("Weapon") || obj.name.Contains("Sword") || obj.name.Contains("Axe") || obj.name.Contains("Dagger") || obj.name.Contains("Mace") || obj.name.Contains("Hammer") || obj.name.Contains("Spear") || obj.name.Contains("Halberd"))
        {
            int damage = 10;
            Co weaponCo = obj.GetComponentInParent<Co>();
            if (weaponCo != null)
            {
                damage = weaponCo.damages;
            }
            TakeDamage(damage);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        
        health -= damage;
        Debug.Log("MonsterBlock took " + damage + " damage. Health remaining: " + health);

        if (health > 0 && animator != null)
        {
            animator.SetTrigger("Hit");
        }

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        
        if (agent != null) agent.enabled = false;
        if (animator != null) animator.SetTrigger("Die");

        // Add coins
        ShopManager.coins += coinsToDrop;
        
        // Update ShopManager UI
        ShopManager shopManager = FindObjectOfType<ShopManager>();
        if (shopManager != null)
        {
            shopManager.DisplayNumber(ShopManager.coins);
        }

        // Save coins
        PlayerPrefs.SetInt("SavedCoins", ShopManager.coins);
        PlayerPrefs.Save();

        Debug.Log("MonsterBlock died! Dropped " + coinsToDrop + " coins. Total coins: " + ShopManager.coins);

        // Trigger the Monster script's ragdoll/death if it exists
        if (monsterScript != null)
        {
            monsterScript.TakeDamage(9999); // Force the Monster script to die and ragdoll
            // The Monster script will handle destroying the object after 10 seconds
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
