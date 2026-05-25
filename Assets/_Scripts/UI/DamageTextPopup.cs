using UnityEngine;

public class DamageTextPopup : MonoBehaviour
{
    private TextMesh textMesh;
    private DamageTextPool owner;
    private Vector3 startPosition;
    private Vector3 horizontalDrift;
    private Color startColor;
    private float duration;
    private float elapsed;
    private bool active;
    private float baseScale;

    public void Initialize(DamageTextPool pool)
    {
        owner = pool;
        textMesh = GetComponent<TextMesh>();
        if (textMesh == null)
        {
            textMesh = gameObject.AddComponent<TextMesh>();
        }

        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.fontSize = 44;
        textMesh.characterSize = 0.08f;
        textMesh.richText = true;
        textMesh.font = UITheme.GameFont;
        gameObject.SetActive(false);
    }

    public void Play(Vector3 position, int amount, bool critical, Color? overrideColor = null)
    {
        if (textMesh == null)
        {
            Initialize(owner);
        }

        startPosition = position + new Vector3(Random.Range(-0.35f, 0.35f), 1.35f, Random.Range(-0.25f, 0.25f));
        horizontalDrift = new Vector3(Random.Range(-0.6f, 0.6f), 0f, Random.Range(-0.3f, 0.3f));
        duration = critical ? 1.05f : 0.85f;
        elapsed = 0f;
        active = true;
        baseScale = critical ? 0.46f : 0.34f;
        startColor = overrideColor ?? (critical ? new Color(1f, 0.18f, 0.08f, 1f) : new Color(1f, 0.86f, 0.28f, 1f));
        textMesh.text = critical ? "CRIT " + amount : "-" + amount;
        textMesh.color = startColor;
        transform.position = startPosition;
        transform.localScale = Vector3.one * baseScale * 0.55f;
        gameObject.SetActive(true);
    }

    public void PlayText(Vector3 position, string text, Color color, bool emphasized)
    {
        if (textMesh == null)
        {
            Initialize(owner);
        }

        startPosition = position + new Vector3(Random.Range(-0.25f, 0.25f), 1.45f, Random.Range(-0.2f, 0.2f));
        horizontalDrift = new Vector3(Random.Range(-0.25f, 0.25f), 0f, Random.Range(-0.2f, 0.2f));
        duration = emphasized ? 1.05f : 0.85f;
        elapsed = 0f;
        active = true;
        baseScale = emphasized ? 0.42f : 0.32f;
        startColor = color;
        textMesh.text = text;
        textMesh.color = startColor;
        transform.position = startPosition;
        transform.localScale = Vector3.one * baseScale * 0.55f;
        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (!active)
        {
            return;
        }

        elapsed += Time.unscaledDeltaTime;
        float t = Mathf.Clamp01(elapsed / duration);
        float arc = Mathf.Sin(t * Mathf.PI) * 0.42f;
        transform.position = startPosition + horizontalDrift * t + Vector3.up * (1.15f * t + arc);

        float pop = t < 0.22f ? Mathf.Lerp(0.55f, 1.22f, t / 0.22f) : Mathf.Lerp(1.22f, 0.88f, (t - 0.22f) / 0.78f);
        transform.localScale = Vector3.one * baseScale * pop;

        if (textMesh != null)
        {
            Color color = startColor;
            color.a = Mathf.SmoothStep(1f, 0f, Mathf.Clamp01((t - 0.45f) / 0.55f));
            textMesh.color = color;
        }

        if (Camera.main != null)
        {
            transform.forward = Camera.main.transform.forward;
        }

        if (elapsed >= duration)
        {
            active = false;
            owner.Release(this);
        }
    }
}
