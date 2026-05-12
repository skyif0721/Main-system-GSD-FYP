using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputEvents
{
    public InputEventContext inputEventContext {  get; private set; } = InputEventContext.DEFAULT;

    public void ChangeInputEventContext(InputEventContext newContext)
    {
        Debug.Log($"InputEvents: context {inputEventContext} -> {newContext}");
        this.inputEventContext = newContext;
    }

    public event Action<InputEventContext> onInteractPressed;
    public void InteractPressed()
    {
        if (onInteractPressed != null)
        {
            onInteractPressed(this.inputEventContext);
        }
    }

    public event Action<Vector2> onMovePressed;
    public void MovePressed(Vector2 MoveDir)
    {
        if (onMovePressed != null)
        {
            onMovePressed(MoveDir);
        }
    }

    public event Action onEnablePlayerMovement;
    public void EnablePlayerMovement()
    {
        if (onEnablePlayerMovement != null)
        {
            onEnablePlayerMovement();
        }
    }

    public event Action onDisablePlayerMovement;
    public void DisablePlayerMovement()
    {
        if (onDisablePlayerMovement != null)
        {
            onDisablePlayerMovement();
        }
    }

    public event Action<InputEventContext> onOpenShopPressed;

    public void OpenShopPressed()
    {
        if (onOpenShopPressed != null)
        {
            onOpenShopPressed(this.inputEventContext);
        }
    }
}
