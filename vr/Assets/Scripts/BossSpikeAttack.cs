using UnityEngine;
using System.Collections;

/// <summary>
/// Creates a huge spike VFX when the boss hits the player.
/// Attach to the boss GameObject alongside BossAttackController.
/// Spawns a large spike mesh + particle burst at the player's position.
/// </summary>
public class BossSpikeAttack : MonoBehaviour
{
    [Header("Spike Settings")]
    public float spikeHeight = 4f;
    public float spikeWidth = 1.5f;
    public int spikeDamage = 35;
    public float spikeRadius = 3f;
    public float spikeCooldown = 10f;
    public Color spikeColor = new Color(0.6f, 0.1f, 0.1f, 1f);

    [Header("References")]
    public MonsterStat monsterStat;

    private Transform _playerTransform;
    private PlayerStats _playerStats;
    private float _lastSpikeTime = -999f;
    private bool _isDead;

    void Start()
    {
        if (monsterStat == null)
            monsterStat = GetComponent<MonsterStat>();

        GameObject player = GameObject.Find("Complete XR Origin Set Up Variant");
        if (player != null)
        {
            _playerTransform = player.transform;
            _playerStats = player.GetComponent<PlayerStats>();
        }
    }

    void Update()
    {
        if (_isDead || monsterStat == null || monsterStat.health <= 0)
        {
            _isDead = true;
            return;
        }

        if (_playerTransform == null) return;

        float dist = Vector3.Distance(transform.position, _playerTransform.position);

        // Spike attack when player is within range
        if (dist < spikeRadius * 2f && Time.time - _lastSpikeTime >= spikeCooldown)
        {
            StartCoroutine(SpikeAttack());
        }
    }

    IEnumerator SpikeAttack()
    {
        _lastSpikeTime = Time.time;

        if (_playerTransform == null) yield break;

        Vector3 targetPos = _playerTransform.position;

        // Warning indicator on ground
        GameObject warning = CreateWarningCircle(targetPos, spikeRadius);
        yield return new WaitForSeconds(0.8f);

        // Play boss shout SFX
        if (GameAudioManager.Instance != null)
            GameAudioManager.Instance.PlayBossShout();

        // Spawn the spike
        GameObject spike = CreateSpikeMesh(targetPos);

        // Damage player if in range
        if (_playerTransform != null && _playerStats != null)
        {
            float dist = Vector3.Distance(targetPos, _playerTransform.position);
            if (dist <= spikeRadius)
            {
                _playerStats.TakeDamage(spikeDamage);
                Debug.Log($"[BossSpikeAttack] Spike hit player for {spikeDamage} damage!");
            }
        }

        // Spawn particle burst
        SpawnSpikeVFX(targetPos);

        if (warning != null) Destroy(warning);

        // Spike stays for a moment then sinks
        yield return new WaitForSeconds(1.5f);

        // Sink the spike
        float sinkTime = 0.8f;
        float timer = 0f;
        Vector3 startPos = spike.transform.position;
        while (timer < sinkTime)
        {
            timer += Time.deltaTime;
            float t = timer / sinkTime;
            spike.transform.position = Vector3.Lerp(startPos, startPos - Vector3.up * spikeHeight, t);
            yield return null;
        }

        Destroy(spike);
    }

    GameObject CreateSpikeMesh(Vector3 position)
    {
        // Create a tall cone-like spike using a cylinder
        GameObject spike = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        spike.name = "BossSpike";
        Destroy(spike.GetComponent<Collider>());

        // Start below ground, then rise up
        spike.transform.position = position - Vector3.up * 0.5f;
        spike.transform.localScale = new Vector3(spikeWidth, spikeHeight * 0.5f, spikeWidth);

        // Create spike material
        Renderer r = spike.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = spikeColor;
        if (mat.HasProperty("_EmissionColor"))
        {
            mat.SetColor("_EmissionColor", spikeColor * 2f);
            mat.EnableKeyword("_EMISSION");
        }
        r.material = mat;

        // Animate rising
        StartCoroutine(RiseSpike(spike.transform, position));

        return spike;
    }

    IEnumerator RiseSpike(Transform spike, Vector3 targetPos)
    {
        if (spike == null) yield break;

        Vector3 startPos = targetPos - Vector3.up * spikeHeight;
        Vector3 endPos = targetPos + Vector3.up * (spikeHeight * 0.3f);
        float riseTime = 0.3f;
        float timer = 0f;

        while (timer < riseTime && spike != null)
        {
            timer += Time.deltaTime;
            float t = timer / riseTime;
            // Ease out
            t = 1f - (1f - t) * (1f - t);
            spike.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }
    }

    GameObject CreateWarningCircle(Vector3 center, float radius)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = "SpikeWarning";
        Destroy(go.GetComponent<Collider>());
        go.transform.position = center + Vector3.up * 0.05f;
        go.transform.localScale = new Vector3(radius * 2f, 0.02f, radius * 2f);

        Renderer r = go.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.SetFloat("_Mode", 3f);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.renderQueue = 3000;
        mat.color = new Color(1f, 0f, 0f, 0.4f);
        r.material = mat;

        // Pulse the warning
        StartCoroutine(PulseWarning(r, 0.8f));

        return go;
    }

    IEnumerator PulseWarning(Renderer r, float duration)
    {
        float timer = 0f;
        while (timer < duration && r != null)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.PingPong(timer * 4f, 0.5f) + 0.2f;
            r.material.color = new Color(1f, 0f, 0f, alpha);
            yield return null;
        }
    }

    void SpawnSpikeVFX(Vector3 pos)
    {
        GameObject psGO = new GameObject("SpikeVFX");
        psGO.transform.position = pos;
        ParticleSystem ps = psGO.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.duration = 0.5f;
        main.loop = false;
        main.startLifetime = 1.5f;
        main.startSpeed = 6f;
        main.startSize = 0.4f;
        main.startColor = spikeColor;
        main.maxParticles = 120;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 0.5f;

        var emit = ps.emission;
        emit.rateOverTime = 0;
        emit.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 100) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 15f;
        shape.radius = 0.5f;

        var psr = psGO.GetComponent<ParticleSystemRenderer>();
        Shader shader = Shader.Find("Particles/Standard Unlit");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        if (shader != null)
        {
            Material mat = new Material(shader);
            mat.color = spikeColor;
            psr.material = mat;
        }

        ps.Play();
        Destroy(psGO, 3f);
    }
}
