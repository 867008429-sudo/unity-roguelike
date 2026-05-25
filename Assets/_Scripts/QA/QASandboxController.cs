using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class QASandboxController : MonoBehaviour
{
    public PlayerStats playerStats;
    public PlayerController playerController;
    public CharacterAnimationController playerAnimation;
    public UIManager uiManager;
    public GameObject skeletonPrefab;
    public GameObject slimePrefab;
    public Transform enemySpawnRoot;
    public Transform[] enemySpawnPoints;
    public bool showPanel = true;

    private readonly List<EnemyStats> spawnedEnemies = new List<EnemyStats>();
    private CharacterAnimationController selectedAnimation;
    private EnemyStats selectedEnemy;
    private Rect windowRect = new Rect(16f, 16f, 380f, 690f);
    private Vector2 scroll;
    private int attackStep = 1;
    private bool spawnEnemiesAwake = true;

    private void Awake()
    {
        CacheSceneReferences();
        selectedAnimation = playerAnimation;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F2))
        {
            showPanel = !showPanel;
        }

        RemoveMissingEnemies();
    }

    private void OnGUI()
    {
        if (!showPanel)
        {
            return;
        }

        windowRect = GUI.Window(GetInstanceID(), windowRect, DrawWindow, "QA Sandbox - Animation Tuning");
    }

    private void DrawWindow(int id)
    {
        scroll = GUILayout.BeginScrollView(scroll, GUILayout.Width(360f), GUILayout.Height(635f));
        GUILayout.Label("F2 toggles this panel. Values apply immediately.");
        GUILayout.Space(6f);

        DrawPlayerControls();
        DrawEnemyControls();
        DrawAnimationTuning();

        GUILayout.EndScrollView();
        GUI.DragWindow(new Rect(0f, 0f, 10000f, 24f));
    }

    private void DrawPlayerControls()
    {
        GUILayout.Label("Player");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Select Player"))
        {
            selectedAnimation = playerAnimation;
            selectedEnemy = null;
        }

        if (GUILayout.Button("Heal"))
        {
            playerStats?.Heal(999f);
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Damage"))
        {
            DamagePlayer();
        }

        if (GUILayout.Button("Kill"))
        {
            KillPlayer();
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Gold +100"))
        {
            playerStats?.AddGold(100);
        }

        if (GUILayout.Button("XP +Level"))
        {
            GrantLevel();
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Relic Panel"))
        {
            uiManager?.QueueRelicChoice(0);
        }

        if (GUILayout.Button("Shake"))
        {
            GameManager.Instance?.cameraFollow?.AddShake(0.22f, 0.18f);
        }
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Reload Scene"))
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Attack " + attackStep))
        {
            playerAnimation?.PlayAttack(GetPlayerFacing(), attackStep);
            attackStep = attackStep % 3 + 1;
        }

        if (GUILayout.Button("Dash Anim"))
        {
            playerAnimation?.PlayDash(GetPlayerFacing());
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Hurt Anim"))
        {
            playerAnimation?.PlayHurt(-GetPlayerFacing());
        }

        if (GUILayout.Button("Death Anim"))
        {
            playerAnimation?.PlayDeath();
        }
        GUILayout.EndHorizontal();
    }

    private void DrawEnemyControls()
    {
        GUILayout.Space(8f);
        GUILayout.Label("Enemies");
        spawnEnemiesAwake = GUILayout.Toggle(spawnEnemiesAwake, "Spawn with AI enabled");

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Spawn Skeleton"))
        {
            SpawnEnemy(skeletonPrefab);
        }

        if (GUILayout.Button("Spawn Slime"))
        {
            SpawnEnemy(slimePrefab);
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Select Last Enemy"))
        {
            SelectLastEnemy();
        }

        if (GUILayout.Button("Clear Enemies"))
        {
            ClearEnemies();
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Enemy Attack"))
        {
            selectedAnimation?.PlayAttack(GetDirectionFromSelectedToPlayer(), 2);
            selectedEnemy?.GetComponent<EnemyResourceAnimationDriver>()?.PlayAttack();
        }

        if (GUILayout.Button("Charge"))
        {
            selectedAnimation?.PlayChargeWindup(GetDirectionFromSelectedToPlayer(), 0.55f);
            selectedEnemy?.GetComponent<EnemyResourceAnimationDriver>()?.PlayChargeWindup();
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Spit"))
        {
            selectedAnimation?.PlaySpitWindup(GetDirectionFromSelectedToPlayer(), 0.45f);
            selectedEnemy?.GetComponent<EnemyResourceAnimationDriver>()?.PlaySpitWindup();
        }

        if (GUILayout.Button("Enemy Hurt"))
        {
            selectedAnimation?.PlayHurt(-GetDirectionFromSelectedToPlayer());
            selectedEnemy?.TakeDamage(1f, playerStats != null ? playerStats.transform.position : transform.position);
        }
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Kill Selected Enemy") && selectedEnemy != null)
        {
            selectedEnemy.TakeDamage(selectedEnemy.maxHP + selectedEnemy.def + 999f, playerStats != null ? playerStats.transform.position : transform.position, true);
        }

        GUILayout.Label("Spawned enemies: " + spawnedEnemies.Count);
    }

    private void DrawAnimationTuning()
    {
        GUILayout.Space(8f);
        GUILayout.Label("Selected Animation");
        if (selectedAnimation == null)
        {
            GUILayout.Label("No CharacterAnimationController selected.");
            return;
        }

        GUILayout.Label("Target: " + selectedAnimation.name);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Player Defaults"))
        {
            selectedAnimation.ApplyPreset(CharacterAnimationPreset.Player);
        }

        if (GUILayout.Button("Skeleton Defaults"))
        {
            selectedAnimation.ApplyPreset(CharacterAnimationPreset.Skeleton);
        }
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Slime Defaults"))
        {
            selectedAnimation.ApplyPreset(CharacterAnimationPreset.Slime);
        }

        selectedAnimation.idleBobHeight = Slider("Idle Bob Height", selectedAnimation.idleBobHeight, 0f, 0.2f);
        selectedAnimation.idleBobSpeed = Slider("Idle Bob Speed", selectedAnimation.idleBobSpeed, 0.2f, 7f);
        selectedAnimation.idleBreathScale = Slider("Idle Breath Scale", selectedAnimation.idleBreathScale, 0f, 0.16f);
        selectedAnimation.moveSwayAngle = Slider("Move Sway Angle", selectedAnimation.moveSwayAngle, 0f, 22f);
        selectedAnimation.movePulse = Slider("Move Pulse", selectedAnimation.movePulse, 0f, 0.2f);
        selectedAnimation.moveSpeed = Slider("Move Speed", selectedAnimation.moveSpeed, 1f, 18f);
        selectedAnimation.hurtDuration = Slider("Hurt Duration", selectedAnimation.hurtDuration, 0.05f, 0.7f);
        selectedAnimation.deathDuration = Slider("Death Duration", selectedAnimation.deathDuration, 0.2f, 2f);
        selectedAnimation.attackPoseScale = Slider("Attack Pose", selectedAnimation.attackPoseScale, 0.4f, 1.8f);
        selectedAnimation.attackLungeScale = Slider("Attack Lunge", selectedAnimation.attackLungeScale, 0.3f, 1.8f);
        selectedAnimation.dashStretchScale = Slider("Dash Stretch", selectedAnimation.dashStretchScale, 0.4f, 1.8f);
        selectedAnimation.hurtKnockbackScale = Slider("Hurt Weight", selectedAnimation.hurtKnockbackScale, 0.4f, 1.8f);
        selectedAnimation.deathFallScale = Slider("Death Fall", selectedAnimation.deathFallScale, 0.3f, 1.6f);

        GUILayout.BeginHorizontal();
        selectedAnimation.skeletonMotion = GUILayout.Toggle(selectedAnimation.skeletonMotion, "Skeleton");
        selectedAnimation.slimeMotion = GUILayout.Toggle(selectedAnimation.slimeMotion, "Slime");
        GUILayout.EndHorizontal();
    }

    private float Slider(string label, float value, float min, float max)
    {
        GUILayout.Label(label + ": " + value.ToString("0.00"));
        return GUILayout.HorizontalSlider(value, min, max);
    }

    private void CacheSceneReferences()
    {
        if (playerStats == null)
        {
            playerStats = FindObjectOfType<PlayerStats>();
        }

        if (playerController == null && playerStats != null)
        {
            playerController = playerStats.GetComponent<PlayerController>();
        }

        if (playerAnimation == null && playerStats != null)
        {
            playerAnimation = playerStats.GetComponent<CharacterAnimationController>();
        }

        if (uiManager == null)
        {
            uiManager = FindObjectOfType<UIManager>();
        }
    }

    private void DamagePlayer()
    {
        if (playerStats == null)
        {
            return;
        }

        playerStats.TakeDamage(15f, playerStats.transform.position - GetPlayerFacing() * 2f);
    }

    private void KillPlayer()
    {
        if (playerStats == null)
        {
            return;
        }

        playerStats.TakeDamage(playerStats.currentHP + playerStats.def + 999f, playerStats.transform.position - GetPlayerFacing() * 2f);
    }

    private void GrantLevel()
    {
        if (playerStats == null)
        {
            return;
        }

        playerStats.AddXP(Mathf.Max(1, playerStats.xpToNextLevel - playerStats.currentXP));
    }

    private Vector3 GetPlayerFacing()
    {
        if (playerController != null)
        {
            return playerController.GetFacingDirection();
        }

        return playerStats != null ? playerStats.transform.forward : Vector3.forward;
    }

    private Vector3 GetDirectionFromSelectedToPlayer()
    {
        if (selectedAnimation == null || playerStats == null)
        {
            return Vector3.forward;
        }

        Vector3 direction = playerStats.transform.position - selectedAnimation.transform.position;
        direction.y = 0f;
        return direction.sqrMagnitude > 0.01f ? direction.normalized : selectedAnimation.transform.forward;
    }

    private void SpawnEnemy(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogWarning("QA sandbox enemy prefab is not assigned.");
            return;
        }

        Vector3 position = GetSpawnPosition();
        GameObject enemyObject = Instantiate(prefab, position, Quaternion.identity, enemySpawnRoot);
        EnemyStats enemyStats = enemyObject.GetComponent<EnemyStats>();
        EnemyAI enemyAI = enemyObject.GetComponent<EnemyAI>();
        if (enemyAI != null)
        {
            enemyAI.enabled = spawnEnemiesAwake;
            if (playerStats != null)
            {
                enemyAI.playerTransform = playerStats.transform;
            }
        }

        if (enemyStats != null)
        {
            spawnedEnemies.Add(enemyStats);
            selectedEnemy = enemyStats;
            selectedAnimation = enemyStats.GetComponent<CharacterAnimationController>();
        }
    }

    private Vector3 GetSpawnPosition()
    {
        RemoveMissingEnemies();
        if (enemySpawnPoints != null && enemySpawnPoints.Length > 0)
        {
            Transform point = enemySpawnPoints[spawnedEnemies.Count % enemySpawnPoints.Length];
            if (point != null)
            {
                return point.position;
            }
        }

        return playerStats != null ? playerStats.transform.position + Vector3.forward * 4f : transform.position + Vector3.forward * 4f;
    }

    private void SelectLastEnemy()
    {
        RemoveMissingEnemies();
        if (spawnedEnemies.Count <= 0)
        {
            return;
        }

        selectedEnemy = spawnedEnemies[spawnedEnemies.Count - 1];
        selectedAnimation = selectedEnemy != null ? selectedEnemy.GetComponent<CharacterAnimationController>() : null;
    }

    private void ClearEnemies()
    {
        for (int i = 0; i < spawnedEnemies.Count; i++)
        {
            if (spawnedEnemies[i] != null)
            {
                Destroy(spawnedEnemies[i].gameObject);
            }
        }

        spawnedEnemies.Clear();
        selectedEnemy = null;
        selectedAnimation = playerAnimation;
    }

    private void RemoveMissingEnemies()
    {
        for (int i = spawnedEnemies.Count - 1; i >= 0; i--)
        {
            if (spawnedEnemies[i] == null)
            {
                spawnedEnemies.RemoveAt(i);
            }
        }
    }
}
