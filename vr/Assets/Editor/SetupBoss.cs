using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class SetupBoss
{
    [MenuItem("Tools/Setup Boss Monster")]
    public static void Execute()
    {
        GameObject boss = GameObject.Find("Boss_ClaySoldier");
        if (boss == null)
        {
            Debug.LogError("[SetupBoss] Boss_ClaySoldier not found!");
            return;
        }

        // Scale: make it HUGE
        boss.transform.localScale = new Vector3(30f, 30f, 30f);
        Debug.Log("[SetupBoss] Boss scaled to 30x.");

        // Stats: 500 HP, 25 damage
        MonsterStat stat = boss.GetComponent<MonsterStat>();
        if (stat != null)
        {
            stat.health = 500;
            stat.damageToPlayer = 25;
            stat.coinsToDrop = 200;
            stat.attackRange = 5f;
            stat.attackCooldown = 2.0f;
            EditorUtility.SetDirty(stat);
            Debug.Log("[SetupBoss] Boss stats: 500 HP, 25 DMG, 200 coins.");
        }

        // NavMeshAgent: bigger, faster
        var agent = boss.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            agent.speed = 5f;
            agent.acceleration = 12f;
            agent.stoppingDistance = 4f;
            agent.radius = 1.5f;
            agent.height = 4f;
            EditorUtility.SetDirty(agent);
        }

        // Collider: bigger
        var capsule = boss.GetComponent<CapsuleCollider>();
        if (capsule != null)
        {
            capsule.radius = 1.0f;
            capsule.height = 3.0f;
            capsule.center = new Vector3(0f, 1.5f, 0f);
            EditorUtility.SetDirty(capsule);
        }

        // BossAttackController
        BossAttackController bac = boss.GetComponent<BossAttackController>();
        if (bac == null)
            bac = boss.AddComponent<BossAttackController>();
        bac.monsterStat = stat;
        bac.groundSlamRadius = 10f;
        bac.groundSlamDamage = 30;
        bac.groundSlamCooldown = 8f;
        bac.chargeSpeed = 18f;
        bac.chargeDuration = 1.5f;
        bac.chargeDamage = 35;
        bac.chargeCooldown = 12f;
        bac.stompRadius = 6f;
        bac.stompDamage = 20;
        bac.stompCooldown = 5f;
        EditorUtility.SetDirty(bac);
        Debug.Log("[SetupBoss] BossAttackController added with slam/charge/stomp.");

        // BossTrap: stronger
        BossTrap trap = boss.GetComponent<BossTrap>();
        if (trap != null)
        {
            trap.trapDamage = 25;
            trap.spawnInterval = 6f;
            trap.trapsPerSpawn = 6;
            trap.minSpawnRadius = 4f;
            trap.maxSpawnRadius = 15f;
            EditorUtility.SetDirty(trap);
        }

        // Change boss color to dark crimson
        ChangeBossColor(boss);

        // Wire BossHealthBarUI
        WireBossHealthBar(stat);

        // Save
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("[SetupBoss] Boss setup complete! 500 HP, dark crimson, with attacks.");
    }

    static void ChangeBossColor(GameObject boss)
    {
        SkinnedMeshRenderer smr = boss.GetComponentInChildren<SkinnedMeshRenderer>();
        if (smr == null)
        {
            Debug.LogWarning("[SetupBoss] No SkinnedMeshRenderer found on boss!");
            return;
        }

        Material bossMat = new Material(Shader.Find("Standard"));
        bossMat.name = "BossCrimsonMaterial";
        bossMat.color = new Color(0.5f, 0.05f, 0.05f, 1f);
        bossMat.SetColor("_EmissionColor", new Color(0.3f, 0.02f, 0.02f, 1f));
        bossMat.EnableKeyword("_EMISSION");
        bossMat.SetFloat("_Metallic", 0.3f);
        bossMat.SetFloat("_Glossiness", 0.6f);

        string matPath = "Assets/Materials/BossCrimsonMaterial.mat";
        if (!System.IO.Directory.Exists("Assets/Materials"))
            System.IO.Directory.CreateDirectory("Assets/Materials");

        AssetDatabase.CreateAsset(bossMat, matPath);
        AssetDatabase.Refresh();

        Material savedMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (savedMat != null)
        {
            Material[] mats = new Material[smr.sharedMaterials.Length];
            for (int i = 0; i < mats.Length; i++)
                mats[i] = savedMat;
            smr.sharedMaterials = mats;
            EditorUtility.SetDirty(smr);
            Debug.Log("[SetupBoss] Boss color changed to dark crimson.");
        }
    }

    static void WireBossHealthBar(MonsterStat stat)
    {
        BossHealthBarUI bossUI = Object.FindObjectOfType<BossHealthBarUI>(true);
        if (bossUI == null)
        {
            Debug.LogWarning("[SetupBoss] BossHealthBarUI not found!");
            return;
        }

        bossUI.bossDisplayName = "CLAY GOLEM";
        bossUI.autoFindMonster = true;

        if (bossUI.healthSlider != null)
        {
            bossUI.healthSlider.maxValue = stat != null ? stat.health : 500;
            bossUI.healthSlider.value = bossUI.healthSlider.maxValue;
        }

        if (bossUI.bossNameText != null)
            bossUI.bossNameText.text = "CLAY GOLEM";

        if (bossUI.healthText != null)
            bossUI.healthText.text = "500 / 500";

        EditorUtility.SetDirty(bossUI);
        Debug.Log("[SetupBoss] BossHealthBarUI wired to CLAY GOLEM.");
    }
}
