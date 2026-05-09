using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class FixAllWeaponGrab
{
    public static void Execute()
    {
        int fixed_count = 0;

        // All weapon parent GameObjects that need XRGrabInteractable configured
        // Each has a child "GameObject" that is the manually placed attach point
        string[] weaponPaths = new string[]
        {
            "--- WEAPONS ---/Untitled",
            "--- WEAPONS ---/长剑",
            "--- WEAPONS ---/Simple Melee Weapons/01 Dagger.002",
            "--- WEAPONS ---/Simple Melee Weapons/02 Sword.002",
            "--- WEAPONS ---/Simple Melee Weapons/03 Long Sword.002",
            "--- WEAPONS ---/Simple Melee Weapons/04 Axe.002",
            "--- WEAPONS ---/Simple Melee Weapons/05 Battleaxe.002",
            "--- WEAPONS ---/Simple Melee Weapons/06 Mace.002",
            "--- WEAPONS ---/Simple Melee Weapons/07 Heavy Mace.002",
            "--- WEAPONS ---/Simple Melee Weapons/08 Hammer.002",
            "--- WEAPONS ---/Simple Melee Weapons/09 Warhammer.002",
            "--- WEAPONS ---/Simple Melee Weapons/10 Spear.002",
            "--- WEAPONS ---/Simple Melee Weapons/11 Halberd.002",
        };

        foreach (string path in weaponPaths)
        {
            // Find by name (last segment of path)
            string name = path.Contains("/") ? path.Substring(path.LastIndexOf('/') + 1) : path;
            GameObject[] all = Object.FindObjectsOfType<GameObject>(true);
            GameObject weaponGO = null;
            foreach (var go in all)
            {
                if (go.name == name && go.transform.parent != null)
                {
                    weaponGO = go;
                    break;
                }
            }

            if (weaponGO == null)
            {
                Debug.LogWarning($"[FixAllWeaponGrab] Could not find: {name}");
                continue;
            }

            // Find the child attach point (named "GameObject", "w", or "default")
            Transform attachPoint = weaponGO.transform.Find("GameObject");
            if (attachPoint == null) attachPoint = weaponGO.transform.Find("w");
            if (attachPoint == null) attachPoint = weaponGO.transform.Find("default");

            // Ensure Rigidbody exists
            Rigidbody rb = weaponGO.GetComponent<Rigidbody>();
            if (rb == null) rb = weaponGO.AddComponent<Rigidbody>();
            rb.useGravity  = true;
            rb.isKinematic = false;
            rb.mass        = 0.5f;
            rb.drag        = 2f;
            rb.angularDrag = 5f;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            EditorUtility.SetDirty(rb);

            // Ensure Collider exists (add BoxCollider if none)
            Collider col = weaponGO.GetComponentInChildren<Collider>();
            if (col == null)
            {
                BoxCollider bc = weaponGO.AddComponent<BoxCollider>();
                bc.isTrigger = false;
                EditorUtility.SetDirty(bc);
            }

            // Add or get XRGrabInteractable
            XRGrabInteractable grab = weaponGO.GetComponent<XRGrabInteractable>();
            if (grab == null) grab = weaponGO.AddComponent<XRGrabInteractable>();

            // --- Key settings ---
            // VelocityTracking: weapon follows hand movement naturally (not teleport)
            grab.movementType           = XRBaseInteractable.MovementType.VelocityTracking;
            grab.trackPosition          = true;
            grab.trackRotation          = false;   // No spinning
            grab.throwOnDetach          = true;
            grab.throwSmoothingDuration = 0.1f;
            grab.throwVelocityScale     = 1.5f;
            grab.useDynamicAttach       = false;

            // Wire up the manually placed attach point
            if (attachPoint != null)
            {
                grab.attachTransform = attachPoint;
                Debug.Log($"[FixAllWeaponGrab] {name}: attachTransform = {attachPoint.name} at {attachPoint.localPosition}");
            }
            else
            {
                Debug.LogWarning($"[FixAllWeaponGrab] {name}: No attach point child found!");
            }

            EditorUtility.SetDirty(grab);
            fixed_count++;
        }

        // Also fix coin
        GameObject coin = GameObject.Find("coin");
        if (coin != null)
        {
            XRGrabInteractable coinGrab = coin.GetComponent<XRGrabInteractable>();
            if (coinGrab != null)
            {
                coinGrab.movementType  = XRBaseInteractable.MovementType.VelocityTracking;
                coinGrab.trackPosition = true;
                coinGrab.trackRotation = false;
                Rigidbody coinRb = coin.GetComponent<Rigidbody>();
                if (coinRb != null)
                {
                    coinRb.constraints = RigidbodyConstraints.FreezeRotation;
                    EditorUtility.SetDirty(coinRb);
                }
                EditorUtility.SetDirty(coinGrab);
                Debug.Log("[FixAllWeaponGrab] coin: VelocityTracking, no spin");
                fixed_count++;
            }
        }

        EditorSceneManager.SaveOpenScenes();
        Debug.Log($"[FixAllWeaponGrab] Done. Fixed {fixed_count} objects. Scene saved.");
    }
}
