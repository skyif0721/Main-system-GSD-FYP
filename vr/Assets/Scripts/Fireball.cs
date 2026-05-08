using UnityEngine;

public class Fireball : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 50;
    public float lifetime = 5f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = transform.forward * speed;
        }
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter(Collider other)
    {
        MonsterStat monster = other.GetComponentInParent<MonsterStat>();
        if (monster != null)
        {
            monster.TakeDamage(damage);
            Debug.Log("Fireball hit boss for " + damage + " damage!");
            Destroy(gameObject);
        }
        else if (!other.CompareTag("Player") && !other.name.Contains("Controller"))
        {
            // Destroy on hitting environment
            Destroy(gameObject);
        }
    }
}
