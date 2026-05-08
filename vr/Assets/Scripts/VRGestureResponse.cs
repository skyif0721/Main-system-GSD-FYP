using UnityEngine;

/// <summary>
/// Responds to VRGestureDetector events with real game effects:
///   BLOCK  -> Player takes 0 damage while blocking. Shield visual turns ON/OFF.
///   PUSH   -> All nearby enemies are knocked back and take damage.
/// </summary>
public class VRGestureResponse : MonoBehaviour
{
    [Header("References (auto-found if empty)")]
    public VRGestureDetector detector;
    public PlayerStats playerStats;

    [Header("Shield Visual")]
    [Tooltip("The shield GameObject that appears when blocking")]
    public GameObject shieldVisual;

    [Header("Block Settings")]
    [Tooltip("Damage reduction multiplier while blocking (0 = no damage, 0.5 = half damage)")]
    public float blockDamageMultiplier = 0f;

    [Header("Push Settings")]
    [Tooltip("Radius around the player to search for enemies to push")]
    public float pushRadius = 3.5f;
    [Tooltip("Force applied to enemies when pushed")]
    public float pushForce = 8f;
    [Tooltip("Damage dealt to enemies on push")]
    public int pushDamage = 25;

    [Header("Fireball Settings")]
    public GameObject fireballPrefab;
    public Transform fireballSpawnPoint;

    // Static flag so MonsterStat can check if player is blocking
    public static bool PlayerIsBlocking = false;
    public static float BlockDamageMultiplier = 0f;

    void Start()
    {
        AutoFindReferences();
        SetShieldVisible(false);
    }

    void AutoFindReferences()
    {
        if (detector == null)
            detector = GetComponent<VRGestureDetector>();
        if (detector == null)
            detector = FindObjectOfType<VRGestureDetector>();

        if (playerStats == null)
        {
            GameObject player = GameObject.Find("Complete XR Origin Set Up Variant");
            if (player != null) playerStats = player.GetComponent<PlayerStats>();
        }

        if (detector == null) Debug.LogWarning("[VRGestureResponse] No VRGestureDetector found!");
        if (playerStats == null) Debug.LogWarning("[VRGestureResponse] No PlayerStats found!");
    }

    void OnEnable()
    {
        if (detector != null)
        {
            detector.OnBlockStart   += HandleBlockStart;
            detector.OnBlockEnd     += HandleBlockEnd;
            detector.OnPushDetected += HandlePush;
        }
    }

    void OnDisable()
    {
        if (detector != null)
        {
            detector.OnBlockStart   -= HandleBlockStart;
            detector.OnBlockEnd     -= HandleBlockEnd;
            detector.OnPushDetected -= HandlePush;
        }
    }

    // ─── BLOCK ────────────────────────────────────────────────────────────────

    void HandleBlockStart()
    {
        PlayerIsBlocking = true;
        BlockDamageMultiplier = blockDamageMultiplier;
        SetShieldVisible(true);
        Debug.Log("[VRGestureResponse] Blocking ON – damage reduced!");
    }

    void HandleBlockEnd()
    {
        PlayerIsBlocking = false;
        BlockDamageMultiplier = 1f;
        SetShieldVisible(false);
        Debug.Log("[VRGestureResponse] Blocking OFF");
    }

    void SetShieldVisible(bool visible)
    {
        if (shieldVisual != null)
            shieldVisual.SetActive(visible);
    }

    // ─── PUSH ─────────────────────────────────────────────────────────────────

    void HandlePush()
    {
        Debug.Log("[VRGestureResponse] Push! Shooting fireball.");

        if (fireballPrefab != null)
        {
            Transform spawnPoint = fireballSpawnPoint != null ? fireballSpawnPoint : (playerStats != null ? playerStats.transform : transform);
            
            // Try to get the camera forward direction
            Vector3 forward = spawnPoint.forward;
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                forward = mainCam.transform.forward;
                spawnPoint = mainCam.transform;
            }

            GameObject fireball = Instantiate(fireballPrefab, spawnPoint.position + forward * 0.5f, Quaternion.LookRotation(forward));
        }
        else
        {
            // Fallback to old push logic if no fireball prefab
            Vector3 origin = playerStats != null
                ? playerStats.transform.position
                : transform.position;

            Collider[] hits = Physics.OverlapSphere(origin, pushRadius);
            foreach (Collider col in hits)
            {
                MonsterStat monster = col.GetComponentInParent<MonsterStat>();
                if (monster != null)
                {
                    monster.TakeDamage(pushDamage);
                    Rigidbody rb = col.GetComponentInParent<Rigidbody>();
                    if (rb != null)
                    {
                        Vector3 dir = (col.transform.position - origin).normalized;
                        rb.AddForce(dir * pushForce, ForceMode.Impulse);
                    }
                }
            }
        }
    }

    // Draw push radius in editor for easy tuning
    void OnDrawGizmosSelected()
    {
        Vector3 origin = playerStats != null ? playerStats.transform.position : transform.position;
        Gizmos.color = new Color(0f, 0.5f, 1f, 0.3f);
        Gizmos.DrawSphere(origin, pushRadius);
        Gizmos.color = new Color(0f, 0.5f, 1f, 1f);
        Gizmos.DrawWireSphere(origin, pushRadius);
    }
}
