using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class QuickSceneBuilder
{
    [MenuItem("RPG Tools/Build Complete Scene", false, 0)]
    public static void BuildCompleteScene()
    {
        if (EditorSceneManager.GetActiveScene().path == string.Empty)
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        ClearScene();
        CreateGroundAndWalls();
        CreateObstacles();
        CreateLighting();
        CreateSpawnPoints();
        CreatePlayer();
        CreateCamera();
        CreateManagers();
        CreateUICanvas();
        BuildNavMesh();

        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        EditorUtility.DisplayDialog("Done", "Dungeon scene rebuilt.", "OK");
    }

    private static void ClearScene()
    {
        foreach (GameObject obj in Object.FindObjectsOfType<GameObject>())
        {
            if (obj.transform.parent == null && obj.scene == EditorSceneManager.GetActiveScene())
            {
                Object.DestroyImmediate(obj);
            }
        }
    }

    private static void CreateGroundAndWalls()
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(4f, 1f, 4f);
        ground.GetComponent<Renderer>().sharedMaterial = CreateMaterial(new Color(0.25f, 0.23f, 0.22f));
        ground.isStatic = true;

        GameObject walls = new GameObject("Walls");
        CreateCube("Wall_North", new Vector3(0f, 2f, 20.25f), new Vector3(40.5f, 4f, 0.5f), new Color(0.35f, 0.3f, 0.28f), walls.transform);
        CreateCube("Wall_South", new Vector3(0f, 2f, -20.25f), new Vector3(40.5f, 4f, 0.5f), new Color(0.35f, 0.3f, 0.28f), walls.transform);
        CreateCube("Wall_East", new Vector3(20.25f, 2f, 0f), new Vector3(0.5f, 4f, 40.5f), new Color(0.35f, 0.3f, 0.28f), walls.transform);
        CreateCube("Wall_West", new Vector3(-20.25f, 2f, 0f), new Vector3(0.5f, 4f, 40.5f), new Color(0.35f, 0.3f, 0.28f), walls.transform);
    }

    private static void CreateObstacles()
    {
        GameObject root = new GameObject("Obstacles");
        CreateCylinder("CentralPillar", new Vector3(-5f, 2f, 0f), new Vector3(1.5f, 4f, 1.5f), new Color(0.45f, 0.42f, 0.38f), root.transform);
        CreateCube("Rubble_A", new Vector3(-12f, 0.3f, 8f), new Vector3(2f, 0.6f, 2f), new Color(0.3f, 0.28f, 0.25f), root.transform);
        CreateCube("Rubble_B", new Vector3(10f, 0.3f, -8f), new Vector3(2f, 0.6f, 2f), new Color(0.3f, 0.28f, 0.25f), root.transform);
        CreateCube("Crate_A", new Vector3(14f, 0.5f, 10f), Vector3.one, new Color(0.5f, 0.35f, 0.2f), root.transform);
        CreateCube("Crate_B", new Vector3(-15f, 0.5f, -10f), new Vector3(1.2f, 1f, 1.2f), new Color(0.5f, 0.35f, 0.2f), root.transform);
        CreateCylinder("SmallPillar_1", new Vector3(0f, 1f, 10f), new Vector3(0.6f, 2f, 0.6f), new Color(0.45f, 0.42f, 0.38f), root.transform);
        CreateCylinder("SmallPillar_2", new Vector3(-16f, 1f, -4f), new Vector3(0.5f, 2f, 0.5f), new Color(0.45f, 0.42f, 0.38f), root.transform);
    }

    private static void CreateLighting()
    {
        GameObject directional = new GameObject("Directional Light");
        Light dirLight = directional.AddComponent<Light>();
        dirLight.type = LightType.Directional;
        dirLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        dirLight.intensity = 0.3f;
        dirLight.color = new Color(1f, 0.91f, 0.75f);
        dirLight.shadows = LightShadows.Soft;

        GameObject lightRoot = new GameObject("Lights");
        CreateTorch(lightRoot.transform, new Vector3(-15f, 3f, 12f), 10f, 2f);
        CreateTorch(lightRoot.transform, new Vector3(15f, 3f, 12f), 10f, 2f);
        CreateTorch(lightRoot.transform, new Vector3(-15f, 3f, -12f), 10f, 2f);
        CreateTorch(lightRoot.transform, new Vector3(15f, 3f, -12f), 10f, 2f);
        CreateTorch(lightRoot.transform, new Vector3(0f, 4f, 0f), 8f, 1.5f);
    }

    private static void CreateSpawnPoints()
    {
        Vector3[] positions =
        {
            new Vector3(-10f, 0f, 14f),
            new Vector3(12f, 0f, 14f),
            new Vector3(-14f, 0f, -6f),
            new Vector3(14f, 0f, -10f),
            new Vector3(-6f, 0f, -14f),
            new Vector3(8f, 0f, -6f)
        };

        GameObject parent = new GameObject("SpawnPoints");
        for (int i = 0; i < positions.Length; i++)
        {
            GameObject spawn = new GameObject("SpawnPoint_" + i);
            spawn.transform.SetParent(parent.transform);
            spawn.transform.position = positions[i];
        }
    }

    private static void CreatePlayer()
    {
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.tag = "Player";
        player.transform.position = new Vector3(0f, 1f, 0f);
        player.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
        player.GetComponent<Renderer>().sharedMaterial = CreateMaterial(new Color(0.2f, 0.4f, 0.8f));

        Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());
        CharacterController controller = player.AddComponent<CharacterController>();
        controller.center = new Vector3(0f, 1f, 0f);
        controller.radius = 0.4f;
        controller.height = 2f;

        PlayerStats stats = player.AddComponent<PlayerStats>();
        player.AddComponent<PlayerController>();

        GameObject healthBarObject = new GameObject("PlayerHealthBar");
        healthBarObject.transform.SetParent(player.transform);
        WorldHealthBar healthBar = healthBarObject.AddComponent<WorldHealthBar>();
        healthBar.target = player.transform;
        healthBar.offset = new Vector3(0f, 2.5f, 0f);
        healthBar.showOnlyWhenDamaged = false;
        stats.OnHealthChanged.AddListener(hp => healthBar.SetHealth(hp, stats.maxHP));
    }

    private static void CreateCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.fieldOfView = GameConfig.CameraFOV;
        camera.nearClipPlane = GameConfig.CameraNearClip;
        camera.farClipPlane = GameConfig.CameraFarClip;
        cameraObject.transform.position = GameConfig.CameraOffset;
        cameraObject.transform.rotation = Quaternion.Euler(GameConfig.CameraRotation);
        cameraObject.AddComponent<AudioListener>();
        cameraObject.AddComponent<CameraFollow>();
    }

    private static void CreateManagers()
    {
        new GameObject("GameManager").AddComponent<GameManager>();
        new GameObject("CombatManager").AddComponent<CombatManager>();

        GameObject waveManagerObject = new GameObject("WaveManager");
        WaveManager waveManager = waveManagerObject.AddComponent<WaveManager>();

        GameObject spawnParent = GameObject.Find("SpawnPoints");
        if (spawnParent != null)
        {
            Transform[] spawns = new Transform[spawnParent.transform.childCount];
            for (int i = 0; i < spawns.Length; i++)
            {
                spawns[i] = spawnParent.transform.GetChild(i);
            }

            waveManager.spawnPoints = spawns;
        }
    }

    private static void CreateUICanvas()
    {
        GameObject canvasObject = new GameObject("UICanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasObject.AddComponent<GraphicRaycaster>();

        Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        GameObject expPanel = CreateUIBlock("ExpBarPanel", canvas.transform, new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(400f, 24f), Color.clear);
        CreateUIText("LevelText", expPanel.transform, "Lv.1", 16, Color.white, new Vector2(0f, 0f), new Vector2(50f, 20f), font);
        GameObject expBarBg = CreateUIBlock("ExpBarBg", expPanel.transform, new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(240f, 18f), new Color(0.2f, 0.2f, 0.2f, 0.5f));
        Image expFill = CreateUIBlock("ExpFill", expBarBg.transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(240f, 18f), new Color(0.2f, 0.6f, 0.86f)).GetComponent<Image>();
        expFill.type = Image.Type.Filled;
        expFill.fillMethod = Image.FillMethod.Horizontal;
        expFill.fillAmount = 0f;
        CreateUIText("XPValueText", expPanel.transform, "0 / 30 XP", 14, Color.white, new Vector2(160f, 0f), new Vector2(100f, 20f), font);

        GameObject goldPanel = CreateUIBlock("GoldPanel", canvas.transform, new Vector2(1f, 1f), new Vector2(-90f, -40f), new Vector2(140f, 36f), new Color(0f, 0f, 0f, 0.4f));
        CreateUIText("GoldLabel", goldPanel.transform, "G:", 18, new Color(1f, 0.84f, 0f), new Vector2(-40f, 0f), new Vector2(25f, 20f), font);
        CreateUIText("GoldValue", goldPanel.transform, "0", 20, new Color(1f, 0.84f, 0f), new Vector2(20f, 0f), new Vector2(100f, 30f), font);

        GameObject wavePanel = CreateUIBlock("WavePanel", canvas.transform, new Vector2(0.5f, 0.7f), Vector2.zero, new Vector2(420f, 60f), Color.clear);
        wavePanel.SetActive(false);
        CreateUIText("WaveText", wavePanel.transform, string.Empty, 32, Color.white, Vector2.zero, new Vector2(420f, 60f), font);

        GameObject levelPanel = CreateUIBlock("LevelUpPanel", canvas.transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(320f, 80f), Color.clear);
        levelPanel.SetActive(false);
        CreateUIText("LevelUpText", levelPanel.transform, "LEVEL UP!", 40, new Color(1f, 0.84f, 0f), Vector2.zero, new Vector2(300f, 80f), font);

        GameObject deathPanel = CreateUIBlock("DeathPanel", canvas.transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1920f, 1080f), new Color(0f, 0f, 0f, 0.7f));
        deathPanel.SetActive(false);
        CreateUIText("DeathTitle", deathPanel.transform, "You Have Fallen", 48, new Color(0.91f, 0.3f, 0.24f), new Vector2(0f, 180f), new Vector2(400f, 60f), font);
        CreateUIText("DeathStats", deathPanel.transform, string.Empty, 24, Color.white, Vector2.zero, new Vector2(420f, 120f), font);
        CreateUIText("DeathRestart", deathPanel.transform, "Press R to try again", 20, Color.white, new Vector2(0f, -150f), new Vector2(320f, 40f), font);

        GameObject uiManagerObject = new GameObject("UIManager");
        UIManager uiManager = uiManagerObject.AddComponent<UIManager>();
        uiManager.mainCanvas = canvas;
        uiManager.expBarPanel = expPanel;
        uiManager.expBarFill = expFill;
        uiManager.expBarLevelText = expPanel.transform.Find("LevelText").GetComponent<Text>();
        uiManager.expBarValueText = expPanel.transform.Find("XPValueText").GetComponent<Text>();
        uiManager.goldPanel = goldPanel;
        uiManager.goldText = goldPanel.transform.Find("GoldValue").GetComponent<Text>();
        uiManager.waveNotificationPanel = wavePanel;
        uiManager.waveText = wavePanel.transform.Find("WaveText").GetComponent<Text>();
        uiManager.levelUpPanel = levelPanel;
        uiManager.levelUpText = levelPanel.transform.Find("LevelUpText").GetComponent<Text>();
        uiManager.deathPanel = deathPanel;
        uiManager.deathTitleText = deathPanel.transform.Find("DeathTitle").GetComponent<Text>();
        uiManager.deathStatsText = deathPanel.transform.Find("DeathStats").GetComponent<Text>();
        uiManager.deathRestartText = deathPanel.transform.Find("DeathRestart").GetComponent<Text>();
    }

    private static void BuildNavMesh()
    {
        UnityEditor.AI.NavMeshBuilder.ClearAllNavMeshes();
        UnityEditor.AI.NavMeshBuilder.BuildNavMesh();
    }

    private static Material CreateMaterial(Color color)
    {
        Material material = new Material(Shader.Find("Standard"));
        material.color = color;
        return material;
    }

    private static GameObject CreateCube(string name, Vector3 position, Vector3 scale, Color color, Transform parent)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = name;
        obj.transform.SetParent(parent);
        obj.transform.position = position;
        obj.transform.localScale = scale;
        obj.GetComponent<Renderer>().sharedMaterial = CreateMaterial(color);
        obj.isStatic = true;
        return obj;
    }

    private static GameObject CreateCylinder(string name, Vector3 position, Vector3 scale, Color color, Transform parent)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        obj.name = name;
        obj.transform.SetParent(parent);
        obj.transform.position = position;
        obj.transform.localScale = scale;
        obj.GetComponent<Renderer>().sharedMaterial = CreateMaterial(color);
        obj.isStatic = true;
        return obj;
    }

    private static void CreateTorch(Transform parent, Vector3 position, float range, float intensity)
    {
        GameObject torch = new GameObject("Torch");
        torch.transform.SetParent(parent);
        torch.transform.position = position;
        Light light = torch.AddComponent<Light>();
        light.type = LightType.Point;
        light.range = range;
        light.intensity = intensity;
        light.color = new Color(1f, 0.53f, 0f);
        light.shadows = LightShadows.Hard;
    }

    private static GameObject CreateUIBlock(string name, Transform parent, Vector2 anchor, Vector2 anchoredPosition, Vector2 size, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        Image image = obj.AddComponent<Image>();
        image.color = color;
        return obj;
    }

    private static GameObject CreateUIText(string name, Transform parent, string content, int fontSize, Color color, Vector2 anchoredPosition, Vector2 size, Font font)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        Text text = obj.AddComponent<Text>();
        text.font = font;
        text.text = content;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = TextAnchor.MiddleCenter;
        return obj;
    }
}
