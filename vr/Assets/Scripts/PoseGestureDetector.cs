using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// Detects the 9 supported gestures purely from controller pose / velocity.
/// All velocities are computed RELATIVE TO THE HEAD (player's body) so
/// walking / locomotion does not trigger false positives.
/// Mutual-exclusion priority + a refractory window prevents one swing from
/// being interpreted as multiple different gestures.
///
/// Gesture priority (high → low):
///     Block  >  Konan SY  >  Qigong  >  Wrist (both)  >  Wrist (single)
///   >  Rapier (single)  >  Split (single)
///
/// "rapier" vs "split" is decided per-swing by which direction wins
/// (forward-along-controller vs downward).
/// </summary>
public class PoseGestureDetector : MonoBehaviour
{
    // ────────────────────────────────────────────────────────────────────────
    [Header("References (auto-found if empty)")]
    public Transform leftController;
    public Transform rightController;
    public Transform headTransform;
    [Tooltip("Used to detect if the corresponding hand is currently holding something.")]
    public NearFarInteractor leftSelector;
    public NearFarInteractor rightSelector;

    [Header("Output")]
    [Tooltip("If set, recognized gestures will be dispatched here. " +
             "Otherwise FindObjectOfType<GestureActionHandler>() will be used.")]
    public GestureActionHandler actionHandler;
    [Tooltip("Optional UI text/3D text mesh that displays the last recognized gesture.")]
    public TMPro.TMP_Text recognizedLabel;

    public event System.Action<string> OnGestureRecognized;

    // ────────────────────────────────────────────────────────────────────────
    [Header("Cooldowns / refractory")]
    [Tooltip("Per-gesture cooldown (s) so the same gesture won't refire in a flurry.")]
    public float perGestureCooldown = 0.6f;
    [Tooltip("After ANY gesture fires, no other gesture can fire for this many seconds. " +
             "Prevents one swing from triggering multiple moves.")]
    public float globalRefractory   = 0.35f;

    [Header("Walking suppression")]
    [Tooltip("If the head itself moves faster than this (m/s), no gesture is detected. " +
             "Stops walking / teleporting from triggering wrist / split / rapier.")]
    public float headMaxSpeedForDetection = 0.6f;

    [Header("Wrist flourish detection")]
    [Tooltip("Master switch – disable to skip wrist detection entirely.")]
    public bool  wristDetectionEnabled = false;
    [Tooltip("Required angular speed of the wrist (deg/s) over the sample window.")]
    public float wristAngularSpeed = 450f;
    [Tooltip("How long the rapid spin must be sustained, seconds.")]
    public float wristMinDuration  = 0.30f;
    [Tooltip("During wrist flourish the controller must STAY in roughly the same place. " +
             "If linear (body-relative) speed exceeds this, it is treated as a swing instead.")]
    public float wristMaxLinearSpeed = 0.8f;

    [Header("Rapier (forward thrust)")]
    [Tooltip("Body-relative forward speed (m/s) along controller forward axis.")]
    public float rapierForwardSpeed = 1.4f;
    [Tooltip("How aligned the velocity must be with controller forward (cos angle).")]
    public float rapierAlignment    = 0.75f;
    [Tooltip("Forward speed must beat downward speed by at least this factor to be rapier.")]
    public float rapierVsSplitRatio = 1.3f;

    [Header("Split (downward chop)")]
    [Tooltip("Body-relative downward speed (m/s) of the controller.")]
    public float splitDownwardSpeed = 1.4f;
    [Tooltip("Downward speed must beat forward speed by at least this factor to be split.")]
    public float splitVsRapierRatio = 1.3f;

    [Header("Qigong / Konan SY")]
    public float twoHandTogetherMaxDist = 0.45f;
    public float qigongHoldDuration = 0.7f;
    public float konanHoldDuration  = 0.5f;
    public float konanAboveHead     = 0.25f;

