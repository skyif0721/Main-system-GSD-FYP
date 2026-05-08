using Ink.Parsed;
using System.Drawing;
using UnityEngine;
using UnityEngine.UI;

public class Monster : MonoBehaviour
{
    private Rigidbody[] _ragdollRigidbodies;
    public GameObject targetObject;
    public float targetTime = 1f;
    private bool isRagdollEnabled = false;
    [SerializeField] private int health = 100;
    public int healthtemp = 0;
    public static Monster Instance;
    public int deadCount = 0;

    public GameObject shopManager;


    // Self-contained timer for destruction
    private float deathTimer = 0f;
    private bool isDead = false;

    public void UpdateHealth()
    {
        healthtemp = health;
        // Debug.Log("Monster Health: " + healthtemp);
    }

    private void Awake()
    {
        _ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
        DisableRagdoll();
        Instance = this;
    }

    private void Update()
    {
        // Self-contained timer management
        if (isDead && !isRagdollEnabled)
        {
            deathTimer += Time.deltaTime;
            if (deathTimer >= targetTime)
            {
                DestroySelf();
            }
        }
        UpdateHealth();
    }


    private void DisableRagdoll()
    {
        foreach (var rigidbody in _ragdollRigidbodies)
        {
            rigidbody.isKinematic = true;
        }
    }

    private void EnableRagdoll()
    {
        foreach (var rigidbody in _ragdollRigidbodies)
        {
            rigidbody.isKinematic = false;
        }
        isRagdollEnabled = true;
    }

    public void TakeDamage(int damage)
    {
        // Self-contained damage handling
        health -= damage;

        // Show floating damage text
        GameObject textObj = new GameObject("DamageText");
        // Position it slightly above the monster
        textObj.transform.position = transform.position + Vector3.up * 1.5f;
        FloatingDamageText damageText = textObj.AddComponent<FloatingDamageText>();
        damageText.Setup(damage);

        if (health <= 0 && !isRagdollEnabled)
        {
            Die();
        }
    }

    private void Die()
    {
        // Self-contained death handling
        EnableRagdoll();
        isDead = true;
        deadCount += 1;

        // Start self-destruction
        deathTimer = 10f;

        // Optionally destroy target object
        if (targetObject != null)
        {
            Destroy(targetObject);
        }

        // gain coin
        ShopManager.coins += 50;
        if (shopManager != null)
        {
            var sm = shopManager.GetComponent<ShopManager>();
            sm.DisplayNumber(ShopManager.coins);
        }

    }

    private void DestroySelf()
    {
        // Self-contained destruction
        Destroy(gameObject);
    }
}


