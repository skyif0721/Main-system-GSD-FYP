using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class monster : MonoBehaviour
{
    private Rigidbody[] _ragdollRigidbodies;
    public GameObject targetObject;
    public float targetTime = 1f;
    private bool isRagdollEnabled = false;
    public int health = 2;

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
        waitTillRagdoll();
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

    private void waitTillRagdoll() {
        targetTime -= Time.deltaTime;
        if (targetTime < 0)
        {
            EnableRagdoll();
        }
    }

}