    [Header("Block")]
    public float blockMaxDist        = 0.45f;
    public float blockHeightVsHead   = -0.15f;
    public float blockHoldDuration   = 0.4f;

    [Header("Debug")]
    public bool debugLog = false;

    // ────────────────────────────────────────────────────────────────────────
    Vector3   _lastLeftPos, _lastRightPos, _lastHeadPos;
    Quaternion _lastLeftRot, _lastRightRot;
    float _leftSpinAccumTime, _rightSpinAccumTime;
    float _qigongHoldTime, _konanHoldTime, _blockHoldTime;
    float _globalLockUntil;
    readonly Dictionary<string, float> _lastFiredAt = new Dictionary<string, float>();
    bool _initialized;

    void Start() => AutoFind();

    void AutoFind()
    {
        if (leftController == null)
        {
            var go = GameObject.Find("Left Controller");
            if (go != null) leftController = go.transform;
        }
        if (rightController == null)
        {
            var go = GameObject.Find("Right Controller");
            if (go != null) rightController = go.transform;
        }
        if (headTransform == null && Camera.main != null)
            headTransform = Camera.main.transform;

        if (leftSelector == null && leftController != null)
            leftSelector = leftController.GetComponentInChildren<NearFarInteractor>();
        if (rightSelector == null && rightController != null)
            rightSelector = rightController.GetComponentInChildren<NearFarInteractor>();

        if (actionHandler == null)
            actionHandler = FindObjectOfType<GestureActionHandler>();

        if (recognizedLabel == null)
        {
            GameObject lbl = GameObject.Find("RecognizedText");
            if (lbl != null) recognizedLabel = lbl.GetComponent<TMPro.TMP_Text>();
        }

        if (leftController  != null) { _lastLeftPos  = leftController.position;  _lastLeftRot  = leftController.rotation; }
        if (rightController != null) { _lastRightPos = rightController.position; _lastRightRot = rightController.rotation; }
        if (headTransform   != null) { _lastHeadPos  = headTransform.position; }

        _initialized = leftController != null && rightController != null && headTransform != null;
    }

    void Update()
    {
        if (!_initialized)
        {
            AutoFind();
            if (!_initialized) return;
        }

        float dt = Mathf.Max(Time.deltaTime, 0.0001f);

        // ── Head-relative velocities ────────────────────────────────────────
        Vector3 headVel  = (headTransform.position  - _lastHeadPos)  / dt;
        Vector3 leftVelW  = (leftController.position  - _lastLeftPos)  / dt;
        Vector3 rightVelW = (rightController.position - _lastRightPos) / dt;
        // Subtract head/body translation: this isolates the actual hand motion
        Vector3 leftVel  = leftVelW  - headVel;
        Vector3 rightVel = rightVelW - headVel;

        float leftAngSpeed  = Quaternion.Angle(_lastLeftRot,  leftController.rotation)  / dt;
        float rightAngSpeed = Quaternion.Angle(_lastRightRot, rightController.rotation) / dt;

        _lastLeftPos  = leftController.position;
        _lastRightPos = rightController.position;
        _lastLeftRot  = leftController.rotation;
        _lastRightRot = rightController.rotation;
        _lastHeadPos  = headTransform.position;

        bool leftHolding  = leftSelector  != null && leftSelector.hasSelection;
        bool rightHolding = rightSelector != null && rightSelector.hasSelection;
        bool headMoving   = headVel.magnitude > headMaxSpeedForDetection;

        if (debugLog && (leftVel.magnitude > 0.4f || rightVel.magnitude > 0.4f ||
                         leftAngSpeed > 100f || rightAngSpeed > 100f))
        {
            Debug.Log($"[PoseGD] L vel={leftVel.magnitude:F2} ang={leftAngSpeed:F0}  " +
                      $"R vel={rightVel.magnitude:F2} ang={rightAngSpeed:F0}  " +
                      $"head={headVel.magnitude:F2} (walking={headMoving})  " +
                      $"hold L={leftHolding} R={rightHolding}");
        }

        // ── Hold-style gestures evaluate every frame; they ignore refractory
        //    so that block / qigong / konan can be entered at any time. ─────
        TickBlock(dt);
        TickKonanSY(dt, leftHolding, rightHolding);
        TickQigong(dt, leftHolding, rightHolding, leftVel, rightVel);

        // Swing-style gestures are gated by the global refractory + walking
        if (Time.time < _globalLockUntil) return;
        if (headMoving)                   return;

        // Wrist flourish — disabled by default (was causing too many false positives)
        if (wristDetectionEnabled)
        {
            TickWrist(dt, leftAngSpeed, rightAngSpeed,
                      leftVel.magnitude, rightVel.magnitude,
                      leftHolding, rightHolding);
            if (Time.time < _globalLockUntil) return;
        }

        // Split vs Rapier per controller — pick the dominant direction so a single
        // swing only fires one of them.
        TryRapierOrSplit(leftController,  leftVel,  true);
        if (Time.time < _globalLockUntil) return;
        TryRapierOrSplit(rightController, rightVel, false);
    }

