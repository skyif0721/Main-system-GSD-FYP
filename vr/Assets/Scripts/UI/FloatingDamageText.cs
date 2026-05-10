using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// One floating "-XX HP" text instance that fades + drifts upward then
/// destroys itself. Spawn via <see cref="DamagePopupSpawner"/>.
/// </summary>
public class FloatingDamageText : MonoBehaviour
{
    public TextMeshPro textMesh;
    public float moveSpeed = 1f;
    public float fadeSpeed = 1f;
    public float lifetime  = 1.2f;

    private Color _baseColor;
    private Transform _camTransform;

    void Awake()
    {
        if (textMesh == null) textMesh = GetComponent<TextMeshPro>();
        if (textMesh == null) textMesh = gameObject.AddComponent<TextMeshPro>();

        textMesh.alignment = TextAlignmentOptions.Center;
        if (textMesh.fontSize < 1f) textMesh.fontSize = 5f;
        textMesh.fontStyle = FontStyles.Bold;

        if (Camera.main != null) _camTransform = Camera.main.transform;
    }

    /// <summary>
    /// Display the popup for the given damage amount.
    /// If the text and color have already been set externally
    /// (by <see cref="DamagePopupSpawner"/>) those values are kept.
    /// </summary>
    public void Setup(int damageAmount)
    {
        // Only overwrite the text if it's empty – preserves "-25 HP" etc.
        if (string.IsNullOrEmpty(textMesh.text))
            textMesh.text = damageAmount.ToString();

        // Use whatever color was set on the text mesh as the base
        _baseColor = textMesh.color;
        if (_baseColor.a < 0.01f) _baseColor = Color.red;
        textMesh.color = _baseColor;

        StartCoroutine(AnimateAndDestroy());
    }

    void Update()
    {
        // Always face the camera (billboarding)
        if (_camTransform != null)
        {
            Vector3 toCam = _camTransform.position - transform.position;
            if (toCam.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(-toCam, Vector3.up);
        }
    }

    private IEnumerator AnimateAndDestroy()
    {
        float timer = 0f;
        Vector3 startPos = transform.position;
        while (timer < lifetime)
        {
            timer += Time.deltaTime;

            // Drift upward
            transform.position = startPos + Vector3.up * (moveSpeed * timer);

            // Fade out in the second half
            if (timer > lifetime * 0.5f)
            {
                float alpha = Mathf.Lerp(1f, 0f, (timer - lifetime * 0.5f) / (lifetime * 0.5f));
                Color c = _baseColor;
                c.a = alpha;
                textMesh.color = c;
            }

            yield return null;
        }
        Destroy(gameObject);
    }
}
