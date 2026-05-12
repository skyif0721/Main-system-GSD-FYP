using UnityEngine;
using UnityEngine.UI;

public class HealthHPbar : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider slider;          // Assign in Inspector if possible

    [Header("Target")]
    [SerializeField] private GameObject monsterObject;  // Assign in Inspector if possible
    private MonsterStat monsterStat;

    private int health;

    void Awake()
    {
        // Fallback: try to find a Slider on this object or its children
        if (slider == null)
        {
            slider = GetComponent<Slider>();
            if (slider == null)
                slider = GetComponentInChildren<Slider>(true);
        }
    }

    void Start()
    {
        // If not assigned, try to find the monster by tag (set your monster¡¦s tag to "Monster")
        if (monsterObject == null)
        {
            monsterObject = GameObject.FindGameObjectWithTag("Monster");
        }

        if (monsterObject != null)
        {
            monsterStat = monsterObject.GetComponent<MonsterStat>();
            if (monsterStat == null)
            {
                Debug.LogError("MonsterStat component not found on: " + monsterObject.name);
            }
        }
        else
        {
            Debug.LogError("Monster GameObject not assigned or not found!");
        }

        if (slider == null)
        {
            Debug.LogError("Slider reference is missing on " + name);
        }
        else
        {
            // Initialize slider range (ideally from monsterStat.maxHealth)
            slider.minValue = 0;
            slider.maxValue = monsterStat != null && 100 > 0 ? 100 : 100;
        }

        UpdateHealth();
    }

    void Update()
    {
        UpdateHealth();
    }

    void UpdateHealth()
    {
        if (monsterStat == null)
        {
            // Optional: try to recover if monster spawns later
            if (monsterObject != null)
                monsterStat = monsterObject.GetComponent<MonsterStat>();

            if (monsterStat == null)
            {
                // Avoid spamming every frame if desired
                // Debug.LogWarning("MonsterStat not available yet.");
                return;
            }
        }

        if (slider == null)
        {
            // Attempt late lookup once
            slider = GetComponent<Slider>() ?? GetComponentInChildren<Slider>(true);
            if (slider == null)
            {
                // Avoid NRE
                return;
            }
        }

        health = monsterStat.health;
        slider.value = health;
    }

    // Optional helper to set the target dynamically
    public void SetTarget(GameObject target)
    {
        monsterObject = target;
        monsterStat = target ? target.GetComponent<MonsterStat>() : null;
        UpdateHealth();
    }
}