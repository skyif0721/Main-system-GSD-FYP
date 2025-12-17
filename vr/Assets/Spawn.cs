using UnityEngine;
using System.Collections;

public class Spawn : MonoBehaviour
{
    public GameObject item;
    public float rate = 0.5f;
    public int maxItems = 10;
    public int currentItems = 0;

    void Start()
    {
        if (maxItems > 0)
        {
            InvokeRepeating("Duplicate", 0f, rate);
        }
    }

    void Duplicate()
    {
        // Stop if we've reached max items
        if (currentItems >= maxItems)
        {
            CancelInvoke("Duplicate");
            return;
        }

        GameObject clone = Instantiate(item, transform.position, transform.rotation) as GameObject;
        Physics.IgnoreCollision(clone.GetComponent<Collider>(), GetComponent<Collider>());
        currentItems++;
    }
}