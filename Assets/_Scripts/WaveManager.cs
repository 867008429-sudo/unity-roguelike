using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public Transform[] spawnPoints;
    public int currentWave;
    public int aliveEnemies;
    public bool waveInProgress;
    public GameObject skeletonPrefab;
    public GameObject slimePrefab;

    private readonly List<GameObject> spawnedEnemies = new List<GameObject>();
    private UIManager uiManager;
    private bool awaitingPostBossReward;

    private void Start()
    {
        uiManager = FindObjectOfType<UIManager>();
        Invoke(nameof(StartNextWave), 1f);
    }

    public void StartNextWave()
    {
        awaitingPostBossReward = false;

        if (GameManager.Instance != null && GameManager.Instance.isGameOver)
        {
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("WaveManager: No spawn points assigned.");
            return;
        }

        currentWave++;

        int skeletonCount = currentWave == 1
            ? GameConfig.Wave1SkeletonCount
            : currentWave == 2
                ? GameConfig.Wave2SkeletonCount
                : GameConfig.Wave2SkeletonCount + (currentWave - 2) * GameConfig.SkeletonIncrement;

        int slimeCount = currentWave == 1
            ? GameConfig.Wave1SlimeCount
            : currentWave == 2
                ? GameConfig.Wave2SlimeCount
                : GameConfig.Wave2SlimeCount + (currentWave - 2) * GameConfig.SlimeIncrement;

        float healthMultiplier = 1f + (currentWave - 1) * GameConfig.WaveHPScaling;
        float attackMultiplier = 1f + (currentWave - 1) * GameConfig.WaveATKScaling;
        bool bossWave = currentWave % 5 == 0;

        if (uiManager != null)
        {
            uiManager.ShowWaveNotification(GetWaveIntroMessage(bossWave));
        }

        List<int> availableSpawnIndices = new List<int>();
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            availableSpawnIndices.Add(i);
        }

        for (int i = 0; i < skeletonCount; i++)
        {
            SpawnEnemy(skeletonPrefab, EnemyStats.EnemyType.Skeleton, healthMultiplier, attackMultiplier, availableSpawnIndices);
        }

        for (int i = 0; i < slimeCount; i++)
        {
            SpawnEnemy(slimePrefab, EnemyStats.EnemyType.Slime, healthMultiplier, attackMultiplier, availableSpawnIndices);
        }

        if (bossWave)
        {
            SpawnBoss(healthMultiplier, attackMultiplier, availableSpawnIndices);
        }

        waveInProgress = true;
    }

    private void SpawnEnemy(
        GameObject prefab,
        EnemyStats.EnemyType enemyType,
        float healthMultiplier,
        float attackMultiplier,
        List<int> availableSpawnIndices)
    {
        int index = availableSpawnIndices[0];
        availableSpawnIndices.RemoveAt(0);
        if (availableSpawnIndices.Count == 0)
        {
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                availableSpawnIndices.Add(i);
            }
        }

        Vector3 spawnPosition = spawnPoints[index].position + new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
        GameObject enemy = prefab != null
            ? Instantiate(prefab, spawnPosition, Quaternion.identity)
            : CreateFallbackEnemy(enemyType, spawnPosition);

        enemy.tag = "Enemy";

        EnemyStats stats = enemy.GetComponent<EnemyStats>();
        if (stats == null)
        {
            stats = enemy.AddComponent<EnemyStats>();
        }

        if (enemyType == EnemyStats.EnemyType.Skeleton)
        {
            stats.enemyName = "Skeleton";
            stats.enemyType = EnemyStats.EnemyType.Skeleton;
            stats.maxHP = GameConfig.SkeletonMaxHP * healthMultiplier;
            stats.atk = GameConfig.SkeletonATK * attackMultiplier;
            stats.def = GameConfig.SkeletonDEF;
            stats.xpReward = GameConfig.SkeletonXPReward;
            stats.goldMin = GameConfig.SkeletonGoldMin;
            stats.goldMax = GameConfig.SkeletonGoldMax;
            stats.potionDropRate = GameConfig.SkeletonPotionRate;
        }
        else
        {
            stats.enemyName = "Slime";
            stats.enemyType = EnemyStats.EnemyType.Slime;
            stats.maxHP = GameConfig.SlimeMaxHP * healthMultiplier;
            stats.atk = GameConfig.SlimeATK * attackMultiplier;
            stats.def = GameConfig.SlimeDEF;
            stats.xpReward = GameConfig.SlimeXPReward;
            stats.goldMin = GameConfig.SlimeGoldMin;
            stats.goldMax = GameConfig.SlimeGoldMax;
            stats.potionDropRate = GameConfig.SlimePotionRate;
        }

        if (ShouldBecomeElite())
        {
            MakeElite(enemy, stats);
        }

        stats.currentHP = stats.maxHP;

        GameObject healthBarObject = new GameObject("EnemyHealthBar");
        healthBarObject.transform.SetParent(enemy.transform);
        WorldHealthBar healthBar = healthBarObject.AddComponent<WorldHealthBar>();
        healthBar.target = enemy.transform;
        healthBar.showName = true;
        healthBar.displayName = stats.enemyName;
        healthBar.showOnlyWhenDamaged = true;
        healthBar.offset = enemyType == EnemyStats.EnemyType.Skeleton ? new Vector3(0f, 1.5f, 0f) : new Vector3(0f, 0.8f, 0f);
        healthBar.barWidth = 1.2f;
        healthBar.barHeight = 0.12f;
        healthBar.Initialize(stats.maxHP, stats.enemyName);
        stats.OnHealthChanged.AddListener(hp => healthBar.SetHealth(hp, stats.maxHP));

        EnemyAI ai = enemy.GetComponent<EnemyAI>();
        if (ai == null)
        {
            ai = enemy.AddComponent<EnemyAI>();
        }

        if (enemyType == EnemyStats.EnemyType.Skeleton)
        {
            ai.moveSpeed = GameConfig.SkeletonMoveSpeed;
            ai.patrolSpeed = GameConfig.SkeletonPatrolSpeed;
            ai.attackRange = GameConfig.SkeletonAttackRange;
            ai.alertRange = GameConfig.SkeletonAlertRange;
            ai.chaseRange = GameConfig.SkeletonChaseRange;
            ai.attackCooldown = GameConfig.SkeletonAttackCooldown;
            ai.patrolDistance = GameConfig.SkeletonPatrolDistance;
            ai.patrolPause = GameConfig.SkeletonPatrolPause;
            ai.useRandomPatrol = false;
            ai.canCharge = currentWave >= 3 || stats.isElite;
            ai.canSpitProjectile = false;
            ai.specialCooldown = 5.4f;
            ai.chargeWindup = 0.58f;
            ai.chargeDistance = 4.0f;
        }
        else
        {
            ai.moveSpeed = GameConfig.SlimeMoveSpeed;
            ai.patrolSpeed = GameConfig.SlimePatrolSpeed;
            ai.attackRange = GameConfig.SlimeAttackRange;
            ai.alertRange = GameConfig.SlimeAlertRange;
            ai.chaseRange = GameConfig.SlimeChaseRange;
            ai.attackCooldown = GameConfig.SlimeAttackCooldown;
            ai.patrolRadius = GameConfig.SlimePatrolRadius;
            ai.directionChangeInterval = GameConfig.SlimeDirChangeInterval;
            ai.patrolPause = GameConfig.SlimePatrolPause;
            ai.useRandomPatrol = true;
            ai.canCharge = false;
            ai.canSpitProjectile = currentWave >= 2 || stats.isElite;
            ai.specialCooldown = 3.8f;
            ai.projectileWindup = 0.4f;
            ai.projectileSpeed = 7.5f;
        }

        ai.detectionInterval = enemyType == EnemyStats.EnemyType.Skeleton
            ? GameConfig.SkeletonDetectionInterval
            : GameConfig.SlimeDetectionInterval;

        UnityEngine.AI.NavMeshAgent navMeshAgent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (navMeshAgent == null)
        {
            navMeshAgent = enemy.AddComponent<UnityEngine.AI.NavMeshAgent>();
        }

        navMeshAgent.speed = ai.patrolSpeed;
        navMeshAgent.stoppingDistance = 0.1f;
        navMeshAgent.radius = 0.3f;
        navMeshAgent.height = enemyType == EnemyStats.EnemyType.Skeleton ? 1.5f : 0.8f;
        navMeshAgent.updateRotation = false;

        stats.OnDeath.AddListener(() => OnEnemyDeath(enemy));
        aliveEnemies++;
        spawnedEnemies.Add(enemy);
    }

    private GameObject CreateFallbackEnemy(EnemyStats.EnemyType enemyType, Vector3 spawnPosition)
    {
        GameObject enemy;
        if (enemyType == EnemyStats.EnemyType.Skeleton)
        {
            enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemy.transform.localScale = new Vector3(0.8f, 1.8f, 0.8f);
            enemy.GetComponent<Renderer>().material.color = new Color(0.9f, 0.85f, 0.75f);
        }
        else
        {
            enemy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            enemy.transform.localScale = new Vector3(0.8f, 0.6f, 0.8f);
            enemy.GetComponent<Renderer>().material.color = new Color(0.3f, 0.8f, 0.3f);
        }

        enemy.transform.position = spawnPosition;
        return enemy;
    }

    private string GetWaveIntroMessage(bool bossWave)
    {
        if (bossWave)
        {
            return "Boss Wave " + currentWave;
        }

        if (currentWave == 2)
        {
            return "Wave 2: Slimes spit acid";
        }

        if (currentWave == 3)
        {
            return "Wave 3: Skeletons charge";
        }

        return "Wave " + currentWave + " Begins";
    }

    private bool ShouldBecomeElite()
    {
        if (currentWave < 2)
        {
            return false;
        }

        float eliteChance = Mathf.Clamp01(0.16f + currentWave * 0.015f);
        if (Random.value < eliteChance)
        {
            return true;
        }

        return currentWave % 5 == 0 && Random.value < 0.45f;
    }

    private void MakeElite(GameObject enemy, EnemyStats stats)
    {
        stats.isElite = true;
        stats.enemyName = "Elite " + stats.enemyName;
        stats.maxHP *= GameConfig.EliteHealthMultiplier;
        stats.atk *= GameConfig.EliteAttackMultiplier;
        stats.def += 2f;
        stats.xpReward = Mathf.RoundToInt(stats.xpReward * 1.8f);
        stats.goldMin += 6;
        stats.goldMax += 10;
        enemy.transform.localScale *= GameConfig.EliteScaleMultiplier;

        foreach (Renderer renderer in enemy.GetComponentsInChildren<Renderer>())
        {
            if (renderer != null)
            {
                renderer.material.color = Color.Lerp(renderer.material.color, new Color(1f, 0.55f, 0.15f), 0.55f);
            }
        }
    }

    private void SpawnBoss(float healthMultiplier, float attackMultiplier, List<int> availableSpawnIndices)
    {
        EnemyStats.EnemyType bossType = currentWave % 10 == 0 ? EnemyStats.EnemyType.Slime : EnemyStats.EnemyType.Skeleton;
        int startAlive = aliveEnemies;
        SpawnEnemy(bossType == EnemyStats.EnemyType.Skeleton ? skeletonPrefab : slimePrefab, bossType, healthMultiplier, attackMultiplier, availableSpawnIndices);
        if (aliveEnemies <= startAlive)
        {
            return;
        }

        GameObject boss = spawnedEnemies[spawnedEnemies.Count - 1];
        EnemyStats stats = boss.GetComponent<EnemyStats>();
        EnemyAI ai = boss.GetComponent<EnemyAI>();
        if (stats == null || ai == null)
        {
            return;
        }

        stats.isBoss = true;
        stats.isElite = false;
        stats.enemyName = bossType == EnemyStats.EnemyType.Skeleton ? "Bone Tyrant" : "Gelatinous Horror";
        stats.maxHP *= GameConfig.BossHealthMultiplier;
        stats.currentHP = stats.maxHP;
        stats.atk *= GameConfig.BossAttackMultiplier;
        stats.def += 4f;
        stats.goldMin += GameConfig.BossGoldBonus;
        stats.goldMax += GameConfig.BossGoldBonus;
        stats.xpReward += GameConfig.BossXPBonus;
        stats.potionDropRate = 1f;
        boss.transform.localScale *= GameConfig.BossScaleMultiplier;
        WorldHealthBar healthBar = boss.GetComponentInChildren<WorldHealthBar>();
        if (healthBar != null)
        {
            healthBar.displayName = stats.enemyName;
            healthBar.Initialize(stats.maxHP, stats.enemyName);
            healthBar.SetHealth(stats.currentHP, stats.maxHP);
        }

        foreach (Renderer renderer in boss.GetComponentsInChildren<Renderer>())
        {
            if (renderer != null)
            {
                renderer.material.color = bossType == EnemyStats.EnemyType.Skeleton
                    ? new Color(0.62f, 0.18f, 0.88f)
                    : new Color(0.8f, 0.2f, 0.95f);
            }
        }

        ai.moveSpeed *= 0.9f;
        ai.attackRange += 0.8f;
        ai.alertRange += 2f;
        ai.chaseRange += 3f;
        ai.attackCooldown *= 0.9f;
    }

    private void OnEnemyDeath(GameObject enemy)
    {
        aliveEnemies--;
        spawnedEnemies.Remove(enemy);

        if (aliveEnemies <= 0 && waveInProgress)
        {
            waveInProgress = false;

            if (GameManager.Instance != null && GameManager.Instance.playerStats != null)
            {
                PlayerStats playerStats = GameManager.Instance.playerStats;
                float healAmount = playerStats.maxHP * GameConfig.WaveClearHealPercent;
                int goldBonus = GameConfig.WaveClearGoldBonusBase + currentWave * GameConfig.WaveClearGoldBonusPerWave;

                playerStats.Heal(healAmount);
                playerStats.AddGold(goldBonus);

                if (uiManager != null)
                {
                    uiManager.ShowWaveClearNotification(currentWave, Mathf.RoundToInt(healAmount), goldBonus);
                }
            }

            if (currentWave % 5 == 0 && uiManager != null)
            {
                awaitingPostBossReward = true;
                uiManager.QueueRelicChoice(currentWave);
            }
            else
            {
                Invoke(nameof(StartNextWave), GameConfig.WaveSpawnDelay);
            }
        }
    }

    public void HandlePostBossRewardResolved()
    {
        if (!awaitingPostBossReward)
        {
            return;
        }

        awaitingPostBossReward = false;
        if (GameManager.Instance != null && GameManager.Instance.isGameOver)
        {
            return;
        }

        Invoke(nameof(StartNextWave), GameConfig.WaveSpawnDelay);
    }
}
