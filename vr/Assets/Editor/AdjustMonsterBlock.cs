using UnityEngine;
using UnityEditor;

public class AdjustMonsterBlock
{
    public static void Execute()
    {
        GameObject monsterBlock = GameObject.Find("MonsterBlock");
        if (monsterBlock != null)
        {
            // Move it slightly further away and higher so it's visible
            GameObject xrOrigin = GameObject.Find("Complete XR Origin Set Up Variant");
            if (xrOrigin != null)
            {
                monsterBlock.transform.position = xrOrigin.transform.position + xrOrigin.transform.forward * 3f + Vector3.up * 1.5f;
            }
            
            // Ensure it has a collider
            if (monsterBlock.GetComponent<Collider>() == null)
            {
                monsterBlock.AddComponent<BoxCollider>();
            }
            
            Debug.Log("Adjusted MonsterBlock position.");
        }
    }
}
