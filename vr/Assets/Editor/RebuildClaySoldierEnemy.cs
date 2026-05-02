using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine.AI;

public class RebuildClaySoldierEnemy
{
    public static void Execute()
    {
        string folderPath = "Assets/Game object/character/Enemy/claySoldier";
        string animFolderPath = folderPath + "/New Folder";

        // 1. Delete old controller and recreate
        string controllerPath = folderPath + "/ClaySoldierController.controller";
        AssetDatabase.DeleteAsset(controllerPath);
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

        // Add Parameters
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Hit", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Die", AnimatorControllerParameterType.Trigger);

        AnimatorStateMachine rootStateMachine = controller.layers[0].stateMachine;

        // Load the correct 'mixamo.com' clips (NOT the 'Armature|...' one)
        AnimationClip idleClip    = GetClipByName(animFolderPath + "/Idle (1).fbx", "mixamo.com");
        AnimationClip walkClip    = GetClipByName(animFolderPath + "/Mutant Walking.fbx", "mixamo.com");
        AnimationClip attackClip  = GetClipByName(animFolderPath + "/Fist Fight B.fbx", "mixamo.com");
        AnimationClip hitClip     = GetClipByName(animFolderPath + "/Hit To Body.fbx", "mixamo.com");
        AnimationClip dieClip     = GetClipByName(animFolderPath + "/Sword And Shield Death.fbx", "mixamo.com");

        Debug.Log("Idle: " + (idleClip != null ? idleClip.name : "NULL"));
        Debug.Log("Walk: " + (walkClip != null ? walkClip.name : "NULL"));
        Debug.Log("Attack: " + (attackClip != null ? attackClip.name : "NULL"));
        Debug.Log("Hit: " + (hitClip != null ? hitClip.name : "NULL"));
        Debug.Log("Die: " + (dieClip != null ? dieClip.name : "NULL"));

        // Create States
        AnimatorState idleState   = rootStateMachine.AddState("Idle");
        idleState.motion = idleClip;

        AnimatorState walkState   = rootStateMachine.AddState("Walk");
        walkState.motion = walkClip;

        AnimatorState attackState = rootStateMachine.AddState("Attack");
        attackState.motion = attackClip;

        AnimatorState hitState    = rootStateMachine.AddState("Hit");
        hitState.motion = hitClip;

        AnimatorState dieState    = rootStateMachine.AddState("Die");
        dieState.motion = dieClip;

        // Set Default State
        rootStateMachine.defaultState = idleState;

        // Idle <-> Walk
        AnimatorStateTransition idleToWalk = idleState.AddTransition(walkState);
        idleToWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
        idleToWalk.hasExitTime = false;
        idleToWalk.duration = 0.1f;

        AnimatorStateTransition walkToIdle = walkState.AddTransition(idleState);
        walkToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
        walkToIdle.hasExitTime = false;
        walkToIdle.duration = 0.1f;

        // Any State -> Attack
        AnimatorStateTransition anyToAttack = rootStateMachine.AddAnyStateTransition(attackState);
        anyToAttack.AddCondition(AnimatorConditionMode.If, 0, "Attack");
        anyToAttack.hasExitTime = false;
        anyToAttack.duration = 0.05f;
        anyToAttack.canTransitionToSelf = false;

        AnimatorStateTransition attackToIdle = attackState.AddTransition(idleState);
        attackToIdle.hasExitTime = true;
        attackToIdle.exitTime = 0.9f;
        attackToIdle.duration = 0.1f;

        // Any State -> Hit
        AnimatorStateTransition anyToHit = rootStateMachine.AddAnyStateTransition(hitState);
        anyToHit.AddCondition(AnimatorConditionMode.If, 0, "Hit");
        anyToHit.hasExitTime = false;
        anyToHit.duration = 0.05f;
        anyToHit.canTransitionToSelf = false;

        AnimatorStateTransition hitToIdle = hitState.AddTransition(idleState);
        hitToIdle.hasExitTime = true;
        hitToIdle.exitTime = 0.9f;
        hitToIdle.duration = 0.1f;

        // Any State -> Die (no return)
        AnimatorStateTransition anyToDie = rootStateMachine.AddAnyStateTransition(dieState);
        anyToDie.AddCondition(AnimatorConditionMode.If, 0, "Die");
        anyToDie.hasExitTime = false;
        anyToDie.duration = 0.05f;
        anyToDie.canTransitionToSelf = false;

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();

        // 2. Rebuild Prefab
        string newPrefabPath = "Assets/Prefabs/ClaySoldierEnemy.prefab";
        AssetDatabase.DeleteAsset(newPrefabPath);

        GameObject modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(folderPath + "/claySoldier.fbx");
        if (modelPrefab == null)
        {
            Debug.LogError("Could not find claySoldier.fbx");
            return;
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(modelPrefab);
        instance.name = "ClaySoldierEnemy";

        // Animator
        Animator animator = instance.GetComponent<Animator>();
        if (animator == null) animator = instance.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller;
        animator.applyRootMotion = false;

        // NavMeshAgent
        NavMeshAgent agent = instance.AddComponent<NavMeshAgent>();
        agent.speed = 3.5f;
        agent.stoppingDistance = 1.5f;
        agent.radius = 0.4f;
        agent.height = 2f;
        agent.baseOffset = 0f;

        // Collider
        CapsuleCollider col = instance.AddComponent<CapsuleCollider>();
        col.center = new Vector3(0, 1f, 0);
        col.height = 2f;
        col.radius = 0.5f;
        col.isTrigger = true;

        // MonsterBlock
        MonsterBlock mb = instance.AddComponent<MonsterBlock>();
        mb.health = 50;
        mb.damageToPlayer = 10;
        mb.coinsToDrop = 20;
        mb.attackRange = 1.5f;
        mb.attackCooldown = 1.5f;

        // Save Prefab
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");

        GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(instance, newPrefabPath);
        Object.DestroyImmediate(instance);

        // 3. Update Spawner
        GameObject spawnerObj = GameObject.Find("MonsterSpawner");
        if (spawnerObj != null)
        {
            MonsterSpawner spawner = spawnerObj.GetComponent<MonsterSpawner>();
            if (spawner != null)
            {
                spawner.monsterPrefab = savedPrefab;
                EditorUtility.SetDirty(spawnerObj);
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Successfully rebuilt ClaySoldierEnemy with correct animation clips!");
    }

    private static AnimationClip GetClipByName(string path, string clipName)
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
        foreach (Object asset in assets)
        {
            if (asset is AnimationClip clip && clip.name == clipName)
                return clip;
        }
        // Fallback: return first non-preview clip
        foreach (Object asset in assets)
        {
            if (asset is AnimationClip clip && !clip.name.StartsWith("__preview__"))
                return clip;
        }
        Debug.LogWarning("No clip found in: " + path);
        return null;
    }
}
