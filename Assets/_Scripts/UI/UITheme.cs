using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum UIButtonStyle
{
    Primary,
    Secondary,
    Outline,
    Ghost,
    Danger,
    Icon
}

public static class UITheme
{
    public static readonly Color BackgroundColor = new Color(0.02f, 0.018f, 0.015f, 1f);
    public static readonly Color ForegroundColor = new Color(0.96f, 0.95f, 0.9f, 1f);
    public static readonly Color PanelTint = new Color(0.08f, 0.085f, 0.105f, 0.94f);
    public static readonly Color OptionTint = new Color(0.12f, 0.13f, 0.16f, 0.97f);
    public static readonly Color TitleColor = new Color(1f, 0.92f, 0.76f, 1f);
    public static readonly Color BodyColor = new Color(0.94f, 0.94f, 0.9f, 1f);
    public static readonly Color HintColor = new Color(0.74f, 0.82f, 0.9f, 1f);
    public static readonly Color GoldColor = new Color(1f, 0.86f, 0.22f, 1f);
    public static readonly Color PrimaryColor = new Color(0.88f, 0.63f, 0.18f, 1f);
    public static readonly Color PrimaryTextColor = new Color(0.08f, 0.055f, 0.025f, 1f);
    public static readonly Color SecondaryColor = new Color(0.16f, 0.18f, 0.23f, 0.98f);
    public static readonly Color MutedColor = new Color(0.14f, 0.15f, 0.18f, 0.88f);
    public static readonly Color DangerColor = new Color(0.72f, 0.19f, 0.16f, 1f);
    public static readonly Color BorderColor = new Color(0.33f, 0.34f, 0.38f, 0.92f);
    public static readonly Color FocusColor = new Color(1f, 0.79f, 0.3f, 1f);
    public static readonly Color DisabledColor = new Color(0.22f, 0.22f, 0.24f, 0.78f);

    private static Font cachedFont;
    private static TMP_FontAsset cachedTMPFont;
    private static TMP_FontAsset cachedHudTMPFont;
    private static TMP_FontAsset cachedChineseTMPFont;
    private static Sprite panelSprite;
    private static Sprite buttonSprite;
    private static Sprite barFillSprite;
    private static Sprite barBackSprite;

    public static Font GameFont
    {
        get
        {
            if (cachedFont == null)
            {
                cachedFont = Resources.Load<Font>("Fonts/FusionPixel/FusionPixelZhHans");
                if (cachedFont == null)
                {
                    cachedFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
                }
            }

            return cachedFont;
        }
    }

    public static TMP_FontAsset GameTMPFont
    {
        get
        {
            if (cachedTMPFont == null)
            {
                cachedTMPFont = Resources.Load<TMP_FontAsset>("Fonts/MedievalSharp/MedievalSharp SDF");
                if (cachedTMPFont == null)
                {
                    cachedTMPFont = TMP_Settings.defaultFontAsset;
                }
            }

            return cachedTMPFont;
        }
    }

    public static TMP_FontAsset HudTMPFont
    {
        get
        {
            if (cachedHudTMPFont == null)
            {
                cachedHudTMPFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
                if (cachedHudTMPFont == null)
                {
                    cachedHudTMPFont = TMP_Settings.defaultFontAsset;
                }
            }

            return cachedHudTMPFont;
        }
    }

    public static TMP_FontAsset ChineseTMPFont
    {
        get
        {
            if (cachedChineseTMPFont == null)
            {
                cachedChineseTMPFont = Resources.Load<TMP_FontAsset>("Fonts/FusionPixel/FusionPixelZhHans SDF");
                if (cachedChineseTMPFont == null && GameFont != null)
                {
                    cachedChineseTMPFont = TMP_FontAsset.CreateFontAsset(GameFont);
                    if (cachedChineseTMPFont != null)
                    {
                        cachedChineseTMPFont.name = "FusionPixelZhHans Runtime SDF";
                        cachedChineseTMPFont.atlasPopulationMode = AtlasPopulationMode.Dynamic;
                    }
                }

                if (cachedChineseTMPFont == null)
                {
                    cachedChineseTMPFont = TMP_Settings.defaultFontAsset;
                }
            }

            return cachedChineseTMPFont;
        }
    }

