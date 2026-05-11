using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Highlights spawned monsters with a glowing ring and a pillar of light
/// so the player can see where they are. Attach to the MonsterSpawner
/// or a manager object in the training/shop scene.
/// </summary>
public class MonsterSpawnHighlight : MonoBehaviour
{
    [Header("Highlight Settings")]
    public Color highlightColor = new Color(1f, 0.3f, 0.2f, 0.6f);
    public float ringRadius = 1.2f;
    public float pillarHeight = 8f;
    public float pulseSpeed = 2f;

    [Header("Auto-detect")]
    public float checkInterval = 1f;

    private Dictionary<MonsterStat, GameObject> _highlights = new Dictionary<MonsterStat, GameObject>();
    private float _lastCheckTime;

    void Update()
    {
        if (Time.time - _lastCheckTime < checkInterval) return;
        _lastCheckTime = Time.time;

        // Find all monsters
        MonsterStat[] monsters = FindObjectsOfType<MonsterStat>();

        // Add highlights for new monsters
        foreach (var monster in monsters)
        {
            if (monster == null) continue;
            if (!_highlights.ContainsKey(monster))
            {
                GameObject highlight = CreateHighlight(monster.transform);
                _highlights[monster] = highlight;
            }
        }

        // Remove highlights for dead monsters
        List<MonsterStat> toRemove = new List<MonsterStat>();
        foreach (var kvp in _highlights)
        {
            if (kvp.Key == null || kvp.Key.health <= 0)
            {
                if (kvp.Value != null) Destroy(kvp.Value);
                toRemove.Add(kvp.Key);
            }
            else
            {
                // Update position
                if (kvp.Value != null)
                    kvp.Value.transform.position = kvp.Key.transform.position;
            }
        }
        foreach (var key in toRemove)
            _highlights.Remove(key);
    }

    GameObject CreateHighlight(Transform monster)
    {
        GameObject root = new GameObject("MonsterHighlight");
        root.transform.position = monster.position;

        // Ground ring
        GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.name = "Ring";
        ring.transform.SetParent(root.transform);
        ring.transform.localPosition = new Vector3(0f, 0.05f, 0f);
        ring.transform.localScale = new Vector3(ringRadius, 0.02f, ringRadius);
        Object.Destroy(ring.GetComponent<Collider>());

        Material ringMat = new Material(Shader.Find("Standard"));
        SetTransparent(ringMat, highlightColor);
        if (ringMat.HasProperty("_EmissionColor"))
        {
            ringMat.SetColor("_EmissionColor", highlightColor * 2f);
            ringMat.EnableKeyword("_EMISSION");
        }
        ring.GetComponent<Renderer>().material = ringMat;

        // Light pillar
        GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pillar.name = "Pillar";
        pillar.transform.SetParent(root.transform);
        pillar.transform.localPosition = new Vector3(0f, pillarHeight * 0.5f, 0f);
        pillar.transform.localScale = new Vector3(0.15f, pillarHeight * 0.5f, 0.15f);
        Object.Destroy(pillar.GetComponent<Collider>());

        Material pillarMat = new Material(Shader.Find("Standard"));
        Color pillarColor = highlightColor;
        pillarColor.a = 0.25f;
        SetTransparent(pillarMat, pillarColor);
        if (pillarMat.HasProperty("_EmissionColor"))
        {
            pillarMat.SetColor("_EmissionColor", highlightColor * 1.5f);
            pillarMat.EnableKeyword("_EMISSION");
        }
        pillar.GetComponent<Renderer>().material = pillarMat;

        // Pulse animation
        HighlightPulse pulse = root.AddComponent<HighlightPulse>();
        pulse.ringRenderer = ring.GetComponent<Renderer>();
        pulse.pillarRenderer = pillar.GetComponent<Renderer>();
        pulse.baseColor = highlightColor;
        pulse.pulseSpeed = pulseSpeed;

        return root;
    }

    void SetTransparent(Material mat, Color color)
    {
        mat.SetFloat("_Mode", 3f);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.renderQueue = 3000;
        mat.color = color;
    }
}

/// <summary>
/// Simple pulse animation for monster highlight rings/pillars.
/// </summary>
public class HighlightPulse : MonoBehaviour
{
    public Renderer ringRenderer;
    public Renderer pillarRenderer;
    public Color baseColor;
    public float pulseSpeed = 2f;

    void Update()
    {
        float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
        float alpha = Mathf.Lerp(0.2f, 0.7f, pulse);

        if (ringRenderer != null)
        {
            Color c = baseColor;
            c.a = alpha;
            ringRenderer.material.color = c;
        }

        if (pillarRenderer != null)
        {
            Color c = baseColor;
            c.a = alpha * 0.4f;
            pillarRenderer.material.color = c;
        }
    }
}
