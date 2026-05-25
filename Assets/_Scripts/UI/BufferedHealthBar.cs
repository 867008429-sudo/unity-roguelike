using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BufferedHealthBar : MonoBehaviour
{
    public Image backImage;
    public Image bufferFill;
    public Image mainFill;
    public TMP_Text label;
    public float bufferDelay = 0.45f;
    public float bufferChaseSpeed = 2.8f;

    private Coroutine bufferRoutine;
    private float bufferValue = 1f;
    private float mainValue = 1f;

    public static BufferedHealthBar Create(Transform parent)
    {
        GameObject root = new GameObject("PlayerBufferedHealthBar");
        root.transform.SetParent(parent, false);
        RectTransform rect = root.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(22f, -22f);
        rect.sizeDelta = new Vector2(310f, 38f);

        Image back = root.AddComponent<Image>();
        back.color = new Color(0.08f, 0.08f, 0.09f, 0.92f);

        GameObject bufferObj = CreateFill("Buffer", rect, new Color(1f, 0.9f, 0.82f, 0.95f));
        GameObject mainObj = CreateFill("Main", rect, new Color(0.88f, 0.18f, 0.16f, 1f));
        TMP_Text text = CreateLabel(rect);

        BufferedHealthBar bar = root.AddComponent<BufferedHealthBar>();
        bar.backImage = back;
        bar.bufferFill = bufferObj.GetComponent<Image>();
        bar.mainFill = mainObj.GetComponent<Image>();
        bar.label = text;
        UITheme.ApplyBar(back, bar.mainFill);
        if (bar.bufferFill != null)
        {
            bar.bufferFill.sprite = UITheme.BarFillSprite;
            bar.bufferFill.type = Image.Type.Filled;
            bar.bufferFill.fillMethod = Image.FillMethod.Horizontal;
            bar.bufferFill.fillOrigin = 0;
        }

        return bar;
    }

    public void SetHealth(float current, float max)
    {
        float nextValue = max > 0f ? Mathf.Clamp01(current / max) : 0f;
        if (label != null)
        {
            label.text = "HP " + Mathf.CeilToInt(current) + " / " + Mathf.CeilToInt(max);
        }

        bool damaged = nextValue < mainValue;
        mainValue = nextValue;
        if (mainFill != null)
        {
            mainFill.fillAmount = mainValue;
        }

        if (!damaged)
        {
            bufferValue = nextValue;
            if (bufferFill != null)
            {
                bufferFill.fillAmount = bufferValue;
            }

            return;
        }

        if (bufferRoutine != null)
        {
            StopCoroutine(bufferRoutine);
        }

        bufferRoutine = StartCoroutine(BufferRoutine(nextValue));
    }

    private static GameObject CreateFill(string name, RectTransform parent, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(8f, 8f);
        rect.offsetMax = new Vector2(-8f, -8f);
        Image image = obj.AddComponent<Image>();
        image.color = color;
        image.type = Image.Type.Filled;
        image.fillMethod = Image.FillMethod.Horizontal;
        image.fillOrigin = 0;
        image.fillAmount = 1f;
        return obj;
    }

    private static TMP_Text CreateLabel(RectTransform parent)
    {
        GameObject obj = new GameObject("Label");
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        TextMeshProUGUI text = obj.AddComponent<TextMeshProUGUI>();
        text.alignment = TextAlignmentOptions.Center;
        UITheme.ApplyHudTMPText(text, Color.white, 15f, true);
        text.outlineWidth = 0.04f;
        text.outlineColor = new Color(0f, 0f, 0f, 0.75f);
        return text;
    }

    private IEnumerator BufferRoutine(float target)
    {
        yield return new WaitForSecondsRealtime(bufferDelay);

        while (bufferValue > target + 0.002f)
        {
            bufferValue = Mathf.MoveTowards(bufferValue, target, bufferChaseSpeed * Time.unscaledDeltaTime);
            if (bufferFill != null)
            {
                bufferFill.fillAmount = bufferValue;
            }

            yield return null;
        }

        bufferValue = target;
        if (bufferFill != null)
        {
            bufferFill.fillAmount = bufferValue;
        }

        bufferRoutine = null;
    }
}
