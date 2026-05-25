using UnityEngine;
public class HealthPotion : MonoBehaviour {
    public float healAmount = 30f;
    public float pickupRange = 0.8f;
    public float floatAmplitude = 0.1f;
    public float floatPeriod = 2f;
    public float lifetime = 60f;
    public float pickupCooldown = 0.3f;
    private float spawnTime;
    private Vector3 startPosition;
    private const string VisualRootName = "PotionVisual";

    private void Start() {
        spawnTime = Time.time;
        startPosition = transform.position;
        EnsurePotionVisual();
        SphereCollider col = gameObject.AddComponent<SphereCollider>();
        col.isTrigger = true; col.radius = pickupRange;
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null) { rb = gameObject.AddComponent<Rigidbody>(); rb.isKinematic = true; }
    }
    private void Update() {
        if (Time.time - spawnTime > lifetime) { Destroy(gameObject); return; }
        float off = Mathf.Sin(Time.time * (2f * Mathf.PI / floatPeriod)) * floatAmplitude;
        transform.position = startPosition + new Vector3(0, off, 0);
    }
    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player") && Time.time - spawnTime >= pickupCooldown) {
            PlayerStats stats = other.GetComponent<PlayerStats>();
            if (stats != null && stats.currentHP < stats.maxHP) { stats.Heal(healAmount); Destroy(gameObject); }
        }
    }

    private void EnsurePotionVisual() {
        if (transform.Find(VisualRootName) != null) {
            return;
        }

        Renderer rootRenderer = GetComponent<Renderer>();
        Renderer[] childRenderers = GetComponentsInChildren<Renderer>();
        if (rootRenderer == null && childRenderers.Length > 0) {
            return;
        }

        if (rootRenderer != null && childRenderers.Length > 1) {
            return;
        }

        if (rootRenderer != null) {
            rootRenderer.enabled = false;
        }

        GameObject visualRoot = new GameObject(VisualRootName);
        visualRoot.transform.SetParent(transform, false);
        visualRoot.transform.localPosition = Vector3.up * 0.28f;
        visualRoot.transform.localRotation = Quaternion.Euler(0f, 0f, -10f);
        visualRoot.transform.localScale = Vector3.one;

        Material glassMaterial = CreateMaterial("Potion_RubyGlass", new Color(1f, 0.05f, 0.08f, 0.88f), new Color(0.55f, 0.02f, 0.03f, 1f));
        Material liquidMaterial = CreateMaterial("Potion_DeepLiquid", new Color(0.75f, 0.02f, 0.04f, 1f), new Color(0.35f, 0.01f, 0.02f, 1f));
        Material corkMaterial = CreateMaterial("Potion_Cork", new Color(0.45f, 0.28f, 0.12f, 1f), Color.black);
        Material highlightMaterial = CreateMaterial("Potion_Highlight", new Color(1f, 0.85f, 0.85f, 0.95f), new Color(0.7f, 0.12f, 0.12f, 1f));

        CreatePart("BottleBody", PrimitiveType.Capsule, visualRoot.transform, new Vector3(0f, 0f, 0f), Quaternion.identity, new Vector3(0.16f, 0.24f, 0.16f), glassMaterial);
        CreatePart("LiquidCore", PrimitiveType.Sphere, visualRoot.transform, new Vector3(0f, -0.08f, 0f), Quaternion.identity, new Vector3(0.28f, 0.22f, 0.28f), liquidMaterial);
        CreatePart("BottleNeck", PrimitiveType.Cylinder, visualRoot.transform, new Vector3(0f, 0.34f, 0f), Quaternion.identity, new Vector3(0.07f, 0.12f, 0.07f), glassMaterial);
        CreatePart("Cork", PrimitiveType.Cylinder, visualRoot.transform, new Vector3(0f, 0.51f, 0f), Quaternion.identity, new Vector3(0.085f, 0.055f, 0.085f), corkMaterial);
        CreatePart("FrontHighlight", PrimitiveType.Sphere, visualRoot.transform, new Vector3(-0.07f, 0.08f, -0.12f), Quaternion.identity, new Vector3(0.055f, 0.12f, 0.025f), highlightMaterial);

        Light glow = visualRoot.AddComponent<Light>();
        glow.type = LightType.Point;
        glow.color = new Color(1f, 0.12f, 0.12f, 1f);
        glow.range = 0.8f;
        glow.intensity = 0.35f;
        glow.shadows = LightShadows.None;
    }

    private GameObject CreatePart(string partName, PrimitiveType primitiveType, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale, Material material) {
        GameObject part = GameObject.CreatePrimitive(primitiveType);
        part.name = partName;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPosition;
        part.transform.localRotation = localRotation;
        part.transform.localScale = localScale;

        Renderer renderer = part.GetComponent<Renderer>();
        if (renderer != null) {
            renderer.material = material;
        }

        Collider collider = part.GetComponent<Collider>();
        if (collider != null) {
            Destroy(collider);
        }

        return part;
    }

    private Material CreateMaterial(string materialName, Color baseColor, Color emissionColor) {
        Shader shader = Shader.Find("Standard");
        if (shader == null) {
            shader = Shader.Find("Universal Render Pipeline/Lit");
        }

        if (shader == null) {
            shader = Shader.Find("Diffuse");
        }

        if (shader == null) {
            shader = Shader.Find("Hidden/InternalErrorShader");
        }

        Material material = new Material(shader);
        material.name = materialName;
        material.color = baseColor;

        if (emissionColor.maxColorComponent > 0f) {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", emissionColor);
        }

        return material;
    }
}
