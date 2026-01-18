using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class InputManager : MonoBehaviour
{

    public void MovePressed(InputAction.CallbackContext context)
    {
        if (context.performed || context.canceled)
        {
            GameEventsManager.instance.inputEvents.MovePressed(context.ReadValue<Vector2>());
        }
    }

    public void InteractPressed(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            GameEventsManager.instance.inputEvents.InteractPressed();
        }
    }

}
