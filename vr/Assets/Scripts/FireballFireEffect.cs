using UnityEngine;

/// <summary>
/// Adds a campfire-style fire particle effect to the fireball projectile.
/// Attach to the Fireball prefab. Creates fire particles at runtime.
/// Also plays the fire SFX when spawned.
/// </summary>
public class FireballFireEffect : MonoBehaviour
{
    [Header("Fire Settings")]
    public float fireSize = 0.3f;
    public int particleCount = 30;
    public Color fireColorStart = new Color(1f, 0.6f, 0.1f, 1f);
    public Color fireColorEnd = new Color(1f, 0.1f, 0f, 0.3f);

    private ParticleSystem _firePS;

    void Start()
    {
        CreateFireEffect();

        // Play fire SFX
        if (GameAudioManager.Instance != null)
            GameAudioManager.Instance.PlayFireball();
    }

    void CreateFireEffect()
    {
        GameObject psGO = new GameObject("FireEffect");
        psGO.transform.SetParent(transform);
        psGO.transform.localPosition = Vector3.zero;
        psGO.transform.localRotation = Quaternion.identity;

        _firePS = psGO.AddComponent<ParticleSystem>();
        _firePS.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = _firePS.main;
        main.duration = 5f;
        main.loop = true;
        main.startLifetime = 0.4f;
        main.startSpeed = 1.5f;
        main.startSize = fireSize;
        main.startColor = fireColorStart;
        main.maxParticles = 60;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.gravityModifier = -0.5f; // Fire goes up

        var emit = _firePS.emission;
        emit.rateOverTime = particleCount;

        var shape = _firePS.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = fireSize * 0.3f;

        // Color over lifetime
        var col = _firePS.colorOverLifetime;
        col.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(fireColorStart, 0f),
                new GradientColorKey(new Color(1f, 0.3f, 0.05f), 0.5f),
                new GradientColorKey(fireColorEnd, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.8f, 0.3f),
                new GradientAlphaKey(0f, 1f)
            });
        col.color = grad;

        // Size over lifetime - shrink
        var sol = _firePS.sizeOverLifetime;
        sol.enabled = true;
        sol.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));

        // Trail / smoke
        var trail = _firePS.trails;
        trail.enabled = false; // Keep it simple

        // Renderer
        var psr = psGO.GetComponent<ParticleSystemRenderer>();
        Shader shader = Shader.Find("Particles/Standard Unlit");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        if (shader != null)
        {
            Material mat = new Material(shader);
            mat.color = fireColorStart;
            // Set additive blending for fire glow
            mat.SetFloat("_Mode", 1f); // Additive
            psr.material = mat;
        }

        // Add a point light for fire glow
        GameObject lightGO = new GameObject("FireLight");
        lightGO.transform.SetParent(transform);
        lightGO.transform.localPosition = Vector3.zero;
        Light fireLight = lightGO.AddComponent<Light>();
        fireLight.type = LightType.Point;
        fireLight.color = new Color(1f, 0.5f, 0.1f);
        fireLight.intensity = 2f;
        fireLight.range = 3f;

        _firePS.Play();
    }
}
