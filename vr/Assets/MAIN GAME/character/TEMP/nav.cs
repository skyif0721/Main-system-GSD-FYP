using UnityEngine;
using UnityEngine.AI;

public class nav : MonoBehaviour
{
    public GameObject targetObject;  // 要跟随的物体
    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // 自动查找目标物体（通过标签）
        if (targetObject == null)
        {
            // 方法1：通过标签查找
            GameObject foundObj = GameObject.FindGameObjectWithTag("FollowTarget");
            if (foundObj != null)
                targetObject = foundObj;

            // 方法2：通过物体名称查找
            // targetObject = GameObject.Find("ObjectName");
        }
    }

    void Update()
    {
        if (targetObject != null)
            agent.SetDestination(targetObject.transform.position);
    }
}