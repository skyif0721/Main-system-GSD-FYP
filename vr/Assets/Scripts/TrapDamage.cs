using UnityEngine;

public class TrapDamage : MonoBehaviour
{
    public int damage = 20;
    private bool hasDamaged = false;

    void OnTriggerEnter(Collider other)
    {
        if (hasDamaged) return;

        if (other.CompareTag("Player") || other.name.Contains("XR Origin"))
        {
            PlayerStats stats = other.GetComponentInParent<PlayerStats>();
            if (stats != null)
            {
                stats.TakeDamage(damage);
                hasDamaged = true;
                Debug.Log("Player hit by boss trap!");
            }
        }
    }
}
