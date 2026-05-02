using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public class FixWalkAnimation
{
    public static void Execute()
    {
        string controllerPath = "Assets/Game object/character/Enemy/claySoldier/ClaySoldierController.controller";
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);

        if (controller == null)
        {
            Debug.LogError("Controller not found!");
            return;
        }

        AnimatorStateMachine sm = controller.layers[0].stateMachine;

        // Log all states and transitions
        foreach (ChildAnimatorState state in sm.states)
        {
            AnimationClip clip = state.state.motion as AnimationClip;
            string clipInfo = clip != null ? $"clip='{clip.name}' loop={clip.isLooping}" : "NULL";
            Debug.Log($"State: {state.state.name} | {clipInfo}");

            foreach (AnimatorStateTransition t in state.state.transitions)
            {
                string conds = "";
                foreach (var c in t.conditions)
                    conds += $"[{c.parameter} {c.mode} {c.threshold}] ";
                Debug.Log($"  -> {t.destinationState?.name} | exitTime={t.hasExitTime}({t.exitTime}) | conditions: {conds}");
            }
        }

        // Fix: ensure Idle->Walk and Walk->Idle transitions use correct threshold
        AnimatorState idleState = null, walkState = null;
        foreach (ChildAnimatorState s in sm.states)
        {
            if (s.state.name == "Idle") idleState = s.state;
            if (s.state.name == "Walk") walkState = s.state;
        }

        if (idleState != null && walkState != null)
        {
            // Clear and re-add transitions
            idleState.transitions = new AnimatorStateTransition[0];
            walkState.transitions = new AnimatorStateTransition[0];

            // Idle -> Walk
            AnimatorStateTransition idleToWalk = idleState.AddTransition(walkState);
            idleToWalk.AddCondition(AnimatorConditionMode.Greater, 0.01f, "Speed");
            idleToWalk.hasExitTime = false;
            idleToWalk.duration = 0.1f;
            idleToWalk.offset = 0f;

            // Walk -> Idle
            AnimatorStateTransition walkToIdle = walkState.AddTransition(idleState);
            walkToIdle.AddCondition(AnimatorConditionMode.Less, 0.01f, "Speed");
            walkToIdle.hasExitTime = false;
            walkToIdle.duration = 0.1f;

            Debug.Log("Fixed Idle<->Walk transitions.");
        }

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();

        // Also fix the prefab: ensure applyRootMotion = false and NavMeshAgent updatePosition = true
        string prefabPath = "Assets/Prefabs/ClaySoldierEnemy.prefab";
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
        if (prefabRoot != null)
        {
            Animator anim = prefabRoot.GetComponent<Animator>();
            if (anim != null)
            {
                anim.applyRootMotion = false;
                Debug.Log("Set applyRootMotion = false");
            }

            UnityEngine.AI.NavMeshAgent agent = prefabRoot.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
            {
                agent.updatePosition = true;
                agent.updateRotation = true;
                Debug.Log("Set NavMeshAgent updatePosition/Rotation = true");
            }

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }

        Debug.Log("Walk animation fix complete!");
    }
}
