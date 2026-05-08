using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DamageFlash : MonoBehaviour
{
    public static DamageFlash Instance;
    
    private Image flashImage;
    private Coroutine flashCoroutine;

    void Awake()
    {
        Instance = this;
        
        // Create a separate GameObject for the Canvas so it doesn't interfere with the Camera
        GameObject canvasObj = new GameObject("DamageFlashCanvas");
        canvasObj.transform.SetParent(transform, false);
        
        // Set up WorldSpace canvas for VR
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 100;
        
        RectTransform canvasRT = canvasObj.GetComponent<RectTransform>();
        canvasRT.sizeDelta = new Vector2(2f, 2f); // 2x2 meters
        canvasRT.localPosition = new Vector3(0f, 0f, 0.3f); // 30cm in front of camera
        
        GameObject imageObj = new GameObject("FlashImage");
        imageObj.transform.SetParent(canvasObj.transform, false);
        
        flashImage = imageObj.AddComponent<Image>();
        flashImage.color = new Color(1f, 0f, 0f, 0f); // Transparent red
        
        RectTransform rt = flashImage.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        
        // Make sure it doesn't block raycasts
        flashImage.raycastTarget = false;
    }

    public void Flash(float durationMs = 500f)
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }
        flashCoroutine = StartCoroutine(FlashRoutine(durationMs / 1000f));
    }

    private IEnumerator FlashRoutine(float durationSeconds)
    {
        float halfDuration = durationSeconds / 2f;
        
        // Fade in
        float timer = 0f;
        while (timer < halfDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 0.5f, timer / halfDuration);
            flashImage.color = new Color(1f, 0f, 0f, alpha);
            yield return null;
        }
        
        // Fade out
        timer = 0f;
        while (timer < halfDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(0.5f, 0f, timer / halfDuration);
            flashImage.color = new Color(1f, 0f, 0f, alpha);
            yield return null;
        }
        
        flashImage.color = new Color(1f, 0f, 0f, 0f);
    }
}
