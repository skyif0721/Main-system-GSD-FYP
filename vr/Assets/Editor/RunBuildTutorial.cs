using UnityEditor;

public static class RunBuildTutorial
{
    [MenuItem("Tools/Run Build Tutorial Now")]
    public static void Execute()
    {
        BuildTutorialScene.Build();
    }
}
