using UnityEngine;
using UnityEditor;
using UnityEngine.AI;

public class SetupGroundAndAI
{
    public static void Execute()
    {
        // 1. Snap XR Origin to ground
        GameObject xrOrigin = GameObject.Find("Complete XR Origin Set Up Variant");
        if (xrOrigin != null)
        {
            CharacterController cc = xrOrigin.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            Vector3 pos = xrOrigin.transform.position;
            
            // Try Raycast first for hard surfaces like platforms
            if (Physics.Raycast(pos + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 50f))
            {
                xrOrigin.transform.position = hit.point;
                Debug.Log("Snapped XR Origin to Collider at: " + hit.point);
            }
            else if (Terrain.activeTerrain != null)
            {
                pos.y = Terrain.activeTerrain.SampleHeight(pos) + Terrain.activeTerrain.transform.position.y;
                xrOrigin.transform.position = pos;
                Debug.Log("Snapped XR Origin to Terrain at: " + pos);
            }

            if (cc != null) cc.enabled = true;
        }

        // 2. Setup MonsterBlock AI
        GameObject monsterBlock = GameObject.Find("MonsterBlock");
        if (monsterBlock != null)
        {
            Rigidbody rb = monsterBlock.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

            NavMeshAgent agent = monsterBlock.GetComponent<NavMeshAgent>();
            if (agent == null)
            {
                agent = monsterBlock.AddComponent<NavMeshAgent>();
            }
            
            agent.speed = 2.5f;
            agent.stoppingDistance = 1.0f;

            // Snap MonsterBlock to NavMesh
            NavMeshHit navHit;
            if (NavMesh.SamplePosition(monsterBlock.transform.position, out navHit, 10f, NavMesh.AllAreas))
            {
                monsterBlock.transform.position = navHit.position;
                Debug.Log("Snapped MonsterBlock to NavMesh at: " + navHit.position);
            }
            else if (Physics.Raycast(monsterBlock.transform.position + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 50f))
            {
                monsterBlock.transform.position = hit.point;
                Debug.Log("Snapped MonsterBlock to Collider at: " + hit.point);
            }
        }
    }
}
