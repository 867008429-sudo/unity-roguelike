using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class KayKitSceneIntegrator
{
    private const string PrefabFolder = "Assets/_Prefabs/KayKit";

    [MenuItem("RPG Tools/Apply KayKit Art", false, 20)]
    public static void ApplyKayKitArt()
    {
        EnsureFolders();

        Material slimeMaterial = CreateMaterial("KayKit_Slime_Gel", new Color(0.22f, 0.85f, 0.42f, 0.9f));
        GameObject skeletonPrefab = CreateEnemyPrefab(
            "SkeletonEnemy_KayKit",
            EnemyStats.EnemyType.Skeleton,
            "Assets/_ImportedArt/KayKit/Skeletons/Characters/fbx/Skeleton_Minion.fbx",
            new Vector3(0f, 0f, 0f),
            Vector3.zero,
            new Vector3(0.72f, 0.72f, 0.72f),
            null);

        GameObject slimePrefab = CreateEnemyPrefab(
            "SlimeEnemy_KayKitStyle",
            EnemyStats.EnemyType.Slime,
            null,
            Vector3.zero,
            Vector3.zero,
            Vector3.one,
            slimeMaterial);

        ApplyPlayerVisual();
        ApplyWavePrefabs(skeletonPrefab, slimePrefab);
        ApplyDungeonVisuals();

        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
    }

    private static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/_Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "_Prefabs");
        }

        if (!AssetDatabase.IsValidFolder(PrefabFolder))
        {
            AssetDatabase.CreateFolder("Assets/_Prefabs", "KayKit");
        }
    }

    private static Material CreateMaterial(string name, Color color)
    {
        string path = PrefabFolder + "/" + name + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("Standard"));
            AssetDatabase.CreateAsset(material, path);
        }

        material.color = color;
        EditorUtility.SetDirty(material);
        return material;
    }

    private static void ApplyPlayerVisual()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            return;
        }

        MeshRenderer rootRenderer = player.GetComponent<MeshRenderer>();
        if (rootRenderer != null)
        {
            rootRenderer.enabled = false;
        }

        ClearNamedChild(player.transform, "KayKitVisual");
        GameObject visualRoot = new GameObject("KayKitVisual");
        visualRoot.transform.SetParent(player.transform, false);
        AddVisual(
            visualRoot.transform,
            "Assets/_ImportedArt/KayKit/Adventurers/Characters/fbx/Knight.fbx",
            "Player_Knight_Model",
            new Vector3(0f, -0.02f, 0f),
            Vector3.zero,
            new Vector3(0.78f, 0.78f, 0.78f));
    }

    private static void ApplyWavePrefabs(GameObject skeletonPrefab, GameObject slimePrefab)
    {
        WaveManager waveManager = Object.FindObjectOfType<WaveManager>();
        if (waveManager == null)
        {
            return;
        }

        waveManager.skeletonPrefab = skeletonPrefab;
        waveManager.slimePrefab = slimePrefab;
        EditorUtility.SetDirty(waveManager);
    }

    private static GameObject CreateEnemyPrefab(
        string prefabName,
        EnemyStats.EnemyType enemyType,
        string visualPath,
        Vector3 visualPosition,
        Vector3 visualEuler,
        Vector3 visualScale,
        Material fallbackMaterial)
    {
        GameObject root = new GameObject(prefabName);
        root.tag = "Enemy";

        CapsuleCollider collider = root.AddComponent<CapsuleCollider>();
        bool skeleton = enemyType == EnemyStats.EnemyType.Skeleton;
        collider.center = skeleton ? new Vector3(0f, 0.85f, 0f) : new Vector3(0f, 0.35f, 0f);
        collider.radius = skeleton ? 0.35f : 0.45f;
        collider.height = skeleton ? 1.7f : 0.7f;

        EnemyStats stats = root.AddComponent<EnemyStats>();
        stats.enemyType = enemyType;
        root.AddComponent<EnemyAI>();

        GameObject visualRoot = new GameObject("KayKitVisual");
        visualRoot.transform.SetParent(root.transform, false);
        if (!string.IsNullOrEmpty(visualPath))
        {
            AddVisual(visualRoot.transform, visualPath, prefabName + "_Model", visualPosition, visualEuler, visualScale);
        }
        else
        {
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            body.name = "SlimeBody";
            body.transform.SetParent(visualRoot.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.35f, 0f);
            body.transform.localScale = new Vector3(0.9f, 0.55f, 0.9f);
            body.GetComponent<Renderer>().sharedMaterial = fallbackMaterial;
            Object.DestroyImmediate(body.GetComponent<Collider>());
        }

        string path = PrefabFolder + "/" + prefabName + ".prefab";
        GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return savedPrefab;
    }

    private static void ApplyDungeonVisuals()
    {
        GameObject ground = GameObject.Find("Ground");
        if (ground != null)
        {
            MeshRenderer renderer = ground.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.enabled = false;
            }
        }

        GameObject walls = GameObject.Find("Walls");
        if (walls != null)
        {
            foreach (MeshRenderer renderer in walls.GetComponentsInChildren<MeshRenderer>())
            {
                renderer.enabled = false;
            }
        }

        DisablePlaceholderColliders("Obstacles");
        HidePlaceholderRenderers("Obstacles");
        HidePlaceholderRenderers("DecorPillar_A");
        HidePlaceholderRenderers("DecorPillar_B");
        HidePlaceholderRenderers("DecorPillar_C");
        HidePlaceholderRenderers("DecorPillar_D");
        HidePlaceholderRenderers("DecorTorch_A");
        HidePlaceholderRenderers("DecorTorch_B");
        HidePlaceholderRenderers("DecorTorch_C");
        HidePlaceholderRenderers("DecorTorch_D");

        ReplaceFloor();
        ReplaceWalls();
        ReplaceProps();
    }

    private static void HidePlaceholderRenderers(string objectName)
    {
        GameObject obj = GameObject.Find(objectName);
        if (obj == null)
        {
            return;
        }

        foreach (MeshRenderer renderer in obj.GetComponentsInChildren<MeshRenderer>())
        {
            renderer.enabled = false;
        }
    }

    private static void DisablePlaceholderColliders(string objectName)
    {
        GameObject obj = GameObject.Find(objectName);
        if (obj == null)
        {
            return;
        }

        DisableColliders(obj);
    }

    private static void DisableColliders(GameObject root)
    {
        foreach (Collider collider in root.GetComponentsInChildren<Collider>())
        {
            collider.enabled = false;
        }
    }

    private static void ReplaceFloor()
    {
        DestroyExisting("KayKit_FloorTiles");
        GameObject floorRoot = new GameObject("KayKit_FloorTiles");
        floorRoot.isStatic = true;

        for (int x = -5; x < 5; x++)
        {
            for (int z = -5; z < 5; z++)
            {
                GameObject tile = AddVisual(
                    floorRoot.transform,
                    "Assets/_ImportedArt/KayKit/Dungeon/fbx/floor_tile_large.fbx",
                    "FloorTile",
                    new Vector3(x * 4f + 2f, 0f, z * 4f + 2f),
                    Vector3.zero,
                    Vector3.one);
                if (tile != null)
                {
                    tile.isStatic = true;
                }
            }
        }
    }

    private static void ReplaceWalls()
    {
        DestroyExisting("KayKit_WallVisuals");
        GameObject wallRoot = new GameObject("KayKit_WallVisuals");
        wallRoot.isStatic = true;

        for (int i = -5; i < 5; i++)
        {
            AddVisual(wallRoot.transform, "Assets/_ImportedArt/KayKit/Dungeon/fbx/wall.fbx", "Wall_North_Art", new Vector3(i * 4f + 2f, 0f, 20f), Vector3.zero, Vector3.one);
            AddVisual(wallRoot.transform, "Assets/_ImportedArt/KayKit/Dungeon/fbx/wall.fbx", "Wall_South_Art", new Vector3(i * 4f + 2f, 0f, -20f), new Vector3(0f, 180f, 0f), Vector3.one);
            AddVisual(wallRoot.transform, "Assets/_ImportedArt/KayKit/Dungeon/fbx/wall.fbx", "Wall_East_Art", new Vector3(20f, 0f, i * 4f + 2f), new Vector3(0f, 90f, 0f), Vector3.one);
            AddVisual(wallRoot.transform, "Assets/_ImportedArt/KayKit/Dungeon/fbx/wall.fbx", "Wall_West_Art", new Vector3(-20f, 0f, i * 4f + 2f), new Vector3(0f, -90f, 0f), Vector3.one);
        }
    }

    private static void ReplaceProps()
    {
        DestroyExisting("KayKit_DungeonProps");
        GameObject propRoot = new GameObject("KayKit_DungeonProps");

        Vector3[] pillarPositions =
        {
            new Vector3(8f, 0f, 12f),
            new Vector3(-10f, 0f, -8f),
            new Vector3(-14f, 0f, 10f),
            new Vector3(16f, 0f, -12f)
        };

        foreach (Vector3 position in pillarPositions)
        {
            GameObject pillar = AddVisual(propRoot.transform, "Assets/_ImportedArt/KayKit/Dungeon/fbx/pillar.fbx", "Pillar_Art", position, Vector3.zero, Vector3.one);
            ConfigureSolidCapsule(pillar, new Vector3(0f, 1.5f, 0f), 0.42f, 3.0f);
        }

        Vector3[] torchPositions =
        {
            new Vector3(-17f, 1.4f, 16f),
            new Vector3(18f, 1.4f, 15f),
            new Vector3(17f, 1.4f, -16f),
            new Vector3(-18f, 1.4f, -14f)
        };

        foreach (Vector3 position in torchPositions)
        {
            AddVisual(propRoot.transform, "Assets/_ImportedArt/KayKit/Dungeon/fbx/torch_lit.fbx", "Torch_Art", position, Vector3.zero, Vector3.one);
        }

        GameObject chest = AddVisual(propRoot.transform, "Assets/_ImportedArt/KayKit/Dungeon/fbx/chest.fbx", "LootChest_Art", new Vector3(3f, 0f, -8f), new Vector3(0f, 35f, 0f), Vector3.one);
        ConfigureSolidBox(chest, new Vector3(0f, 0.55f, 0f), new Vector3(1.4f, 1.1f, 1.1f));
        ConfigureInteractableHint(chest, "Open", "Prompts/Keyboard_E");
        ConfigureLootChest(chest);
        GameObject barrel = AddVisual(propRoot.transform, "Assets/_ImportedArt/KayKit/Dungeon/fbx/barrel_large.fbx", "Barrel_Art", new Vector3(-7f, 0f, 5f), new Vector3(0f, -20f, 0f), Vector3.one);
        ConfigureSolidCapsule(barrel, new Vector3(0f, 0.65f, 0f), 0.45f, 1.3f);
        GameObject crate = AddVisual(propRoot.transform, "Assets/_ImportedArt/KayKit/Dungeon/fbx/box_large.fbx", "Crate_Art", new Vector3(11f, 0f, -5f), new Vector3(0f, 12f, 0f), Vector3.one);
        ConfigureSolidBox(crate, new Vector3(0f, 0.55f, 0f), new Vector3(1.2f, 1.1f, 1.2f));
    }

    private static void ConfigureInteractableHint(GameObject target, string hintText, string spritePath)
    {
        if (target == null)
        {
            return;
        }

        InteractableHint hint = target.GetComponent<InteractableHint>();
        if (hint == null)
        {
            hint = target.AddComponent<InteractableHint>();
        }

        hint.hintText = hintText;
        hint.promptSpriteResourcePath = spritePath;
        EditorUtility.SetDirty(target);
    }

    private static void ConfigureSolidBox(GameObject target, Vector3 center, Vector3 size)
    {
        if (target == null)
        {
            return;
        }

        BoxCollider collider = target.GetComponent<BoxCollider>();
        if (collider == null)
        {
            collider = target.AddComponent<BoxCollider>();
        }

        collider.isTrigger = false;
        collider.center = center;
        collider.size = size;
        EditorUtility.SetDirty(target);
    }

    private static void ConfigureLootChest(GameObject target)
    {
        if (target == null)
        {
            return;
        }

        LootChestInteractable chest = target.GetComponent<LootChestInteractable>();
        if (chest == null)
        {
            chest = target.AddComponent<LootChestInteractable>();
        }

        chest.goldReward = 20;
        chest.xpReward = 15;
        chest.interactDistance = 2.6f;
        EditorUtility.SetDirty(target);
    }

    private static void ConfigureSolidCapsule(GameObject target, Vector3 center, float radius, float height)
    {
        if (target == null)
        {
            return;
        }

        CapsuleCollider collider = target.GetComponent<CapsuleCollider>();
        if (collider == null)
        {
            collider = target.AddComponent<CapsuleCollider>();
        }

        collider.isTrigger = false;
        collider.center = center;
        collider.radius = radius;
        collider.height = height;
        collider.direction = 1;
        EditorUtility.SetDirty(target);
    }

    private static GameObject AddVisual(
        Transform parent,
        string assetPath,
        string name,
        Vector3 localPosition,
        Vector3 localEuler,
        Vector3 localScale)
    {
        GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (asset == null)
        {
            Debug.LogWarning("Missing art asset: " + assetPath);
            return null;
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(asset);
        instance.name = name;
        instance.transform.SetParent(parent, false);
        instance.transform.localPosition = localPosition;
        instance.transform.localRotation = Quaternion.Euler(localEuler);
        instance.transform.localScale = localScale;
        return instance;
    }

    private static void ClearNamedChild(Transform parent, string childName)
    {
        Transform child = parent.Find(childName);
        if (child != null)
        {
            Object.DestroyImmediate(child.gameObject);
        }
    }

    private static void DestroyExisting(string name)
    {
        GameObject existing = GameObject.Find(name);
        if (existing != null)
        {
            Object.DestroyImmediate(existing);
        }
    }
}
