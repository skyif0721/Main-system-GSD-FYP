using UnityEngine;
using UnityEditor;

public class UpdateMonsterBlockCollider
{
    public static void Execute()
    {
        GameObject monsterBlock = GameObject.Find("MonsterBlock");
        if (monsterBlock != null)
        {
            // Add a trigger collider for weapon hit detection
            BoxCollider triggerCollider = monsterBlock.AddComponent<BoxCollider>();
            triggerCollider.isTrigger = true;
            triggerCollider.size = new Vector3(1.2f, 1.2f, 1.2f); // Slightly larger than the visual mesh
            
            Debug.Log("Added trigger collider to MonsterBlock for better hit detection.");
        }
    }
}
