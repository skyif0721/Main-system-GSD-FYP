using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

public class WireWeaponShop
{
    public static void Execute()
    {
        // Find WeaponShopManager
        WeaponShopManager wsm = Object.FindObjectOfType<WeaponShopManager>();
        if (wsm == null) { Debug.LogError("WeaponShopManager not found!"); return; }

        // ── Weapon GameObjects (the actual 3D objects under --- WEAPONS ---) ──
        // Map: (weaponName, sceneObjectName, parentName)
        var weaponDefs = new (string name, int price, string goName, string parentName)[]
        {
            ("Dagger",      20,  "01 Dagger.002",   "e Melee Weaponsapons"),
            ("Sword",       40,  "02 Sword.002",    "e Melee Weaponsapons"),
            ("Long Sword",  60,  "03 Long Sword.002","e Melee Weaponsapons"),
            ("Axe",         80,  "04 Axe.002",      "e Melee Weaponsapons"),
            ("Battleaxe",   100, "05 Battleaxe.002","e Melee Weaponsapons"),
            ("Mace",        120, "06 Mace.002",      "e Melee Weaponsapons"),
            ("Heavy Mace",  150, "07 Heavy Mace.002","e Melee Weaponsapons"),
            ("Hammer",      180, "08 Hammer.002",    "e Melee Weaponsapons"),
            ("Warhammer",   220, "09 Warhammer.002", "e Melee Weaponsapons"),
            ("Spear",       260, "10 Spear.002",     "e Melee Weaponsapons"),
            ("Halberd",     300, "11 Halberd.002",   "e Melee Weaponsapons"),
            ("长剑",         350, "长剑",              "--- WEAPONS ---"),
        };

        // ── UI Row GameObjects ──
        var rowNames = new string[]
        {
            "Row_00_Dagger",
            "Row_01_Sword",
            "Row_02_Long_Sword",
            "Row_03_Axe",
            "Row_04_Battleaxe",
            "Row_05_Mace",
            "Row_06_Heavy_Mace",
            "Row_07_Hammer",
            "Row_08_Warhammer",
            "Row_09_Spear",
            "Row_10_Halberd",
            "Row_11_Changjian",  // may not exist yet, will skip if missing
        };

        // Find the scroll content parent
        GameObject content = GameObject.Find("Content");
        if (content == null)
        {
            // Try finding by path
            var allGOs = Object.FindObjectsOfType<GameObject>(true);
            foreach (var go in allGOs)
            {
                if (go.name == "Content" && go.transform.parent != null &&
                    go.transform.parent.name == "Viewport")
                {
                    content = go;
                    break;
                }
            }
        }

        if (content == null) { Debug.LogError("Could not find ScrollArea Content!"); return; }

        // Build new weapons list
        var newWeapons = new List<WeaponShopManager.WeaponEntry>();

        for (int i = 0; i < weaponDefs.Length; i++)
        {
            var def = weaponDefs[i];

            // Find weapon GO
            GameObject weaponGO = null;
            var allGOs = Object.FindObjectsOfType<GameObject>(true);
            foreach (var go in allGOs)
            {
                if (go.name == def.goName && go.transform.parent != null &&
                    go.transform.parent.name == def.parentName)
                {
                    weaponGO = go;
                    break;
                }
            }
            // Fallback: search by name only
            if (weaponGO == null)
            {
                foreach (var go in allGOs)
                {
                    if (go.name == def.goName)
                    {
                        weaponGO = go;
                        break;
                    }
                }
            }

            // Find row UI
            GameObject rowUI = null;
            if (i < rowNames.Length)
            {
                Transform rowT = content.transform.Find(rowNames[i]);
                if (rowT != null) rowUI = rowT.gameObject;
            }

            var entry = new WeaponShopManager.WeaponEntry
            {
                weaponName     = def.name,
                price          = def.price,
                weaponObject   = weaponGO,
                rowUI          = rowUI,
                handleOffset   = Vector3.zero,
                handleRotation = Vector3.zero,
            };

            newWeapons.Add(entry);

            string goStatus  = weaponGO != null ? "OK" : "MISSING";
            string rowStatus = rowUI    != null ? "OK" : "MISSING";
            Debug.Log($"[WireWeaponShop] [{i}] {def.name} | weaponGO={goStatus} | rowUI={rowStatus}");
        }

        wsm.weapons = newWeapons;
        EditorUtility.SetDirty(wsm);
        EditorSceneManager.SaveOpenScenes();
        Debug.Log($"[WireWeaponShop] Done. Wired {newWeapons.Count} weapons. Scene saved.");
    }
}
