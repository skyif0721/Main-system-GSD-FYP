using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.XR;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class FixVRAndVerifyShop
{
    public static void Execute()
    {
        bool modified = false;

        // ── 1. DISABLE XR INTERACTION SIMULATOR ──────────────────────────────
        GameObject sim = GameObject.Find("XR Interaction Simulator");
        if (sim != null)
        {
            if (sim.activeSelf)
            {
                sim.SetActive(false);
                EditorUtility.SetDirty(sim);
                Debug.Log("[Fix] Disabled XR Interaction Simulator");
                modified = true;
            }
            else
            {
                Debug.Log("[OK] XR Interaction Simulator already disabled");
            }
        }

        // ── 2. ENSURE TrackedPoseDriver.ignoreTrackingState = true ───────────
        TrackedPoseDriver[] tpds = Object.FindObjectsOfType<TrackedPoseDriver>(true);
        foreach (var tpd in tpds)
        {
            if (!tpd.ignoreTrackingState)
            {
                tpd.ignoreTrackingState = true;
                EditorUtility.SetDirty(tpd);
                Debug.Log($"[Fix] TrackedPoseDriver.ignoreTrackingState=true on {tpd.gameObject.name}");
                modified = true;
            }
        }

        // ── 3. ENSURE XROrigin = Floor ────────────────────────────────────────
        XROrigin[] origins = Object.FindObjectsOfType<XROrigin>(true);
        foreach (var origin in origins)
        {
            if (origin.RequestedTrackingOriginMode != XROrigin.TrackingOriginMode.Floor)
            {
                origin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Floor;
                EditorUtility.SetDirty(origin);
                Debug.Log($"[Fix] XROrigin TrackingOriginMode=Floor on {origin.gameObject.name}");
                modified = true;
            }
        }

        // ── 4. VERIFY WeaponShopManager wiring ───────────────────────────────
        WeaponShopManager wsm = Object.FindObjectOfType<WeaponShopManager>();
        if (wsm != null)
        {
            Debug.Log($"[Shop] WeaponShopManager has {wsm.weapons.Count} weapons:");
            for (int i = 0; i < wsm.weapons.Count; i++)
            {
                var w = wsm.weapons[i];
                string goStatus  = w.weaponObject != null ? w.weaponObject.name : "NULL ❌";
                string rowStatus = w.rowUI        != null ? w.rowUI.name        : "NULL ❌";
                Debug.Log($"  [{i}] {w.weaponName} | GO={goStatus} | row={rowStatus}");

                // If weapon GO is missing, try to find it by name
                if (w.weaponObject == null && !string.IsNullOrEmpty(w.weaponName))
                {
                    var allGOs = Object.FindObjectsOfType<GameObject>(true);
                    foreach (var go in allGOs)
                    {
                        // Match by weapon name patterns
                        if (go.name.Contains(w.weaponName) || w.weaponName.Contains(go.name))
                        {
                            // Only pick objects that have a MeshRenderer (actual weapon mesh)
                            if (go.GetComponent<MeshRenderer>() != null || go.GetComponentInChildren<MeshRenderer>() != null)
                            {
                                w.weaponObject = go;
                                EditorUtility.SetDirty(wsm);
                                Debug.Log($"  [Fix] Auto-assigned {go.name} to weapon [{i}] {w.weaponName}");
                                modified = true;
                                break;
                            }
                        }
                    }
                }
            }

            // Verify spawn point
            Debug.Log($"[Shop] SpawnPoint: {(wsm.spawnPoint != null ? wsm.spawnPoint.name : "NULL ❌")}");
            Debug.Log($"[Shop] RightController: {(wsm.rightControllerTransform != null ? wsm.rightControllerTransform.name : "NULL ❌")}");
        }
        else
        {
            Debug.LogError("[Shop] WeaponShopManager NOT FOUND!");
        }

        // ── 5. VERIFY weapon GOs have XRGrabInteractable + correct attach ─────
        if (wsm != null)
        {
            foreach (var w in wsm.weapons)
            {
                if (w.weaponObject == null) continue;
                XRGrabInteractable grab = w.weaponObject.GetComponent<XRGrabInteractable>();
                if (grab == null)
                {
                    Debug.LogWarning($"[Shop] {w.weaponName} ({w.weaponObject.name}) has NO XRGrabInteractable!");
                }
                else
                {
                    string attachName = grab.attachTransform != null ? grab.attachTransform.name : "NULL";
                    string moveType   = grab.movementType.ToString();
                    Debug.Log($"[Shop] {w.weaponName}: grab OK | attach={attachName} | move={moveType} | trackRot={grab.trackRotation}");
                }
            }
        }

        if (modified)
        {
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[Done] Fixes applied and scene saved.");
        }
        else
        {
            Debug.Log("[Done] No changes needed.");
        }
    }
}
