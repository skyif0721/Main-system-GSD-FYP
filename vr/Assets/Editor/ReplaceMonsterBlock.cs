using UnityEngine;
using UnityEditor;

public class ReplaceMonsterBlock
{
    public static void Execute()
    {
        GameObject monsterBlock = GameObject.Find("MonsterBlock");
        GameObject runningNPC = GameObject.Find("Running (3)");

        if (monsterBlock != null && runningNPC != null)
        {
            // 1. Copy the MonsterBlock script to the NPC
            MonsterBlock oldScript = monsterBlock.GetComponent<MonsterBlock>();
            MonsterBlock newScript = runningNPC.AddComponent<MonsterBlock>();
            
            // Copy values
            newScript.health = oldScript.health;
            newScript.damageToPlayer = oldScript.damageToPlayer;
            newScript.coinsToDrop = oldScript.coinsToDrop;
            newScript.attackRange = oldScript.attackRange;
            newScript.attackCooldown = oldScript.attackCooldown;

            // 2. Ensure the NPC has a NavMeshAgent (it probably does, but just in case)
            UnityEngine.AI.NavMeshAgent agent = runningNPC.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent == null)
            {
                agent = runningNPC.AddComponent<UnityEngine.AI.NavMeshAgent>();
            }

            // 3. Ensure the NPC has a Collider for hit detection
            Collider col = runningNPC.GetComponent<Collider>();
            if (col == null)
            {
                CapsuleCollider capCol = runningNPC.AddComponent<CapsuleCollider>();
                capCol.height = 2f;
                capCol.radius = 0.5f;
                capCol.center = new Vector3(0, 1f, 0);
                capCol.isTrigger = true; // For weapon hits
            }
            else
            {
                col.isTrigger = true;
            }

            // 4. Disable or Destroy the old red block
            monsterBlock.SetActive(false);
            
            // Rename the NPC so it's clear it's the monster now
            runningNPC.name = "Monster_RunningNPC";

            Debug.Log("Successfully replaced the red block with the Running NPC!");
        }
        else
        {
            if (monsterBlock == null) Debug.LogError("Could not find MonsterBlock");
            if (runningNPC == null) Debug.LogError("Could not find Running (3)");
        }
    }
}
