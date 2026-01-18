using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Collider))]
public class NPCEvent : MonoBehaviour
{
    [Header("Dialogue (optional)")]
    [SerializeField] private string dialogueKnotName;

    [SerializeField] private GameObject talkableMark;

    private @XRIDefaultInputActions inputActions;

    private bool playerIsNear = false;

    private void Awake()
    {
        inputActions = new @XRIDefaultInputActions();
        talkableMark.SetActive(false);
    }

    private void OnEnable()
    {
        GameEventsManager.instance.inputEvents.onInteractPressed += InteractPressed;
    }

    private void OnDisable()
    {
        GameEventsManager.instance.inputEvents.onInteractPressed -= InteractPressed;
    }

    private void InteractPressed(InputEventContext inputEventContext)
    {
        if (!playerIsNear || !inputEventContext.Equals(InputEventContext.DEFAULT))
        {
            return;
        }

        if (!dialogueKnotName.Equals(""))
        {
            GameEventsManager.instance.dialogueEvents.EnterDialogue(dialogueKnotName);
        }
        else
        {
            GameEventsManager.instance.inputEvents.InteractPressed();
        }
    }

    private void OnTriggerEnter(Collider otherCollider)
    {
        if (otherCollider.CompareTag("Player"))
        {
            playerIsNear = true;
            talkableMark.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider otherCollider)
    {
        if (otherCollider.CompareTag("Player"))
        {
            playerIsNear = false;
            talkableMark.SetActive(false);
        }
    }

}
