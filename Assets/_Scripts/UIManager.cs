using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    private const string ControlsHelpSeenKey = "ControlsHelpSeen";

    public Canvas mainCanvas;
    public GameObject expBarPanel;
    public Image expBarFill;
    public Text expBarLevelText;
    public Text expBarValueText;
    public GameObject goldPanel;
    public Text goldText;
    public GameObject waveNotificationPanel;
    public Text waveText;
    public GameObject deathPanel;
    public Text deathTitleText;
    public Text deathStatsText;
    public Text deathRestartText;
    private TMP_Text deathTitleTMP;
    private TMP_Text deathStatsTMP;
    private TMP_Text deathRestartTMP;
    public GameObject levelUpPanel;
    public Text levelUpText;

    private float expLerpTarget;
    private float expLerpCurrent;
    private PlayerStats playerStats;
    private PlayerController playerController;
    private GameObject buildPanel;
    private Text buildSummaryText;
    private BufferedHealthBar playerHealthBar;
    private GameObject modalMaskObject;
    private CanvasGroup modalMaskGroup;
    private int pausedModalCount;
    private GameObject modernHudRoot;
    private TMP_Text hudLevelText;
    private TMP_Text hudXpText;
    private TMP_Text hudGoldText;
    private int displayedGold;
    private int targetGold;
    private Coroutine goldRollRoutine;
    private GameObject pausePanel;
    private GameObject pauseMainPage;
    private GameObject pauseSettingsPage;
    private GameObject pauseQuitPage;
    private Slider pauseVolumeSlider;
    private bool pausePanelOpen;
    private GameObject controlsHelpPanel;
    private CanvasGroup controlsHelpGroup;
    private bool controlsHelpOpen;
    private bool controlsHelpAutoPending;
    private float controlsHelpHideTime;
    private bool initialized;
    private bool initializationAttempted;

    private GameObject upgradePanel;
    private Text upgradeHeaderText;
    private Text upgradeHintText;
    private Text[] upgradeOptionTexts;
    private UpgradeChoice[] activeChoices;
    private GameObject relicPanel;
    private Text relicHeaderText;
    private Text relicHintText;
    private Text[] relicOptionTexts;
    private UpgradeChoice[] activeRelicChoices;
    private bool awaitingUpgradeChoice;
    private bool awaitingRelicChoice;
    private int pendingUpgradeChoices;
    private int pendingRelicChoices;
    private float nextBuildSummaryUpdateTime;

    private struct UpgradeChoice
    {
        public string title;
        public string description;
        public System.Action<PlayerStats, PlayerController> apply;
    }

    private void Awake()
    {
        initialized = false;
        initializationAttempted = false;
    }

    private void OnEnable()
    {
        if (Application.isPlaying && mainCanvas != null && modernHudRoot == null)
        {
            initialized = false;
            initializationAttempted = false;
        }
    }

    private void Start()
    {
        InitializeOnce();
    }

    private void Update()
    {
        if (!initialized && !initializationAttempted)
        {
            InitializeOnce();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePausePanel();
        }

        if (Input.GetKeyDown(KeyCode.F1) && !IsStartMenuVisible())
        {
            ToggleControlsHelp();
        }

        UpdateControlsHelp();

        if (expBarFill != null)
        {
            expLerpCurrent = Mathf.Lerp(expLerpCurrent, expLerpTarget, 3f * Time.unscaledDeltaTime);
            expBarFill.fillAmount = expLerpCurrent;
        }

        if (Time.unscaledTime >= nextBuildSummaryUpdateTime)
        {
            nextBuildSummaryUpdateTime = Time.unscaledTime + 0.25f;
            UpdateBuildSummary();
        }

        if (awaitingUpgradeChoice)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            {
                ApplyUpgradeChoice(0);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            {
                ApplyUpgradeChoice(1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
            {
                ApplyUpgradeChoice(2);
            }
        }
        else if (awaitingRelicChoice)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            {
                ApplyRelicChoice(0);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            {
                ApplyRelicChoice(1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
            {
                ApplyRelicChoice(2);
            }
        }
    }

    private void InitializeOnce()
    {
        if (initialized || initializationAttempted)
        {
            return;
        }

        initializationAttempted = true;

        if (mainCanvas == null)
        {
            GameObject uiCanvasObject = GameObject.Find("UICanvas");
            mainCanvas = uiCanvasObject != null ? uiCanvasObject.GetComponent<Canvas>() : FindObjectOfType<Canvas>();
        }
        else if (mainCanvas.name != "UICanvas")
        {
            GameObject uiCanvasObject = GameObject.Find("UICanvas");
            if (uiCanvasObject != null)
            {
                Canvas uiCanvas = uiCanvasObject.GetComponent<Canvas>();
                if (uiCanvas != null)
                {
                    mainCanvas = uiCanvas;
                }
            }
        }

        if (mainCanvas != null)
        {
            ConfigureMainCanvas();
            Transform root = mainCanvas.transform;
            expBarPanel = root.Find("ExpBarPanel")?.gameObject;
            if (expBarPanel != null)
            {
                Image expBarBg = expBarPanel.transform.Find("ExpBarBg")?.GetComponent<Image>();
                expBarFill = expBarPanel.transform.Find("ExpBarBg/ExpFill")?.GetComponent<Image>();
                expBarLevelText = expBarPanel.transform.Find("LevelText")?.GetComponent<Text>();
                expBarValueText = expBarPanel.transform.Find("XPValueText")?.GetComponent<Text>();
                UITheme.ApplyBar(expBarBg, expBarFill);
            }

            goldPanel = root.Find("GoldPanel")?.gameObject;
            if (goldPanel != null)
            {
                goldText = goldPanel.transform.Find("GoldValue")?.GetComponent<Text>();
            }

            waveNotificationPanel = root.Find("WavePanel")?.gameObject;
            if (waveNotificationPanel != null)
            {
                waveText = waveNotificationPanel.transform.Find("WaveText")?.GetComponent<Text>();
            }

            levelUpPanel = root.Find("LevelUpPanel")?.gameObject;
            if (levelUpPanel != null)
            {
                levelUpText = levelUpPanel.transform.Find("LevelUpText")?.GetComponent<Text>();
            }

            deathPanel = root.Find("DeathPanel")?.gameObject;
            if (deathPanel != null)
            {
                deathTitleText = deathPanel.transform.Find("DeathTitle")?.GetComponent<Text>();
                deathStatsText = deathPanel.transform.Find("DeathStats")?.GetComponent<Text>();
                deathRestartText = deathPanel.transform.Find("DeathRestart")?.GetComponent<Text>();
                EnsureDeathPanelTMP();
            }

            EnsureModalMask();
            BuildModernHud();
        }

        playerStats = FindObjectOfType<PlayerStats>();
        playerController = FindObjectOfType<PlayerController>();
        if (playerStats != null)
        {
            playerStats.OnXPChanged.AddListener(UpdateExpBar);
            playerStats.OnGoldChanged.AddListener(UpdateGold);
            playerStats.OnLevelUp.AddListener(OnLevelUp);
            playerStats.OnHealthChanged.AddListener(OnHPChanged);
            UpdateExpBar(playerStats.currentXP);
            UpdateGold(playerStats.gold);
            EnsurePlayerHealthBar();
            OnHPChanged(playerStats.currentHP);
            if (expBarLevelText != null)
            {
                expBarLevelText.text = "Lv." + playerStats.level;
            }
        }

        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.OnKillCountChanged.AddListener(OnKillsChanged);
            gameManager.OnGameOver.AddListener(OnGameOver);
        }

        if (deathPanel != null)
        {
            deathPanel.SetActive(false);
        }

        if (waveNotificationPanel != null)
        {
            waveNotificationPanel.SetActive(false);
        }

        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(false);
        }

        CreateUpgradePanel();
        CreateRelicPanel();
        CreateControlsHelpPanel();
        HideBuildPanel();
        UITheme.ApplyToCanvas(mainCanvas);

        controlsHelpAutoPending = PlayerPrefs.GetInt(ControlsHelpSeenKey, 0) == 0;
        initialized = true;
    }

    public void UpdateExpBar(int xp)
    {
        int max = playerStats != null ? playerStats.xpToNextLevel : 1;
        expLerpTarget = max > 0 ? (float)xp / max : 1f;
        if (hudXpText != null)
        {
            hudXpText.text = xp + " / " + max + " XP";
        }

        if (expBarValueText != null)
        {
            expBarValueText.text = xp + " / " + max + " XP";
        }
    }

    public void UpdateGold(int gold)
    {
        targetGold = gold;
        if (goldRollRoutine != null)
        {
            StopCoroutine(goldRollRoutine);
        }

        goldRollRoutine = StartCoroutine(GoldRollRoutine(gold));

        if (goldText != null)
        {
            goldText.text = gold.ToString();
            StartCoroutine(GoldPop());
        }
    }

    public void ShowWaveNotification(string message)
    {
        if (waveNotificationPanel == null || waveText == null)
        {
            return;
        }

        waveText.text = message;
        StartCoroutine(WaveAnimation());
    }

    public void ShowWaveClearNotification(int wave, int healAmount, int goldBonus)
    {
        ShowWaveNotification("Wave " + wave + " Cleared  +" + healAmount + " HP  +" + goldBonus + " Gold");
    }

    public void QueueRelicChoice(int wave)
    {
        pendingRelicChoices++;
        ShowWaveNotification("Relic Reward Unlocked");
        if (!awaitingUpgradeChoice && !awaitingRelicChoice)
        {
            ShowRelicChoices(wave);
        }
    }

    public void ShowDeathPanel(int kills, int gold, int level)
    {
        ShowDeathPanel(kills, gold, level, 0, MetaProgression.SoulStones);
    }

    public void ShowDeathPanel(int kills, int gold, int level, int soulReward, int totalSoulStones)
    {
        if (deathPanel != null)
        {
            StartCoroutine(DeathDelayed(kills, gold, level, soulReward, totalSoulStones));
        }
    }

    private IEnumerator GoldPop()
    {
        if (goldText == null)
        {
            yield break;
        }

        Vector3 originalScale = goldText.transform.localScale;
        float elapsed = 0f;
        while (elapsed < 0.15f)
        {
            elapsed += Time.unscaledDeltaTime;
            goldText.transform.localScale = Vector3.Lerp(originalScale, originalScale * 1.3f, elapsed / 0.15f);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < 0.15f)
        {
            elapsed += Time.unscaledDeltaTime;
            goldText.transform.localScale = Vector3.Lerp(originalScale * 1.3f, originalScale, elapsed / 0.15f);
            yield return null;
        }

        goldText.transform.localScale = originalScale;
    }

    private IEnumerator GoldRollRoutine(int newGold)
    {
        if (hudGoldText == null)
        {
            displayedGold = newGold;
            yield break;
        }

        int start = displayedGold;
        int end = newGold;
        if (start == end)
        {
            hudGoldText.text = end.ToString();
            yield break;
        }

        RectTransform rect = hudGoldText.rectTransform;
        UIAnimationUtility.PunchScale(rect, 0.16f, 0.22f);

        float elapsed = 0f;
        float duration = Mathf.Clamp(Mathf.Abs(end - start) * 0.025f, 0.18f, 0.7f);
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            displayedGold = Mathf.RoundToInt(Mathf.Lerp(start, end, t));
            hudGoldText.text = displayedGold.ToString();
            yield return null;
        }

        displayedGold = end;
        hudGoldText.text = displayedGold.ToString();
        goldRollRoutine = null;
    }

    private IEnumerator WaveAnimation()
    {
        waveNotificationPanel.SetActive(true);
        waveText.transform.localScale = Vector3.one * 1.5f;

        float elapsed = 0f;
        while (elapsed < 1f)
        {
            elapsed += Time.unscaledDeltaTime;
            waveText.transform.localScale = Vector3.Lerp(Vector3.one * 1.5f, Vector3.one, elapsed);
            yield return null;
        }

        yield return new WaitForSecondsRealtime(1f);

        elapsed = 0f;
        while (elapsed < 1f)
        {
            elapsed += Time.unscaledDeltaTime;
            Color color = waveText.color;
            color.a = 1f - elapsed;
            waveText.color = color;
            yield return null;
        }

        waveNotificationPanel.SetActive(false);
        waveText.color = Color.white;
    }

    private void OnLevelUp(int level)
    {
        pendingUpgradeChoices++;

        if (expBarLevelText != null)
        {
            expBarLevelText.text = "Lv." + level;
        }

        if (hudLevelText != null)
        {
            hudLevelText.text = "Lv." + level;
        }

        if (expBarFill != null)
        {
            StartCoroutine(LevelFlash());
        }

        if (levelUpPanel != null && levelUpText != null)
        {
            StartCoroutine(ShowLevelUpBanner());
        }

        if (!awaitingUpgradeChoice)
        {
            ShowUpgradeChoices();
        }
    }

    private IEnumerator LevelFlash()
    {
        Color originalColor = expBarFill.color;
        for (int i = 0; i < 2; i++)
        {
            expBarFill.color = new Color(1f, 0.84f, 0f);
            yield return new WaitForSecondsRealtime(0.15f);
            expBarFill.color = originalColor;
            yield return new WaitForSecondsRealtime(0.15f);
        }
    }

    private IEnumerator ShowLevelUpBanner()
    {
        levelUpPanel.SetActive(true);
        levelUpText.text = "LEVEL UP!";
        levelUpText.transform.localScale = Vector3.one * 0.5f;

        float elapsed = 0f;
        while (elapsed < 0.3f)
        {
            elapsed += Time.unscaledDeltaTime;
            levelUpText.transform.localScale = Vector3.Lerp(Vector3.one * 0.5f, Vector3.one * 1.2f, elapsed / 0.3f);
            yield return null;
        }

        levelUpText.transform.localScale = Vector3.one;
        yield return new WaitForSecondsRealtime(1.7f);

        elapsed = 0f;
        while (elapsed < 0.5f)
        {
            elapsed += Time.unscaledDeltaTime;
            Color color = levelUpText.color;
            color.a = 1f - elapsed / 0.5f;
            levelUpText.color = color;
            yield return null;
        }

        levelUpPanel.SetActive(false);
        levelUpText.color = Color.white;
    }

    private IEnumerator DeathDelayed(int kills, int gold, int level, int soulReward, int totalSoulStones)
    {
        yield return new WaitForSecondsRealtime(1f);
        deathPanel.SetActive(true);

        if (deathTitleText != null)
        {
            deathTitleText.text = "You Have Fallen";
        }
        if (deathTitleTMP != null)
        {
            deathTitleTMP.text = "YOU HAVE FALLEN";
        }

        if (deathStatsText != null)
        {
            deathStatsText.text = "击杀: " + kills + "\n金币: " + gold + "\n等级: Lv." + level + "\n魂石 +" + soulReward + "  总魂石: " + totalSoulStones;
            UITheme.ApplyText(deathStatsText);
        }
        if (deathStatsTMP != null)
        {
            deathStatsTMP.text = "Kills: " + kills + "\nGold Collected: " + gold + "\nLevel Reached: Lv." + level + "\nSoul Stones +" + soulReward + "  Total: " + totalSoulStones;
        }

        if (deathRestartText != null)
        {
            deathRestartText.text = "Press R to try again";
            StartCoroutine(Blink(deathRestartText));
        }
        if (deathRestartTMP != null)
        {
            deathRestartTMP.text = "Press R to try again";
            StartCoroutine(Blink(deathRestartTMP));
        }
    }

    private IEnumerator Blink(Text text)
    {
        while (true)
        {
            text.color = new Color(1f, 1f, 1f, 0.3f);
            yield return new WaitForSecondsRealtime(0.5f);
            text.color = Color.white;
            yield return new WaitForSecondsRealtime(0.5f);
        }
    }

    private IEnumerator Blink(TMP_Text text)
    {
        while (true)
        {
            text.color = new Color(1f, 1f, 1f, 0.3f);
            yield return new WaitForSecondsRealtime(0.5f);
            text.color = UITheme.BodyColor;
            yield return new WaitForSecondsRealtime(0.5f);
        }
    }

    private void OnHPChanged(float hp)
    {
        if (playerStats == null)
        {
            return;
        }

        EnsurePlayerHealthBar();
        if (playerHealthBar != null)
        {
            playerHealthBar.SetHealth(hp, playerStats.maxHP);
        }
    }

    private void OnKillsChanged(int kills)
    {
    }

    private void OnGameOver()
    {
        awaitingUpgradeChoice = false;
        awaitingRelicChoice = false;
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
        }
        if (relicPanel != null)
        {
            relicPanel.SetActive(false);
        }

        Time.timeScale = 1f;
    }

    private void CreateBuildPanel()
    {
        if (mainCanvas == null || buildPanel != null)
        {
            return;
        }

        buildPanel = new GameObject("BuildPanel");
        buildPanel.transform.SetParent(mainCanvas.transform, false);
        RectTransform rect = buildPanel.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(18f, -18f);
        rect.sizeDelta = new Vector2(280f, 168f);

        Image image = buildPanel.AddComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.42f);

        buildSummaryText = CreateText(buildPanel.transform, string.Empty, 16, new Vector2(0f, -2f), new Vector2(252f, 148f), TextAnchor.UpperLeft, Color.white);
    }

    private void HideBuildPanel()
    {
        if (buildPanel == null && mainCanvas != null)
        {
            Transform existing = mainCanvas.transform.Find("BuildPanel");
            buildPanel = existing != null ? existing.gameObject : null;
        }

        if (buildPanel != null)
        {
            buildPanel.SetActive(false);
        }
    }

    private void CreateUpgradePanel()
    {
        if (mainCanvas == null || upgradePanel != null)
        {
            return;
        }

        upgradePanel = CreatePanel("UpgradePanel", mainCanvas.transform, new Vector2(760f, 420f), UITheme.PanelTint);
        upgradeHeaderText = CreateText(upgradePanel.transform, "选择祝福", 31, new Vector2(0f, 170f), new Vector2(640f, 42f), TextAnchor.MiddleCenter, UITheme.TitleColor);
        upgradeHintText = CreateText(upgradePanel.transform, "按 1 / 2 / 3 选择", 19, new Vector2(0f, -186f), new Vector2(380f, 34f), TextAnchor.MiddleCenter, UITheme.HintColor);

        upgradeOptionTexts = new Text[3];
        for (int i = 0; i < upgradeOptionTexts.Length; i++)
        {
            GameObject optionPanel = CreatePanel("UpgradeOption_" + i, upgradePanel.transform, new Vector2(692f, 92f), UITheme.OptionTint);
            optionPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 102f - i * 102f);
            CreateChoiceBadge(optionPanel.transform, i + 1);
            upgradeOptionTexts[i] = CreateText(optionPanel.transform, string.Empty, 20, new Vector2(34f, 0f), new Vector2(592f, 76f), TextAnchor.MiddleLeft, UITheme.BodyColor);
            ConfigureChoiceText(upgradeOptionTexts[i]);
        }

        upgradePanel.SetActive(false);
    }

    private void CreateRelicPanel()
    {
        if (mainCanvas == null || relicPanel != null)
        {
            return;
        }

        relicPanel = CreatePanel("RelicPanel", mainCanvas.transform, new Vector2(820f, 450f), UITheme.PanelTint);
        relicHeaderText = CreateText(relicPanel.transform, "选择遗物", 32, new Vector2(0f, 176f), new Vector2(680f, 44f), TextAnchor.MiddleCenter, UITheme.TitleColor);
        relicHintText = CreateText(relicPanel.transform, "Boss 奖励 - 按 1 / 2 / 3 选择", 19, new Vector2(0f, -196f), new Vector2(460f, 34f), TextAnchor.MiddleCenter, UITheme.HintColor);

        relicOptionTexts = new Text[3];
        for (int i = 0; i < relicOptionTexts.Length; i++)
        {
            GameObject optionPanel = CreatePanel("RelicOption_" + i, relicPanel.transform, new Vector2(730f, 98f), UITheme.OptionTint);
            optionPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 104f - i * 108f);
            CreateChoiceBadge(optionPanel.transform, i + 1);
            relicOptionTexts[i] = CreateText(optionPanel.transform, string.Empty, 20, new Vector2(36f, 0f), new Vector2(620f, 80f), TextAnchor.MiddleLeft, UITheme.BodyColor);
            ConfigureChoiceText(relicOptionTexts[i]);
        }

        relicPanel.SetActive(false);
    }

    private void ConfigureChoiceText(Text text)
    {
        if (text == null)
        {
            return;
        }

        text.supportRichText = true;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 13;
        text.resizeTextMaxSize = text.fontSize;
        text.lineSpacing = 1.08f;
    }

    private void EnsureDeathPanelTMP()
    {
        if (deathPanel == null || deathTitleTMP != null)
        {
            return;
        }

        if (deathTitleText != null || deathStatsText != null || deathRestartText != null)
        {
            UITheme.ApplyText(deathTitleText);
            UITheme.ApplyText(deathStatsText);
            UITheme.ApplyText(deathRestartText);
            return;
        }

        deathTitleTMP = deathPanel.transform.Find("DeathTitleTMP")?.GetComponent<TMP_Text>();
        deathStatsTMP = deathPanel.transform.Find("DeathStatsTMP")?.GetComponent<TMP_Text>();
        deathRestartTMP = deathPanel.transform.Find("DeathRestartTMP")?.GetComponent<TMP_Text>();
        if (deathTitleTMP != null && deathStatsTMP != null && deathRestartTMP != null)
        {
            return;
        }

        Image panelImage = deathPanel.GetComponent<Image>();
        if (panelImage != null)
        {
            UITheme.ApplyPanel(panelImage, false);
            panelImage.color = new Color(0.05f, 0.04f, 0.045f, 0.96f);
        }

        deathTitleTMP = CreateTMPText(deathPanel.transform, "YOU HAVE FALLEN", 38f, new Vector2(0f, 76f), new Vector2(520f, 54f), TextAlignmentOptions.Center, UITheme.TitleColor, true);
        deathTitleTMP.gameObject.name = "DeathTitleTMP";
        deathStatsTMP = CreateTMPText(deathPanel.transform, string.Empty, 23f, new Vector2(0f, 4f), new Vector2(520f, 90f), TextAlignmentOptions.Center, UITheme.BodyColor, false);
        deathStatsTMP.gameObject.name = "DeathStatsTMP";
        deathStatsTMP.enableWordWrapping = true;
        deathRestartTMP = CreateTMPText(deathPanel.transform, "Press R to try again", 21f, new Vector2(0f, -82f), new Vector2(420f, 34f), TextAlignmentOptions.Center, UITheme.HintColor, false);
        deathRestartTMP.gameObject.name = "DeathRestartTMP";
    }

    private void ShowUpgradeChoices()
    {
        if (upgradePanel == null || playerStats == null || playerController == null)
        {
            pendingUpgradeChoices = 0;
            awaitingUpgradeChoice = false;
            Time.timeScale = 1f;
            return;
        }

        activeChoices = GenerateUpgradeChoices();
        for (int i = 0; i < activeChoices.Length; i++)
        {
            upgradeOptionTexts[i].text = "<b>" + (i + 1) + ". " + activeChoices[i].title + "</b>\n" + activeChoices[i].description;
        }

        pendingUpgradeChoices = Mathf.Max(0, pendingUpgradeChoices - 1);
        awaitingUpgradeChoice = true;
        upgradePanel.transform.SetAsLastSibling();
        upgradePanel.SetActive(true);
        UIAnimationUtility.PopIn(upgradePanel.GetComponent<RectTransform>(), GetOrAddCanvasGroup(upgradePanel));
        Time.timeScale = 1f;
    }

    private void ShowRelicChoices(int wave)
    {
        if (relicPanel == null || playerStats == null || playerController == null)
        {
            pendingRelicChoices = 0;
            awaitingRelicChoice = false;
            Time.timeScale = 1f;
            return;
        }

        if (relicHeaderText != null)
        {
            relicHeaderText.text = "选择遗物";
        }

        activeRelicChoices = GenerateRelicChoices();
        for (int i = 0; i < activeRelicChoices.Length; i++)
        {
            relicOptionTexts[i].text = "<b>" + (i + 1) + ". " + activeRelicChoices[i].title + "</b>\n" + activeRelicChoices[i].description;
        }

        pendingRelicChoices = Mathf.Max(0, pendingRelicChoices - 1);
        awaitingRelicChoice = true;
        relicPanel.transform.SetAsLastSibling();
        relicPanel.SetActive(true);
        UIAnimationUtility.PopIn(relicPanel.GetComponent<RectTransform>(), GetOrAddCanvasGroup(relicPanel));
        Time.timeScale = 1f;
    }

    private UpgradeChoice[] GenerateUpgradeChoices()
    {
        UpgradeChoice[] pool =
        {
            new UpgradeChoice
            {
                title = "战士之力",
                description = "攻击力 +5",
                apply = (stats, controller) => stats.IncreaseAttack(5f)
            },
            new UpgradeChoice
            {
                title = "钢铁守卫",
                description = "防御力 +2",
                apply = (stats, controller) => stats.IncreaseDefense(2f)
            },
            new UpgradeChoice
            {
                title = "生命涌动",
                description = "最大生命值 +25，并完全恢复生命",
                apply = (stats, controller) => stats.IncreaseMaxHealth(25f, true)
            },
            new UpgradeChoice
            {
                title = "迅捷出手",
                description = "攻击冷却 -12%",
                apply = (stats, controller) => controller.MultiplyAttackCooldown(0.88f)
            },
            new UpgradeChoice
            {
                title = "长柄打击",
                description = "攻击范围 +0.4",
                apply = (stats, controller) => controller.IncreaseAttackRange(0.4f)
            },
            new UpgradeChoice
            {
                title = "战斗步法",
                description = "移动速度 +1，闪避冷却 -0.2 秒",
                apply = (stats, controller) =>
                {
                    controller.IncreaseMoveSpeed(1f);
                    controller.ReduceDodgeCooldown(0.2f);
                }
            },
            new UpgradeChoice
            {
                title = "横扫钢刃",
                description = "每次挥击可额外命中 1 个敌人",
                apply = (stats, controller) => controller.IncreaseMaxTargets(1)
            },
            new UpgradeChoice
            {
                title = "锋刃打磨",
                description = "挥击伤害 +15%",
                apply = (stats, controller) => controller.IncreaseDamageMultiplier(0.15f)
            },
            new UpgradeChoice
            {
                title = "暴击流派：杀手本能",
                description = "暴击率 +8%，暴击倍率 +0.25。暴击命中会触发爆发特效",
                apply = (stats, controller) =>
                {
                    stats.IncreaseCritChance(0.08f);
                    stats.IncreaseCritMultiplier(0.25f);
                    stats.IncreaseCritBuild(1);
                }
            },
            new UpgradeChoice
            {
                title = "暴击流派：处决锋芒",
                description = "暴击率 +10%，暴击爆发伤害更强",
                apply = (stats, controller) =>
                {
                    stats.IncreaseCritChance(0.10f);
                    stats.IncreaseCritBuild(1);
                    controller.IncreaseDamageMultiplier(0.08f);
                }
            },
            new UpgradeChoice
            {
                title = "鲜血契约",
                description = "生命偷取 +6%",
                apply = (stats, controller) => stats.IncreaseLifeSteal(0.06f)
            },
            new UpgradeChoice
            {
                title = "燃烧流派：余烬之刃",
                description = "燃烧几率 +18%，燃烧秒伤 +4。燃烧会扩散到附近敌人",
                apply = (stats, controller) =>
                {
                    stats.IncreaseBurnChance(0.18f, 4f);
                    stats.IncreaseBurnBuild(1);
                }
            },
            new UpgradeChoice
            {
                title = "燃烧流派：燎原火",
                description = "燃烧几率 +12%，燃烧扩散范围更大",
                apply = (stats, controller) =>
                {
                    stats.IncreaseBurnChance(0.12f, 3f);
                    stats.IncreaseBurnBuild(1);
                }
            },
            new UpgradeChoice
            {
                title = "闪电流派：震荡核心",
                description = "冲击波几率 +16%。闪电层数越高，冲击波越强",
                apply = (stats, controller) =>
                {
                    stats.IncreaseShockwaveChance(0.16f);
                    stats.IncreaseLightningBuild(1);
                }
            },
            new UpgradeChoice
            {
                title = "闪电流派：连锁电流",
                description = "冲击波几率 +10%，额外命中 +1，闪电连锁范围更大",
                apply = (stats, controller) =>
                {
                    stats.IncreaseShockwaveChance(0.10f);
                    stats.IncreaseLightningBuild(1);
                    controller.IncreaseMaxTargets(1);
                }
            }
        };

        List<int> pickedIndices = new List<int>();
        while (pickedIndices.Count < 3)
        {
            int candidate = Random.Range(0, pool.Length);
            if (!pickedIndices.Contains(candidate))
            {
                pickedIndices.Add(candidate);
            }
        }

        return new[]
        {
            pool[pickedIndices[0]],
            pool[pickedIndices[1]],
            pool[pickedIndices[2]]
        };
    }

    private UpgradeChoice[] GenerateRelicChoices()
    {
        UpgradeChoice[] pool =
        {
            new UpgradeChoice
            {
                title = "泰坦之心",
                description = "最大生命值 +60，完全恢复生命，防御力 +2",
                apply = (stats, controller) =>
                {
                    stats.IncreaseMaxHealth(60f, true);
                    stats.IncreaseDefense(2f);
                }
            },
            new UpgradeChoice
            {
                title = "收割者徽记",
                description = "生命偷取 +12%，伤害 +15%",
                apply = (stats, controller) =>
                {
                    stats.IncreaseLifeSteal(0.12f);
                    controller.IncreaseDamageMultiplier(0.15f);
                }
            },
            new UpgradeChoice
            {
                title = "风暴双刃",
                description = "额外命中 +2，攻击范围 +0.5",
                apply = (stats, controller) =>
                {
                    controller.IncreaseMaxTargets(2);
                    controller.IncreaseAttackRange(0.5f);
                }
            },
            new UpgradeChoice
            {
                title = "暴击遗物：刺客印记",
                description = "暴击率 +15%，暴击倍率 +0.5，暴击流派层数 +2",
                apply = (stats, controller) =>
                {
                    stats.IncreaseCritChance(0.15f);
                    stats.IncreaseCritMultiplier(0.5f);
                    stats.IncreaseCritBuild(2);
                }
            },
            new UpgradeChoice
            {
                title = "踏风之靴",
                description = "移动速度 +2，闪避冷却 -0.4 秒，攻击冷却 -10%",
                apply = (stats, controller) =>
                {
                    controller.IncreaseMoveSpeed(2f);
                    controller.ReduceDodgeCooldown(0.4f);
                    controller.MultiplyAttackCooldown(0.9f);
                }
            },
            new UpgradeChoice
            {
                title = "燃烧遗物：炼狱印记",
                description = "燃烧几率 +35%，燃烧秒伤 +10，燃烧流派层数 +2",
                apply = (stats, controller) =>
                {
                    stats.IncreaseBurnChance(0.35f, 10f);
                    stats.IncreaseBurnBuild(2);
                }
            },
            new UpgradeChoice
            {
                title = "闪电遗物：雷霆王冠",
                description = "冲击波几率 +30%，伤害 +20%，闪电流派层数 +2",
                apply = (stats, controller) =>
                {
                    stats.IncreaseShockwaveChance(0.30f);
                    controller.IncreaseDamageMultiplier(0.20f);
                    stats.IncreaseLightningBuild(2);
                }
            }
        };

        List<int> pickedIndices = new List<int>();
        while (pickedIndices.Count < 3)
        {
            int candidate = Random.Range(0, pool.Length);
            if (!pickedIndices.Contains(candidate))
            {
                pickedIndices.Add(candidate);
            }
        }

        return new[]
        {
            pool[pickedIndices[0]],
            pool[pickedIndices[1]],
            pool[pickedIndices[2]]
        };
    }

    private void ApplyUpgradeChoice(int index)
    {
        if (!awaitingUpgradeChoice || activeChoices == null || index < 0 || index >= activeChoices.Length)
        {
            return;
        }

        activeChoices[index].apply?.Invoke(playerStats, playerController);
        awaitingUpgradeChoice = false;
        bool showNextPanel = pendingUpgradeChoices > 0 || pendingRelicChoices > 0;
        if (showNextPanel)
        {
            upgradePanel.SetActive(false);
        }
        else
        {
            UIAnimationUtility.FadeOut(GetOrAddCanvasGroup(upgradePanel), 0.12f);
        }
        Time.timeScale = 1f;

        if (pendingUpgradeChoices > 0)
        {
            ShowUpgradeChoices();
        }
        else if (pendingRelicChoices > 0)
        {
            ShowRelicChoices(0);
        }
    }

    private void ApplyRelicChoice(int index)
    {
        if (!awaitingRelicChoice || activeRelicChoices == null || index < 0 || index >= activeRelicChoices.Length)
        {
            return;
        }

        activeRelicChoices[index].apply?.Invoke(playerStats, playerController);
        awaitingRelicChoice = false;
        bool showNextPanel = pendingUpgradeChoices > 0 || pendingRelicChoices > 0;
        if (showNextPanel)
        {
            relicPanel.SetActive(false);
        }
        else
        {
            UIAnimationUtility.FadeOut(GetOrAddCanvasGroup(relicPanel), 0.12f);
        }
        Time.timeScale = 1f;

        if (pendingUpgradeChoices > 0)
        {
            ShowUpgradeChoices();
            return;
        }

        if (pendingRelicChoices > 0)
        {
            ShowRelicChoices(0);
            return;
        }

        WaveManager waveManager = FindObjectOfType<WaveManager>();
        if (waveManager != null)
        {
            waveManager.HandlePostBossRewardResolved();
        }
    }

    private void CreateChoiceBadge(Transform parent, int number)
    {
        GameObject badge = new GameObject("ChoiceBadge_" + number);
        badge.transform.SetParent(parent, false);
        RectTransform rect = badge.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0.5f);
        rect.anchorMax = new Vector2(0f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(36f, 0f);
        rect.sizeDelta = new Vector2(42f, 42f);

        Image image = badge.AddComponent<Image>();
        image.sprite = UITheme.ButtonSprite;
        image.type = Image.Type.Sliced;
        image.color = new Color(0.22f, 0.25f, 0.3f, 1f);
        image.raycastTarget = false;

        CreateTMPText(badge.transform, number.ToString(), 22f, Vector2.zero, new Vector2(34f, 34f), TextAlignmentOptions.Center, UITheme.TitleColor, true);
    }

    private GameObject CreatePanel(string name, Transform parent, Vector2 size, Color color)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        Image image = panel.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        UITheme.ApplyPanel(image, name.Contains("Option") || name.Contains("Button"));
        panel.AddComponent<CanvasGroup>();
        return panel;
    }

    private GameObject CreateUIBlock(string name, Transform parent, Vector2 anchor, Vector2 anchoredPosition, Vector2 size, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        Image image = obj.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return obj;
    }

    private void BuildModernHud()
    {
        if (mainCanvas == null || modernHudRoot != null)
        {
            return;
        }

        if (expBarPanel != null)
        {
            expBarPanel.SetActive(false);
        }

        if (goldPanel != null)
        {
            goldPanel.SetActive(false);
        }

        HidePlayerWorldHealthBar();

        modernHudRoot = new GameObject("ModernHudRoot");
        modernHudRoot.transform.SetParent(mainCanvas.transform, false);
        RectTransform rootRect = modernHudRoot.AddComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0f, 1f);
        rootRect.anchorMax = new Vector2(0f, 1f);
        rootRect.pivot = new Vector2(0f, 1f);
        rootRect.anchoredPosition = new Vector2(16f, -16f);
        rootRect.sizeDelta = new Vector2(360f, 176f);

        Image rootImage = modernHudRoot.AddComponent<Image>();
        rootImage.color = new Color(0.05f, 0.05f, 0.07f, 0.92f);
        rootImage.raycastTarget = false;
        UITheme.ApplyPanel(rootImage, false);
        CanvasGroup hudGroup = modernHudRoot.AddComponent<CanvasGroup>();

        GameObject titleBar = CreatePanel("HudTitle", modernHudRoot.transform, new Vector2(332f, 28f), new Color(0.1f, 0.1f, 0.12f, 0.98f));
        SetTopLeft(titleBar.GetComponent<RectTransform>(), new Vector2(14f, -12f), new Vector2(332f, 28f));
        CreateHudTMPText(titleBar.transform, "ADVENTURER", 14f, new Vector2(-82f, 0f), new Vector2(160f, 22f), TextAlignmentOptions.MidlineLeft, new Color(1f, 0.93f, 0.8f), true);
        hudLevelText = CreateHudTMPText(titleBar.transform, "Lv.1", 14f, new Vector2(126f, 0f), new Vector2(70f, 22f), TextAlignmentOptions.MidlineRight, new Color(0.86f, 0.94f, 1f), true);

        GameObject hpPanel = CreatePanel("HudHP", modernHudRoot.transform, new Vector2(332f, 46f), new Color(0.11f, 0.08f, 0.08f, 0.98f));
        SetTopLeft(hpPanel.GetComponent<RectTransform>(), new Vector2(14f, -46f), new Vector2(332f, 46f));
        CreateHudIcon(hpPanel.transform, "HP", new Vector2(-142f, 0f), new Color(0.55f, 0.13f, 0.13f, 1f));

        GameObject hpBar = BufferedHealthBar.Create(hpPanel.transform).gameObject;
        RectTransform hpRect = hpBar.GetComponent<RectTransform>();
        hpRect.anchorMin = Vector2.zero;
        hpRect.anchorMax = Vector2.one;
        hpRect.offsetMin = new Vector2(48f, 10f);
        hpRect.offsetMax = new Vector2(-12f, -10f);
        playerHealthBar = hpBar.GetComponent<BufferedHealthBar>();

        GameObject xpPanel = CreatePanel("HudXP", modernHudRoot.transform, new Vector2(332f, 34f), new Color(0.08f, 0.1f, 0.14f, 0.9f));
        SetTopLeft(xpPanel.GetComponent<RectTransform>(), new Vector2(14f, -98f), new Vector2(332f, 34f));
        CreateHudIcon(xpPanel.transform, "XP", new Vector2(-142f, 0f), new Color(0.12f, 0.28f, 0.55f, 1f));
        GameObject xpBack = CreateUIBlock("XPBack", xpPanel.transform, new Vector2(0.5f, 0.5f), new Vector2(-3f, 0f), new Vector2(206f, 12f), new Color(0.14f, 0.15f, 0.18f, 1f));
        GameObject xpFillObj = CreateUIBlock("XPFill", xpBack.transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(206f, 12f), new Color(0.3f, 0.7f, 1f, 1f));
        expBarFill = xpFillObj.GetComponent<Image>();
        expBarFill.type = Image.Type.Filled;
        expBarFill.fillMethod = Image.FillMethod.Horizontal;
        expBarFill.fillOrigin = 0;
        hudXpText = CreateHudTMPText(xpPanel.transform, string.Empty, 11f, new Vector2(112f, 0f), new Vector2(84f, 20f), TextAlignmentOptions.MidlineRight, new Color(0.85f, 0.93f, 1f), true);

        GameObject goldHud = CreatePanel("HudGold", modernHudRoot.transform, new Vector2(332f, 30f), new Color(0.1f, 0.08f, 0.05f, 0.92f));
        SetTopLeft(goldHud.GetComponent<RectTransform>(), new Vector2(14f, -138f), new Vector2(332f, 30f));
        CreateHudIcon(goldHud.transform, "$", new Vector2(-142f, 0f), new Color(0.64f, 0.43f, 0.08f, 1f));
        CreateHudTMPText(goldHud.transform, "GOLD", 11f, new Vector2(-98f, 0f), new Vector2(70f, 20f), TextAlignmentOptions.MidlineLeft, UITheme.GoldColor, true);
        hudGoldText = CreateHudTMPText(goldHud.transform, "0", 20f, new Vector2(88f, 0f), new Vector2(160f, 24f), TextAlignmentOptions.MidlineRight, new Color(1f, 0.88f, 0.34f), true);
        UITheme.ApplyHudGoldEmphasis(hudGoldText);

        hudGroup.alpha = 0f;
        UIAnimationUtility.FadeIn(hudGroup, 0.2f);
    }

    private void CreateHudIcon(Transform parent, string text, Vector2 position, Color color)
    {
        GameObject icon = new GameObject("HudIcon_" + text);
        icon.transform.SetParent(parent, false);
        RectTransform rect = icon.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(34f, 26f);

        Image image = icon.AddComponent<Image>();
        image.sprite = UITheme.ButtonSprite;
        image.type = Image.Type.Sliced;
        image.color = color;
        image.raycastTarget = false;

        CreateHudTMPText(icon.transform, text, text.Length > 1 ? 11f : 17f, Vector2.zero, rect.sizeDelta, TextAlignmentOptions.Center, Color.white, true);
    }

    private void SetTopLeft(RectTransform rect, Vector2 position, Vector2 size)
    {
        if (rect == null)
        {
            return;
        }

        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private void CreateControlsHelpPanel()
    {
        if (mainCanvas == null || controlsHelpPanel != null)
        {
            return;
        }

        controlsHelpPanel = CreatePanel("ControlsHelpPanel", mainCanvas.transform, new Vector2(388f, 318f), new Color(0.045f, 0.05f, 0.065f, 0.94f));
        RectTransform rect = controlsHelpPanel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(1f, 0f);
        rect.anchoredPosition = new Vector2(-18f, 18f);
        rect.sizeDelta = new Vector2(388f, 318f);

        controlsHelpGroup = GetOrAddCanvasGroup(controlsHelpPanel);
        controlsHelpGroup.alpha = 0f;
        controlsHelpGroup.interactable = false;
        controlsHelpGroup.blocksRaycasts = false;

        CreateHudTMPText(controlsHelpPanel.transform, "CONTROLS", 19f, new Vector2(-126f, 130f), new Vector2(120f, 28f), TextAlignmentOptions.MidlineLeft, UITheme.TitleColor, true);
        CreateHudTMPText(controlsHelpPanel.transform, "F1", 13f, new Vector2(148f, 130f), new Vector2(54f, 24f), TextAlignmentOptions.Center, UITheme.HintColor, true);

        CreateControlsHelpRow(0, "WASD", "Move");
        CreateControlsHelpRow(1, "J / LMB", "Attack");
        CreateControlsHelpRow(2, "Space", "Dash");
        CreateControlsHelpRow(3, "E", "Interact / open chest");
        CreateControlsHelpRow(4, "1 2 3", "Choose upgrade or relic");
        CreateControlsHelpRow(5, "Esc", "Pause");
        CreateControlsHelpRow(6, "R", "Restart after death");

        CreateHudTMPText(controlsHelpPanel.transform, "Press F1 to hide or show this panel.", 12f, new Vector2(0f, -136f), new Vector2(330f, 24f), TextAlignmentOptions.Center, UITheme.HintColor, false);
        controlsHelpPanel.SetActive(false);
    }

    private void CreateControlsHelpRow(int index, string keyLabel, string actionLabel)
    {
        float y = 94f - index * 32f;
        GameObject keyBadge = CreateUIBlock("Key_" + keyLabel.Replace(" ", string.Empty), controlsHelpPanel.transform, new Vector2(0.5f, 0.5f), new Vector2(-114f, y), new Vector2(112f, 24f), new Color(0.12f, 0.14f, 0.18f, 1f));
        Image badgeImage = keyBadge.GetComponent<Image>();
        badgeImage.sprite = UITheme.ButtonSprite;
        badgeImage.type = Image.Type.Sliced;
        CreateHudTMPText(keyBadge.transform, keyLabel, 12f, Vector2.zero, new Vector2(96f, 20f), TextAlignmentOptions.Center, new Color(0.96f, 0.92f, 0.82f), true);

        CreateHudTMPText(controlsHelpPanel.transform, actionLabel, 14f, new Vector2(58f, y), new Vector2(200f, 24f), TextAlignmentOptions.MidlineLeft, UITheme.BodyColor, false);
    }

    private void ToggleControlsHelp()
    {
        if (controlsHelpPanel == null)
        {
            CreateControlsHelpPanel();
        }

        if (controlsHelpPanel == null)
        {
            return;
        }

        if (controlsHelpOpen)
        {
            HideControlsHelp();
            return;
        }

        ShowControlsHelp(0f);
    }

    private void UpdateControlsHelp()
    {
        if (!initialized || controlsHelpPanel == null || IsStartMenuVisible())
        {
            return;
        }

        if (controlsHelpAutoPending && Time.timeScale > 0f)
        {
            controlsHelpAutoPending = false;
            PlayerPrefs.SetInt(ControlsHelpSeenKey, 1);
            PlayerPrefs.Save();
            ShowControlsHelp(14f);
        }

        if (controlsHelpOpen && controlsHelpHideTime > 0f && Time.unscaledTime >= controlsHelpHideTime)
        {
            HideControlsHelp();
        }
    }

    private void ShowControlsHelp(float autoHideSeconds)
    {
        controlsHelpPanel.transform.SetAsLastSibling();
        controlsHelpPanel.SetActive(true);
        controlsHelpOpen = true;
        controlsHelpHideTime = autoHideSeconds > 0f ? Time.unscaledTime + autoHideSeconds : 0f;
        UIAnimationUtility.FadeIn(controlsHelpGroup, 0.18f);
    }

    private void HideControlsHelp()
    {
        controlsHelpOpen = false;
        controlsHelpHideTime = 0f;
        UIAnimationUtility.FadeOut(controlsHelpGroup, 0.14f);
    }

    private bool IsStartMenuVisible()
    {
        GameObject startMenuRoot = GameObject.Find("StartMenuRoot");
        return startMenuRoot != null && startMenuRoot.activeInHierarchy;
    }

    private void TogglePausePanel()
    {
        if (pausePanelOpen)
        {
            ResumeFromPause();
            return;
        }

        EnsurePausePanel();
        ShowPauseMainPage();
        pausePanelOpen = true;
        OpenModal(pausePanel, true);
    }

    private void EnsurePausePanel()
    {
        if (pausePanel != null || mainCanvas == null)
        {
            return;
        }

        pausePanel = CreatePanel("PausePanel", mainCanvas.transform, new Vector2(540f, 430f), new Color(0.04f, 0.04f, 0.055f, 0.95f));
        pausePanel.transform.SetAsLastSibling();

        pauseMainPage = CreatePausePage("PauseMainPage");
        pauseSettingsPage = CreatePausePage("PauseSettingsPage");
        pauseQuitPage = CreatePausePage("PauseQuitPage");

        BuildPauseMainPage();
        BuildPauseSettingsPage();
        BuildPauseQuitPage();
        ShowPauseMainPage();
        pausePanel.SetActive(false);
    }

    private GameObject CreatePausePage(string pageName)
    {
        GameObject page = new GameObject(pageName);
        page.transform.SetParent(pausePanel.transform, false);
        RectTransform rect = page.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return page;
    }

    private void BuildPauseMainPage()
    {
        CreateTMPText(pauseMainPage.transform, "PAUSED", 36f, new Vector2(0f, 156f), new Vector2(380f, 48f), TextAlignmentOptions.Center, new Color(1f, 0.92f, 0.76f), true);
        CreatePauseButton(pauseMainPage.transform, "继续游戏", new Vector2(0f, 76f), ResumeFromPause, UIButtonStyle.Primary);
        CreatePauseButton(pauseMainPage.transform, "设置", new Vector2(0f, 12f), ShowPauseSettingsPage, UIButtonStyle.Outline);
        CreatePauseButton(pauseMainPage.transform, "退出游戏", new Vector2(0f, -52f), ShowPauseQuitPage, UIButtonStyle.Danger);
        CreateText(pauseMainPage.transform, "按 ESC 也可以返回游戏", 16, new Vector2(0f, -138f), new Vector2(360f, 30f), TextAnchor.MiddleCenter, UITheme.HintColor);
    }

    private void BuildPauseSettingsPage()
    {
        CreateTMPText(pauseSettingsPage.transform, "SETTINGS", 32f, new Vector2(0f, 156f), new Vector2(380f, 44f), TextAlignmentOptions.Center, UITheme.TitleColor, true);

        GameObject audioRow = CreateUIBlock("PauseAudioRow", pauseSettingsPage.transform, new Vector2(0.5f, 0.5f), new Vector2(0f, 68f), new Vector2(420f, 86f), new Color(0.1f, 0.11f, 0.14f, 0.96f));
        Image rowImage = audioRow.GetComponent<Image>();
        rowImage.sprite = UITheme.PanelSprite;
        rowImage.type = Image.Type.Sliced;
        CreateText(audioRow.transform, "主音量", 20, new Vector2(-140f, 18f), new Vector2(110f, 28f), TextAnchor.MiddleLeft, UITheme.BodyColor);
        CreateText(audioRow.transform, "调整游戏整体音量", 14, new Vector2(-104f, -16f), new Vector2(190f, 24f), TextAnchor.MiddleLeft, UITheme.HintColor);
        pauseVolumeSlider = CreatePauseSlider(audioRow.transform, new Vector2(104f, 0f));
        pauseVolumeSlider.value = AudioListener.volume;
        pauseVolumeSlider.onValueChanged.AddListener(value => AudioListener.volume = value);

        GameObject controlsRow = CreateUIBlock("PauseControlsRow", pauseSettingsPage.transform, new Vector2(0.5f, 0.5f), new Vector2(0f, -34f), new Vector2(420f, 62f), new Color(0.08f, 0.09f, 0.115f, 0.92f));
        Image controlsImage = controlsRow.GetComponent<Image>();
        controlsImage.sprite = UITheme.PanelSprite;
        controlsImage.type = Image.Type.Sliced;
        CreateText(controlsRow.transform, "按键说明", 18, new Vector2(-134f, 0f), new Vector2(120f, 28f), TextAnchor.MiddleLeft, UITheme.BodyColor);
        CreateText(controlsRow.transform, "游戏中按 F1 开关", 15, new Vector2(72f, 0f), new Vector2(210f, 28f), TextAnchor.MiddleLeft, UITheme.HintColor);

        CreatePauseButton(pauseSettingsPage.transform, "返回", new Vector2(0f, -142f), ShowPauseMainPage, UIButtonStyle.Ghost, new Vector2(260f, 48f));
    }

    private void BuildPauseQuitPage()
    {
        CreateTMPText(pauseQuitPage.transform, "QUIT GAME?", 32f, new Vector2(0f, 130f), new Vector2(380f, 44f), TextAlignmentOptions.Center, UITheme.TitleColor, true);
        CreateText(pauseQuitPage.transform, "当前局内进度可能不会保存。", 18, new Vector2(0f, 62f), new Vector2(390f, 32f), TextAnchor.MiddleCenter, UITheme.HintColor);
        CreatePauseButton(pauseQuitPage.transform, "取消", new Vector2(-110f, -46f), ShowPauseMainPage, UIButtonStyle.Ghost, new Vector2(170f, 50f));
        CreatePauseButton(pauseQuitPage.transform, "退出游戏", new Vector2(110f, -46f), ConfirmPauseQuitGame, UIButtonStyle.Danger, new Vector2(170f, 50f));
    }

    private Button CreatePauseButton(Transform parent, string label, Vector2 position, UnityEngine.Events.UnityAction action, UIButtonStyle style, Vector2? size = null)
    {
        GameObject obj = new GameObject(label + "Button");
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size ?? new Vector2(310f, 54f);
        rect.anchoredPosition = position;

        Image image = obj.AddComponent<Image>();
        Button button = obj.AddComponent<Button>();
        button.onClick.AddListener(action);

        Text buttonText = CreateText(obj.transform, label, size.HasValue ? 18 : 21, Vector2.zero, rect.sizeDelta - new Vector2(28f, 10f), TextAnchor.MiddleCenter, UITheme.BodyColor);
        UITheme.ApplyButton(image, buttonText, style, true);
        return button;
    }

    private Slider CreatePauseSlider(Transform parent, Vector2 position)
    {
        GameObject sliderObject = new GameObject("PauseMasterVolumeSlider");
        sliderObject.transform.SetParent(parent, false);
        RectTransform rect = sliderObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(220f, 28f);
        rect.anchoredPosition = position;

        Slider slider = sliderObject.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;

        GameObject background = CreatePauseSliderImage("Background", sliderObject.transform, Vector2.zero, Vector2.one, new Color(0.11f, 0.11f, 0.12f, 1f));
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObject.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = new Vector2(8f, 6f);
        fillAreaRect.offsetMax = new Vector2(-8f, -6f);

        GameObject fill = CreatePauseSliderImage("Fill", fillArea.transform, Vector2.zero, Vector2.one, UITheme.GoldColor);
        GameObject handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(sliderObject.transform, false);
        RectTransform handleAreaRect = handleArea.AddComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.offsetMin = new Vector2(10f, 0f);
        handleAreaRect.offsetMax = new Vector2(-10f, 0f);

        GameObject handle = CreatePauseSliderImage("Handle", handleArea.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), UITheme.TitleColor);
        handle.GetComponent<RectTransform>().sizeDelta = new Vector2(22f, 30f);

        slider.targetGraphic = handle.GetComponent<Image>();
        slider.fillRect = fill.GetComponent<RectTransform>();
        slider.handleRect = handle.GetComponent<RectTransform>();
        slider.direction = Slider.Direction.LeftToRight;
        background.GetComponent<Image>().raycastTarget = true;

        return slider;
    }

    private GameObject CreatePauseSliderImage(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Color color)
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

    private void ShowPauseMainPage()
    {
        SetPausePage(pauseMainPage);
    }

    private void ShowPauseSettingsPage()
    {
        if (pauseVolumeSlider != null)
        {
            pauseVolumeSlider.value = AudioListener.volume;
        }

        SetPausePage(pauseSettingsPage);
    }

    private void ShowPauseQuitPage()
    {
        SetPausePage(pauseQuitPage);
    }

    private void SetPausePage(GameObject activePage)
    {
        if (pauseMainPage != null)
        {
            pauseMainPage.SetActive(activePage == pauseMainPage);
        }

        if (pauseSettingsPage != null)
        {
            pauseSettingsPage.SetActive(activePage == pauseSettingsPage);
        }

        if (pauseQuitPage != null)
        {
            pauseQuitPage.SetActive(activePage == pauseQuitPage);
        }
    }

    private void ResumeFromPause()
    {
        pausePanelOpen = false;
        CloseModal(pausePanel, true);
    }

    private void ConfirmPauseQuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void HidePlayerWorldHealthBar()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            return;
        }

        Transform playerBar = player.transform.Find("PlayerHealthBar");
        if (playerBar != null)
        {
            playerBar.gameObject.SetActive(false);
        }
    }

    private TMP_Text CreateTMPText(Transform parent, string content, float fontSize, Vector2 position, Vector2 size, TextAlignmentOptions alignment, Color color, bool bold)
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
        text.alignment = alignment;
        UITheme.ApplyTMPText(text, color, fontSize, bold);
        text.outlineWidth = bold ? 0.08f : 0.04f;
        text.outlineColor = new Color(0f, 0f, 0f, 0.82f);
        return text;
    }

    private TMP_Text CreateHudTMPText(Transform parent, string content, float fontSize, Vector2 position, Vector2 size, TextAlignmentOptions alignment, Color color, bool bold)
    {
        GameObject obj = new GameObject("HudTMPText");
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        TextMeshProUGUI text = obj.AddComponent<TextMeshProUGUI>();
        text.text = content;
        text.alignment = alignment;
        UITheme.ApplyHudTMPText(text, color, fontSize, bold);
        text.outlineWidth = bold ? 0.035f : 0.02f;
        text.outlineColor = new Color(0f, 0f, 0f, 0.68f);
        return text;
    }

    private void ConfigureMainCanvas()
    {
        if (mainCanvas == null)
        {
            return;
        }

        mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        RectTransform rect = mainCanvas.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.localPosition = Vector3.zero;
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;
        }

        CanvasScaler scaler = mainCanvas.GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = mainCanvas.gameObject.AddComponent<CanvasScaler>();
        }

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        if (mainCanvas.GetComponent<GraphicRaycaster>() == null)
        {
            mainCanvas.gameObject.AddComponent<GraphicRaycaster>();
        }
    }

    public void OpenModal(GameObject panel, bool pauseGame)
    {
        if (panel == null)
        {
            return;
        }

        EnsureModalMask();
        if (modalMaskObject != null)
        {
            modalMaskObject.transform.SetAsLastSibling();
            modalMaskObject.SetActive(true);
            UIAnimationUtility.FadeIn(modalMaskGroup, 0.16f);
        }

        panel.transform.SetAsLastSibling();
        panel.SetActive(true);
        UIAnimationUtility.PopIn(panel.GetComponent<RectTransform>(), GetOrAddCanvasGroup(panel));

        if (pauseGame)
        {
            pausedModalCount++;
            Time.timeScale = 0f;
        }
    }

    public void CloseModal(GameObject panel, bool resumeGame)
    {
        if (panel != null)
        {
            UIAnimationUtility.FadeOut(GetOrAddCanvasGroup(panel), 0.14f);
        }

        if (resumeGame && pausedModalCount > 0)
        {
            pausedModalCount--;
        }

        if (pausedModalCount == 0)
        {
            Time.timeScale = 1f;
            if (modalMaskGroup != null)
            {
                UIAnimationUtility.FadeOut(modalMaskGroup, 0.14f);
            }
        }
    }

    private void EnsurePlayerHealthBar()
    {
        if (mainCanvas == null || playerHealthBar != null)
        {
            return;
        }

        Transform existing = mainCanvas.transform.Find("PlayerBufferedHealthBar");
        if (existing != null)
        {
            playerHealthBar = existing.GetComponent<BufferedHealthBar>();
        }

        if (playerHealthBar == null)
        {
            playerHealthBar = BufferedHealthBar.Create(mainCanvas.transform);
        }
    }

    private void EnsureModalMask()
    {
        if (mainCanvas == null || modalMaskObject != null)
        {
            return;
        }

        modalMaskObject = new GameObject("UIModalMask");
        modalMaskObject.transform.SetParent(mainCanvas.transform, false);
        RectTransform rect = modalMaskObject.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = modalMaskObject.AddComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.58f);
        modalMaskGroup = modalMaskObject.AddComponent<CanvasGroup>();
        modalMaskGroup.alpha = 0f;
        modalMaskGroup.blocksRaycasts = false;
        modalMaskGroup.interactable = false;
        modalMaskObject.SetActive(false);
    }

    private CanvasGroup GetOrAddCanvasGroup(GameObject obj)
    {
        CanvasGroup group = obj.GetComponent<CanvasGroup>();
        if (group == null)
        {
            group = obj.AddComponent<CanvasGroup>();
        }

        return group;
    }

    private void UpdateBuildSummary()
    {
        if (buildSummaryText == null || playerStats == null || playerController == null)
        {
            return;
        }

        buildSummaryText.text =
            "Build\n" +
            "ATK  " + playerStats.atk.ToString("0") +
            "   DEF  " + playerStats.def.ToString("0") + "\n" +
            "Crit  " + (playerStats.critChance * 100f).ToString("0") + "% x" + playerStats.critMultiplier.ToString("0.00") + "\n" +
            "Lifesteal  " + (playerStats.lifeStealPercent * 100f).ToString("0") + "%\n" +
            "Burn  " + (playerStats.burnChance * 100f).ToString("0") + "% @" + playerStats.burnDamagePerSecond.ToString("0") + "/s\n" +
            "Shockwave  " + (playerStats.shockwaveChance * 100f).ToString("0") + "%\n" +
            "Build  Crit " + playerStats.critBuildLevel +
            "  Burn " + playerStats.burnBuildLevel +
            "  Lightning " + playerStats.lightningBuildLevel + "\n" +
            "Targets  " + playerController.maxTargetsPerAttack +
            "   Range  " + playerController.attackRange.ToString("0.0");
    }

    private Text CreateText(Transform parent, string content, int fontSize, Vector2 position, Vector2 size, TextAnchor anchor, Color color)
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
        text.font = UITheme.GameFont;
        text.fontSize = fontSize;
        text.text = content;
        text.alignment = anchor;
        text.color = color;
        text.raycastTarget = false;
        UITheme.ApplyText(text);
        return text;
    }
}
