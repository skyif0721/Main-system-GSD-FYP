using UnityEngine;

public class Monster : MonoBehaviour
{
    private Rigidbody[] _ragdollRigidbodies;
    public GameObject targetObject;
    public float targetTime = 1f;
    private bool isRagdollEnabled = false;
    [SerializeField] private int health = 100;
    public int healthtemp = 0;
    public static Monster Instance;


    // Self-contained timer for destruction
    private float deathTimer = 0f;
    private bool isDead = false;

    public void UpdateHealth()
    {
        healthtemp = health;
        Debug.Log("Monster Health: " + healthtemp);
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

        // Start self-destruction
        deathTimer = 10f;

        // Optionally destroy target object
        if (targetObject != null)
        {
            Destroy(targetObject);
        }
    }

    private void DestroySelf()
    {
        // Self-contained destruction
        Destroy(gameObject);
    }
}


