using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenuManager : MonoBehaviour
{
    private const string SaveAvailableKey = "SaveAvailable";
    private static StartMenuManager instance;

    private Canvas canvas;
    private GameObject menuRoot;
    private GameObject mainPanel;
    private GameObject settingsPanel;
    private GameObject progressionPanel;
    private GameObject quitConfirmPanel;
    private GameObject audioSettingsPage;
    private GameObject videoSettingsPage;
    private GameObject controlsSettingsPage;
    private Text statusText;
    private Text noSaveHintText;
    private Text progressionSummaryText;
    private Text progressionStatusText;
    private Slider masterVolumeSlider;
    private Sprite backdropSprite;
    private bool gameStarted;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (FindObjectOfType<StartMenuManager>() != null)
        {
            return;
        }

        GameObject obj = new GameObject("StartMenuManager");
        obj.AddComponent<StartMenuManager>();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "SampleScene")
        {
            enabled = false;
            return;
        }

        EnsureCanvas();
        EnsureEventSystem();
        BuildMenu();
        ShowMainMenu();
    }

    private void Update()
    {
        if (!gameStarted && Time.timeScale != 0f)
        {
            Time.timeScale = 0f;
        }
    }

    private void EnsureCanvas()
    {
        GameObject canvasObject = GameObject.Find("StartMenuCanvas");
        if (canvasObject == null)
        {
            canvasObject = new GameObject("StartMenuCanvas");
        }

        canvas = canvasObject.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = canvasObject.AddComponent<Canvas>();
        }

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 30000;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = canvasObject.AddComponent<CanvasScaler>();
        }

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        if (canvasObject.GetComponent<GraphicRaycaster>() == null)
        {
            canvasObject.AddComponent<GraphicRaycaster>();
        }
    }

    private void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<StandaloneInputModule>();
    }

