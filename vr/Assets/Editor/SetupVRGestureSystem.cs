using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor utility: sets up VRGestureDetector + VRGestureResponse on the player
/// and creates the shield visual. Run via Tools > Setup VR Gesture System.
/// </summary>
public class SetupVRGestureSystem
{
    [MenuItem("Tools/Setup VR Gesture System")]
    public static void Setup()
    {
        // ── 1. Find the XR player root ────────────────────────────────────────
        GameObject player = GameObject.Find("Complete XR Origin Set Up Variant");
        if (player == null)
        {
            Debug.LogError("[SetupVRGestureSystem] Could not find 'Complete XR Origin Set Up Variant'.");
            return;
        }

        // ── 2. Find controller and head transforms ────────────────────────────
        Transform leftCtrl  = FindDeepChild(player.transform, "Left Controller");
        Transform rightCtrl = FindDeepChild(player.transform, "Right Controller");
        Transform mainCam   = FindDeepChild(player.transform, "Main Camera");

        if (leftCtrl == null || rightCtrl == null || mainCam == null)
        {
            Debug.LogError("[SetupVRGestureSystem] Could not find Left Controller / Right Controller / Main Camera inside the XR rig.");
            return;
        }

        // ── 3. Add VRGestureDetector to player ────────────────────────────────
        VRGestureDetector detector = player.GetComponent<VRGestureDetector>();
        if (detector == null)
            detector = player.AddComponent<VRGestureDetector>();

        detector.leftController  = leftCtrl;
        detector.rightController = rightCtrl;
        detector.headTransform   = mainCam;

        // ── 4. Create Shield Visual (Sphere child of Main Camera) ─────────────
        // Remove old one if it exists
        Transform existingShield = mainCam.Find("ShieldVisual");
        if (existingShield != null)
            Object.DestroyImmediate(existingShield.gameObject);

        GameObject shield = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        shield.name = "ShieldVisual";
        shield.transform.SetParent(mainCam, false);
        shield.transform.localPosition = new Vector3(0f, -0.1f, 0.55f); // in front of face
        shield.transform.localScale    = new Vector3(0.55f, 0.45f, 0.05f); // flat disc shape

        // Remove collider so it doesn't interfere
        Object.DestroyImmediate(shield.GetComponent<SphereCollider>());

        // Assign transparent blue material
        Material shieldMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/ShieldMaterial.mat");
        if (shieldMat != null)
        {
            // Make sure material is transparent
            shieldMat.SetFloat("_Mode", 3);
            shieldMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            shieldMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            shieldMat.SetInt("_ZWrite", 0);
            shieldMat.DisableKeyword("_ALPHATEST_ON");
            shieldMat.EnableKeyword("_ALPHABLEND_ON");
            shieldMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            shieldMat.renderQueue = 3000;
            shield.GetComponent<Renderer>().material = shieldMat;
        }

        shield.SetActive(false); // hidden by default

        // ── 5. Add VRGestureResponse to player ────────────────────────────────
        VRGestureResponse response = player.GetComponent<VRGestureResponse>();
        if (response == null)
            response = player.AddComponent<VRGestureResponse>();

        response.detector      = detector;
        response.shieldVisual  = shield;

        // Find PlayerStats
        PlayerStats ps = player.GetComponent<PlayerStats>();
        if (ps == null) ps = player.GetComponentInChildren<PlayerStats>();
        response.playerStats = ps;

        // ── 6. Mark scene dirty ───────────────────────────────────────────────
        EditorUtility.SetDirty(player);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[SetupVRGestureSystem] ✅ Done! VRGestureDetector + VRGestureResponse added to player. ShieldVisual created under Main Camera.");
    }

    static Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            Transform found = FindDeepChild(child, name);
            if (found != null) return found;
        }
        return null;
    }
}