    // ────────────────────────────────────────────────────────────────────────
    void TickBlock(float dt)
    {
        bool poseOk = IsBlockPose();
        _blockHoldTime = poseOk ? _blockHoldTime + dt : 0f;
        if (_blockHoldTime >= blockHoldDuration) { Fire("Block (both)"); _blockHoldTime = 0f; }
    }

    void TickKonanSY(float dt, bool leftHolding, bool rightHolding)
    {
        if (leftHolding || rightHolding) { _konanHoldTime = 0f; return; }
        bool poseOk = IsKonanPose();
        _konanHoldTime = poseOk ? _konanHoldTime + dt : 0f;
        if (_konanHoldTime >= konanHoldDuration) { Fire("konan sy (both)"); _konanHoldTime = 0f; }
    }

    void TickQigong(float dt, bool leftHolding, bool rightHolding,
                    Vector3 leftVel, Vector3 rightVel)
    {
        if (leftHolding || rightHolding) { _qigongHoldTime = 0f; return; }
        bool poseOk = IsQigongPose() &&
                      leftVel.magnitude  < 0.4f &&
                      rightVel.magnitude < 0.4f;
        _qigongHoldTime = poseOk ? _qigongHoldTime + dt : 0f;
        if (_qigongHoldTime >= qigongHoldDuration) { Fire("qigong (both)"); _qigongHoldTime = 0f; }
    }

    void TickWrist(float dt, float leftAng, float rightAng,
                   float leftLinSpeed, float rightLinSpeed,
                   bool leftHolding, bool rightHolding)
    {
        // Wrist requires: hand free, fast rotation, AND the hand staying roughly still.
        bool leftSpinning  = !leftHolding  &&
                             leftAng       > wristAngularSpeed &&
                             leftLinSpeed  < wristMaxLinearSpeed;
        bool rightSpinning = !rightHolding &&
                             rightAng      > wristAngularSpeed &&
                             rightLinSpeed < wristMaxLinearSpeed;

        _leftSpinAccumTime  = leftSpinning  ? _leftSpinAccumTime  + dt : 0f;
        _rightSpinAccumTime = rightSpinning ? _rightSpinAccumTime + dt : 0f;

        bool leftReady  = _leftSpinAccumTime  >= wristMinDuration;
        bool rightReady = _rightSpinAccumTime >= wristMinDuration;

        if (leftReady && rightReady)
        { Fire("wrist (both)"); _leftSpinAccumTime = _rightSpinAccumTime = 0f; }
        else if (leftReady)
        { Fire("wrist (L)");    _leftSpinAccumTime = 0f; }
        else if (rightReady)
        { Fire("wrist (R)");    _rightSpinAccumTime = 0f; }
    }

