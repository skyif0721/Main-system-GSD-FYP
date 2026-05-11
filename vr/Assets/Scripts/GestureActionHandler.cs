using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiVRy;

/// <summary>
/// Central handler that turns recognised gesture names from the Mivry
/// gesture recognizer (or any string-source) into in-game actions.
///
/// Wire this up by adding it to a manager GameObject in the scene, then
/// hook the Mivry component's <c>OnGestureCompletion</c> UnityEvent to
/// <see cref="OnGestureCompleted"/>. Alternatively call
/// <see cref="HandleGestureByName"/> directly from any other script.
///
/// Supported gestures (exact name match; case-insensitive):
///   wrist (L)    / 左內外腕花  – left flourish (small AOE around left hand)
///   wrist (R)    / 右內外腕花  – right flourish (small AOE around right hand)
///   rapier (L)   / 左刺劍       – left thrust (launches piercing projectile)
///   rapier (R)   / 右刺劍       – right thrust (launches piercing projectile)
///   split (L)    / 左劈         – left downward chop (cone damage)
///   split (R)    / 右劈         – right downward chop (cone damage)
///   wrist (both) / 花手         – dual flourish (big AOE around player)
///   konan sy (both) / 港南sy    – signature ultimate (fireball barrage)
///   Block (both) / 格擋         – guard pose (invulnerable while held)
/// </summary>
public class GestureActionHandler : MonoBehaviour
{
    [Header("Hand / Player references (auto-found if empty)")]
    public Transform leftController;
    public Transform rightController;
    public Transform headTransform;
    public PlayerStats playerStats;

    [Header("Projectiles")]
    [Tooltip("Used by 'rapier' (single piercing shot) and 'konan sy' (barrage).")]
    public GameObject fireballPrefab;
    [Tooltip("Optional dedicated prefab for the rapier thrust. Falls back to fireballPrefab.")]
    public GameObject rapierProjectilePrefab;

    [Header("Visual FX (optional)")]
    public GameObject shieldVisual;
    public GameObject wristFlourishVfxPrefab;
    public GameObject splitSlashVfxPrefab;
    public GameObject konanUltimateVfxPrefab;

    [Header("Combat tuning")]
    [Tooltip("Radius for single-hand wrist flourish AOE.")]
    public float wristSingleRadius   = 1.4f;
    public int   wristSingleDamage   = 18;
    [Tooltip("Radius for both-hands wrist flourish AOE.")]
    public float wristBothRadius     = 3.5f;
    public int   wristBothDamage     = 35;
    public float wristBothKnockback  = 6f;

    public float splitConeRange      = 2.5f;
    public float splitConeAngleDeg   = 60f;
    public int   splitDamage         = 45;

    public int   rapierDamage        = 60;
    public float rapierSpeed         = 18f;

    public int   konanProjectiles    = 8;
    public float konanSpreadDeg      = 35f;
    public int   konanDamagePerShot  = 25;

    public float blockHoldSeconds    = 2.5f;

    // Cooldown so the same gesture cannot retrigger every frame
    [Header("UI")]
    [Tooltip("Optional TMP text that displays the last-handled gesture name.")]
    public TMPro.TMP_Text recognizedLabel;

    [Header("Misc")]
    public float perGestureCooldown  = 0.4f;
    private readonly Dictionary<string, float> _lastFiredAt = new Dictionary<string, float>();

    // ────────────────────────────────────────────────────────────────────────
    void Awake()
    {
        AutoFind();
    }

