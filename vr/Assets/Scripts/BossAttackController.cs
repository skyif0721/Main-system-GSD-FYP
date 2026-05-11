using UnityEngine;
using System.Collections;

/// <summary>
/// Boss attack controller with multiple attack patterns:
/// - Ground Slam: AoE damage around the boss
/// - Charge: Rush toward the player at high speed
/// - Stomp: Shockwave that damages nearby player
/// The boss cycles through attacks with cooldowns.
/// </summary>
public class BossAttackController : MonoBehaviour
{
    [Header("Attack Settings")]
    public float groundSlamRadius = 8f;
    public int groundSlamDamage = 25;
    public float groundSlamCooldown = 8f;

    public float chargeSpeed = 15f;
    public float chargeDuration = 1.5f;
    public int chargeDamage = 30;
    public float chargeCooldown = 12f;

    public float stompRadius = 5f;
    public int stompDamage = 15;
    public float stompCooldown = 5f;

    [Header("Visual Feedback")]
    public Color slamWarningColor = new Color(1f, 0.2f, 0f, 0.5f);
    public Color chargeWarningColor = new Color(1f, 0f, 0f, 0.8f);

    [Header("References")]
    public MonsterStat monsterStat;

    private Transform _playerTransform;
    private PlayerStats _playerStats;
    private Animator _animator;
    private UnityEngine.AI.NavMeshAgent _agent;

    private float _slamTimer;
    private float _chargeTimer;
    private float _stompTimer;
    private bool _isCharging;
    private bool _isDead;

    void Start()
    {
        if (monsterStat == null)
            monsterStat = GetComponent<MonsterStat>();

        _animator = GetComponent<Animator>();
        _agent = GetComponent<UnityEngine.AI.NavMeshAgent>();

        GameObject player = GameObject.Find("Complete XR Origin Set Up Variant");
        if (player != null)
        {
            _playerTransform = player.transform;
            _playerStats = player.GetComponent<PlayerStats>();
        }

        // Stagger initial timers so attacks don't all fire at once
        _slamTimer = groundSlamCooldown * 0.5f;
        _chargeTimer = 0f;
        _stompTimer = stompCooldown * 0.3f;
    }

    void Update()
    {
        if (_isDead || monsterStat == null || monsterStat.health <= 0)
        {
            _isDead = true;
            return;
        }

        if (_isCharging || _playerTransform == null) return;

        float distToPlayer = Vector3.Distance(transform.position, _playerTransform.position);

        _slamTimer += Time.deltaTime;
        _chargeTimer += Time.deltaTime;
        _stompTimer += Time.deltaTime;

        // Priority: Charge if far, Slam if medium, Stomp if close
        if (distToPlayer > 10f && _chargeTimer >= chargeCooldown)
        {
            StartCoroutine(ChargeAttack());
        }
        else if (distToPlayer < groundSlamRadius && _slamTimer >= groundSlamCooldown)
        {
            StartCoroutine(GroundSlamAttack());
        }
        else if (distToPlayer < stompRadius && _stompTimer >= stompCooldown)
        {
            StompAttack();
        }
    }

    IEnumerator GroundSlamAttack()
    {
        _slamTimer = 0f;

        // Warning: boss raises arms (trigger animation)
        if (_animator != null)
            _animator.SetTrigger("Attack");

        // Brief pause before slam
        if (_agent != null) _agent.isStopped = true;

        // Spawn warning indicator
        GameObject warning = CreateWarningCircle(transform.position, groundSlamRadius, slamWarningColor);
        yield return new WaitForSeconds(1.0f);

        // Deal damage
        if (_playerTransform != null)
        {
            float dist = Vector3.Distance(transform.position, _playerTransform.position);
            if (dist <= groundSlamRadius && _playerStats != null)
            {
                _playerStats.TakeDamage(groundSlamDamage);
                Debug.Log("[Boss] Ground Slam hit player for " + groundSlamDamage + " damage!");
            }
        }

        // Spawn slam VFX
        SpawnSlamVFX(transform.position);

        if (warning != null) Destroy(warning);
        if (_agent != null) _agent.isStopped = false;
    }

