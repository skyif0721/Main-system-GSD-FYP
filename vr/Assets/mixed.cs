using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class mixed : MonoBehaviour
{
    public GameObject targetObject;  // 要跟随的物体
    public Transform targetTransform;
    public GameObject bullet;
    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        Shoot();
    }

    void Shoot() {

            GameObject Clonebullet= Instantiate(bullet, targetTransform.position, targetTransform.rotation);    
            Clonebullet.GetComponent<Rigidbody>().velocity = targetTransform.forward * 10;  
            Destroy(Clonebullet, 2.0f); 

        
    }
}
