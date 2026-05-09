using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class FixWeaponHandles
{
    public static void Execute()
    {
        WeaponShopManager wsm = Object.FindObjectOfType<WeaponShopManager>();
        if (wsm == null) { Debug.LogError("WeaponShopManager not found!"); return; }

        // The 长剑 FBX is imported with X rotation = 90 degrees.
        // Its mesh longest axis is Y (height), so the blade goes up.
        // When held, we want the blade pointing forward (away from player).
        // Handle rotation: rotate -90 on X so blade points forward in hand.
        // Handle offset: move grip point to bottom quarter of the blade.
        for (int i = 0; i < wsm.weapons.Count; i++)
        {
            var w = wsm.weapons[i];
            if (w.weaponName == "长剑")
            {
                // 长剑 is rotated 90° on X at import, blade along Z in world space
                // Attach point: at the handle (bottom), blade pointing forward
                w.handleOffset   = new Vector3(0f, 0f, 0f);
                w.handleRotation = new Vector3(-90f, 0f, 0f); // align blade forward
                Debug.Log("[FixWeaponHandles] Set 长剑 handle rotation to (-90, 0, 0)");
            }
            else if (w.weaponName == "Untitled" || w.weaponName == "default")
            {
                w.handleOffset   = new Vector3(0f, 0f, 0f);
                w.handleRotation = new Vector3(0f, 0f, 0f);
            }
        }

        EditorUtility.SetDirty(wsm);
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("[FixWeaponHandles] Done. Scene saved.");
    }
}
