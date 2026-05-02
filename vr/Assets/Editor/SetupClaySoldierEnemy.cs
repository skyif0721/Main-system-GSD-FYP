using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine.AI;

public class SetupClaySoldierEnemy
{
    public static void Execute()
    {
        string folderPath = "Assets/Game object/character/Enemy/claySoldier";
        string animFolderPath = folderPath + "/New Folder";
        
        // 1. Create Animator Controller
        string controllerPath = folderPath + "/ClaySoldierController.controller";
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        
        // Add Parameters
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Hit", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Die", AnimatorControllerParameterType.Trigger);

        AnimatorStateMachine rootStateMachine = controller.layers[0].stateMachine;

        // Load Animation Clips
        AnimationClip idleClip = GetClip(animFolderPath + "/Idle (1).fbx");
        AnimationClip walkClip = GetClip(animFolderPath + "/Mutant Walking.fbx");
        AnimationClip attackClip = GetClip(animFolderPath + "/Fist Fight B.fbx");
        AnimationClip hitClip = GetClip(animFolderPath + "/Hit To Body.fbx");
        AnimationClip dieClip = GetClip(animFolderPath + "/Sword And Shield Death.fbx");

        // Create States
        AnimatorState idleState = rootStateMachine.AddState("Idle");
        idleState.motion = idleClip;
        
        AnimatorState walkState = rootStateMachine.AddState("Walk");
        walkState.motion = walkClip;
        
        AnimatorState attackState = rootStateMachine.AddState("Attack");
        attackState.motion = attackClip;
        
        AnimatorState hitState = rootStateMachine.AddState("Hit");
        hitState.motion = hitClip;
        
        AnimatorState dieState = rootStateMachine.AddState("Die");
        dieState.motion = dieClip;

        // Set Default State
        rootStateMachine.defaultState = idleState;

        // Transitions
        // Idle <-> Walk
        AnimatorStateTransition idleToWalk = idleState.AddTransition(walkState);
        idleToWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
        idleToWalk.hasExitTime = false;

        AnimatorStateTransition walkToIdle = walkState.AddTransition(idleState);
        walkToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
        walkToIdle.hasExitTime = false;

        // Any State -> Attack
        AnimatorStateTransition anyToAttack = rootStateMachine.AddAnyStateTransition(attackState);
        anyToAttack.AddCondition(AnimatorConditionMode.If, 0, "Attack");
        anyToAttack.hasExitTime = false;
        
        AnimatorStateTransition attackToIdle = attackState.AddTransition(idleState);
        attackToIdle.hasExitTime = true;
        attackToIdle.exitTime = 0.9f;

        // Any State -> Hit
        AnimatorStateTransition anyToHit = rootStateMachine.AddAnyStateTransition(hitState);
        anyToHit.AddCondition(AnimatorConditionMode.If, 0, "Hit");
        anyToHit.hasExitTime = false;
        
        AnimatorStateTransition hitToIdle = hitState.AddTransition(idleState);
        hitToIdle.hasExitTime = true;
        hitToIdle.exitTime = 0.9f;

        // Any State -> Die
        AnimatorStateTransition anyToDie = rootStateMachine.AddAnyStateTransition(dieState);
        anyToDie.AddCondition(AnimatorConditionMode.If, 0, "Die");
        anyToDie.hasExitTime = false;

        // 2. Create Prefab
        GameObject modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(folderPath + "/claySoldier.fbx");
        if (modelPrefab == null)
        {
            Debug.LogError("Could not find claySoldier.fbx");
            return;
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(modelPrefab);
        instance.name = "ClaySoldierEnemy";

        // Add Components
        Animator animator = instance.GetComponent<Animator>();
        if (animator == null) animator = instance.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller;
        animator.applyRootMotion = false;

        NavMeshAgent agent = instance.AddComponent<NavMeshAgent>();
        agent.speed = 3.5f;
        agent.stoppingDistance = 1.5f;

        CapsuleCollider collider = instance.AddComponent<CapsuleCollider>();
        collider.center = new Vector3(0, 1f, 0);
        collider.height = 2f;
        collider.radius = 0.5f;
        collider.isTrigger = true;

        MonsterBlock monsterBlock = instance.AddComponent<MonsterBlock>();
        monsterBlock.health = 50;
        monsterBlock.damageToPlayer = 10;
        monsterBlock.coinsToDrop = 20;
        monsterBlock.attackRange = 1.5f;
        monsterBlock.attackCooldown = 1.5f;

        Monster monster = instance.AddComponent<Monster>();
        monster.targetTime = 5f; // Time before disappearing after death

        // Save Prefab
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }
        string newPrefabPath = "Assets/Prefabs/ClaySoldierEnemy.prefab";
        GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(instance, newPrefabPath);
        
        // Clean up instance
        Object.DestroyImmediate(instance);

        // 3. Update Spawner
        GameObject spawnerObj = GameObject.Find("MonsterSpawner");
        if (spawnerObj != null)
        {
            MonsterSpawner spawner = spawnerObj.GetComponent<MonsterSpawner>();
            if (spawner != null)
            {
                spawner.monsterPrefab = savedPrefab;
                Debug.Log("Updated MonsterSpawner with ClaySoldierEnemy prefab.");
            }
        }

        Debug.Log("Successfully created ClaySoldierEnemy prefab and Animator Controller!");
    }

    private static AnimationClip GetClip(string path)
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
        foreach (Object asset in assets)
        {
            if (asset is AnimationClip && !asset.name.StartsWith("__preview__"))
            {
                return asset as AnimationClip;
            }
        }
        Debug.LogWarning("Could not find AnimationClip in " + path);
        return null;
    }
}
