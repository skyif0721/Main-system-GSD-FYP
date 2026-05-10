using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

/// <summary>
/// Finalize the HP/MP bars:
///   - Remove the "HP"/"MP" text labels (no labels wanted)
///   - Wire PlayerStats.healthSlider / manaSlider to the existing sliders
///   - Mirror the same FrameBackground + HP_Slider + MP_Slider into
///     VR_HP_Canvas so the VR camera HUD also shows them
///   - Delete the old "HP" slider in PlayerUI (1) so it doesn't conflict
///   - DO NOT touch any sizes, anchors, or scales the user has set
/// </summary>
public class FinalizeBars
{
    [MenuItem("Tools/Finalize HP MP Bars")]
    public static void Run()
    {
        // ── Remove "HP"/"MP" text labels under the sliders ────────────────
        RemoveLabel("--- UI ---/PlayerUI/PlayerUICanvas/PlayerStatus/HP_Slider/Label");
        RemoveLabel("--- UI ---/PlayerUI/PlayerUICanvas/PlayerStatus/MP_Slider/Label");

        // ── Wire PlayerStats sliders ──────────────────────────────────────
        Slider hpSlider = FindByPath("--- UI ---/PlayerUI/PlayerUICanvas/PlayerStatus/HP_Slider")?.GetComponent<Slider>();
        Slider mpSlider = FindByPath("--- UI ---/PlayerUI/PlayerUICanvas/PlayerStatus/MP_Slider")?.GetComponent<Slider>();

        PlayerStats ps = Object.FindObjectOfType<PlayerStats>();
        if (ps != null)
        {
            if (hpSlider != null)
            {
                ps.healthSlider   = hpSlider;
                hpSlider.maxValue = ps.maxHealth;
                hpSlider.value    = ps.maxHealth;
            }
            if (mpSlider != null)
            {
                ps.manaSlider   = mpSlider;
                mpSlider.maxValue = ps.maxMana;
                mpSlider.value    = ps.maxMana;
            }
            EditorUtility.SetDirty(ps);
            Debug.Log("[FinalizeBars] PlayerStats sliders wired (PlayerUI/PlayerStatus).");
        }
        else
        {
            Debug.LogWarning("[FinalizeBars] PlayerStats not found.");
        }

        // ── Delete the old single HP slider in PlayerUI (1) (was the unused one) ─
        var oldHP = FindByPath("--- UI ---/PlayerUI (1)/PlayerUICanvas/PlayerStatus/HP");
        if (oldHP != null)
        {
            Object.DestroyImmediate(oldHP);
            Debug.Log("[FinalizeBars] Removed legacy HP slider in PlayerUI (1).");
        }

        // ── Mirror the bars to VR_HP_Canvas (so VR cam also shows them) ──
        MirrorToVRCanvas(ps);

        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("[FinalizeBars] Done!");
    }

    static GameObject FindByPath(string path)
    {
        // Walk the hierarchy by name (handles names with " / " spaces and slashes)
        var parts = path.Split('/');
        GameObject[] roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        Transform current = null;
        foreach (var root in roots)
        {
            if (root.name == parts[0]) { current = root.transform; break; }
        }
        if (current == null) return null;
        for (int i = 1; i < parts.Length; i++)
        {
            Transform next = null;
            for (int c = 0; c < current.childCount; c++)
            {
                if (current.GetChild(c).name == parts[i]) { next = current.GetChild(c); break; }
            }
            if (next == null) return null;
            current = next;
        }
        return current.gameObject;
    }

    static void RemoveLabel(string path)
    {
        var go = FindByPath(path);
        if (go != null)
        {
            Object.DestroyImmediate(go);
            Debug.Log($"[FinalizeBars] Removed label: {path}");
        }
    }

    /// <summary>
    /// Copy FrameBackground / HP_Slider / MP_Slider from the regular PlayerStatus
    /// into VR_HP_Canvas. Also wire its sliders to PlayerStats by adding a small
    /// MonoBehaviour mirror, OR (simpler) wire one of them directly. Since
    /// PlayerStats only supports one slider per stat, we instead keep the bars
    /// in BOTH UIs in sync via an updater script.
    /// </summary>
    static void MirrorToVRCanvas(PlayerStats ps)
    {
        var vrCanvas = FindByPath("--- PLAYER ---/Complete XR Origin Set Up Variant/Camera Offset/Main Camera/VR_HP_Canvas");
        if (vrCanvas == null)
        {
            Debug.LogWarning("[FinalizeBars] VR_HP_Canvas not found.");
            return;
        }

        // Source objects (the user-tuned ones)
        GameObject srcFrame = FindByPath("--- UI ---/PlayerUI/PlayerUICanvas/PlayerStatus/FrameBackground");
        GameObject srcHP    = FindByPath("--- UI ---/PlayerUI/PlayerUICanvas/PlayerStatus/HP_Slider");
        GameObject srcMP    = FindByPath("--- UI ---/PlayerUI/PlayerUICanvas/PlayerStatus/MP_Slider");
        if (srcFrame == null || srcHP == null || srcMP == null)
        {
            Debug.LogWarning("[FinalizeBars] Could not find source bars to mirror.");
            return;
        }

        // Wipe any existing children of VR canvas so we don't duplicate
        for (int i = vrCanvas.transform.childCount - 1; i >= 0; i--)
            Object.DestroyImmediate(vrCanvas.transform.GetChild(i).gameObject);

        // Duplicate the three components into the VR canvas, preserving their
        // relative RectTransform settings (anchors, pivots, sizes).
        GameObject frameCopy = (GameObject)Object.Instantiate(srcFrame, vrCanvas.transform, false);
        frameCopy.name = "FrameBackground";

        GameObject hpCopy = (GameObject)Object.Instantiate(srcHP, vrCanvas.transform, false);
        hpCopy.name = "HP_Slider";

        GameObject mpCopy = (GameObject)Object.Instantiate(srcMP, vrCanvas.transform, false);
        mpCopy.name = "MP_Slider";

        // Wire a "BarMirror" component that copies the slider values from the
        // master sliders every frame so both UIs stay in sync.
        AddOrUpdateMirror(hpCopy.GetComponent<Slider>(), srcHP.GetComponent<Slider>());
        AddOrUpdateMirror(mpCopy.GetComponent<Slider>(), srcMP.GetComponent<Slider>());

        Debug.Log("[FinalizeBars] Mirrored bars into VR_HP_Canvas.");
    }

    static void AddOrUpdateMirror(Slider mirror, Slider master)
    {
        if (mirror == null || master == null) return;
        var mb = mirror.GetComponent<SliderMirror>();
        if (mb == null) mb = mirror.gameObject.AddComponent<SliderMirror>();
        mb.master = master;
    }
}