private void BuildMenu()
    {
        if (menuRoot != null || canvas == null)
        {
            return;
        }

        menuRoot = new GameObject("StartMenuRoot");
        menuRoot.transform.SetParent(canvas.transform, false);
        RectTransform rootRect = menuRoot.AddComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        Image background = menuRoot.AddComponent<Image>();
        background.sprite = CreateBackdropSprite();
        background.color = Color.white;
        background.raycastTarget = true;

        CanvasGroup group = menuRoot.AddComponent<CanvasGroup>();
        group.alpha = 1f;
        group.interactable = true;
        group.blocksRaycasts = true;

        CreateTitleBlock();
        mainPanel = CreatePanel("MainMenuPanel", new Vector2(480f, 430f), new Vector2(0f, -92f));
        settingsPanel = CreatePanel("SettingsPanel", new Vector2(640f, 430f), new Vector2(0f, -72f));
        progressionPanel = CreatePanel("ProgressionPanel", new Vector2(680f, 500f), new Vector2(0f, -70f));
        quitConfirmPanel = CreatePanel("QuitConfirmPanel", new Vector2(520f, 260f), new Vector2(0f, -64f));

        BuildMainPanel();
        BuildSettingsPanel();
        BuildProgressionPanel();
        BuildQuitConfirmPanel();
    }

    private void CreateTitleBlock()
    {
        TMP_Text title = CreateText(menuRoot.transform, "DUNGEON ADVENTURER", 46f, new Vector2(0f, 242f), new Vector2(760f, 70f), UITheme.TitleColor, true);
        title.alignment = TextAlignmentOptions.Center;

        TMP_Text subtitle = CreateText(menuRoot.transform, "Roguelike Survival", 22f, new Vector2(0f, 190f), new Vector2(520f, 36f), UITheme.HintColor, false);
        subtitle.alignment = TextAlignmentOptions.Center;
    }

    private Sprite CreateBackdropSprite()
    {
        if (backdropSprite != null)
        {
            return backdropSprite;
        }

        const int size = 128;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        Color top = new Color(0.035f, 0.04f, 0.055f, 1f);
        Color middle = new Color(0.075f, 0.06f, 0.05f, 1f);
        Color bottom = new Color(0.015f, 0.014f, 0.018f, 1f);
        Vector2 center = new Vector2(0.5f, 0.54f);

        for (int y = 0; y < size; y++)
        {
            float v = y / (float)(size - 1);
            Color vertical = v < 0.48f
                ? Color.Lerp(bottom, middle, v / 0.48f)
                : Color.Lerp(middle, top, (v - 0.48f) / 0.52f);

            for (int x = 0; x < size; x++)
            {
                float u = x / (float)(size - 1);
                float distance = Vector2.Distance(new Vector2(u, v), center);
                float vignette = Mathf.Clamp01((distance - 0.18f) / 0.72f);
                float grain = Mathf.PerlinNoise(u * 16f, v * 16f) * 0.025f;
                Color color = Color.Lerp(vertical + new Color(grain, grain, grain, 0f), UITheme.BackgroundColor, vignette * 0.82f);
                color.a = 1f;
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply(false, true);
        backdropSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
        backdropSprite.name = "StartMenuBackdrop";
        return backdropSprite;
    }

    private GameObject CreatePanel(string name, Vector2 size, Vector2 position)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(menuRoot.transform, false);
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        Image image = panel.AddComponent<Image>();
        image.color = UITheme.PanelTint;
        image.raycastTarget = true;
        UITheme.ApplyPanel(image, false);

        return panel;
    }

private void BuildMainPanel()
    {
        bool hasSaveData = HasSaveData();
        CreateButton(mainPanel.transform, "新游戏", new Vector2(0f, 144f), StartNewGame, true, UIButtonStyle.Primary);
        CreateButton(mainPanel.transform, "读入存档", new Vector2(0f, 72f), LoadGame, hasSaveData, UIButtonStyle.Secondary);
        CreateButton(mainPanel.transform, "成长", new Vector2(0f, 0f), ShowProgression, true, UIButtonStyle.Outline);
        CreateButton(mainPanel.transform, "设置", new Vector2(0f, -72f), ShowSettings, true, UIButtonStyle.Outline);
        CreateButton(mainPanel.transform, "退出游戏", new Vector2(0f, -144f), QuitGame, true, UIButtonStyle.Danger);

        noSaveHintText = CreateLegacyText(mainPanel.transform, "暂无可用存档", 15, new Vector2(0f, 36f), new Vector2(390f, 24f), UITheme.HintColor, TextAnchor.MiddleCenter);
        noSaveHintText.gameObject.SetActive(!hasSaveData);
        statusText = CreateLegacyText(mainPanel.transform, string.Empty, 16, new Vector2(0f, -198f), new Vector2(390f, 28f), UITheme.HintColor, TextAnchor.MiddleCenter);
    }

    private void BuildSettingsPanel()
    {
        CreateLegacyText(settingsPanel.transform, "设置", 30, new Vector2(0f, 164f), new Vector2(420f, 44f), UITheme.TitleColor, TextAnchor.MiddleCenter);

        CreateButton(settingsPanel.transform, "音频", new Vector2(-170f, 108f), ShowAudioSettings, true, UIButtonStyle.Primary, new Vector2(128f, 44f));
        CreateButton(settingsPanel.transform, "画面", new Vector2(0f, 108f), ShowVideoSettings, true, UIButtonStyle.Outline, new Vector2(128f, 44f));
        CreateButton(settingsPanel.transform, "操作", new Vector2(170f, 108f), ShowControlsSettings, true, UIButtonStyle.Outline, new Vector2(128f, 44f));

        audioSettingsPage = CreateContentPage(settingsPanel.transform, "AudioSettingsPage");
        videoSettingsPage = CreateContentPage(settingsPanel.transform, "VideoSettingsPage");
        controlsSettingsPage = CreateContentPage(settingsPanel.transform, "ControlsSettingsPage");

        BuildAudioSettingsPage();
        BuildVideoSettingsPage();
        BuildControlsSettingsPage();
        ShowAudioSettings();

        CreateButton(settingsPanel.transform, "返回", new Vector2(0f, -174f), ShowMainMenu, true, UIButtonStyle.Ghost);
    }

    private void BuildAudioSettingsPage()
    {
        Transform volumeRow = CreateFieldRow(audioSettingsPage.transform, "主音量Field", new Vector2(0f, 28f), new Vector2(520f, 78f));
        CreateLegacyText(volumeRow, "主音量", 20, new Vector2(-154f, 14f), new Vector2(128f, 28f), UITheme.BodyColor, TextAnchor.MiddleLeft);
        CreateLegacyText(volumeRow, "调整游戏整体音量", 15, new Vector2(-126f, -15f), new Vector2(184f, 24f), UITheme.HintColor, TextAnchor.MiddleLeft);
        masterVolumeSlider = CreateSlider(volumeRow, new Vector2(112f, 0f));
        masterVolumeSlider.value = AudioListener.volume;
        masterVolumeSlider.onValueChanged.AddListener(value => AudioListener.volume = value);

        CreateLegacyText(audioSettingsPage.transform, "后续将接入音乐 / 音效独立音量", 16, new Vector2(0f, -52f), new Vector2(460f, 28f), UITheme.HintColor, TextAnchor.MiddleCenter);
    }

    private void BuildVideoSettingsPage()
    {
        Transform displayRow = CreateFieldRow(videoSettingsPage.transform, "画面设置Field", new Vector2(0f, 28f), new Vector2(520f, 78f));
        CreateLegacyText(displayRow, "画面", 20, new Vector2(-154f, 14f), new Vector2(128f, 28f), UITheme.BodyColor, TextAnchor.MiddleLeft);
        CreateLegacyText(displayRow, "分辨率 / 全屏 / 特效质量将在后续接入", 15, new Vector2(24f, -2f), new Vector2(360f, 28f), UITheme.HintColor, TextAnchor.MiddleLeft);
    }

    private void BuildControlsSettingsPage()
    {
        Transform inputRow = CreateFieldRow(controlsSettingsPage.transform, "操作设置Field", new Vector2(0f, 28f), new Vector2(520f, 78f));
        CreateLegacyText(inputRow, "操作", 20, new Vector2(-154f, 14f), new Vector2(128f, 28f), UITheme.BodyColor, TextAnchor.MiddleLeft);
        CreateLegacyText(inputRow, "键位显示 / 重绑定将在后续接入", 15, new Vector2(24f, -2f), new Vector2(360f, 28f), UITheme.HintColor, TextAnchor.MiddleLeft);
    }

    private void BuildQuitConfirmPanel()
    {
        CreateLegacyText(quitConfirmPanel.transform, "确认退出游戏？", 28, new Vector2(0f, 72f), new Vector2(420f, 44f), UITheme.TitleColor, TextAnchor.MiddleCenter);
        CreateLegacyText(quitConfirmPanel.transform, "未保存的进度可能会丢失。", 17, new Vector2(0f, 24f), new Vector2(420f, 30f), UITheme.HintColor, TextAnchor.MiddleCenter);
        CreateButton(quitConfirmPanel.transform, "取消", new Vector2(-112f, -72f), ShowMainMenu, true, UIButtonStyle.Ghost, new Vector2(170f, 50f));
        CreateButton(quitConfirmPanel.transform, "退出", new Vector2(112f, -72f), ConfirmQuitGame, true, UIButtonStyle.Danger, new Vector2(170f, 50f));
        quitConfirmPanel.SetActive(false);
    }

    private GameObject CreateContentPage(Transform parent, string name)
    {
        GameObject page = new GameObject(name);
        page.transform.SetParent(parent, false);
        RectTransform rect = page.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(560f, 180f);
        rect.anchoredPosition = new Vector2(0f, -20f);
        return page;
    }

    private Transform CreateFieldRow(Transform parent, string name, Vector2 position, Vector2 size)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        Image image = obj.AddComponent<Image>();
        image.sprite = UITheme.PanelSprite;
        image.type = Image.Type.Sliced;
        image.color = UITheme.MutedColor;
        image.raycastTarget = false;

        Outline outline = obj.AddComponent<Outline>();
        outline.effectColor = UITheme.BorderColor;
        outline.effectDistance = new Vector2(1f, -1f);

        return obj.transform;
    }

    private Button CreateButton(Transform parent, string label, Vector2 position, UnityEngine.Events.UnityAction action, bool interactable, UIButtonStyle style, Vector2? size = null)
    {
        GameObject obj = new GameObject(label + "Button");
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size ?? new Vector2(330f, 54f);
        rect.anchoredPosition = position;

        Image image = obj.AddComponent<Image>();
        Button button = obj.AddComponent<Button>();
        button.interactable = interactable;
        if (action != null)
        {
            button.onClick.AddListener(action);
        }

        Text buttonText = CreateLegacyText(obj.transform, label, size.HasValue ? 20 : 23, Vector2.zero, rect.sizeDelta - new Vector2(30f, 12f), UITheme.BodyColor, TextAnchor.MiddleCenter);
        UITheme.ApplyButton(image, buttonText, style, interactable);

        return button;
    }

    private Slider CreateSlider(Transform parent, Vector2 position)
    {
        GameObject sliderObject = new GameObject("MasterVolumeSlider");
        sliderObject.transform.SetParent(parent, false);
        RectTransform rect = sliderObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(300f, 28f);
        rect.anchoredPosition = position;

        Slider slider = sliderObject.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;

        GameObject background = CreateSliderImage("Background", sliderObject.transform, Vector2.zero, Vector2.one, new Color(0.11f, 0.11f, 0.12f, 1f));
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObject.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = new Vector2(8f, 6f);
        fillAreaRect.offsetMax = new Vector2(-8f, -6f);

        GameObject fill = CreateSliderImage("Fill", fillArea.transform, Vector2.zero, Vector2.one, UITheme.GoldColor);
        GameObject handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(sliderObject.transform, false);
        RectTransform handleAreaRect = handleArea.AddComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.offsetMin = new Vector2(10f, 0f);
        handleAreaRect.offsetMax = new Vector2(-10f, 0f);

        GameObject handle = CreateSliderImage("Handle", handleArea.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), UITheme.TitleColor);
        handle.GetComponent<RectTransform>().sizeDelta = new Vector2(24f, 30f);

        slider.targetGraphic = handle.GetComponent<Image>();
        slider.fillRect = fill.GetComponent<RectTransform>();
        slider.handleRect = handle.GetComponent<RectTransform>();
        slider.direction = Slider.Direction.LeftToRight;
        background.GetComponent<Image>().raycastTarget = true;

        return slider;
    }

    private GameObject CreateSliderImage(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = obj.AddComponent<Image>();
        image.color = color;
        image.sprite = UITheme.BarBackSprite;
        image.type = Image.Type.Sliced;

        return obj;
    }

    private TMP_Text CreateText(Transform parent, string content, float fontSize, Vector2 position, Vector2 size, Color color, bool bold)
    {
        GameObject obj = new GameObject("TMPText");
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        TextMeshProUGUI text = obj.AddComponent<TextMeshProUGUI>();
        text.text = content;
        UITheme.ApplyTMPText(text, color, fontSize, bold);
        text.enableWordWrapping = false;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        text.outlineWidth = bold ? 0.08f : 0.04f;
        text.outlineColor = new Color(0f, 0f, 0f, 0.8f);
        return text;
    }

    private Text CreateLegacyText(Transform parent, string content, int fontSize, Vector2 position, Vector2 size, Color color, TextAnchor alignment)
    {
        GameObject obj = new GameObject("Text");
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Text text = obj.AddComponent<Text>();
        text.text = content;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = alignment;
        text.raycastTarget = false;
        UITheme.ApplyText(text);
        return text;
    }

    private bool HasSaveData()
    {
        return PlayerPrefs.GetInt(SaveAvailableKey, 0) == 1;
    }

    private void StartNewGame()
    {
        gameStarted = true;
        Time.timeScale = 1f;
        if (menuRoot != null)
        {
            menuRoot.SetActive(false);
        }
    }

    private void LoadGame()
    {
        if (!HasSaveData())
        {
            SetStatus("暂无可用存档");
            return;
        }

        SetStatus("读档系统将在存档模块完成后接入");
    }

    private void ShowAudioSettings()
    {
        ShowSettingsPage(audioSettingsPage);
    }

    private void ShowVideoSettings()
    {
        ShowSettingsPage(videoSettingsPage);
    }

    private void ShowControlsSettings()
    {
        ShowSettingsPage(controlsSettingsPage);
    }

    private void ShowSettingsPage(GameObject activePage)
    {
        if (audioSettingsPage != null)
        {
            audioSettingsPage.SetActive(activePage == audioSettingsPage);
        }

        if (videoSettingsPage != null)
        {
            videoSettingsPage.SetActive(activePage == videoSettingsPage);
        }

        if (controlsSettingsPage != null)
        {
            controlsSettingsPage.SetActive(activePage == controlsSettingsPage);
        }
    }

