using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Reads Grip and Trigger input values from the XRI Default Input Actions
/// and drives the Oculus hand skin Animator parameters so the hand animates
/// when the player presses buttons on the Meta Quest controller.
/// </summary>
public class HandAnimationController : MonoBehaviour
{
    public enum HandSide { Left, Right }

    [Header("Hand Side")]
    public HandSide handSide = HandSide.Left;

    [Header("Input Actions (auto-resolved from XRI Default Input Actions)")]
    [Tooltip("Leave empty to auto-find from XRI Default Input Actions asset.")]
    public InputActionReference gripAction;
    public InputActionReference triggerAction;

    [Header("Animator")]
    [Tooltip("Leave empty to auto-find on this GameObject or its children.")]
    public Animator handAnimator;

    // Animator parameter hashes
    private static readonly int GripHash    = Animator.StringToHash("Grip");
    private static readonly int TriggerHash = Animator.StringToHash("Trigger");

    // Fallback: direct action references resolved at runtime
    private InputAction _grip;
    private InputAction _trigger;

    void Awake()
    {
        // Auto-find animator
        if (handAnimator == null)
            handAnimator = GetComponentInChildren<Animator>();

        // Use assigned InputActionReferences if provided
        if (gripAction != null)
            _grip = gripAction.action;
        if (triggerAction != null)
            _trigger = triggerAction.action;

        // Otherwise find them by name in the enabled InputActionAssets
        if (_grip == null || _trigger == null)
            FindActionsFromAssets();
    }

    void FindActionsFromAssets()
    {
        string gripName    = handSide == HandSide.Left ? "XRI Left Interaction/Select Value"   : "XRI Right Interaction/Select Value";
        string triggerName = handSide == HandSide.Left ? "XRI Left Interaction/Activate Value" : "XRI Right Interaction/Activate Value";

        foreach (var asset in Resources.FindObjectsOfTypeAll<InputActionAsset>())
        {
            if (_grip == null)
            {
                var a = asset.FindAction(gripName);
                if (a != null) _grip = a;
            }
            if (_trigger == null)
            {
                var a = asset.FindAction(triggerName);
                if (a != null) _trigger = a;
            }
            if (_grip != null && _trigger != null) break;
        }

        if (_grip == null)
            Debug.LogWarning("[HandAnimationController] Could not find Grip action for " + handSide);
        if (_trigger == null)
            Debug.LogWarning("[HandAnimationController] Could not find Trigger action for " + handSide);
    }

    void OnEnable()
    {
        _grip?.Enable();
        _trigger?.Enable();
    }

    void OnDisable()
    {
        _grip?.Disable();
        _trigger?.Disable();
    }

    void Update()
    {
        if (handAnimator == null) return;

        float gripValue    = _grip    != null ? _grip.ReadValue<float>()    : 0f;
        float triggerValue = _trigger != null ? _trigger.ReadValue<float>() : 0f;

        handAnimator.SetFloat(GripHash,    gripValue);
        handAnimator.SetFloat(TriggerHash, triggerValue);
    }
}
