using UnityEngine;
using System.Collections;

/// <summary>
/// Procedural hit-feedback VFX that plays whenever the player damages a
/// monster. No prefab needed – everything is spawned at runtime.
///
/// Effect = bright impact spark burst + a quick red emissive flash on
/// the monster's renderers.
/// </summary>
public static class MonsterHitVFX
{
    static Shader _particleShader;

    /// <summary>
    /// Play the hit VFX on the given monster. <paramref name="hitPoint"/>
    /// is the world-space position to spawn the spark burst (defaults to the
    /// monster's transform position if zero).
    /// </summary>
    public static void Play(MonoBehaviour monster, int damage, Vector3 hitPoint = default)
    {
        if (monster == null) return;

        // Default hit point = monster center (slight offset so it's not under feet)
        if (hitPoint == default || hitPoint == Vector3.zero)
        {
            var col = monster.GetComponentInChildren<Collider>();
            hitPoint = col != null ? col.bounds.center
                                   : monster.transform.position + Vector3.up * 1f;
        }

        // 1) Quick red flash on every renderer of the monster
        monster.StartCoroutine(FlashRed(monster));

        // 2) Spark / blood-burst particles at the hit point
        SpawnSparks(hitPoint, damage);
    }

    // ─────────────────────────────────────────────────────────────────────
    static IEnumerator FlashRed(MonoBehaviour monster)
    {
        if (monster == null) yield break;

        var renderers = monster.GetComponentsInChildren<Renderer>();
        if (renderers == null || renderers.Length == 0) yield break;

        // Cache original emission/colour and turn each renderer red briefly
        Color[][] origColors      = new Color[renderers.Length][];
        Color[][] origEmissions   = new Color[renderers.Length][];
        bool[][]  hadEmission     = new bool[renderers.Length][];
        for (int i = 0; i < renderers.Length; i++)
        {
            var mats = renderers[i].materials;          // instanced
            origColors[i]    = new Color[mats.Length];
            origEmissions[i] = new Color[mats.Length];
            hadEmission[i]   = new bool[mats.Length];

            for (int m = 0; m < mats.Length; m++)
            {
                if (mats[m] == null) continue;
                if (mats[m].HasProperty("_Color"))     origColors[i][m]    = mats[m].color;
                if (mats[m].HasProperty("_EmissionColor"))
                {
                    origEmissions[i][m] = mats[m].GetColor("_EmissionColor");
                    hadEmission[i][m]   = mats[m].IsKeywordEnabled("_EMISSION");
                    mats[m].EnableKeyword("_EMISSION");
                    mats[m].SetColor("_EmissionColor", new Color(2.5f, 0.1f, 0.1f, 1f));
                }
                if (mats[m].HasProperty("_Color"))
                    mats[m].color = Color.Lerp(origColors[i][m], Color.red, 0.65f);
            }
        }

        yield return new WaitForSeconds(0.12f);

        // Restore
        if (monster == null) yield break;
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;
            var mats = renderers[i].materials;
            for (int m = 0; m < mats.Length; m++)
            {
                if (mats[m] == null) continue;
                if (mats[m].HasProperty("_Color"))         mats[m].color = origColors[i][m];
                if (mats[m].HasProperty("_EmissionColor"))
                {
                    mats[m].SetColor("_EmissionColor", origEmissions[i][m]);
                    if (!hadEmission[i][m]) mats[m].DisableKeyword("_EMISSION");
                }
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    static void SpawnSparks(Vector3 worldPos, int damage)
    {
        EnsureShaders();

        GameObject go = new GameObject("HitSparks");
        go.transform.position = worldPos;

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        // Bigger / more particles for bigger damage (clamped)
        int count = Mathf.Clamp(20 + damage / 2, 20, 60);

        var main = ps.main;
        main.duration         = 0.4f;
        main.loop             = false;
        main.startLifetime    = 0.45f;
        main.startSpeed       = 2.5f;
        main.startSize        = 0.04f;
        main.startColor       = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.3f, 0.1f, 1f),
            new Color(1f, 0.85f, 0.2f, 1f));
        main.maxParticles     = 80;
        main.simulationSpace  = ParticleSystemSimulationSpace.World;
        main.gravityModifier  = 0.6f;

        var emit = ps.emission;
        emit.rateOverTime = 0;
        emit.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, count)
        });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius    = 0.05f;

        // Colour-over-lifetime: bright → dark
        var col = ps.colorOverLifetime;
        col.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(1f, 0.9f, 0.4f), 0f),
                new GradientColorKey(new Color(0.7f, 0.1f, 0.05f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f)
            });
        col.color = grad;

        var psr = go.GetComponent<ParticleSystemRenderer>();
        if (psr != null)
        {
            Material mat = new Material(_particleShader);
            mat.color = Color.white;
            psr.material = mat;
        }

        ps.Play();
        Object.Destroy(go, 1.2f);
    }

    static void EnsureShaders()
    {
        if (_particleShader == null)
        {
            _particleShader = Shader.Find("Particles/Standard Unlit");
            if (_particleShader == null) _particleShader = Shader.Find("Sprites/Default");
            if (_particleShader == null) _particleShader = Shader.Find("Standard");
        }
    }
}
