using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UIElements;

public class GameEventsManager : MonoBehaviour
{
    public static GameEventsManager instance { get; private set; }

    public DialogueEvents dialogueEvents;

    public InputEvents inputEvents;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("There is more than one GameEventsManager in the scene.");
        }
        instance = this;

        inputEvents = new InputEvents();
        dialogueEvents = new DialogueEvents();
    }
}
