using UnityEngine;
using TMPro;
using System.Collections;

public class FloatingDamageText : MonoBehaviour
{
    public TextMeshPro textMesh;
    public float moveSpeed = 1f;
    public float fadeSpeed = 1f;
    public float lifetime = 1.5f;
    
    private Color textColor;
    private Transform mainCamera;

    void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
        if (textMesh == null)
        {
            textMesh = gameObject.AddComponent<TextMeshPro>();
        }
        
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.fontSize = 5f;
        textMesh.fontStyle = FontStyles.Bold;
        
        if (Camera.main != null)
        {
            mainCamera = Camera.main.transform;
        }
    }

    public void Setup(int damageAmount)
    {
        textMesh.text = damageAmount.ToString();
        textColor = Color.red;
        textMesh.color = textColor;
        
        StartCoroutine(AnimateAndDestroy());
    }

    void Update()
    {
        // Always face the camera
        if (mainCamera != null)
        {
            transform.LookAt(transform.position + mainCamera.forward);
        }
    }

    private IEnumerator AnimateAndDestroy()
    {
        float timer = 0f;
        
        while (timer < lifetime)
        {
            timer += Time.deltaTime;
            
            // Move up
            transform.position += Vector3.up * moveSpeed * Time.deltaTime;
            
            // Fade out in the second half of lifetime
            if (timer > lifetime / 2f)
            {
                float alpha = Mathf.Lerp(1f, 0f, (timer - (lifetime / 2f)) / (lifetime / 2f));
                textColor.a = alpha;
                textMesh.color = textColor;
            }
            
            yield return null;
        }
        
        Destroy(gameObject);
    }
}
