using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

public class AddChangjianToShop
{
    public static void Execute()
    {
        // Find the ShopManager that holds WeaponShopManager
        WeaponShopManager wsm = Object.FindObjectOfType<WeaponShopManager>();
        if (wsm == null)
        {
            Debug.LogError("WeaponShopManager not found in scene!");
            return;
        }

        // Find the 长剑 GameObject in the scene
        GameObject changjian = GameObject.Find("长剑");
        if (changjian == null)
        {
            Debug.LogError("长剑 GameObject not found in scene!");
            return;
        }

        // Check if already added
        foreach (var w in wsm.weapons)
        {
            if (w.weaponObject == changjian || w.weaponName == "长剑")
            {
                Debug.Log("长剑 already in weapon list.");
                // Update handle offset for 长剑 (it's a longer sword, handle at bottom)
                w.handleOffset   = new Vector3(0f, 0f, 0f);
                w.handleRotation = new Vector3(0f, 0f, 0f);
                EditorUtility.SetDirty(wsm);
                EditorSceneManager.SaveOpenScenes();
                Debug.Log("Updated 长剑 handle settings.");
                return;
            }
        }

        // Add new entry
        var entry = new WeaponShopManager.WeaponEntry
        {
            weaponName     = "长剑",
            price          = 350,
            weaponObject   = changjian,
            rowUI          = null,   // Will need to be wired in UI setup
            handleOffset   = new Vector3(0f, 0f, 0f),
            handleRotation = new Vector3(0f, 0f, 0f)
        };

        wsm.weapons.Add(entry);
        EditorUtility.SetDirty(wsm);
        EditorSceneManager.SaveOpenScenes();

        Debug.Log($"[AddChangjianToShop] Added 长剑 to WeaponShopManager. Total weapons: {wsm.weapons.Count}");
    }
}
