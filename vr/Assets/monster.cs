using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class monster : MonoBehaviour
{
    private Rigidbody[] _ragdollRigidbodies;
    public GameObject targetObject;
    public float targetTime = 1f;
    private bool isRagdollEnabled = false;
    [SerializeField]private float health = 100;
    

    private void Awake()
    {
        _ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
        DisableRagdoll();

    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void DisableRagdoll() {
        foreach (var rigidbody in _ragdollRigidbodies) { 
            rigidbody.isKinematic = true;
        }
    }

    private void EnableRagdoll()
    {
        foreach (var rigidbody in _ragdollRigidbodies)
        {
            rigidbody.isKinematic = false;
        }
        isRagdollEnabled = true;
    }


    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0 && !isRagdollEnabled)
        {
            EnableRagdoll();


            Destroy(targetObject, 5f);
        }
    }



}
