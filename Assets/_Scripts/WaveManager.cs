using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WaveManager : MonoBehaviour
{
    public Transform[] spawnPoints;
    public int currentWave;
    public int aliveEnemies;
    public bool waveInProgress;
    public GameObject skeletonPrefab;
    public GameObject slimePrefab;
    public GameObject nextBattlePortalPrefab;
    public GameObject secretShopPortalPrefab;
    public GameObject shopExitPortalPrefab;
    public Transform portalSpawnCenter;
    public Transform secretShopPoint;
    public float portalSpawnDistance = 2.25f;
    public bool autoStartWaves = true;

    private readonly List<GameObject> spawnedEnemies = new List<GameObject>();
    private readonly List<GameObject> activePortalObjects = new List<GameObject>();
    private readonly List<GameObject> activeShopObjects = new List<GameObject>();
    private UIManager uiManager;
    private bool awaitingPostBossReward;
    private bool awaitingBranchChoice;
    private bool inSecretShop;

    private void Start()
    {
        uiManager = FindObjectOfType<UIManager>();
        if (autoStartWaves)
        {
            Invoke(nameof(StartNextWave), 1f);
        }
    }

    public void StartNextWave()
    {
        ClearActivePortals();
        ClearSecretShop();
        awaitingBranchChoice = false;
        inSecretShop = false;
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
            return "第 " + currentWave + " 波：首领来袭";
        }

        if (currentWave == 2)
        {
            return "第 2 波：史莱姆开始吐酸";
        }

        if (currentWave == 3)
        {
            return "第 3 波：骷髅开始冲锋";
        }

        return "第 " + currentWave + " 波开始";
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
                EnterBranchChoiceState();
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

        EnterBranchChoiceState();
    }

    public void HandlePortalSelected(PortalType portalType)
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver)
        {
            return;
        }

        if (portalType == PortalType.SecretShop)
        {
            EnterSecretShop();
            return;
        }

        BeginNextBattleFromPortal();
    }

    public void SimulateWaveClearForQA()
    {
        CancelInvoke(nameof(StartNextWave));
        ClearCombatDangers();
        waveInProgress = false;
        aliveEnemies = 0;
        currentWave = Mathf.Max(1, currentWave);
        if (uiManager == null)
        {
            uiManager = FindObjectOfType<UIManager>();
        }

        if (uiManager != null)
        {
            uiManager.ShowWaveNotification("QA：模拟清场，选择传送门");
        }

        EnterBranchChoiceState();
    }

    private void EnterBranchChoiceState()
    {
        CancelInvoke(nameof(StartNextWave));
        awaitingBranchChoice = true;
        inSecretShop = false;
        ClearCombatDangers();
        ClearActivePortals();
        ClearSecretShop();
        SpawnBranchPortals();

        if (uiManager != null)
        {
            uiManager.ShowWaveNotification("战斗清场：选择下一步");
        }
    }

    private void EnterSecretShop()
    {
        CancelInvoke(nameof(StartNextWave));
        awaitingBranchChoice = true;
        inSecretShop = true;
        Time.timeScale = 1f;
        ClearActivePortals();
        ClearCombatDangers();
        SpawnSecretShop();

        if (uiManager != null)
        {
            uiManager.ShowWaveNotification("神秘商店：安全整备");
        }
    }

    private void BeginNextBattleFromPortal()
    {
        CancelInvoke(nameof(StartNextWave));
        Time.timeScale = 1f;
        awaitingBranchChoice = false;
        inSecretShop = false;
        ClearActivePortals();
        ClearSecretShop();

        if (uiManager != null)
        {
            uiManager.ShowWaveNotification("传送完成，下一波即将开始");
        }

        Invoke(nameof(StartNextWave), 0.35f);
    }

    private void SpawnBranchPortals()
    {
        Vector3 center = GetPortalCenter();
        Vector3 right = GetPortalRight();
        CreatePortal(PortalType.NextBattleWave, center - right * portalSpawnDistance, nextBattlePortalPrefab, "[E] 进入下一波战斗");
        CreatePortal(PortalType.SecretShop, center + right * portalSpawnDistance, secretShopPortalPrefab, "[E] 进入神秘商店整备");
    }

    private void SpawnSecretShop()
    {
        Vector3 center = GetSecretShopCenter();
        GameObject root = new GameObject("RuntimeSecretShop");
        root.transform.position = center;
        activeShopObjects.Add(root);

        CreateShopPlatform(root.transform, center);
        Transform itemRoot = CreateShopStall(root.transform, center);
        CreateShopItem(itemRoot, center + new Vector3(-1.35f, 0.7f, 0.1f), "战士之力", 35, ShopItemRewardType.AttackBlessing, new Color(1f, 0.68f, 0.26f));
        CreateShopItem(itemRoot, center + new Vector3(0f, 0.7f, 0.1f), "治疗血瓶", 25, ShopItemRewardType.HealthPotion, new Color(0.86f, 0.1f, 0.12f));
        CreateShopItem(itemRoot, center + new Vector3(1.35f, 0.7f, 0.1f), "迅捷之靴", 55, ShopItemRewardType.WindBoots, new Color(0.25f, 0.72f, 1f));

        CreatePortal(PortalType.NextBattleWave, center + new Vector3(0f, 0f, -2.65f), shopExitPortalPrefab, "[E] 整备完毕，返回战斗");
    }

    private void CreatePortal(PortalType portalType, Vector3 position, GameObject prefab, string prompt)
    {
        GameObject portal = prefab != null
            ? Instantiate(prefab, position, Quaternion.identity)
            : CreateFallbackPortalVisual(portalType, position);

        portal.name = portalType == PortalType.SecretShop ? "Portal_SecretShop" : (inSecretShop ? "Portal_ReturnToBattle" : "Portal_NextBattleWave");
        portal.transform.position = SnapToNavMesh(position);

        PortalInteractable interactable = portal.GetComponent<PortalInteractable>();
        if (interactable == null)
        {
            interactable = portal.AddComponent<PortalInteractable>();
        }

        interactable.portalType = portalType;
        interactable.waveManager = this;
        interactable.promptOverride = prompt;
        interactable.interactDistance = 2.6f;

        if (portal.GetComponent<Collider>() == null)
        {
            SphereCollider trigger = portal.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 1.15f;
        }

        activePortalObjects.Add(portal);
    }

    private GameObject CreateFallbackPortalVisual(PortalType portalType, Vector3 position)
    {
        Color color = portalType == PortalType.SecretShop
            ? new Color(0.72f, 0.35f, 1f, 1f)
            : new Color(0.24f, 0.72f, 1f, 1f);

        GameObject root = new GameObject(portalType == PortalType.SecretShop ? "SecretShopPortalVisual" : "NextWavePortalVisual");
        root.transform.position = position;

        CreatePrimitivePart(root.transform, PrimitiveType.Cylinder, "Portal_Base", new Vector3(0f, 0.05f, 0f), new Vector3(1.55f, 0.08f, 1.55f), color * 0.75f);
        CreatePrimitivePart(root.transform, PrimitiveType.Cube, "Portal_LeftPillar", new Vector3(-0.72f, 0.85f, 0f), new Vector3(0.18f, 1.55f, 0.18f), color);
        CreatePrimitivePart(root.transform, PrimitiveType.Cube, "Portal_RightPillar", new Vector3(0.72f, 0.85f, 0f), new Vector3(0.18f, 1.55f, 0.18f), color);
        CreatePrimitivePart(root.transform, PrimitiveType.Cube, "Portal_Top", new Vector3(0f, 1.62f, 0f), new Vector3(1.55f, 0.18f, 0.18f), color);
        CreatePrimitivePart(root.transform, PrimitiveType.Cube, "Portal_Core", new Vector3(0f, 0.85f, 0f), new Vector3(1.05f, 1.25f, 0.08f), new Color(color.r, color.g, color.b, 0.46f));

        Light light = root.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.intensity = 1.7f;
        light.range = 4.2f;
        return root;
    }

    private Transform CreateShopStall(Transform root, Vector3 center)
    {
        GameObject stall = new GameObject("SecretShop_Stall");
        stall.transform.SetParent(root);
        stall.transform.position = center + new Vector3(0f, 0.35f, 0.65f);

        CreatePrimitivePart(stall.transform, PrimitiveType.Cube, "Shop_Counter", Vector3.zero, new Vector3(4.5f, 0.5f, 1.15f), new Color(0.33f, 0.2f, 0.12f));
        CreatePrimitivePart(stall.transform, PrimitiveType.Cube, "Shop_BackShelf", new Vector3(0f, 1.0f, 0.52f), new Vector3(4.3f, 1.1f, 0.2f), new Color(0.22f, 0.13f, 0.08f));
        CreatePrimitivePart(stall.transform, PrimitiveType.Cube, "Shop_Candle", new Vector3(-2.05f, 0.55f, -0.1f), new Vector3(0.15f, 0.42f, 0.15f), new Color(1f, 0.86f, 0.45f));
        CreatePrimitivePart(stall.transform, PrimitiveType.Sphere, "Shop_Crystal", new Vector3(2.05f, 0.62f, -0.08f), new Vector3(0.35f, 0.35f, 0.35f), new Color(0.52f, 0.3f, 1f));
        return stall.transform;
    }

    private void CreateShopPlatform(Transform root, Vector3 center)
    {
        GameObject platform = CreatePrimitivePart(root, PrimitiveType.Cylinder, "SecretShop_SafeZone", Vector3.zero, new Vector3(5.8f, 0.08f, 5.8f), new Color(0.08f, 0.1f, 0.13f));
        platform.transform.position = center + new Vector3(0f, 0.03f, 0f);
        Light light = platform.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(0.62f, 0.48f, 1f);
        light.intensity = 1.2f;
        light.range = 6.5f;
    }

    private void CreateShopItem(Transform root, Vector3 position, string itemName, int price, ShopItemRewardType rewardType, Color color)
    {
        GameObject item = new GameObject("ShopItem_" + itemName);
        item.transform.SetParent(root);
        item.transform.position = position;

        ShopItemInteractable shopItem = item.AddComponent<ShopItemInteractable>();
        shopItem.itemName = itemName;
        shopItem.price = price;
        shopItem.rewardType = rewardType;
        shopItem.destroyAfterPurchase = false;
        shopItem.shopRoot = root;
        shopItem.shopTitle = "神秘商店";
        shopItem.interactDistance = 2.8f;

        CreatePrimitivePart(item.transform, PrimitiveType.Sphere, "Item_Display", Vector3.zero, new Vector3(0.38f, 0.38f, 0.38f), color);
    }

    private GameObject CreatePrimitivePart(Transform parent, PrimitiveType primitiveType, string name, Vector3 localPosition, Vector3 localScale, Color color)
    {
        GameObject part = GameObject.CreatePrimitive(primitiveType);
        part.name = name;
        part.transform.SetParent(parent);
        part.transform.localPosition = localPosition;
        part.transform.localRotation = Quaternion.identity;
        part.transform.localScale = localScale;
        Renderer renderer = part.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = color;
        }

        Collider collider = part.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }

        return part;
    }

    private Vector3 GetPortalCenter()
    {
        if (portalSpawnCenter != null)
        {
            return portalSpawnCenter.position;
        }

        if (GameManager.Instance != null && GameManager.Instance.playerTransform != null)
        {
            return GameManager.Instance.playerTransform.position + Vector3.forward * 2.5f;
        }

        PlayerController player = FindObjectOfType<PlayerController>();
        return player != null ? player.transform.position + Vector3.forward * 2.5f : transform.position;
    }

    private Vector3 GetSecretShopCenter()
    {
        if (secretShopPoint != null)
        {
            return secretShopPoint.position;
        }

        return Vector3.zero;
    }

    private Vector3 GetPortalRight()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return Vector3.right;
        }

        Vector3 right = mainCamera.transform.right;
        right.y = 0f;
        return right.sqrMagnitude > 0.01f ? right.normalized : Vector3.right;
    }

    private Vector3 SnapToNavMesh(Vector3 position)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(position, out hit, 2.5f, NavMesh.AllAreas))
        {
            return hit.position;
        }

        position.y = 0f;
        return position;
    }

    private void ClearCombatDangers()
    {
        for (int i = spawnedEnemies.Count - 1; i >= 0; i--)
        {
            if (spawnedEnemies[i] != null)
            {
                spawnedEnemies[i].SetActive(false);
                Destroy(spawnedEnemies[i]);
            }
        }

        spawnedEnemies.Clear();
        aliveEnemies = 0;

        EnemyAI[] enemies = FindObjectsOfType<EnemyAI>();
        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i] != null)
            {
                enemies[i].gameObject.SetActive(false);
                Destroy(enemies[i].gameObject);
            }
        }

        EnemyProjectile[] projectiles = FindObjectsOfType<EnemyProjectile>();
        for (int i = 0; i < projectiles.Length; i++)
        {
            projectiles[i].gameObject.SetActive(false);
            Destroy(projectiles[i].gameObject);
        }

        EnemyHazardZone[] hazards = FindObjectsOfType<EnemyHazardZone>();
        for (int i = 0; i < hazards.Length; i++)
        {
            hazards[i].gameObject.SetActive(false);
            Destroy(hazards[i].gameObject);
        }
    }

    private void ClearActivePortals()
    {
        for (int i = activePortalObjects.Count - 1; i >= 0; i--)
        {
            if (activePortalObjects[i] != null)
            {
                activePortalObjects[i].SetActive(false);
                Destroy(activePortalObjects[i]);
            }
        }

        activePortalObjects.Clear();
    }

    private void ClearSecretShop()
    {
        for (int i = activeShopObjects.Count - 1; i >= 0; i--)
        {
            if (activeShopObjects[i] != null)
            {
                activeShopObjects[i].SetActive(false);
                Destroy(activeShopObjects[i]);
            }
        }

        activeShopObjects.Clear();
    }
}
