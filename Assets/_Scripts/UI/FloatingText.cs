using UnityEngine;

public class FloatingText : MonoBehaviour
{
    public float floatHeight = 1.2f;
    public float duration = 0.9f;
    public float startScale = 0.28f;
    public float endScale = 0.18f;

    private Vector3 startPosition;
    private float elapsed;
    private TextMesh textMesh;

    public void Initialize(string text, Color color, int fontSize = 28, float characterSize = 0.08f)
    {
        textMesh = GetComponent<TextMesh>();
        if (textMesh == null)
        {
            textMesh = gameObject.AddComponent<TextMesh>();
        }

        textMesh.text = text;
        textMesh.fontSize = fontSize;
        textMesh.color = color;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.characterSize = characterSize;

        startPosition = transform.position;
        elapsed = 0f;
        transform.localScale = Vector3.one * startScale;
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        float t = duration > 0f ? Mathf.Clamp01(elapsed / duration) : 1f;

        transform.position = startPosition + Vector3.up * (floatHeight * t);
        transform.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, t);

        if (Camera.main != null)
        {
            transform.forward = Camera.main.transform.forward;
        }

        if (textMesh != null)
        {
            Color color = textMesh.color;
            color.a = 1f - t;
            textMesh.color = color;
        }

        if (elapsed >= duration)
        {
            Destroy(gameObject);
        }
    }
}
