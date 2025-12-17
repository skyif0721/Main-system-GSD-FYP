using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class co : MonoBehaviour
{
    public int damages = 10;
    public int Totaldamage = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

        private void OnCollisionEnter(Collision collision)
    {
       // Debug.Log("Collided with " + collision.gameObject.name);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Triggered by " + other.gameObject.name);
        if (other.gameObject.CompareTag("monster"))
        {
            monster monsterScript = other.gameObject.GetComponent<monster>();

            
                monsterScript.TakeDamage(damages); // Inflict 50 damage on the monster

            Totaldamage = Totaldamage + damages;



        }
    }
}
