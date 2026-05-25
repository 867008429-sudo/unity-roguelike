using UnityEngine;

public class EnemyHazardZone : MonoBehaviour
{
    public float damagePerTick = 2f;
    public float tickInterval = 0.45f;
    public float lifetime = 3.2f;
    public Vector3 sourcePosition;

    private float spawnTime;
    private float nextTickTime;
    private Renderer hazardRenderer;
    private Vector3 baseScale;

    public static EnemyHazardZone SpawnAcidPool(Vector3 position, float radius, float damagePerTick, Vector3 sourcePosition)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        obj.name = "AcidPool";
        obj.transform.position = position + Vector3.up * 0.04f;
        obj.transform.localScale = new Vector3(radius * 2f, 0.025f, radius * 2f);

        Collider collider = obj.GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }

        Rigidbody rb = obj.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material material = CreateMaterial(new Color(0.18f, 0.9f, 0.12f, 0.72f), new Color(0.02f, 0.45f, 0.02f, 1f));
            renderer.material = material;
        }

        EnemyHazardZone hazard = obj.AddComponent<EnemyHazardZone>();
        hazard.damagePerTick = damagePerTick;
        hazard.sourcePosition = sourcePosition;
        return hazard;
    }

    private void Start()
    {
        spawnTime = Time.time;
        nextTickTime = Time.time;
        hazardRenderer = GetComponent<Renderer>();
        baseScale = transform.localScale;
    }

    private void Update()
    {
        float age = Time.time - spawnTime;
        if (age >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        float pulse = 1f + Mathf.Sin(Time.time * 7f) * 0.04f;
        transform.localScale = new Vector3(baseScale.x * pulse, baseScale.y, baseScale.z * pulse);

        if (hazardRenderer != null)
        {
            Color color = hazardRenderer.material.color;
            color.a = Mathf.Lerp(0.72f, 0.12f, age / lifetime);
            hazardRenderer.material.color = color;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other == null || !other.CompareTag("Player") || Time.time < nextTickTime)
        {
            return;
        }

        PlayerStats playerStats = other.GetComponent<PlayerStats>();
        if (playerStats != null)
        {
            nextTickTime = Time.time + tickInterval;
            playerStats.TakeDamage(damagePerTick, sourcePosition);
        }
    }

    private static Material CreateMaterial(Color color, Color emission)
    {
        Shader shader = Shader.Find("Standard");
        if (shader == null)
        {
            shader = Shader.Find("Universal Render Pipeline/Lit");
        }

        if (shader == null)
        {
            shader = Shader.Find("Diffuse");
        }

        Material material = new Material(shader);
        material.color = color;
        material.EnableKeyword("_EMISSION");
        material.SetColor("_EmissionColor", emission);
        return material;
    }
}
