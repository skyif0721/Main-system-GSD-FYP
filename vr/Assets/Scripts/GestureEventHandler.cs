using UnityEngine;
using UnityEngine.UI;

public class GestureEventHandler : MonoBehaviour
{
    public Text outputText;

    public void OnGestureRecognized(string gestureName)
    {
        if (outputText != null)
        {
            outputText.text = "Recognized: " + gestureName;
        }
        Debug.Log("Recognized: " + gestureName);
    }
}