    /// <summary>
    /// Decide if this controller is doing a rapier (thrust) or a split (chop).
    /// Whichever wins fires – the other is suppressed. If neither dominates,
    /// nothing fires.
    /// </summary>
    void TryRapierOrSplit(Transform hand, Vector3 vel, bool isLeft)
    {
        float linSpeed = vel.magnitude;
        if (linSpeed < Mathf.Min(rapierForwardSpeed, splitDownwardSpeed)) return;

        // Forward component along the controller's forward
        float fwdSpeed  = Vector3.Dot(vel, hand.forward);     // signed
        // Downward component (positive = going down)
        float downSpeed = -vel.y;

        bool rapierCandidate = fwdSpeed >= rapierForwardSpeed &&
                                Vector3.Dot(vel.normalized, hand.forward) > rapierAlignment;
        bool splitCandidate  = downSpeed >= splitDownwardSpeed &&
                                IsMostlyDown(vel);

        if (rapierCandidate && (!splitCandidate ||
                                fwdSpeed > downSpeed * rapierVsSplitRatio))
        {
            Fire(isLeft ? "rapier (L)" : "rapier (R)");
        }
        else if (splitCandidate && (!rapierCandidate ||
                                    downSpeed > fwdSpeed * splitVsRapierRatio))
        {
            Fire(isLeft ? "split (L)" : "split (R)");
        }
        // else: ambiguous swing → ignore
    }

    bool IsMostlyDown(Vector3 v)
        => -v.y > Mathf.Abs(v.x) && -v.y > Mathf.Abs(v.z);

    // ────────────────────────────────────────────────────────────────────────
    bool IsBlockPose()
    {
        float dist = Vector3.Distance(leftController.position, rightController.position);
        if (dist > blockMaxDist) return false;
        float lh = leftController.position.y  - headTransform.position.y;
        float rh = rightController.position.y - headTransform.position.y;
        if (lh < blockHeightVsHead || rh < blockHeightVsHead) return false;

        Vector3 fwd = headTransform.forward;
        Vector3 toL = (leftController.position  - headTransform.position).normalized;
        Vector3 toR = (rightController.position - headTransform.position).normalized;
        return Vector3.Dot(fwd, toL) > 0.2f && Vector3.Dot(fwd, toR) > 0.2f;
    }

    bool IsKonanPose()
    {
        float dist = Vector3.Distance(leftController.position, rightController.position);
        if (dist > twoHandTogetherMaxDist + 0.1f) return false;
        float headY = headTransform.position.y;
        return leftController.position.y  > headY + konanAboveHead &&
               rightController.position.y > headY + konanAboveHead;
    }

    bool IsQigongPose()
    {
        float dist = Vector3.Distance(leftController.position, rightController.position);
        if (dist > twoHandTogetherMaxDist) return false;
        float headY = headTransform.position.y;
        float midY = (leftController.position.y + rightController.position.y) * 0.5f;
        if (midY > headY - 0.05f) return false;
        if (midY < headY - 0.7f)  return false;

        Vector3 fwd = headTransform.forward;
        Vector3 mid = (leftController.position + rightController.position) * 0.5f;
        Vector3 toMid = (mid - headTransform.position).normalized;
        return Vector3.Dot(fwd, toMid) > 0.25f;
    }

    // ────────────────────────────────────────────────────────────────────────
    void Fire(string name)
    {
        if (Time.time < _globalLockUntil) return;
        if (_lastFiredAt.TryGetValue(name, out float t) && Time.time - t < perGestureCooldown)
            return;

        _lastFiredAt[name] = Time.time;
        _globalLockUntil   = Time.time + globalRefractory;

        Debug.Log($"[PoseGestureDetector] Detected: {name}");

        if (recognizedLabel != null)
            recognizedLabel.text = $"<b>{name}</b>";

        OnGestureRecognized?.Invoke(name);

        if (actionHandler != null)
            actionHandler.HandleGestureByName(name);
    }
}
