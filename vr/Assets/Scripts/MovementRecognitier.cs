using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.Events;
using PDollarGestureRecognizer;

/// <summary>
/// Records the controller's path while a button is held, then runs it through
/// the PDollar $1 / $P gesture recognizer.
///
/// Updated for Unity 6 / XRI 3.x:
///   - Replaced obsolete <c>InputHelpers</c> calls with the modern
///     <c>InputDevice.TryGetFeatureValue</c> API.
///   - Hardened <c>EndMovement</c> against missing <c>Camera.main</c> and
///     against not-enough-points clouds (which used to NRE inside PDollar).
///   - The fired gesture is also forwarded to <see cref="GestureActionHandler"/>
///     so trained PDollar gestures play the same combat actions as the
///     pose-based detector.
/// </summary>
public class MovementRecognizer : MonoBehaviour
{
    [Header("Input")]
    public XRNode inputSource = XRNode.RightHand;
    [Tooltip("Which controller button drives recording. Trigger or Grip recommended.")]
    public TriggerButton inputButton = TriggerButton.Trigger;
    [Range(0.01f, 1f)] public float inputThreshold = 0.1f;

    public Transform movementSource;

    [Header("Recording")]
    public float newPositionThresholdDistance = 0.05f;
    public GameObject debugCubePrefab;

    [Header("Mode")]
    [Tooltip("If TRUE, finished gestures are saved as new training samples (.xml) " +
             "named <newGestureName>. If FALSE, finished gestures are RECOGNIZED.")]
    public bool   creationMode    = false;
    public string newGestureName  = "untitled";
    [Range(0.5f, 1f)] public float recognitionThreshold = 0.9f;

    [Header("Output")]
    [Tooltip("Optional handler – if set, recognized gestures are dispatched to it. " +
             "If null, the script will FindObjectOfType<GestureActionHandler> at runtime.")]
    public GestureActionHandler actionHandler;

    [System.Serializable]
    public class UnityStringEvent : UnityEvent<string> { }
    public UnityStringEvent OnRecongnized;

    public enum TriggerButton { Trigger, Grip, PrimaryButton, SecondaryButton }

    // ─────────────────────────────────────────────────────────────────────
    private readonly List<Gesture> _trainingSet = new List<Gesture>();
    private readonly List<Vector3> _positions   = new List<Vector3>();
    private bool _isMoving;

    void Start()
    {
        // Load all trained gesture XMLs from the persistent data path.
        if (Directory.Exists(Application.persistentDataPath))
        {
            string[] files = Directory.GetFiles(Application.persistentDataPath, "*.xml");
            foreach (string file in files)
            {
                try { _trainingSet.Add(GestureIO.ReadGestureFromFile(file)); }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[MovementRecognizer] Failed to load gesture '{file}': {ex.Message}");
                }
            }
            Debug.Log($"[MovementRecognizer] Loaded {_trainingSet.Count} trained gestures.");
        }

        if (movementSource == null) movementSource = transform;
        if (actionHandler  == null) actionHandler  = FindObjectOfType<GestureActionHandler>();
    }

    void Update()
    {
        bool isPressed = ReadButton();

        if (!_isMoving && isPressed)        StartMovement();
        else if (_isMoving && !isPressed)   EndMovement();
        else if (_isMoving &&  isPressed)   UpdateMovement();
    }

    // ─────────────────────────────────────────────────────────────────────
    bool ReadButton()
    {
        InputDevice device = InputDevices.GetDeviceAtXRNode(inputSource);
        if (!device.isValid) return false;

        switch (inputButton)
        {
            case TriggerButton.Trigger:
                if (device.TryGetFeatureValue(CommonUsages.trigger, out float t))
                    return t >= inputThreshold;
                break;
            case TriggerButton.Grip:
                if (device.TryGetFeatureValue(CommonUsages.grip, out float g))
                    return g >= inputThreshold;
                break;
            case TriggerButton.PrimaryButton:
                if (device.TryGetFeatureValue(CommonUsages.primaryButton, out bool pb))
                    return pb;
                break;
            case TriggerButton.SecondaryButton:
                if (device.TryGetFeatureValue(CommonUsages.secondaryButton, out bool sb))
                    return sb;
                break;
        }
        return false;
    }

    // ─────────────────────────────────────────────────────────────────────
    void StartMovement()
    {
        _isMoving = true;
        _positions.Clear();
        _positions.Add(movementSource.position);

        if (debugCubePrefab != null)
            Destroy(Instantiate(debugCubePrefab, movementSource.position, Quaternion.identity), 3f);

        Debug.Log("[MovementRecognizer] Start movement");
    }

    void UpdateMovement()
    {
        Vector3 last = _positions[_positions.Count - 1];
        if (Vector3.Distance(movementSource.position, last) > newPositionThresholdDistance)
        {
            _positions.Add(movementSource.position);
            if (debugCubePrefab != null)
                Destroy(Instantiate(debugCubePrefab, movementSource.position, Quaternion.identity), 3f);
        }
    }

    void EndMovement()
    {
        _isMoving = false;
        Debug.Log($"[MovementRecognizer] End movement, samples={_positions.Count}");

        // Need a few points to form a meaningful gesture
        if (_positions.Count < 5)
        {
            Debug.Log("[MovementRecognizer] Too few points, ignoring.");
            return;
        }

        // Convert world positions to screen-space points (PDollar expects 2D)
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("[MovementRecognizer] No Camera.main – cannot recognize gesture.");
            return;
        }

        Point[] points = new Point[_positions.Count];
        for (int i = 0; i < _positions.Count; i++)
        {
            Vector2 sp = cam.WorldToScreenPoint(_positions[i]);
            points[i] = new Point(sp.x, sp.y, 0);
        }

        Gesture gesture = new Gesture(points);

        if (creationMode)
        {
            gesture.Name = newGestureName;
            _trainingSet.Add(gesture);

            string path = Path.Combine(Application.persistentDataPath, newGestureName + ".xml");
            try
            {
                GestureIO.WriteGesture(points, newGestureName, path);
                Debug.Log($"[MovementRecognizer] Saved new gesture '{newGestureName}' to {path}");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[MovementRecognizer] Failed to save gesture: {ex.Message}");
            }
            return;
        }

        // ── Recognize ──
        if (_trainingSet.Count == 0)
        {
            Debug.LogWarning("[MovementRecognizer] No training set loaded – nothing to recognize against.");
            return;
        }

        Result result;
        try
        {
            result = PointCloudRecognizer.Classify(gesture, _trainingSet.ToArray());
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[MovementRecognizer] Classify failed: {ex.Message}");
            return;
        }

        Debug.Log($"[MovementRecognizer] Recognized: '{result.GestureClass}' score={result.Score:F2}");

        if (result.Score >= recognitionThreshold && !string.IsNullOrEmpty(result.GestureClass))
        {
            OnRecongnized?.Invoke(result.GestureClass);

            // Forward to the central GestureActionHandler so the PDollar path
            // produces the same effects as the pose-based detector.
            if (actionHandler != null)
                actionHandler.HandleGestureByName(result.GestureClass);
        }
    }
}