    void Start()
    {
        // Make sure no defensive buff is still set from a previous play session
        BlockState.Reset();
        // Always start with the shield hidden – it only appears during a Block.
        if (shieldVisual != null) shieldVisual.SetActive(false);
    }

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
        if (playerStats == null)
        {
            var go = GameObject.Find("Complete XR Origin Set Up Variant");
            if (go != null) playerStats = go.GetComponent<PlayerStats>();
            if (playerStats == null) playerStats = FindObjectOfType<PlayerStats>();
        }
        if (recognizedLabel == null)
        {
            GameObject lbl = GameObject.Find("RecognizedText");
            if (lbl != null) recognizedLabel = lbl.GetComponent<TMPro.TMP_Text>();
        }
    }

    // ────────────────────────────────────────────────────────────────────────
    //  ENTRY POINTS
    // ────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Called by the Mivry component's <c>OnGestureCompletion</c> event.
    /// </summary>
    public void OnGestureCompleted(GestureCompletionData data)
    {
        if (data == null) return;
        HandleGestureByName(data.gestureName);
    }

    /// <summary>
    /// Generic string entry point – also useful for testing from buttons.
    /// </summary>
    public void HandleGestureByName(string rawName)
    {
        if (string.IsNullOrWhiteSpace(rawName)) return;

        // Cooldown per-gesture
        if (_lastFiredAt.TryGetValue(rawName, out float t) && Time.time - t < perGestureCooldown)
            return;
        _lastFiredAt[rawName] = Time.time;

        string n = Normalize(rawName);
        Debug.Log($"[GestureActionHandler] Received gesture: '{rawName}' → normalized '{n}'");

        if (recognizedLabel != null)
            recognizedLabel.text = $"<b>{rawName}</b>";

        switch (n)
        {
            // ---- Wrist flourish ------------------------------------------------
            case "wristl":
            case "wrist(l)":
            case "leftwristflower":
            case "左內外腕花":
            case "左内外腕花":
                DoWristFlourish(true);
                break;

            case "wristr":
            case "wrist(r)":
            case "rightwristflower":
            case "右內外腕花":
            case "右内外腕花":
                DoWristFlourish(false);
                break;

            case "wristboth":
            case "wrist(both)":
            case "花手":
                DoWristFlourishBoth();
                break;

            // ---- Rapier thrust -------------------------------------------------
            case "rapierl":
            case "rapier(l)":
            case "左刺劍":
            case "左刺剑":
                DoRapierThrust(true);
                break;

            case "rapierr":
            case "rapier(r)":
            case "右刺劍":
            case "右刺剑":
                DoRapierThrust(false);
                break;

            // ---- Split / chop --------------------------------------------------
            case "splitl":
            case "split(l)":
            case "左劈":
                DoSplitChop(true);
                break;

            case "splitr":
            case "split(r)":
            case "右劈":
                DoSplitChop(false);
                break;

            // ---- Konan SY (signature ultimate) --------------------------------
            case "konansy":
            case "konansyboth":
            case "konansy(both)":
            case "港南sy":
                DoKonanSignature();
                break;

            // ---- Block ---------------------------------------------------------
            case "block":
            case "blockboth":
            case "block(both)":
            case "格擋":
            case "格挡":
                DoBlock();
                break;

            default:
                Debug.LogWarning($"[GestureActionHandler] No action mapped for gesture '{rawName}'");
                break;
        }
    }

    /// <summary>
    /// Lower-case + remove spaces / slashes / dashes so user can use any
    /// formatting in the trained data set. Chinese chars are preserved.
    /// </summary>
    static string Normalize(string s)
    {
        if (s == null) return "";
        var sb = new System.Text.StringBuilder(s.Length);
        foreach (char c in s)
        {
            if (c == ' ' || c == '\t' || c == '_' || c == '-' || c == '/' || c == '\\') continue;
            sb.Append(char.ToLowerInvariant(c));
        }
        return sb.ToString();
    }

    // ────────────────────────────────────────────────────────────────────────
    //  ACTIONS
    // ────────────────────────────────────────────────────────────────────────

    void DoWristFlourish(bool leftHand)
    {
        Transform hand = leftHand ? leftController : rightController;
        if (hand == null) hand = headTransform != null ? headTransform : transform;

        SpawnVfx(wristFlourishVfxPrefab, hand.position, hand.rotation, 1.0f);
        HandGestureVFX.Play(hand,
            leftHand ? "Wrist L" : "Wrist R",
            new Color(1f, 0.6f, 0.2f), 0.9f, 0.2f);

        DamageInSphere(hand.position, wristSingleRadius, wristSingleDamage, 0f);
        Debug.Log($"[Gesture] Wrist flourish ({(leftHand ? "L" : "R")}) at {hand.position}");
    }

    void DoWristFlourishBoth()
    {
        Vector3 origin = playerStats != null ? playerStats.transform.position : transform.position;
        SpawnVfx(wristFlourishVfxPrefab, origin, Quaternion.identity, 2.0f);

        Color c = new Color(1f, 0.4f, 0.8f);
        if (leftController  != null) HandGestureVFX.Play(leftController,  "花手", c, 1.2f, 0.25f);
        if (rightController != null) HandGestureVFX.Play(rightController, "花手", c, 1.2f, 0.25f);

        DamageInSphere(origin, wristBothRadius, wristBothDamage, wristBothKnockback);
        Debug.Log("[Gesture] Wrist flourish (BOTH) – AOE pulse");
    }

    void DoRapierThrust(bool leftHand)
    {
        Transform hand = leftHand ? leftController : rightController;
        if (hand == null) { Debug.LogWarning("[Gesture] No controller found for rapier."); return; }

        // Deduct mana
        if (playerStats != null && !playerStats.UseFireballMana())
        {
            Debug.Log("[Gesture] Rapier cancelled – not enough mana.");
            return;
        }

        GameObject prefab = rapierProjectilePrefab != null ? rapierProjectilePrefab : fireballPrefab;
        if (prefab == null)
        {
            // Fallback: instant ray hit
            Vector3 dir = hand.forward;
            if (Physics.Raycast(hand.position, dir, out RaycastHit hit, 25f))
            {
                var m = hit.collider.GetComponentInParent<MonsterStat>();
                if (m != null) m.TakeDamage(rapierDamage);
            }
            Debug.Log($"[Gesture] Rapier ({(leftHand ? "L" : "R")}) raycast (no projectile prefab)");
            return;
        }

        HandGestureVFX.Play(hand,
            leftHand ? "Rapier L" : "Rapier R",
            new Color(0.4f, 0.9f, 1f), 0.6f, 0.15f);

        Vector3 forward = hand.forward;
        Vector3 spawnPos = hand.position + forward * 0.25f;
        GameObject proj = Instantiate(prefab, spawnPos, Quaternion.LookRotation(forward));

        // If it has Fireball component, retune for rapier behavior
        Fireball fb = proj.GetComponent<Fireball>();
        if (fb != null)
        {
            fb.damage = rapierDamage;
            fb.speed  = rapierSpeed;
        }
        else
        {
            Rigidbody rb = proj.GetComponent<Rigidbody>();
            if (rb != null) rb.velocity = forward * rapierSpeed;
        }
        Debug.Log($"[Gesture] Rapier thrust ({(leftHand ? "L" : "R")})  → {forward}");
    }

    void DoSplitChop(bool leftHand)
    {
        Transform hand = leftHand ? leftController : rightController;
        if (hand == null) hand = headTransform != null ? headTransform : transform;

        Vector3 origin  = hand.position;
        Vector3 forward = hand.forward;

        SpawnVfx(splitSlashVfxPrefab, origin + forward * 0.4f,
                 Quaternion.LookRotation(forward), 1.5f);

        HandGestureVFX.Play(hand,
            leftHand ? "Split L" : "Split R",
            new Color(1f, 0.95f, 0.2f), 0.8f, 0.25f);

        // Cone damage: forward arc, range = splitConeRange, angle = splitConeAngleDeg
        Collider[] hits = Physics.OverlapSphere(origin, splitConeRange);
        float halfAngle = splitConeAngleDeg * 0.5f;
        foreach (var c in hits)
        {
            Vector3 to = c.transform.position - origin;
            if (to.sqrMagnitude < 0.0001f) continue;
            if (Vector3.Angle(forward, to.normalized) > halfAngle) continue;

            var m = c.GetComponentInParent<MonsterStat>();
            if (m != null) m.TakeDamage(splitDamage);
        }
        Debug.Log($"[Gesture] Split chop ({(leftHand ? "L" : "R")})");
    }

    void DoKonanSignature()
    {
        if (fireballPrefab == null)
        {
            Debug.LogWarning("[Gesture] Konan SY needs a fireballPrefab assigned.");
            return;
        }

        // Deduct mana
        if (playerStats != null && !playerStats.UseFireballMana())
        {
            Debug.Log("[Gesture] Konan SY cancelled – not enough mana.");
            return;
        }
        Transform spawn = headTransform != null ? headTransform : transform;
        SpawnVfx(konanUltimateVfxPrefab, spawn.position + spawn.forward * 0.3f,
                 spawn.rotation, 2f);

        Color kc = new Color(1f, 0.3f, 0.1f);
        if (leftController  != null) HandGestureVFX.Play(leftController,  "港南SY!", kc, 1.4f, 0.25f);
        if (rightController != null) HandGestureVFX.Play(rightController, "港南SY!", kc, 1.4f, 0.25f);

        Vector3 forward = spawn.forward;
        for (int i = 0; i < konanProjectiles; i++)
        {
            float ti = (konanProjectiles == 1) ? 0.5f : (float)i / (konanProjectiles - 1);
            float yaw = Mathf.Lerp(-konanSpreadDeg, konanSpreadDeg, ti);
            Quaternion rot = Quaternion.AngleAxis(yaw, Vector3.up) * Quaternion.LookRotation(forward);
            Vector3 dir = rot * Vector3.forward;

            GameObject proj = Instantiate(fireballPrefab,
                                          spawn.position + dir * 0.5f,
                                          Quaternion.LookRotation(dir));
            Fireball fb = proj.GetComponent<Fireball>();
            if (fb != null) fb.damage = konanDamagePerShot;
            else
            {
                Rigidbody rb = proj.GetComponent<Rigidbody>();
                if (rb != null) rb.velocity = dir * 14f;
            }
        }
        Debug.Log($"[Gesture] Konan SY signature – fired {konanProjectiles} projectiles");
    }

    // We track the active Block state with a counter so re-triggering Block
    // while one is already active just refreshes/extends the buff instead of
    // accidentally turning the shield off mid-buff.
    Coroutine _activeBlockCo;

    void DoBlock()
    {
        // If a block is already running, just restart it (fresh duration)
        if (_activeBlockCo != null) StopCoroutine(_activeBlockCo);
        _activeBlockCo = StartCoroutine(BlockCoroutine());
    }

    IEnumerator BlockCoroutine()
    {
        // Show the shield (and keep it on for the whole block duration)
        if (shieldVisual != null) shieldVisual.SetActive(true);

        Color bc = new Color(0.3f, 0.7f, 1f);
        if (leftController  != null) HandGestureVFX.Play(leftController,  "BLOCK", bc, blockHoldSeconds, 0.22f);
        if (rightController != null) HandGestureVFX.Play(rightController, "BLOCK", bc, blockHoldSeconds, 0.22f);

        BlockState.BlockActive = true;
        BlockState.Refresh();
        Debug.Log($"[Gesture] BLOCK – invulnerable for {blockHoldSeconds}s");
        yield return new WaitForSeconds(blockHoldSeconds);

        BlockState.BlockActive = false;
        BlockState.Refresh();

        // Hide shield ONLY if no other defensive buff is active
        if (shieldVisual != null && !BlockState.AnyActive)
            shieldVisual.SetActive(false);

        _activeBlockCo = null;
    }

    // ────────────────────────────────────────────────────────────────────────
    //  HELPERS
    // ────────────────────────────────────────────────────────────────────────

    void DamageInSphere(Vector3 origin, float radius, int damage, float knockback)
    {
        Collider[] hits = Physics.OverlapSphere(origin, radius);
        var hitMonsters = new HashSet<MonsterStat>();
        foreach (var c in hits)
        {
            var m = c.GetComponentInParent<MonsterStat>();
            if (m == null || hitMonsters.Contains(m)) continue;
            hitMonsters.Add(m);
            m.TakeDamage(damage);

            if (knockback > 0f)
            {
                Rigidbody rb = c.GetComponentInParent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 dir = (m.transform.position - origin);
                    if (dir.sqrMagnitude < 0.001f) dir = Vector3.forward;
                    rb.AddForce(dir.normalized * knockback, ForceMode.Impulse);
                }
            }
        }
    }

    void SpawnVfx(GameObject prefab, Vector3 pos, Quaternion rot, float lifetime)
    {
        if (prefab == null) return;
        GameObject vfx = Instantiate(prefab, pos, rot);
        if (lifetime > 0f) Destroy(vfx, lifetime);
    }

    // Visual debug
    void OnDrawGizmosSelected()
    {
        Vector3 c = playerStats != null ? playerStats.transform.position : transform.position;
        Gizmos.color = new Color(1f, 0.6f, 0.1f, 0.25f);
        Gizmos.DrawWireSphere(c, wristBothRadius);
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.25f);
        if (rightController != null) Gizmos.DrawWireSphere(rightController.position, wristSingleRadius);
        if (leftController  != null) Gizmos.DrawWireSphere(leftController.position,  wristSingleRadius);
    }
}