private void ShowSettings()
    {
        mainPanel.SetActive(false);
        settingsPanel.SetActive(true);
        if (progressionPanel != null)
        {
            progressionPanel.SetActive(false);
        }

        HideQuitConfirm();
        ShowAudioSettings();
        SetStatus(string.Empty);
    }

private void ShowMainMenu()
    {
        if (menuRoot != null)
        {
            menuRoot.SetActive(true);
        }

        if (mainPanel != null)
        {
            mainPanel.SetActive(true);
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        if (progressionPanel != null)
        {
            progressionPanel.SetActive(false);
        }

        HideQuitConfirm();
        gameStarted = false;
        Time.timeScale = 0f;
    }

private void QuitGame()
    {
        if (quitConfirmPanel != null)
        {
            quitConfirmPanel.SetActive(true);
        }

        if (mainPanel != null)
        {
            mainPanel.SetActive(false);
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        if (progressionPanel != null)
        {
            progressionPanel.SetActive(false);
        }

        SetStatus(string.Empty);
    }

    private void HideQuitConfirm()
    {
        if (quitConfirmPanel != null)
        {
            quitConfirmPanel.SetActive(false);
        }
    }

    private void ConfirmQuitGame()
    {
        gameStarted = true;
        Time.timeScale = 1f;
        Application.Quit();
        SetStatus("已请求退出游戏");
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }


private void BuildProgressionPanel()
    {
        CreateLegacyText(progressionPanel.transform, "局外成长", 30, new Vector2(0f, 200f), new Vector2(420f, 44f), UITheme.TitleColor, TextAnchor.MiddleCenter);
        progressionSummaryText = CreateLegacyText(progressionPanel.transform, string.Empty, 18, new Vector2(0f, 154f), new Vector2(560f, 34f), UITheme.BodyColor, TextAnchor.MiddleCenter);

        CreateProgressionRow("生命强化", "每级开局最大生命 +10", new Vector2(0f, 82f), MetaProgression.MaxHpLevel, () => TryBuyProgression(MetaProgression.TryUpgradeMaxHp, "生命强化"));
        CreateProgressionRow("攻击训练", "每级开局攻击 +1.5", new Vector2(0f, 4f), MetaProgression.AttackLevel, () => TryBuyProgression(MetaProgression.TryUpgradeAttack, "攻击训练"));
        CreateProgressionRow("护甲训练", "每级开局防御 +0.5", new Vector2(0f, -74f), MetaProgression.DefenseLevel, () => TryBuyProgression(MetaProgression.TryUpgradeDefense, "护甲训练"));

        progressionStatusText = CreateLegacyText(progressionPanel.transform, string.Empty, 16, new Vector2(0f, -154f), new Vector2(560f, 30f), UITheme.HintColor, TextAnchor.MiddleCenter);
        CreateButton(progressionPanel.transform, "返回", new Vector2(0f, -206f), ShowMainMenu, true, UIButtonStyle.Ghost, new Vector2(220f, 48f));
        RefreshProgressionPanel();
        progressionPanel.SetActive(false);
    }

    private void CreateProgressionRow(string title, string description, Vector2 position, int currentLevel, UnityEngine.Events.UnityAction buyAction)
    {
        Transform row = CreateFieldRow(progressionPanel.transform, title + "Row", position, new Vector2(560f, 66f));
        CreateLegacyText(row, title, 20, new Vector2(-194f, 12f), new Vector2(150f, 26f), UITheme.BodyColor, TextAnchor.MiddleLeft);
        CreateLegacyText(row, description, 14, new Vector2(-128f, -14f), new Vector2(280f, 22f), UITheme.HintColor, TextAnchor.MiddleLeft);

        string buttonLabel = GetProgressionButtonLabel(currentLevel);
        CreateButton(row, buttonLabel, new Vector2(190f, 0f), buyAction, currentLevel < MetaProgression.MaxUpgradeLevel, currentLevel >= MetaProgression.MaxUpgradeLevel ? UIButtonStyle.Ghost : UIButtonStyle.Secondary, new Vector2(144f, 42f));
    }

    private string GetProgressionButtonLabel(int currentLevel)
    {
        if (currentLevel >= MetaProgression.MaxUpgradeLevel)
        {
            return "已满级";
        }

        return "Lv." + currentLevel + " 花费 " + MetaProgression.GetCost(currentLevel);
    }

    private void TryBuyProgression(System.Func<bool> upgradeAction, string upgradeName)
    {
        bool success = upgradeAction != null && upgradeAction.Invoke();
        string message = success ? upgradeName + " 已提升" : "魂石不足或已满级";
        RebuildProgressionPanel();
        if (progressionStatusText != null)
        {
            progressionStatusText.text = message;
        }
    }

    private void RebuildProgressionPanel()
    {
        if (progressionPanel == null)
        {
            return;
        }

        for (int i = progressionPanel.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(progressionPanel.transform.GetChild(i).gameObject);
        }

        progressionSummaryText = null;
        progressionStatusText = null;
        BuildProgressionPanel();
        progressionPanel.SetActive(true);
    }

    private void RefreshProgressionPanel()
    {
        if (progressionSummaryText == null)
        {
            return;
        }

        progressionSummaryText.text = "魂石: " + MetaProgression.SoulStones + "    生命 Lv." + MetaProgression.MaxHpLevel + " / 攻击 Lv." + MetaProgression.AttackLevel + " / 防御 Lv." + MetaProgression.DefenseLevel;
    }

    private void ShowProgression()
    {
        if (mainPanel != null)
        {
            mainPanel.SetActive(false);
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        if (progressionPanel != null)
        {
            RefreshProgressionPanel();
            progressionPanel.SetActive(true);
        }

        HideQuitConfirm();
        SetStatus(string.Empty);
    }
}
