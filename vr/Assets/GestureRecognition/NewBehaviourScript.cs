using MiVRy;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnGestureCompleted(GestureCompletionData data)
    {
        if (data.gestureID == 123)
        {
            Debug.Log("Gesture 123 completed with confidence ");
        }
    }
}
