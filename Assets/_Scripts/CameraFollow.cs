using UnityEngine;
public class CameraFollow : MonoBehaviour {
    public Transform target;
    public Vector3 offset = new Vector3(0, 15, -8);
    public Vector3 rotation = new Vector3(60, 0, 0);
    public float followSpeed = 5f;
    private Camera cam;
    private float shakeTimeRemaining;
    private float shakeDuration;
    private float shakeMagnitude;

    private void Awake() { cam = GetComponent<Camera>(); }
    private void Start() {
        if (target == null) { GameObject p = GameObject.FindGameObjectWithTag("Player"); if (p != null) target = p.transform; }
        if (cam != null) { cam.fieldOfView = 60f; cam.nearClipPlane = 0.3f; cam.farClipPlane = 100f; }
        transform.rotation = Quaternion.Euler(rotation);
    }
    private void LateUpdate() {
        if (target == null) return;
        Vector3 tp = target.position + offset;
        Vector3 followedPosition = Vector3.Lerp(transform.position, tp, followSpeed * Time.deltaTime);
        Vector3 shakeOffset = Vector3.zero;
        if (shakeTimeRemaining > 0f)
        {
            shakeTimeRemaining -= Time.unscaledDeltaTime;
            float falloff = shakeDuration > 0f ? Mathf.Clamp01(shakeTimeRemaining / shakeDuration) : 0f;
            shakeOffset = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f) * shakeMagnitude * falloff;
        }

        transform.position = followedPosition + shakeOffset;
        transform.rotation = Quaternion.Euler(rotation);
        if (CombatManager.Instance != null) CombatManager.Instance.UpdateCameraOriginalPosition(followedPosition);
    }

    public void AddShake(float magnitude, float duration)
    {
        if (duration <= 0f || magnitude <= 0f)
        {
            return;
        }

        shakeMagnitude = Mathf.Max(shakeMagnitude, magnitude);
        shakeDuration = Mathf.Max(shakeDuration, duration);
        shakeTimeRemaining = Mathf.Max(shakeTimeRemaining, duration);
    }
}
