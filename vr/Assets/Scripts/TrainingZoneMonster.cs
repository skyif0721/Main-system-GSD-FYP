using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Training zone clay monster - stationary, no walking or attacking.
/// Only detects if the player punches/fists the monster.
/// Shows HP visual above the monster.
/// Respawns after being defeated.
/// </summary>
public class TrainingZoneMonster : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 100;
    public int currentHealth;
    public float respawnDelay = 3f;

    [Header("HP Visual")]
    public float hpBarWidth = 1.2f;
    public float hpBarHeight = 0.15f;
    public float hpBarYOffset = 2.2f;

    [Header("Hit Detection")]
    public float hitCooldown = 0.3f;
    public int fistDamage = 20;

    [Header("Highlight")]
    public Color highlightColor = new Color(1f, 0.8f, 0.2f, 1f);
    public float highlightPulseSpeed = 2f;

    // Runtime
    private GameObject _hpBarRoot;
    private Transform _hpBarFill;
    private Material _hpBarFillMat;
    private TextMeshPro _hpText;
    private float _lastHitTime;
    private bool _isDead;
    private Renderer[] _renderers;
    private Color[] _originalColors;
    private Material _highlightMat;
    private GameObject _highlightRing;

    void Start()
    {
        currentHealth = maxHealth;
        _renderers = GetComponentsInChildren<Renderer>();
        CacheOriginalColors();
        CreateHPBar();
        CreateHighlightRing();
        UpdateHPVisual();

        // Disable NavMeshAgent if present (no walking)
        var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null) agent.enabled = false;
    }

    void Update()
    {
        if (_isDead) return;

        // Make HP bar face camera
        if (_hpBarRoot != null && Camera.main != null)
        {
            _hpBarRoot.transform.rotation = Quaternion.LookRotation(
                _hpBarRoot.transform.position - Camera.main.transform.position);
        }

        // Pulse highlight
        if (_highlightRing != null)
        {
            float pulse = (Mathf.Sin(Time.time * highlightPulseSpeed) + 1f) * 0.5f;
            float scale = 1.5f + pulse * 0.5f;
            _highlightRing.transform.localScale = new Vector3(scale, 0.02f, scale);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (_isDead) return;
        TryDetectHit(collision.gameObject, collision.contacts.Length > 0 ? collision.contacts[0].point : transform.position);
    }

    void OnTriggerEnter(Collider other)
    {
        if (_isDead) return;
        TryDetectHit(other.gameObject, other.ClosestPoint(transform.position));
    }

    void TryDetectHit(GameObject obj, Vector3 hitPoint)
    {
        if (Time.time - _lastHitTime < hitCooldown) return;

        // Detect fist hit (controller or hand)
        bool isHit = false;
        int damage = fistDamage;

        // Check for controller/hand
        if (obj.name.Contains("Controller") || obj.name.Contains("Hand") ||
            obj.name.Contains("Poke") || obj.name.Contains("Direct"))
        {
            isHit = true;
        }

        // Check for weapon
        if (obj.CompareTag("Weapon") || obj.name.Contains("Sword") ||
            obj.name.Contains("Axe") || obj.name.Contains("Dagger"))
        {
            isHit = true;
            WeaponStats ws = obj.GetComponentInParent<WeaponStats>();
            if (ws != null) damage = ws.damage;
        }

        if (isHit)
        {
            _lastHitTime = Time.time;
            TakeDamage(damage, hitPoint);
        }
    }

    public void TakeDamage(int damage, Vector3 hitPoint = default)
    {
        if (_isDead) return;

        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        // Play punch SFX
        if (GameAudioManager.Instance != null)
            GameAudioManager.Instance.PlayPunch();

        // Trigger hit reaction animation
        Animator anim = GetComponent<Animator>();
        if (anim == null) anim = GetComponentInChildren<Animator>();
        if (anim != null && anim.runtimeAnimatorController != null)
        {
            anim.SetTrigger("IsHit");
        }

        // Floating damage text
        DamagePopupSpawner.Spawn(transform, damage);

        // Flash red
        StartCoroutine(FlashRed());

        UpdateHPVisual();

        Debug.Log($"[TrainingMonster] Hit for {damage}! HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void UpdateHPVisual()
    {
        if (_hpBarFill != null)
        {
            float ratio = (float)currentHealth / maxHealth;
            _hpBarFill.localScale = new Vector3(ratio, 1f, 1f);
            _hpBarFill.localPosition = new Vector3(-(1f - ratio) * hpBarWidth * 0.5f, 0f, -0.01f);

            // Color: green -> yellow -> red
            Color fillColor;
            if (ratio > 0.5f)
                fillColor = Color.Lerp(Color.yellow, Color.green, (ratio - 0.5f) * 2f);
            else
                fillColor = Color.Lerp(Color.red, Color.yellow, ratio * 2f);

            if (_hpBarFillMat != null)
                _hpBarFillMat.color = fillColor;
        }

        if (_hpText != null)
            _hpText.text = $"{currentHealth} / {maxHealth}";
    }

    void CreateHPBar()
    {
        _hpBarRoot = new GameObject("HPBar");
        _hpBarRoot.transform.SetParent(transform);
        _hpBarRoot.transform.localPosition = new Vector3(0f, hpBarYOffset, 0f);

        // Background
        GameObject bg = GameObject.CreatePrimitive(PrimitiveType.Quad);
        bg.name = "HPBarBG";
        bg.transform.SetParent(_hpBarRoot.transform);
        bg.transform.localPosition = Vector3.zero;
        bg.transform.localRotation = Quaternion.identity;
        bg.transform.localScale = new Vector3(hpBarWidth, hpBarHeight, 1f);
        Object.Destroy(bg.GetComponent<Collider>());
        Material bgMat = new Material(Shader.Find("Standard"));
        bgMat.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        bg.GetComponent<Renderer>().material = bgMat;

        // Fill
        GameObject fill = GameObject.CreatePrimitive(PrimitiveType.Quad);
        fill.name = "HPBarFill";
        fill.transform.SetParent(_hpBarRoot.transform);
        fill.transform.localPosition = new Vector3(0f, 0f, -0.01f);
        fill.transform.localRotation = Quaternion.identity;
        fill.transform.localScale = new Vector3(hpBarWidth, hpBarHeight, 1f);
        Object.Destroy(fill.GetComponent<Collider>());
        _hpBarFillMat = new Material(Shader.Find("Standard"));
        _hpBarFillMat.color = Color.green;
        fill.GetComponent<Renderer>().material = _hpBarFillMat;
        _hpBarFill = fill.transform;

        // HP Text
        GameObject textGO = new GameObject("HPText");
        textGO.transform.SetParent(_hpBarRoot.transform);
        textGO.transform.localPosition = new Vector3(0f, hpBarHeight + 0.05f, 0f);
        textGO.transform.localRotation = Quaternion.identity;
        _hpText = textGO.AddComponent<TextMeshPro>();
        _hpText.text = $"{maxHealth} / {maxHealth}";
        _hpText.fontSize = 2f;
        _hpText.fontStyle = FontStyles.Bold;
        _hpText.color = Color.white;
        _hpText.alignment = TextAlignmentOptions.Center;
        _hpText.rectTransform.sizeDelta = new Vector2(2f, 0.3f);
    }

    void CreateHighlightRing()
    {
        _highlightRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        _highlightRing.name = "HighlightRing";
        _highlightRing.transform.SetParent(transform);
        _highlightRing.transform.localPosition = new Vector3(0f, 0.05f, 0f);
        _highlightRing.transform.localScale = new Vector3(1.5f, 0.02f, 1.5f);
        Object.Destroy(_highlightRing.GetComponent<Collider>());

        Material mat = new Material(Shader.Find("Standard"));
        mat.SetFloat("_Mode", 3f);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.renderQueue = 3000;
        mat.color = new Color(highlightColor.r, highlightColor.g, highlightColor.b, 0.4f);
        if (mat.HasProperty("_EmissionColor"))
        {
            mat.SetColor("_EmissionColor", highlightColor * 1.5f);
            mat.EnableKeyword("_EMISSION");
        }
        _highlightRing.GetComponent<Renderer>().material = mat;
    }

    void CacheOriginalColors()
    {
        _originalColors = new Color[_renderers.Length];
        for (int i = 0; i < _renderers.Length; i++)
        {
            if (_renderers[i] != null && _renderers[i].material != null)
                _originalColors[i] = _renderers[i].material.color;
        }
    }

    IEnumerator FlashRed()
    {
        for (int i = 0; i < _renderers.Length; i++)
        {
            if (_renderers[i] != null && _renderers[i].material != null)
                _renderers[i].material.color = Color.red;
        }
        yield return new WaitForSeconds(0.1f);
        for (int i = 0; i < _renderers.Length; i++)
        {
            if (_renderers[i] != null && _renderers[i].material != null)
                _renderers[i].material.color = _originalColors[i];
        }
    }

    void Die()
    {
        _isDead = true;
        Debug.Log("[TrainingMonster] Defeated! Respawning...");

        // Drop coins
        if (GameAudioManager.Instance != null)
            GameAudioManager.Instance.PlayMonsterShout();

        // Add coins
        ShopManager.coins += 10;
        ShopManager[] managers = FindObjectsOfType<ShopManager>();
        foreach (var sm in managers)
            sm.DisplayNumber(ShopManager.coins);
        PlayerPrefs.SetInt("SavedCoins", ShopManager.coins);

        StartCoroutine(Respawn());
    }

    IEnumerator Respawn()
    {
        // Hide
        foreach (var r in _renderers)
        {
            if (r != null) r.enabled = false;
        }
        if (_hpBarRoot != null) _hpBarRoot.SetActive(false);
        if (_highlightRing != null) _highlightRing.SetActive(false);

        yield return new WaitForSeconds(respawnDelay);

        // Reset
        currentHealth = maxHealth;
        _isDead = false;
        foreach (var r in _renderers)
        {
            if (r != null) r.enabled = true;
        }
        if (_hpBarRoot != null) _hpBarRoot.SetActive(true);
        if (_highlightRing != null) _highlightRing.SetActive(true);
        UpdateHPVisual();

        Debug.Log("[TrainingMonster] Respawned!");
    }
}
