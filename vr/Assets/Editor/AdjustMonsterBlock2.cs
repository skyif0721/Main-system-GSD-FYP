using UnityEngine;
using UnityEditor;

public class AdjustMonsterBlock2
{
    public static void Execute()
    {
        GameObject monsterBlock = GameObject.Find("MonsterBlock");
        if (monsterBlock != null)
        {
            // Move it to a specific world position where it should be visible
            monsterBlock.transform.position = new Vector3(108.275f, 6.5f, 96.5f);
            Debug.Log("Adjusted MonsterBlock position to: " + monsterBlock.transform.position);
        }
    }
}
