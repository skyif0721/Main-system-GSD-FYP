using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public Slider slider;
    public int health;
    Text text;

    // Add a reference to your monster GameObject
    public GameObject monsterObject; // Drag your monster GameObject here in Inspector
    private Monster monsterScript;   // This will hold the reference to the Monster component

    void UpdateHealth()
    {
        // Check if we have a reference to the monster script
        if (monsterScript != null)
        {
            health = monsterScript.healthtemp;
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

        // Try to find the monster if not assigned
        if (monsterObject == null)
        {
            // Look for any GameObject with "Monster" in its name
            monsterObject = GameObject.Find("Monster"); // Change "Monster" to your monster's name
        }

        // If we found the monster GameObject, get its Monster component
        if (monsterObject != null)
        {
            monsterScript = monsterObject.GetComponent<Monster>();

            if (monsterScript == null)
            {
                Debug.LogError("Found monster GameObject but no Monster script attached!");
            }
        }
        else
        {
            Debug.LogError("Could not find monster GameObject!");
        }

        // Initialize health if we have the script
        if (monsterScript != null)
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
            slider.value = health;


    }
}

