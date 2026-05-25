using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InteractableHint : MonoBehaviour
{
    public string hintText = "Press E";
    public string promptSpriteResourcePath = "Prompts/Keyboard_E";
    public Vector3 worldOffset = new Vector3(0f, 1.8f, 0f);
    public CanvasGroup hintRoot;
    public Text label;
    public TMP_Text tmpLabel;
    public Image promptIcon;
    public float fadeDuration = 0.16f;
    public float floatAmplitude = 0.12f;
    public float floatSpeed = 3.2f;
    public bool useDistanceFallback = true;
    public float showDistance = 2.6f;

    private Coroutine fadeRoutine;
    private RectTransform hintRect;
    private Vector3 baseLocalPosition;
    private bool playerInside;
    private Transform player;
    private float nextPlayerSearchTime;

    private void Awake()
    {
        EnsureHintUI();
        SetVisible(false, true);
    }

    private void Update()
    {
        UpdateDistanceFallback();

        if (hintRoot == null || !playerInside)
        {
            return;
        }

        if (Camera.main != null)
        {
            hintRoot.transform.rotation = Camera.main.transform.rotation;
        }

        if (hintRect != null)
        {
            float y = Mathf.Sin(Time.unscaledTime * floatSpeed) * floatAmplitude;
            hintRect.localPosition = baseLocalPosition + Vector3.up * y;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Show();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Hide();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Show();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Hide();
        }
    }

    public void Show()
    {
        playerInside = true;
        SetVisible(true, false);
    }

    public void Hide()
    {
        playerInside = false;
        SetVisible(false, false);
    }

    public void SetContent(string text, string spriteResourcePath)
    {
        hintText = text;
        promptSpriteResourcePath = spriteResourcePath;
        ApplyContent();
    }

    public void SetFocused(bool focused)
    {
        if (focused)
        {
            Show();
            return;
        }

        Hide();
    }

    private void EnsureHintUI()
    {
        if (hintRoot == null)
        {
            GameObject canvasObject = new GameObject("InteractableHintCanvas");
            canvasObject.transform.SetParent(transform, false);
            canvasObject.transform.localPosition = worldOffset;

            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;

            RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(180f, 48f);
            canvasRect.localScale = Vector3.one * 0.012f;

            hintRoot = canvasObject.AddComponent<CanvasGroup>();

            GameObject panel = new GameObject("HintPanel");
            panel.transform.SetParent(canvasObject.transform, false);
            Image panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.68f);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            GameObject iconObject = new GameObject("PromptIcon");
            iconObject.transform.SetParent(panel.transform, false);
            promptIcon = iconObject.AddComponent<Image>();
            RectTransform iconRect = promptIcon.rectTransform;
            iconRect.anchorMin = new Vector2(0f, 0.5f);
            iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0f, 0.5f);
            iconRect.anchoredPosition = new Vector2(12f, 0f);
            iconRect.sizeDelta = new Vector2(28f, 28f);

            GameObject textObject = new GameObject("HintText");
            textObject.transform.SetParent(panel.transform, false);
            tmpLabel = textObject.AddComponent<TextMeshProUGUI>();
            tmpLabel.alignment = TextAlignmentOptions.MidlineLeft;
            UITheme.ApplyTMPText(tmpLabel, Color.white, 21f, true);
            tmpLabel.outlineWidth = 0.14f;
            tmpLabel.outlineColor = new Color(0f, 0f, 0f, 0.9f);
            label = null;
            RectTransform textRect = tmpLabel.rectTransform;
            textRect.anchorMin = new Vector2(0f, 0f);
            textRect.anchorMax = new Vector2(1f, 1f);
            textRect.offsetMin = new Vector2(48f, 0f);
            textRect.offsetMax = new Vector2(-10f, 0f);
        }

        hintRect = hintRoot.GetComponent<RectTransform>();
        if (hintRect != null)
        {
            baseLocalPosition = hintRect.localPosition;
        }

        ApplyContent();
    }

    private void ApplyContent()
    {
        if (tmpLabel != null)
        {
            tmpLabel.text = hintText;
        }
        else if (label != null)
        {
            label.text = hintText;
        }

        if (promptIcon != null)
        {
            Sprite sprite = !string.IsNullOrEmpty(promptSpriteResourcePath) ? Resources.Load<Sprite>(promptSpriteResourcePath) : null;
            promptIcon.sprite = sprite;
            promptIcon.enabled = sprite != null;
        }
    }

    private void UpdateDistanceFallback()
    {
        if (!useDistanceFallback)
        {
            return;
        }

        if (player == null && Time.unscaledTime >= nextPlayerSearchTime)
        {
            nextPlayerSearchTime = Time.unscaledTime + 0.25f;
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            player = playerObject != null ? playerObject.transform : null;
        }

        if (player == null)
        {
            return;
        }

        bool closeEnough = Vector3.Distance(transform.position, player.position) <= showDistance;
        if (closeEnough && !playerInside)
        {
            Show();
        }
        else if (!closeEnough && playerInside)
        {
            Hide();
        }
    }

    private void SetVisible(bool visible, bool instant)
    {
        if (hintRoot == null)
        {
            return;
        }

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }

        if (instant)
        {
            hintRoot.alpha = visible ? 1f : 0f;
            hintRoot.interactable = visible;
            hintRoot.blocksRaycasts = visible;
            return;
        }

        fadeRoutine = StartCoroutine(FadeRoutine(visible));
    }

    private IEnumerator FadeRoutine(bool visible)
    {
        float start = hintRoot.alpha;
        float end = visible ? 1f : 0f;
        float elapsed = 0f;

        hintRoot.interactable = visible;
        hintRoot.blocksRaycasts = visible;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            hintRoot.alpha = Mathf.Lerp(start, end, elapsed / fadeDuration);
            yield return null;
        }

        hintRoot.alpha = end;
        fadeRoutine = null;
    }
}
