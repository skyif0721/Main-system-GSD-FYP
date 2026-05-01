using MiVRy;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    public string gestureName = "";
    public TrailRenderer gestureTrail; // Drag your trail object here in the Inspector

    // Call this when the user presses the trigger to start drawing
    public void StartDrawing()
    {
        gestureTrail.Clear(); // Wipe old lines
        gestureTrail.emitting = true;
    }

    // Call this when the user releases the trigger
    public void StopDrawing()
    {
        gestureTrail.emitting = false;
    }

    public void OnGestureCompleted(GestureCompletionData data)
    {
        // Turn off the trail when the gesture is recognized
        StopDrawing();

        if (data.gestureID >= 1 && data.gestureID <= 4)
        {
            Debug.Log($"Gesture {data.gestureID} recognized: {data.gestureName}");
            gestureName = data.gestureName;
        }
    }
}