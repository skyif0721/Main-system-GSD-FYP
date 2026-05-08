using GLTFast.Schema;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class HealthHPbar: MonoBehaviour
{
    public Slider slider;
    private int health;
    Text text;

    // Add a reference to your monster GameObject
    public GameObject monsterObject; // Drag your monster GameObject here in Inspector
    private MonsterStat MonsterStat; 




    void UpdateHealth()
    {
        // Check if we have a reference to the monster script
        if (MonsterStat != null)
        {
            health = MonsterStat.health;
            slider.value = health;
            // Debug.Log("Health in Timer: " + health);
        }
        else
        {
            Debug.LogWarning("Monster script not found!");
        }
    }

    void Start()
    {
        // Get the Text component from this GameObject
        text = GetComponent<Text>();
        MonsterStat = GetComponent<MonsterStat>();

        // Try to find the monster if not assigned
        if (monsterObject == null)
        {
            // Look for any GameObject with "Monster" in its name
            monsterObject = GameObject.Find("Monster"); // Change "Monster" to your monster's name
        }

        // If we found the monster GameObject, get its Monster component
        if (monsterObject != null)
        {

            if (MonsterStat == null)
            {
                Debug.LogError("Found monster GameObject but no Monster script attached!");
            }
        }
        else
        {
            Debug.LogError("Could not find monster GameObject!");
        }

        // Initialize health if we have the script
        if (MonsterStat != null)
        {
            UpdateHealth();
            slider.maxValue = 100;
            slider.minValue = 0;
        }
    }

    void Update()
    {
        // Update health every frame

            UpdateHealth();
            


    }
}

