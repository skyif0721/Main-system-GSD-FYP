using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Attach to the green cross health potion.
/// When the player grabs it and tilts their head upward (drinking pose),
/// the potion heals the player to full HP and destroys itself.
/// </summary>
[RequireComponent(typeof(XRGrabInteractable))]
public class HealthPotion : MonoBehaviour
{
    [Header("Drink Detection")]
    [Tooltip("Head tilt angle (degrees above horizon) required to trigger drinking")]
    public float drinkAngleThreshold = 30f;

    [Tooltip("How long the player must hold the drink pose before consuming")]
    public float drinkHoldTime = 1.0f;

    [Header("Visual Feedback")]
    [Tooltip("Optional particle effect to play when consumed")]
    public GameObject consumeVfxPrefab;

    private XRGrabInteractable _grabInteractable;
    private bool _isGrabbed = false;
    private float _drinkTimer = 0f;
    private bool _consumed = false;

    void Awake()
    {
        _grabInteractable = GetComponent<XRGrabInteractable>();
        _grabInteractable.selectEntered.AddListener(OnGrabbed);
        _grabInteractable.selectExited.AddListener(OnReleased);
    }

    void OnDestroy()
    {
        if (_grabInteractable != null)
        {
            _grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            _grabInteractable.selectExited.RemoveListener(OnReleased);
        }
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        _isGrabbed = true;
        _drinkTimer = 0f;
        Debug.Log("[HealthPotion] Grabbed!");
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        _isGrabbed = false;
        _drinkTimer = 0f;
        Debug.Log("[HealthPotion] Released.");
    }

    void Update()
    {
        if (_consumed || !_isGrabbed) return;

        // Check if the player's head is tilted upward (drinking pose)
        if (Camera.main == null) return;

        // Get the camera's forward direction pitch angle
        float pitch = Camera.main.transform.eulerAngles.x;
        // Unity euler X: 0=forward, negative (or >270) = looking up, positive (0-90) = looking down
        // Normalize to -180..180
        if (pitch > 180f) pitch -= 360f;
        // pitch < 0 means looking up
        bool isDrinking = pitch < -drinkAngleThreshold;

        if (isDrinking)
        {
            _drinkTimer += Time.deltaTime;
            if (_drinkTimer >= drinkHoldTime)
            {
                Consume();
            }
        }
        else
        {
            _drinkTimer = 0f;
        }
    }

    private void Consume()
    {
        if (_consumed) return;
        _consumed = true;

        // Play drink SFX
        if (GameAudioManager.Instance != null)
            GameAudioManager.Instance.PlayDrink();

        Debug.Log("[HealthPotion] Consumed! Healing player to full HP.");

        // Heal the player
        PlayerStats stats = FindObjectOfType<PlayerStats>();
        if (stats != null)
        {
            stats.HealFull();
        }
        else
        {
            Debug.LogWarning("[HealthPotion] No PlayerStats found in scene!");
        }

        // Spawn VFX if assigned
        if (consumeVfxPrefab != null)
        {
            GameObject vfx = Instantiate(consumeVfxPrefab, transform.position, Quaternion.identity);
            Destroy(vfx, 3f);
        }
        else
        {
            // Simple built-in particle burst as fallback
            SpawnSimpleHealVFX();
        }

        // Destroy the potion
        Destroy(gameObject);
    }

    private void SpawnSimpleHealVFX()
    {
        GameObject psGO = new GameObject("HealVFX");
        psGO.transform.position = transform.position;
        ParticleSystem ps = psGO.AddComponent<ParticleSystem>();

        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.duration = 0.5f;
        main.loop = false;
        main.startLifetime = 1.0f;
        main.startSpeed = 2f;
        main.startSize = 0.05f;
        main.startColor = new Color(0.2f, 1f, 0.3f, 1f);
        main.maxParticles = 80;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emit = ps.emission;
        emit.rateOverTime = 0;
        emit.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 60) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.1f;

        var psr = psGO.GetComponent<ParticleSystemRenderer>();
        Shader shader = Shader.Find("Particles/Standard Unlit");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        if (shader != null)
        {
            Material mat = new Material(shader);
            mat.color = new Color(0.2f, 1f, 0.3f, 1f);
            psr.material = mat;
        }

        ps.Play();
        Object.Destroy(psGO, 3f);
    }
}
