using System.Collections;
using UnityEngine;

public static class UIAnimationUtility
{
    private static TweenHost host;

    private static TweenHost Host
    {
        get
        {
            if (host == null)
            {
                GameObject obj = new GameObject("UIAnimationUtility");
                Object.DontDestroyOnLoad(obj);
                host = obj.AddComponent<TweenHost>();
            }

            return host;
        }
    }

    public static Coroutine PopIn(RectTransform rect, CanvasGroup group = null, float duration = 0.22f)
    {
        return rect == null ? null : Host.StartCoroutine(PopInRoutine(rect, group, duration));
    }

    public static Coroutine FadeIn(CanvasGroup group, float duration = 0.18f)
    {
        return group == null ? null : Host.StartCoroutine(FadeRoutine(group, group.alpha, 1f, duration, true));
    }

    public static Coroutine FadeOut(CanvasGroup group, float duration = 0.18f)
    {
        return group == null ? null : Host.StartCoroutine(FadeRoutine(group, group.alpha, 0f, duration, false));
    }

    public static Coroutine PunchScale(RectTransform rect, float amount = 0.18f, float duration = 0.18f)
    {
        return rect == null ? null : Host.StartCoroutine(PunchScaleRoutine(rect, amount, duration));
    }

    public static Coroutine SlideFade(CanvasGroup group, RectTransform rect, Vector2 from, Vector2 to, float duration = 0.24f)
    {
        if (group == null || rect == null)
        {
            return null;
        }

        return Host.StartCoroutine(SlideFadeRoutine(group, rect, from, to, duration));
    }

    private static IEnumerator PopInRoutine(RectTransform rect, CanvasGroup group, float duration)
    {
        if (group != null)
        {
            group.alpha = 0f;
            group.blocksRaycasts = true;
            group.interactable = true;
        }

        Vector3 baseScale = rect.localScale == Vector3.zero ? Vector3.one : rect.localScale;
        rect.localScale = baseScale * 0.78f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = EaseOutBack(t);
            rect.localScale = Vector3.LerpUnclamped(baseScale * 0.78f, baseScale, eased);
            if (group != null)
            {
                group.alpha = Mathf.SmoothStep(0f, 1f, t);
            }

            yield return null;
        }

        rect.localScale = baseScale;
        if (group != null)
        {
            group.alpha = 1f;
        }
    }

    private static IEnumerator FadeRoutine(CanvasGroup group, float from, float to, float duration, bool active)
    {
        if (active)
        {
            group.gameObject.SetActive(true);
            group.blocksRaycasts = true;
            group.interactable = true;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            group.alpha = Mathf.SmoothStep(from, to, t);
            yield return null;
        }

        group.alpha = to;
        if (!active)
        {
            group.blocksRaycasts = false;
            group.interactable = false;
            group.gameObject.SetActive(false);
        }
    }

    private static IEnumerator PunchScaleRoutine(RectTransform rect, float amount, float duration)
    {
        Vector3 baseScale = rect.localScale == Vector3.zero ? Vector3.one : rect.localScale;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float wave = Mathf.Sin(t * Mathf.PI);
            rect.localScale = baseScale * (1f + wave * amount);
            yield return null;
        }

        rect.localScale = baseScale;
    }

    private static IEnumerator SlideFadeRoutine(CanvasGroup group, RectTransform rect, Vector2 from, Vector2 to, float duration)
    {
        group.gameObject.SetActive(true);
        group.blocksRaycasts = true;
        group.interactable = true;
        rect.anchoredPosition = from;
        group.alpha = 0f;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            rect.anchoredPosition = Vector2.LerpUnclamped(from, to, EaseOutCubic(t));
            group.alpha = Mathf.SmoothStep(0f, 1f, t);
            yield return null;
        }

        rect.anchoredPosition = to;
        group.alpha = 1f;
    }

    private static float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }

    private static float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    private class TweenHost : MonoBehaviour
    {
    }
}
