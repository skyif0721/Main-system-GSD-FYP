using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Procedural visual feedback that plays at a hand controller (or
/// arbitrary world position) whenever a gesture is recognised.
/// No prefab needed – everything is generated at runtime.
///
/// Effect = expanding colored ring + glow sphere + floating label text.
/// Compatible with URP and the legacy built-in pipeline (auto-detected).
/// </summary>
public static class HandGestureVFX
{
    // Cached shaders, picked once per session.
    static Shader _opaqueShader;
    static Shader _particleShader;

    /// <summary>
    /// Play a hand-anchored VFX with a label and color.
    /// </summary>
    public static void Play(Transform anchor, string label, Color color,
                             float duration = 1.0f, float size = 0.25f)
    {
        if (anchor == null) return;
        EnsureShaders();

        // Parent container, follows the hand
        GameObject root = new GameObject($"GestureFX_{label}");
        root.transform.SetParent(anchor, false);
        root.transform.localPosition = Vector3.zero;
        root.transform.localRotation = Quaternion.identity;

        // 1. Glow sphere (small)
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = "Glow";
        sphere.transform.SetParent(root.transform, false);
        sphere.transform.localPosition = Vector3.zero;
        sphere.transform.localScale    = Vector3.one * 0.02f;
        Object.Destroy(sphere.GetComponent<Collider>());

        Material sphereMat = new Material(_opaqueShader);
        ConfigureFadeMaterial(sphereMat, color, 0.65f);
        sphere.GetComponent<Renderer>().material = sphereMat;

        // 2. Expanding ring (a flattened cylinder, small)
        GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.name = "Ring";
        ring.transform.SetParent(root.transform, false);
        ring.transform.localPosition = Vector3.zero;
        ring.transform.localScale    = new Vector3(0.02f, 0.002f, 0.02f);
        Object.Destroy(ring.GetComponent<Collider>());

        Material ringMat = new Material(_opaqueShader);
        ConfigureFadeMaterial(ringMat, color, 0.85f);
        ring.GetComponent<Renderer>().material = ringMat;

        // 3. Floating label (smaller, closer to hand)
        GameObject labelGO = new GameObject("Label");
        labelGO.transform.SetParent(root.transform, false);
        labelGO.transform.localPosition = new Vector3(0f, 0.08f, 0f);

        TextMeshPro labelTmp = labelGO.AddComponent<TextMeshPro>();
        labelTmp.text = label;
        labelTmp.alignment = TextAlignmentOptions.Center;
        labelTmp.fontSize = 1f;
        labelTmp.fontStyle = FontStyles.Bold;
        labelTmp.color = color;
        RectTransform rt = labelTmp.rectTransform;
        rt.sizeDelta = new Vector2(1f, 0.25f);

        // 4. Particle burst — configure BEFORE Play()
        GameObject psGO = new GameObject("Particles");
        psGO.transform.SetParent(root.transform, false);
        psGO.transform.localPosition = Vector3.zero;
        ParticleSystem ps = psGO.AddComponent<ParticleSystem>();

        // Stop while we change duration (fixes the "system is still playing" warning)
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.duration         = 0.35f;
        main.loop             = false;
        main.startLifetime    = 0.45f;
        main.startSpeed       = 0.8f;
        main.startSize        = 0.018f;
        main.startColor       = color;
        main.maxParticles     = 30;
        main.simulationSpace  = ParticleSystemSimulationSpace.World;

        var emit = ps.emission;
        emit.rateOverTime = 0;
        emit.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, 18)
        });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius    = 0.01f;

        var psr = psGO.GetComponent<ParticleSystemRenderer>();
        Material partMat = new Material(_particleShader);
        partMat.color = color;
        psr.material  = partMat;

        ps.Play();

        // Animator
        var anim = root.AddComponent<HandGestureFXRunner>();
        anim.Init(sphere.transform, ring.transform, sphereMat, ringMat,
                  labelTmp, duration, size, color);
    }

    static void EnsureShaders()
    {
        if (_opaqueShader == null)
        {
            // Use built-in Standard shader (works with both built-in and URP)
            _opaqueShader = Shader.Find("Standard");
            if (_opaqueShader == null) _opaqueShader = Shader.Find("Unlit/Color");
        }
        if (_particleShader == null)
        {
            _particleShader = Shader.Find("Particles/Standard Unlit");
            if (_particleShader == null) _particleShader = Shader.Find("Sprites/Default");
            if (_particleShader == null) _particleShader = _opaqueShader;
        }
    }

    static void ConfigureFadeMaterial(Material m, Color tint, float alpha)
    {
        Color c = tint; c.a = alpha;

        // Built-in Standard shader transparent mode
        if (m.HasProperty("_Mode"))
        {
            m.SetFloat("_Mode", 3f); // Transparent
            m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            m.SetInt("_ZWrite", 0);
            m.DisableKeyword("_ALPHATEST_ON");
            m.EnableKeyword("_ALPHABLEND_ON");
            m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            m.renderQueue = 3000;
        }

        m.color = c;

        if (m.HasProperty("_EmissionColor"))
        {
            m.SetColor("_EmissionColor", tint * 1.5f);
            m.EnableKeyword("_EMISSION");
        }
    }
}

/// <summary>
/// Runtime animator for the gesture VFX – drives ring expansion, sphere
/// pulse and fade-out.
/// </summary>
public class HandGestureFXRunner : MonoBehaviour
{
    private Transform _sphere;
    private Transform _ring;
    private Material  _sphereMat;
    private Material  _ringMat;
    private TextMeshPro _label;
    private float     _duration;
    private float     _finalRingSize;
    private Color     _color;
    private Transform _camTransform;

    public void Init(Transform sphere, Transform ring,
                     Material sphereMat, Material ringMat,
                     TextMeshPro label, float duration, float size, Color color)
    {
        _sphere       = sphere;
        _ring         = ring;
        _sphereMat    = sphereMat;
        _ringMat      = ringMat;
        _label        = label;
        _duration     = Mathf.Max(0.05f, duration);
        _finalRingSize = size;
        _color        = color;
        if (Camera.main != null) _camTransform = Camera.main.transform;
        StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        float t = 0f;
        while (t < _duration)
        {
            t += Time.deltaTime;
            float u = t / _duration;
            float fade = 1f - u;

            if (_ring != null)
            {
                float r = Mathf.Lerp(0.02f, _finalRingSize, u);
                _ring.localScale = new Vector3(r, 0.0015f + 0.0025f * fade, r);
                if (_ringMat != null) SetTintAlpha(_ringMat, _color, fade * 0.85f);
            }

            if (_sphere != null)
            {
                float pulse = Mathf.Sin(u * Mathf.PI);
                float s = Mathf.Lerp(0.018f, 0.07f, pulse);
                _sphere.localScale = Vector3.one * s;
                if (_sphereMat != null) SetTintAlpha(_sphereMat, _color, pulse * 0.75f);
            }

            if (_label != null)
            {
                _label.transform.localPosition = new Vector3(0f, 0.08f + u * 0.07f, 0f);
                if (_camTransform != null)
                {
                    Vector3 toCam = _camTransform.position - _label.transform.position;
                    if (toCam.sqrMagnitude > 0.0001f)
                        _label.transform.rotation = Quaternion.LookRotation(-toCam, Vector3.up);
                }
                Color lc = _color; lc.a = fade;
                _label.color = lc;
            }

            yield return null;
        }
        Destroy(gameObject);
    }

    static void SetTintAlpha(Material m, Color tint, float a)
    {
        Color c = tint; c.a = a;
        m.color = c;
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
    }
}
