using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public static class QASandboxSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/QA_Sandbox.unity";
    private const string KnightPath = "Assets/_ImportedArt/KayKit/Adventurers/Characters/fbx/Knight.fbx";
    private const string SkeletonPrefabPath = "Assets/_Prefabs/KayKit/SkeletonEnemy_KayKit.prefab";
    private const string SlimePrefabPath = "Assets/_Prefabs/KayKit/SlimeEnemy_KayKitStyle.prefab";

    [MenuItem("RPG Tools/Build QA Sandbox", false, 10)]
    public static void BuildQASandbox()
    {
        Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        CreateArena();
        CreateLighting();
        GameObject player = CreatePlayer();
        CreateCamera(player.transform);
        UIManager uiManager = CreateUi();
        CreateManagers(player, uiManager);
        CreateSandboxController(player, uiManager);
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), ScenePath);
        BuildNavMesh();

        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), ScenePath);
        AssetDatabase.Refresh();
        Debug.Log("QA Sandbox scene rebuilt at " + ScenePath);
    }

    private static void CreateArena()
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "QA_Ground";
        ground.transform.localScale = new Vector3(3f, 1f, 3f);
        ground.GetComponent<Renderer>().sharedMaterial = CreateMaterial(new Color(0.18f, 0.2f, 0.22f));
        ground.isStatic = true;

        GameObject walls = new GameObject("QA_Walls");
        CreateCube("Wall_North", new Vector3(0f, 1.4f, 15.25f), new Vector3(30.5f, 2.8f, 0.5f), walls.transform, new Color(0.3f, 0.32f, 0.35f));
        CreateCube("Wall_South", new Vector3(0f, 1.4f, -15.25f), new Vector3(30.5f, 2.8f, 0.5f), walls.transform, new Color(0.3f, 0.32f, 0.35f));
        CreateCube("Wall_East", new Vector3(15.25f, 1.4f, 0f), new Vector3(0.5f, 2.8f, 30.5f), walls.transform, new Color(0.3f, 0.32f, 0.35f));
        CreateCube("Wall_West", new Vector3(-15.25f, 1.4f, 0f), new Vector3(0.5f, 2.8f, 30.5f), walls.transform, new Color(0.3f, 0.32f, 0.35f));

        GameObject marks = new GameObject("QA_Position_Markers");
        CreateMarker("Player_Start", Vector3.zero, marks.transform, new Color(0.2f, 0.55f, 1f));
        CreateMarker("Enemy_Spawn_A", new Vector3(4f, 0.03f, 4f), marks.transform, new Color(1f, 0.5f, 0.25f));
        CreateMarker("Enemy_Spawn_B", new Vector3(-4f, 0.03f, 5f), marks.transform, new Color(1f, 0.5f, 0.25f));
        CreateMarker("Enemy_Spawn_C", new Vector3(5f, 0.03f, -4f), marks.transform, new Color(1f, 0.5f, 0.25f));
    }

    private static void CreateLighting()
    {
        GameObject directional = new GameObject("Directional Light");
        Light dirLight = directional.AddComponent<Light>();
        dirLight.type = LightType.Directional;
        dirLight.transform.rotation = Quaternion.Euler(50f, -35f, 0f);
        dirLight.intensity = 0.65f;
        dirLight.color = new Color(1f, 0.95f, 0.86f);
        dirLight.shadows = LightShadows.Soft;

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.35f, 0.38f, 0.43f);

        CreatePointLight("Cool Fill", new Vector3(-6f, 5f, -6f), new Color(0.45f, 0.65f, 1f), 0.9f, 13f);
        CreatePointLight("Warm Action Light", new Vector3(5f, 4f, 5f), new Color(1f, 0.62f, 0.35f), 1.1f, 10f);
    }

    private static GameObject CreatePlayer()
    {
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.transform.position = new Vector3(0f, 1f, 0f);

        CharacterController controller = player.AddComponent<CharacterController>();
        controller.center = new Vector3(0f, 0f, 0f);
        controller.radius = 0.42f;
        controller.height = 1.9f;

        PlayerStats stats = player.AddComponent<PlayerStats>();
        player.AddComponent<PlayerController>();

        GameObject visualRoot = new GameObject("KayKitVisual");
        visualRoot.transform.SetParent(player.transform, false);
        visualRoot.transform.localPosition = new Vector3(0f, -0.95f, 0f);
        visualRoot.transform.localRotation = Quaternion.identity;
        visualRoot.transform.localScale = Vector3.one;

        GameObject knightAsset = AssetDatabase.LoadAssetAtPath<GameObject>(KnightPath);
        if (knightAsset != null)
        {
            GameObject knight = (GameObject)PrefabUtility.InstantiatePrefab(knightAsset);
            knight.name = "Player_Knight_Model";
            knight.transform.SetParent(visualRoot.transform, false);
            knight.transform.localPosition = Vector3.zero;
            knight.transform.localRotation = Quaternion.identity;
        }
        else
        {
            GameObject fallback = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            fallback.name = "Player_Knight_Model";
            fallback.transform.SetParent(visualRoot.transform, false);
            fallback.GetComponent<Renderer>().sharedMaterial = CreateMaterial(new Color(0.2f, 0.45f, 1f));
        }

        CharacterAnimationController animation = player.AddComponent<CharacterAnimationController>();
        animation.visualRoot = visualRoot.transform;

        GameObject healthBarObject = new GameObject("PlayerHealthBar");
        healthBarObject.transform.SetParent(player.transform, false);
        WorldHealthBar healthBar = healthBarObject.AddComponent<WorldHealthBar>();
        healthBar.target = player.transform;
        healthBar.offset = new Vector3(0f, 1.7f, 0f);
        healthBar.showOnlyWhenDamaged = false;
        stats.OnHealthChanged.AddListener(hp => healthBar.SetHealth(hp, stats.maxHP));

        return player;
    }

    private static void CreateCamera(Transform player)
    {
        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.fieldOfView = GameConfig.CameraFOV;
        camera.nearClipPlane = GameConfig.CameraNearClip;
        camera.farClipPlane = GameConfig.CameraFarClip;
        cameraObject.AddComponent<AudioListener>();

        CameraFollow follow = cameraObject.AddComponent<CameraFollow>();
        follow.target = player;
        follow.offset = GameConfig.CameraOffset;
        follow.rotation = GameConfig.CameraRotation;
    }

    private static void CreateManagers(GameObject player, UIManager uiManager)
    {
        GameManager gameManager = new GameObject("GameManager").AddComponent<GameManager>();
        gameManager.playerTransform = player.transform;
        gameManager.playerStats = player.GetComponent<PlayerStats>();
        gameManager.uiManager = uiManager;

        new GameObject("CombatManager").AddComponent<CombatManager>();
    }

    private static UIManager CreateUi()
    {
        GameObject canvasObject = new GameObject("UICanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject uiManagerObject = new GameObject("UIManager");
        UIManager uiManager = uiManagerObject.AddComponent<UIManager>();
        uiManager.mainCanvas = canvas;
        return uiManager;
    }

    private static void CreateSandboxController(GameObject player, UIManager uiManager)
    {
        GameObject root = new GameObject("QA_Sandbox_Controller");
        QASandboxController sandbox = root.AddComponent<QASandboxController>();
        sandbox.playerStats = player.GetComponent<PlayerStats>();
        sandbox.playerController = player.GetComponent<PlayerController>();
        sandbox.playerAnimation = player.GetComponent<CharacterAnimationController>();
        sandbox.uiManager = uiManager;
        sandbox.skeletonPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SkeletonPrefabPath);
        sandbox.slimePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SlimePrefabPath);

        GameObject enemyRoot = new GameObject("QA_Enemies");
        sandbox.enemySpawnRoot = enemyRoot.transform;

        Vector3[] spawnPositions =
        {
            new Vector3(4f, 0f, 4f),
            new Vector3(-4f, 0f, 5f),
            new Vector3(5f, 0f, -4f),
            new Vector3(-5f, 0f, -3f)
        };

        GameObject spawnRoot = new GameObject("QA_EnemySpawnPoints");
        sandbox.enemySpawnPoints = new Transform[spawnPositions.Length];
        for (int i = 0; i < spawnPositions.Length; i++)
        {
            GameObject point = new GameObject("Spawn_" + i);
            point.transform.SetParent(spawnRoot.transform);
            point.transform.position = spawnPositions[i];
            sandbox.enemySpawnPoints[i] = point.transform;
        }
    }

    private static void BuildNavMesh()
    {
        UnityEditor.AI.NavMeshBuilder.ClearAllNavMeshes();
        UnityEditor.AI.NavMeshBuilder.BuildNavMesh();
    }

    private static GameObject CreateCube(string name, Vector3 position, Vector3 scale, Transform parent, Color color)
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

    private static void CreateMarker(string name, Vector3 position, Transform parent, Color color)
    {
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        marker.name = name;
        marker.transform.SetParent(parent);
        marker.transform.position = position;
        marker.transform.localScale = new Vector3(0.55f, 0.03f, 0.55f);
        marker.GetComponent<Renderer>().sharedMaterial = CreateMaterial(color);
        Object.DestroyImmediate(marker.GetComponent<Collider>());
    }

    private static void CreatePointLight(string name, Vector3 position, Color color, float intensity, float range)
    {
        GameObject obj = new GameObject(name);
        obj.transform.position = position;
        Light light = obj.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
        light.shadows = LightShadows.Soft;
    }

    private static Material CreateMaterial(Color color)
    {
        Material material = new Material(Shader.Find("Standard"));
        material.color = color;
        return material;
    }
}
