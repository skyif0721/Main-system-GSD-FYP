using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
public class HMDInfoManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (!XRSettings.isDeviceActive)
        {
            Debug.Log("No Headset plugged");
        }
        else if (XRSettings.isDeviceActive && (XRSettings.loadedDeviceName ==
            "Mock HMD" || XRSettings.loadedDeviceName == "MockHMDDisplay")) 
            //check old & new toolkit name
        {
            Debug.Log("using Mock HMD");
        } 
        else
        {
            Debug.Log("Our VR device is " + XRSettings.loadedDeviceName);
        }
    }

    // Update is called once per frame Page 7 | 48
    void Update()
    {

    }
}