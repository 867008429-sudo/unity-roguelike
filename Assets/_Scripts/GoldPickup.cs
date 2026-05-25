using UnityEngine;
public class GoldPickup : MonoBehaviour {
    public int goldAmount = 1;
    public float pickupRange = 2.5f;
    public float flySpeed = 10f;
    public float rotationSpeed = 90f;
    public float lifetime = 60f;
    public float pickupCooldown = 0.3f;
    private float spawnTime;
    private float bobSeed;
    private Vector3 basePosition;
    private bool isFlying;
    private Transform playerTransform;
    private void Start() {
        spawnTime = Time.time;
        bobSeed = Random.Range(0f, 100f);
        basePosition = transform.position;
        EnsureReadableGoldVisual();
        SphereCollider col = gameObject.AddComponent<SphereCollider>();
        col.isTrigger = true; col.radius = pickupRange;
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null) { rb = gameObject.AddComponent<Rigidbody>(); rb.isKinematic = true; }
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) playerTransform = p.transform;
    }
    private void Update() {
        if (Time.time - spawnTime > lifetime) { Destroy(gameObject); return; }
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        if (!isFlying) {
            Vector3 p = basePosition;
            p.y += Mathf.Sin((Time.time + bobSeed) * 3.2f) * 0.06f;
            transform.position = p;
        }
        if (isFlying && playerTransform != null) {
            Vector3 dir = (playerTransform.position - transform.position).normalized;
            transform.position += dir * flySpeed * Time.deltaTime;
            if (Vector3.Distance(transform.position, playerTransform.position) < 0.5f) CollectGold();
        }
    }
    private void OnTriggerEnter(Collider other) { if (other.CompareTag("Player") && Time.time - spawnTime >= pickupCooldown && !isFlying) isFlying = true; }
    private void OnTriggerStay(Collider other) { if (other.CompareTag("Player") && Time.time - spawnTime >= pickupCooldown && !isFlying) isFlying = true; }
    private void CollectGold() { if (GameManager.Instance != null && GameManager.Instance.playerStats != null) { GameManager.Instance.playerStats.AddGold(goldAmount); ShowGoldFloatText(); } Destroy(gameObject); }
    private void EnsureReadableGoldVisual() {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer == null) return;

        Material material = new Material(Shader.Find("Standard"));
        material.color = new Color(1f, 0.76f, 0.12f, 1f);
        material.EnableKeyword("_EMISSION");
        material.SetColor("_EmissionColor", new Color(1f, 0.52f, 0.08f, 1f) * 0.7f);
        renderer.material = material;

        if (transform.localScale.x < 0.42f || transform.localScale.z < 0.42f) {
            transform.localScale = new Vector3(0.46f, 0.06f, 0.46f);
        }
    }
    private void ShowGoldFloatText() {
        if (playerTransform == null) return;
        GameObject fo = new GameObject("GoldFloatText");
        fo.transform.position = playerTransform.position + Vector3.up * 2.5f;
        FloatingText floatingText = fo.AddComponent<FloatingText>();
        floatingText.floatHeight = 1.1f;
        floatingText.duration = 0.8f;
        floatingText.Initialize("+" + goldAmount + " G", new Color(1f, 0.84f, 0f, 1f), 28, 0.08f);
    }
}
