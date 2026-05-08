using UnityEngine;
using System;

/// <summary>
/// Detects VR physical gestures: Block (both controllers raised in front) 
/// and Push (both controllers thrust forward quickly).
/// Auto-finds controller and head transforms from the XR rig if not assigned.
/// </summary>
public class VRGestureDetector : MonoBehaviour
{
    [Header("Controller References (auto-found if empty)")]
    public Transform leftController;
    public Transform rightController;
    public Transform headTransform;

    [Header("Block Settings")]
    [Tooltip("Max distance between controllers to count as blocking")]
    public float blockDistanceThreshold = 0.45f;
    [Tooltip("How high controllers must be relative to head (negative = below head)")]
    public float blockHeightThreshold = -0.15f;
    [Tooltip("How long the block state is held after gesture is detected (seconds)")]
    public float blockHoldDuration = 1.5f;

    [Header("Push Settings")]
    [Tooltip("Minimum speed (m/s) both controllers must move forward to trigger push")]
    public float pushVelocityThreshold = 0.8f;
    [Tooltip("How aligned the push must be with head forward (0=any, 1=exact)")]
    public float pushDirectionTolerance = 0.55f;
    [Tooltip("Cooldown between push detections (seconds)")]
    public float pushCooldown = 1.0f;

    // Events other scripts subscribe to
    public event Action OnBlockStart;
    public event Action OnBlockEnd;
    public event Action OnPushDetected;

    // Public read-only state
    public bool IsBlocking { get; private set; }

    private Vector3 lastLeftPos;
    private Vector3 lastRightPos;
    private Vector3 leftVelocity;
    private Vector3 rightVelocity;

    private float blockHoldTimer;
    private float pushCooldownTimer;

    void Start()
    {
        AutoFindReferences();
        if (leftController != null)  lastLeftPos  = leftController.position;
        if (rightController != null) lastRightPos = rightController.position;
    }

    /// <summary>
    /// Tries to find Left Controller, Right Controller and Main Camera
    /// from the XR rig automatically.
    /// </summary>
    void AutoFindReferences()
    {
        if (leftController == null)
        {
            GameObject lc = GameObject.Find("Left Controller");
            if (lc != null) leftController = lc.transform;
        }
        if (rightController == null)
        {
            GameObject rc = GameObject.Find("Right Controller");
            if (rc != null) rightController = rc.transform;
        }
        if (headTransform == null)
        {
            Camera cam = Camera.main;
            if (cam != null) headTransform = cam.transform;
        }

        if (leftController == null || rightController == null || headTransform == null)
            Debug.LogWarning("[VRGestureDetector] Could not auto-find all references. Please assign manually.");
        else
            Debug.Log("[VRGestureDetector] References found automatically.");
    }

    void Update()
    {
        if (leftController == null || rightController == null || headTransform == null) return;

        // Calculate per-frame velocities
        leftVelocity  = (leftController.position  - lastLeftPos)  / Time.deltaTime;
        rightVelocity = (rightController.position - lastRightPos) / Time.deltaTime;
        lastLeftPos   = leftController.position;
        lastRightPos  = rightController.position;

        HandleBlock();
        HandlePush();
    }

    void HandleBlock()
    {
        bool poseIsBlock = CheckBlockPose();

        if (poseIsBlock)
        {
            blockHoldTimer = blockHoldDuration; // reset hold timer while pose is held
            if (!IsBlocking)
            {
                IsBlocking = true;
                OnBlockStart?.Invoke();
                Debug.Log("[VRGestureDetector] BLOCK START");
            }
        }
        else if (IsBlocking)
        {
            blockHoldTimer -= Time.deltaTime;
            if (blockHoldTimer <= 0f)
            {
                IsBlocking = false;
                OnBlockEnd?.Invoke();
                Debug.Log("[VRGestureDetector] BLOCK END");
            }
        }
    }

    void HandlePush()
    {
        if (pushCooldownTimer > 0f)
        {
            pushCooldownTimer -= Time.deltaTime;
            return;
        }

        if (CheckPushGesture())
        {
            pushCooldownTimer = pushCooldown;
            OnPushDetected?.Invoke();
            Debug.Log("[VRGestureDetector] PUSH DETECTED");
        }
    }

    bool CheckBlockPose()
    {
        // Controllers must be close together
        float dist = Vector3.Distance(leftController.position, rightController.position);
        if (dist > blockDistanceThreshold) return false;

        // Both controllers must be at or above the threshold height relative to head
        float leftHeight  = leftController.position.y  - headTransform.position.y;
        float rightHeight = rightController.position.y - headTransform.position.y;
        if (leftHeight < blockHeightThreshold || rightHeight < blockHeightThreshold) return false;

        // Both controllers must be in front of the head
        Vector3 headForward = headTransform.forward;
        Vector3 leftDir  = (leftController.position  - headTransform.position).normalized;
        Vector3 rightDir = (rightController.position - headTransform.position).normalized;
        if (Vector3.Dot(headForward, leftDir)  < 0.2f) return false;
        if (Vector3.Dot(headForward, rightDir) < 0.2f) return false;

        return true;
    }

    bool CheckPushGesture()
    {
        Vector3 headForward = headTransform.forward;

        float leftDot  = Vector3.Dot(leftVelocity.normalized,  headForward);
        float rightDot = Vector3.Dot(rightVelocity.normalized, headForward);

        bool directionOk = leftDot  > pushDirectionTolerance &&
                           rightDot > pushDirectionTolerance;
        bool speedOk     = leftVelocity.magnitude  > pushVelocityThreshold &&
                           rightVelocity.magnitude > pushVelocityThreshold;

        return directionOk && speedOk;
    }
}
