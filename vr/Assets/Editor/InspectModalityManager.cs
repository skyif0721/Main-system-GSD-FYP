using UnityEngine;
using UnityEditor;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

public class InspectModalityManager
{
    public static void Execute()
    {
        XRInputModalityManager modality = Object.FindObjectOfType<XRInputModalityManager>();
        if (modality != null)
        {
            SerializedObject so = new SerializedObject(modality);
            SerializedProperty prop = so.GetIterator();
            prop.Next(true);
            while (prop.NextVisible(false))
            {
                Debug.Log("Property: " + prop.name + " | type: " + prop.propertyType);
            }
        }
        else
        {
            Debug.Log("XRInputModalityManager not found");
        }
    }
}
