using UnityEngine;
using TMPro; // Use TextMeshPro for VR UI

public class ActionUI : MonoBehaviour
{
    public NewBehaviourScript gestureLogic;
    public TextMeshProUGUI uiText; // Drag your UI Text element here

    private string lastKnownGesture = "";

    void Update()
    {
        // Only update the UI if the name has actually changed
        if (gestureLogic.gestureName != lastKnownGesture)
        {
            lastKnownGesture = gestureLogic.gestureName;
            uiText.text = "Detected: " + lastKnownGesture;
        }
    }
}