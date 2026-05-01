using UnityEngine;
using UnityEditor;

public class AdjustMonsterBlock3
{
    public static void Execute()
    {
        GameObject monsterBlock = GameObject.Find("MonsterBlock");
        if (monsterBlock != null)
        {
            // Move it closer and higher
            monsterBlock.transform.position = new Vector3(108.275f, 7.5f, 95.5f);
            Debug.Log("Adjusted MonsterBlock position to: " + monsterBlock.transform.position);
        }
    }
}
