using UnityEditor;
using UnityEngine;

public class AddWeaponTag
{
    [MenuItem("Tools/Add Weapon Tag")]
    public static void AddTag()
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        for (int i = 0; i < tagsProp.arraySize; i++)
            if (tagsProp.GetArrayElementAtIndex(i).stringValue == "Weapon") return;

        tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
        tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = "Weapon";
        tagManager.ApplyModifiedProperties();
        Debug.Log("[AddWeaponTag] 'Weapon' tag added.");
    }
}
