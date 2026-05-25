using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float damage = 6f;
    public float speed = 8f;
    public float lifetime = 4f;
    public float acidRadius = 0.9f;
    public float acidDamagePerTick = 2f;
    public Vector3 direction = Vector3.forward;
    public Vector3 sourcePosition;
    public Vector3 targetPosition;

    private float spawnTime;
    private bool hasImpacted;

    public static EnemyProjectile Spawn(Vector3 position, Vector3 targetPosition, float damage, float speed)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        obj.name = "EnemyProjectile";
        obj.transform.position = position;
        obj.transform.localScale = new Vector3(0.28f, 0.28f, 0.28f);

        Collider collider = obj.GetComponent<Collider>();
        collider.isTrigger = true;

        Rigidbody rb = obj.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material material = CreateMaterial();
            material.color = new Color(0.35f, 1f, 0.28f, 0.95f);
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", new Color(0.08f, 0.55f, 0.06f, 1f));
            renderer.material = material;
        }

        EnemyProjectile projectile = obj.AddComponent<EnemyProjectile>();
        Vector3 direction = targetPosition - position;
        direction.y = 0f;
        projectile.direction = direction.sqrMagnitude > 0.001f ? direction.normalized : Vector3.forward;
        projectile.damage = damage;
        projectile.speed = speed;
        projectile.sourcePosition = position;
        projectile.targetPosition = targetPosition;
        return projectile;
    }

    private void Start()
    {
        spawnTime = Time.time;
    }

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
        transform.Rotate(Vector3.up, 360f * Time.deltaTime, Space.World);

        Vector3 flatCurrent = transform.position;
        flatCurrent.y = 0f;
        Vector3 flatTarget = targetPosition;
        flatTarget.y = 0f;

        if (Vector3.Distance(flatCurrent, flatTarget) <= 0.22f || Time.time - spawnTime >= lifetime)
        {
            Impact(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null || !other.CompareTag("Player"))
        {
            return;
        }

        PlayerStats playerStats = other.GetComponent<PlayerStats>();
        if (playerStats != null)
        {
            playerStats.TakeDamage(damage, sourcePosition);
        }

        Impact(true);
    }

    private void Impact(bool hitPlayer)
    {
        if (hasImpacted)
        {
            return;
        }

        hasImpacted = true;
        if (VisualEffectsManager.Instance != null)
        {
            VisualEffectsManager.Instance.PlayHitBurst(transform.position, new Color(0.3f, 1f, 0.25f, 1f));
        }

        if (!hitPlayer)
        {
            EnemyHazardZone.SpawnAcidPool(transform.position, acidRadius, acidDamagePerTick, sourcePosition);
        }

        Destroy(gameObject);
    }

    private static Material CreateMaterial()
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

        return new Material(shader);
    }
}
