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

    private XRIDefaultInputActions inputActions;

    bool inDialog = false;

    private bool playerIsNear;

    private void Awake()
    {
        inputActions = new XRIDefaultInputActions();
        talkableMark.SetActive(false);
        inDialog = false;
    }

    private void Start()
    {
        inDialog = false;
        playerIsNear = false;
    }

    private void OnEnable()
    {
        if (GameEventsManager.instance?.inputEvents != null)
        {
            GameEventsManager.instance.inputEvents.onInteractPressed += InteractPressed;
        }
    }

    private void OnDisable()
    {
        if (GameEventsManager.instance?.inputEvents != null)
        {
            GameEventsManager.instance.inputEvents.onInteractPressed -= InteractPressed;
        }
    }

    public void InteractButton()
    {
        if (!inDialog)
        {
            InteractPressed(InputEventContext.DEFAULT);
        }
    }

    private void InteractPressed(InputEventContext inputEventContext)
    {
        if (!playerIsNear)
        {
            Debug.Log("NPCEvent: !playerIsNear");
            return;
        }

        if (!inputEventContext.Equals(InputEventContext.DEFAULT))
        {
            Debug.Log("NPCEvent: inputEventContext != DEFAULT)");
            return;
        }

        if (!string.IsNullOrEmpty(dialogueKnotName))
        {
            inDialog = true;
            Debug.Log($"NPCEvent: dialogueKnotNameIs {dialogueKnotName}");
            GameEventsManager.instance.dialogueEvents.EnterDialogue(dialogueKnotName);
        }
        else
        {
            Debug.Log("NPCEvent: dialogueKnotNameIsNull");
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
            inDialog = false;
        }
    }
}