    IEnumerator ChargeAttack()
    {
        _chargeTimer = 0f;
        _isCharging = true;

        if (_animator != null)
            _animator.SetTrigger("Attack");

        // Store target position
        Vector3 targetPos = _playerTransform != null ? _playerTransform.position : transform.position;
        Vector3 chargeDir = (targetPos - transform.position).normalized;

        // Disable NavMeshAgent during charge
        if (_agent != null) _agent.enabled = false;

        // Spawn warning line
        GameObject warning = CreateWarningLine(transform.position, targetPos, chargeWarningColor);
        yield return new WaitForSeconds(0.5f);
        if (warning != null) Destroy(warning);

        // Charge forward
        float elapsed = 0f;
        bool hitPlayer = false;
        Rigidbody rb = GetComponent<Rigidbody>();

        while (elapsed < chargeDuration)
        {
            transform.position += chargeDir * chargeSpeed * Time.deltaTime;
            elapsed += Time.deltaTime;

            // Check if we hit the player
            if (!hitPlayer && _playerTransform != null)
            {
                float dist = Vector3.Distance(transform.position, _playerTransform.position);
                if (dist < 3f)
                {
                    hitPlayer = true;
                    if (_playerStats != null)
                    {
                        _playerStats.TakeDamage(chargeDamage);
                        Debug.Log("[Boss] Charge hit player for " + chargeDamage + " damage!");
                    }
                }
            }

            yield return null;
        }

        // Re-enable NavMeshAgent
        if (_agent != null)
        {
            _agent.enabled = true;
            if (_agent.isOnNavMesh)
                _agent.Warp(transform.position);
        }

        _isCharging = false;
    }

    void StompAttack()
    {
        _stompTimer = 0f;

        if (_animator != null)
            _animator.SetTrigger("Attack");

        if (_playerTransform != null)
        {
            float dist = Vector3.Distance(transform.position, _playerTransform.position);
            if (dist <= stompRadius && _playerStats != null)
            {
                _playerStats.TakeDamage(stompDamage);
                Debug.Log("[Boss] Stomp hit player for " + stompDamage + " damage!");
            }
        }

        // Small shockwave VFX
        SpawnStompVFX(transform.position);
    }

    // ── Visual Effects ──────────────────────────────────────────────────────

    GameObject CreateWarningCircle(Vector3 center, float radius, Color color)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = "SlamWarning";
        Destroy(go.GetComponent<Collider>());
        go.transform.position = center + Vector3.up * 0.1f;
        go.transform.localScale = new Vector3(radius * 2f, 0.02f, radius * 2f);

        Renderer r = go.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.SetFloat("_Mode", 3f);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.renderQueue = 3000;
        mat.color = color;
        r.material = mat;

        Destroy(go, 1.5f);
        return go;
    }

    GameObject CreateWarningLine(Vector3 from, Vector3 to, Color color)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "ChargeWarning";
        Destroy(go.GetComponent<Collider>());

        Vector3 mid = (from + to) / 2f;
        float dist = Vector3.Distance(from, to);
        go.transform.position = mid + Vector3.up * 0.15f;
        go.transform.localScale = new Vector3(1f, 0.05f, dist);
        go.transform.LookAt(to);

        Renderer r = go.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.SetFloat("_Mode", 3f);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.renderQueue = 3000;
        mat.color = color;
        r.material = mat;

        Destroy(go, 0.8f);
        return go;
    }

    void SpawnSlamVFX(Vector3 pos)
    {
        GameObject psGO = new GameObject("SlamVFX");
        psGO.transform.position = pos;
        ParticleSystem ps = psGO.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.duration = 0.3f;
        main.loop = false;
        main.startLifetime = 1.5f;
        main.startSpeed = 8f;
        main.startSize = 0.5f;
        main.startColor = new Color(0.8f, 0.3f, 0.1f, 1f);
        main.maxParticles = 100;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 1f;

        var emit = ps.emission;
        emit.rateOverTime = 0;
        emit.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 80) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = groundSlamRadius * 0.5f;

        var psr = psGO.GetComponent<ParticleSystemRenderer>();
        Shader shader = Shader.Find("Particles/Standard Unlit");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        if (shader != null)
        {
            Material mat = new Material(shader);
            mat.color = new Color(0.8f, 0.3f, 0.1f, 1f);
            psr.material = mat;
        }

        ps.Play();
        Destroy(psGO, 3f);
    }

    void SpawnStompVFX(Vector3 pos)
    {
        GameObject psGO = new GameObject("StompVFX");
        psGO.transform.position = pos;
        ParticleSystem ps = psGO.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.duration = 0.2f;
        main.loop = false;
        main.startLifetime = 0.8f;
        main.startSpeed = 5f;
        main.startSize = 0.3f;
        main.startColor = new Color(0.6f, 0.2f, 0.05f, 1f);
        main.maxParticles = 40;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 2f;

        var emit = ps.emission;
        emit.rateOverTime = 0;
        emit.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 30) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = stompRadius * 0.3f;

        var psr = psGO.GetComponent<ParticleSystemRenderer>();
        Shader shader = Shader.Find("Particles/Standard Unlit");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        if (shader != null)
        {
            Material mat = new Material(shader);
            mat.color = new Color(0.6f, 0.2f, 0.05f, 1f);
            psr.material = mat;
        }

        ps.Play();
        Destroy(psGO, 2f);
    }
}