    public static Sprite PanelSprite
    {
        get
        {
            if (panelSprite == null)
            {
                panelSprite = Resources.Load<Sprite>("UI/Skin/KenneyUI/Grey/Default/button_rectangle_depth_flat");
            }

            return panelSprite;
        }
    }

    public static Sprite ButtonSprite
    {
        get
        {
            if (buttonSprite == null)
            {
                buttonSprite = Resources.Load<Sprite>("UI/Skin/KenneyUI/Blue/Default/button_rectangle_depth_flat");
            }

            return buttonSprite;
        }
    }

    public static Sprite BarFillSprite
    {
        get
        {
            if (barFillSprite == null)
            {
                barFillSprite = Resources.Load<Sprite>("UI/Skin/KenneyUI/Blue/Default/slide_horizontal_color_section_wide");
            }

            return barFillSprite;
        }
    }

    public static Sprite BarBackSprite
    {
        get
        {
            if (barBackSprite == null)
            {
                barBackSprite = Resources.Load<Sprite>("UI/Skin/KenneyUI/Grey/Default/slide_horizontal_grey_section_wide");
            }

            return barBackSprite;
        }
    }

    public static void ApplyToCanvas(Canvas canvas)
    {
        if (canvas == null)
        {
            return;
        }

        Text[] texts = canvas.GetComponentsInChildren<Text>(true);
        foreach (Text text in texts)
        {
            ApplyText(text);
        }

        foreach (Image image in canvas.GetComponentsInChildren<Image>(true))
        {
            if (image == null || image.sprite != null)
            {
                continue;
            }

            string lowerName = image.gameObject.name.ToLowerInvariant();
            if (lowerName.Contains("panel") || lowerName.Contains("option") || lowerName.Contains("button"))
            {
                ApplyPanel(image, lowerName.Contains("option") || lowerName.Contains("button"));
            }
        }
    }

    public static void ApplyText(Text text)
    {
        if (text == null)
        {
            return;
        }

        text.font = GameFont;
        text.supportRichText = true;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = Mathf.Max(10, Mathf.RoundToInt(text.fontSize * 0.65f));
        text.resizeTextMaxSize = text.fontSize;
        if (text.color == Color.black)
        {
            text.color = new Color(0.96f, 0.95f, 0.9f, 1f);
        }
    }

    public static void ApplyTMPText(TMP_Text text, Color color, float fontSize, bool bold = false)
    {
        if (text == null)
        {
            return;
        }

        text.font = GameTMPFont;
        text.fontSize = fontSize;
        text.color = color;
        text.enableWordWrapping = false;
        text.richText = true;
        text.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        text.raycastTarget = false;
    }

    public static void ApplyHudTMPText(TMP_Text text, Color color, float fontSize, bool bold = false)
    {
        if (text == null)
        {
            return;
        }

        text.font = HudTMPFont;
        text.fontSize = fontSize;
        text.color = color;
        text.enableWordWrapping = false;
        text.richText = true;
        text.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        text.characterSpacing = 0f;
        text.wordSpacing = 0f;
        text.raycastTarget = false;
    }

    public static void ApplyOptionText(TMP_Text text)
    {
        if (text == null)
        {
            return;
        }

        ApplyChineseTMPText(text, BodyColor, 20f, false);
        text.enableWordWrapping = true;
        text.lineSpacing = 8f;
        text.outlineWidth = 0.04f;
        text.outlineColor = new Color(0f, 0f, 0f, 0.78f);
    }

    public static void ApplyChineseTMPText(TMP_Text text, Color color, float fontSize, bool bold = false)
    {
        if (text == null)
        {
            return;
        }

        text.font = ChineseTMPFont;
        text.fontSize = fontSize;
        text.color = color;
        text.enableWordWrapping = false;
        text.richText = true;
        text.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        text.characterSpacing = 0f;
        text.wordSpacing = 0f;
        text.raycastTarget = false;
    }

