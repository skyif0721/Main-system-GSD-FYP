using Ink.Parsed;
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
        var gem = GameEventsManager.instance;
        if (gem?.inputEvents != null)
        {
            gem.inputEvents.onInteractPressed += InteractPressed;
        }
        else
        {
            Debug.LogWarning($"{name}: GameEventsManager.instance or inputEvents is null in OnEnable.");
        }
    }

    private void OnDisable()
    {
        var gem = GameEventsManager.instance;
        if (gem?.inputEvents != null)
        {
            gem.inputEvents.onInteractPressed -= InteractPressed;
        }
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
        if (otherCollider.CompareTag("Player") || otherCollider.name.Contains("XR Origin") || otherCollider.GetComponentInParent<PlayerStats>() != null)
        {
            playerIsNear = true;
            talkableMark.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider otherCollider)
    {
        if (otherCollider.CompareTag("Player") || otherCollider.name.Contains("XR Origin") || otherCollider.GetComponentInParent<PlayerStats>() != null)
        {
            playerIsNear = false;
            talkableMark.SetActive(false);
        }
    }

}
