using UnityEngine;
using UnityEngine.InputSystem;

public class NewbieMovement : MonoBehaviour
{
    public Transform xrOrigin;
    public Transform mainCamera;
    private Animator animator;
    
    // Reference to the left controller move action
    public InputActionReference moveAction;

    private Vector3 previousPosition;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (xrOrigin != null)
        {
            previousPosition = xrOrigin.position;
        }
        
        // Try to find the default move action if not assigned
        if (moveAction == null)
        {
            // This is a common path for the default XRI input actions
            var inputActionAsset = Resources.Load<InputActionAsset>("XRI Default Input Actions");
            if (inputActionAsset == null)
            {
                // Try to find it in the scene
                var playerInput = FindObjectOfType<UnityEngine.InputSystem.PlayerInput>();
                if (playerInput != null)
                {
                    inputActionAsset = playerInput.actions;
                }
            }
            
            if (inputActionAsset != null)
            {
                var action = inputActionAsset.FindAction("XRI LeftHand Locomotion/Move");
                if (action != null)
                {
                    moveAction = InputActionReference.Create(action);
                }
            }
        }
    }

    void Update()
    {
        if (animator == null || xrOrigin == null || mainCamera == null) return;

        // 1. Update position to follow the camera (but stay on the ground)
        Vector3 targetPosition = mainCamera.position;
        targetPosition.y = xrOrigin.position.y; // Keep it at the origin's floor level
        transform.position = targetPosition;

        // 2. Update rotation to face the camera's forward direction (yaw only)
        Vector3 cameraForward = mainCamera.forward;
        cameraForward.y = 0;
        if (cameraForward.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(cameraForward);
        }

        // 3. Calculate movement for animation
        Vector2 input = Vector2.zero;
        
        if (moveAction != null && moveAction.action != null)
        {
            input = moveAction.action.ReadValue<Vector2>();
        }
        else
        {
            // Fallback to calculating velocity based on position change
            Vector3 velocity = (xrOrigin.position - previousPosition) / Time.deltaTime;
            previousPosition = xrOrigin.position;

            // Convert world velocity to local velocity relative to the camera's facing direction
            Vector3 localVelocity = transform.InverseTransformDirection(velocity);
            
            // Normalize to -1 to 1 range (assuming max speed is around 2-3 m/s)
            float maxSpeed = 2.5f;
            input.x = Mathf.Clamp(localVelocity.x / maxSpeed, -1f, 1f);
            input.y = Mathf.Clamp(localVelocity.z / maxSpeed, -1f, 1f);
        }

        // Update animator parameters
        animator.SetFloat("Forward", input.y);
        animator.SetFloat("Right", input.x);
    }
}
