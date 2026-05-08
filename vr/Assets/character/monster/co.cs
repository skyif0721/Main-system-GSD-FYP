using UnityEngine;

public class WeaponMonster : MonoBehaviour  // Better to use PascalCase for class names
{
    public int damages = 10;
    public int totalDamage = 0;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Triggered by " + other.gameObject.name);

        if (other.gameObject.CompareTag("monster"))
        {
            Monster monsterScript = other.gameObject.GetComponent<Monster>();

            if (monsterScript != null)
            {
                monsterScript.TakeDamage(damages); // Inflict damage on the monster
                totalDamage = totalDamage + damages;

                // Self-contained: Optional - Destroy this object after hitting monster
                // Destroy(gameObject);
            }
        }
    }
}