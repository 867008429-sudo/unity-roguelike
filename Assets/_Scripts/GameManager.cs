using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameManager>();
            }

            return instance;
        }
        private set
        {
            instance = value;
        }
    }

    public Transform playerTransform;
    public PlayerStats playerStats;
    public WaveManager waveManager;
    public UIManager uiManager;
    public CameraFollow cameraFollow;
    public bool isGameOver;
    public int totalKills;

    public UnityEvent<int> OnKillCountChanged = new UnityEvent<int>();
    public UnityEvent OnGameOver = new UnityEvent();

    private int comboCount;
    private float lastKillTime = -999f;
    private GameObject comboTextObject;
    private TextMesh comboTextMesh;
    private float comboDisplayTimer;
    private bool comboVisible;

    private void Awake()
    {
        Time.timeScale = 1f;
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                playerStats = player.GetComponent<PlayerStats>();
            }
        }

        if (waveManager == null)
        {
            waveManager = FindObjectOfType<WaveManager>();
        }

        if (uiManager == null)
        {
            uiManager = FindObjectOfType<UIManager>();
        }

        if (cameraFollow == null && Camera.main != null)
        {
            cameraFollow = Camera.main.GetComponent<CameraFollow>();
        }

        if (playerStats != null)
        {
            playerStats.OnDeath.AddListener(HandlePlayerDeath);
        }

        CreateComboText();
    }

    public void AddKill()
    {
        totalKills++;
        OnKillCountChanged?.Invoke(totalKills);

        float now = Time.time;
        comboCount = now - lastKillTime <= GameConfig.ComboTimeWindow ? comboCount + 1 : 1;
        lastKillTime = now;

        if (comboCount >= 2)
        {
            ShowCombo();
        }
    }

    public void HandlePlayerDeath()
    {
        if (isGameOver)
        {
            return;
        }

        isGameOver = true;
        Time.timeScale = 1f;
        OnGameOver?.Invoke();

        int levelReached = playerStats != null ? playerStats.level : 1;
        int goldCollected = playerStats != null ? playerStats.gold : 0;
        int soulReward = MetaProgression.CalculateSoulReward(totalKills, levelReached);
        MetaProgression.AddSoulStones(soulReward);

        if (uiManager != null)
        {
            uiManager.ShowDeathPanel(
                totalKills,
                goldCollected,
                levelReached,
                soulReward,
                MetaProgression.SoulStones);
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    private void Update()
    {
        if (isGameOver && Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }

        if (!comboVisible)
        {
            return;
        }

        comboDisplayTimer -= Time.deltaTime;

        if (comboTextObject != null && Camera.main != null)
        {
            Vector3 position = Camera.main.transform.position + Camera.main.transform.forward * 10f;
            comboTextObject.transform.position = position;
            comboTextObject.transform.forward = Camera.main.transform.forward;
        }

        if (comboDisplayTimer <= 0.5f && comboTextMesh != null)
        {
            Color color = comboTextMesh.color;
            color.a = Mathf.Max(0f, comboDisplayTimer / 0.5f);
            comboTextMesh.color = color;
        }

        if (comboTextObject != null && comboTextObject.transform.localScale.x > 1.01f)
        {
            comboTextObject.transform.localScale = Vector3.Lerp(comboTextObject.transform.localScale, Vector3.one, 3f * Time.deltaTime);
        }

        if (comboDisplayTimer <= 0f)
        {
            comboVisible = false;
            comboTextObject.SetActive(false);
        }
    }

    private void CreateComboText()
    {
        comboTextObject = new GameObject("ComboTextDisplay");
        comboTextObject.transform.SetParent(transform);
        comboTextMesh = comboTextObject.AddComponent<TextMesh>();
        comboTextMesh.anchor = TextAnchor.MiddleCenter;
        comboTextMesh.alignment = TextAlignment.Center;
        comboTextMesh.fontSize = 48;
        comboTextMesh.characterSize = 0.08f;
        comboTextMesh.color = new Color(1f, 0.9f, 0.2f, 0f);
        comboTextMesh.text = string.Empty;
        comboTextObject.SetActive(false);
    }

    private void ShowCombo()
    {
        comboTextObject.SetActive(true);
        comboTextObject.transform.localScale = Vector3.one * 1.5f;
        comboTextMesh.text = "x" + comboCount + " KILL!";
        comboTextMesh.color = new Color(1f, 0.9f, 0.2f, 1f);
        comboDisplayTimer = 1.5f;
        comboVisible = true;
    }
}
