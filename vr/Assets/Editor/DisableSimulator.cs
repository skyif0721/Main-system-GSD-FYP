using UnityEngine;
using UnityEditor;

public class DisableSimulator
{
    public static void Execute()
    {
        GameObject sim = GameObject.Find("XR Interaction Simulator");
        if (sim != null)
        {
            sim.SetActive(false);
            EditorUtility.SetDirty(sim);
            Debug.Log("Disabled XR Interaction Simulator");
        }
        else
        {
            Debug.Log("XR Interaction Simulator not found");
        }
    }
}