    public static void ApplyGoldEmphasis(TMP_Text text)
    {
        if (text == null)
        {
            return;
        }

        ApplyTMPText(text, GoldColor, 24f, true);
        text.outlineWidth = 0.18f;
        text.outlineColor = new Color(0.12f, 0.07f, 0f, 1f);

        Shadow shadow = text.GetComponent<Shadow>();
        if (shadow == null)
        {
            shadow = text.gameObject.AddComponent<Shadow>();
        }

        shadow.effectColor = new Color(0f, 0f, 0f, 0.7f);
        shadow.effectDistance = new Vector2(2f, -2f);
    }

    public static void ApplyHudGoldEmphasis(TMP_Text text)
    {
        if (text == null)
        {
            return;
        }

        ApplyHudTMPText(text, GoldColor, text.fontSize, true);
        text.outlineWidth = 0.035f;
        text.outlineColor = new Color(0.08f, 0.045f, 0f, 0.9f);

        Shadow shadow = text.GetComponent<Shadow>();
        if (shadow == null)
        {
            shadow = text.gameObject.AddComponent<Shadow>();
        }

        shadow.effectColor = new Color(0f, 0f, 0f, 0.48f);
        shadow.effectDistance = new Vector2(1f, -1f);
    }

    public static void ApplyPanel(Image image, bool buttonLike = false)
    {
        if (image == null)
        {
            return;
        }

        image.sprite = buttonLike ? ButtonSprite : PanelSprite;
        image.type = Image.Type.Sliced;
        Color tint = buttonLike ? OptionTint : PanelTint;
        image.color = tint;
    }

    public static void ApplyButton(Image image, Text text, UIButtonStyle style, bool interactable = true)
    {
        if (image == null)
        {
            return;
        }

        image.sprite = ButtonSprite;
        image.type = Image.Type.Sliced;

        Color background = OptionTint;
        Color foreground = BodyColor;

        switch (style)
        {
            case UIButtonStyle.Primary:
                background = PrimaryColor;
                foreground = PrimaryTextColor;
                break;
            case UIButtonStyle.Secondary:
                background = SecondaryColor;
                foreground = BodyColor;
                break;
            case UIButtonStyle.Outline:
                background = new Color(0.08f, 0.085f, 0.105f, 0.72f);
                foreground = TitleColor;
                break;
            case UIButtonStyle.Ghost:
                background = new Color(0.08f, 0.085f, 0.105f, 0.18f);
                foreground = HintColor;
                break;
            case UIButtonStyle.Danger:
                background = DangerColor;
                foreground = new Color(1f, 0.93f, 0.9f, 1f);
                break;
            case UIButtonStyle.Icon:
                background = new Color(0.08f, 0.085f, 0.105f, 0.52f);
                foreground = BodyColor;
                break;
        }

        if (!interactable)
        {
            background = DisabledColor;
            foreground = new Color(0.58f, 0.58f, 0.6f, 1f);
        }

        image.color = background;

        if (text != null)
        {
            text.color = foreground;
            ApplyText(text);
        }

        Outline outline = image.GetComponent<Outline>();
        if (style == UIButtonStyle.Outline && outline == null)
        {
            outline = image.gameObject.AddComponent<Outline>();
        }

        if (outline != null)
        {
            outline.enabled = style == UIButtonStyle.Outline || style == UIButtonStyle.Ghost;
            outline.effectColor = style == UIButtonStyle.Ghost ? new Color(0.28f, 0.29f, 0.34f, 0.42f) : BorderColor;
            outline.effectDistance = new Vector2(1.25f, -1.25f);
        }
    }

    public static void ApplyBar(Image back, Image fill)
    {
        if (back != null)
        {
            back.sprite = BarBackSprite;
            back.type = Image.Type.Sliced;
            back.color = new Color(0.12f, 0.12f, 0.14f, 0.92f);
        }

        if (fill != null)
        {
            fill.sprite = BarFillSprite;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = 0;
        }
    }
}
