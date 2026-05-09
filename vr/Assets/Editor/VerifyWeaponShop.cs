using UnityEngine;
using UnityEditor;

public class VerifyWeaponShop
{
    public static void Execute()
    {
        WeaponShopManager wsm = Object.FindObjectOfType<WeaponShopManager>();
        if (wsm == null) { Debug.LogError("WeaponShopManager not found!"); return; }

        Debug.Log($"WeaponShopManager has {wsm.weapons.Count} weapons:");
        for (int i = 0; i < wsm.weapons.Count; i++)
        {
            var w = wsm.weapons[i];
            string goName  = w.weaponObject != null ? w.weaponObject.name : "NULL";
            string rowName = w.rowUI        != null ? w.rowUI.name        : "NULL";
            Debug.Log($"  [{i}] {w.weaponName} | price={w.price} | GO={goName} | row={rowName}");
        }

        Debug.Log($"SpawnPoint: {(wsm.spawnPoint != null ? wsm.spawnPoint.name : "NULL")}");
        Debug.Log($"RightController: {(wsm.rightControllerTransform != null ? wsm.rightControllerTransform.name : "NULL")}");
    }
}
