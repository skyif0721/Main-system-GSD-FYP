using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;

public class CheckInputSystem
{
    public static void Execute()
    {
        Debug.Log("Input System Active: " + InputSystem.settings.updateMode);
        
        var devices = InputSystem.devices;
        Debug.Log("Connected Devices:");
        foreach (var device in devices)
        {
            Debug.Log("- " + device.name + " (" + device.layout + ")");
        }
    }
}