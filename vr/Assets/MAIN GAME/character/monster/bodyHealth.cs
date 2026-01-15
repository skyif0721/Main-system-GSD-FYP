
using UnityEngine;

public class bodyHealth : MonoBehaviour
{
    public int health;


    // Start is called before the first frame update
    void Start()
    {
        health = 100;
    }

    // Update is called once per frame
    void Update()
    {
        healthdisplay();
       if (health <= 0)
        {
            Destroy(gameObject);
            Debug.Log("dead!" + health);
        }
    }

    void OnTriggerEnter(Collider gameObject)
    {
        if (gameObject.CompareTag("Destroyer"))
        {
            Debug.Log("±»´¥·¢Æ÷»÷ÖÐ£¡");
        }
    }

    void healthdisplay() {
        Debug.Log("health:" + health);
    }
}

