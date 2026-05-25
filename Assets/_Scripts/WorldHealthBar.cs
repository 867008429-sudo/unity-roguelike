using UnityEngine;

public class WorldHealthBar : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 2.5f, 0);
    public float barWidth = 2f;
    public float barHeight = 0.2f;
    public Color fgColor = new Color(0.91f, 0.3f, 0.24f);
    public Color bgColor = new Color(0.2f, 0.2f, 0.2f);
    public bool showName = false;
    public string displayName = "";
    public bool showOnlyWhenDamaged = false;
    public float lerpSpeed = 3f;

    private GameObject barBG;
    private GameObject barFG;
    private TextMesh hpText;
    private TextMesh nameText;
    private float displayedHP = 1f;
    private float maxHP;
    private float currentHP;
    private float damageShowTimer;
    private bool hasBeenDamaged;

    private void Start()
    {
        CreateBar();
        TryAutoBindHealth();
    }

    private void TryAutoBindHealth()
    {
        if (target != null)
        {
            EnemyStats enemyStats = target.GetComponentInParent<EnemyStats>();
            if (enemyStats != null)
            {
                enemyStats.OnHealthChanged.AddListener(hp => SetHealth(hp, enemyStats.maxHP));
                SetInitialHealth(enemyStats.currentHP, enemyStats.maxHP);
                displayName = enemyStats.enemyName;
                if (nameText != null)
                {
                    nameText.text = displayName;
                }

                return;
            }

            PlayerStats playerStats = target.GetComponentInParent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.OnHealthChanged.AddListener(hp => SetHealth(hp, playerStats.maxHP));
                SetInitialHealth(playerStats.currentHP, playerStats.maxHP);
                if (string.IsNullOrEmpty(displayName))
                {
                    displayName = "Player";
                }

                if (nameText != null)
                {
                    nameText.text = displayName;
                }

                return;
            }
        }

        if (target == null)
        {
            EnemyStats enemyStats = FindObjectOfType<EnemyStats>();
            if (enemyStats != null)
            {
                target = enemyStats.transform;
                enemyStats.OnHealthChanged.AddListener(hp => SetHealth(hp, enemyStats.maxHP));
                SetInitialHealth(enemyStats.currentHP, enemyStats.maxHP);
                displayName = enemyStats.enemyName;
                if (nameText != null)
                {
                    nameText.text = displayName;
                }
            }
        }
    }

    private void CreateBar()
    {
        barBG = GameObject.CreatePrimitive(PrimitiveType.Cube);
        barBG.name = "HPBar_BG";
        barBG.transform.SetParent(transform);
        barBG.transform.localPosition = Vector3.zero;
        barBG.transform.localScale = new Vector3(barWidth, barHeight, 0.05f);
        Destroy(barBG.GetComponent<Collider>());
        barBG.GetComponent<Renderer>().material.color = bgColor;

        barFG = GameObject.CreatePrimitive(PrimitiveType.Cube);
        barFG.name = "HPBar_FG";
        barFG.transform.SetParent(barBG.transform);
        barFG.transform.localPosition = new Vector3(0f, 0f, -0.01f);
        barFG.transform.localScale = Vector3.one;
        barFG.transform.localRotation = Quaternion.identity;
        Destroy(barFG.GetComponent<Collider>());
        barFG.GetComponent<Renderer>().material.color = fgColor;

        if (!showOnlyWhenDamaged)
        {
            GameObject textObject = new GameObject("HPText");
            textObject.transform.SetParent(transform);
            textObject.transform.localPosition = new Vector3(0f, barHeight * 2f, 0f);
            textObject.transform.localScale = Vector3.one * 0.15f;
            hpText = textObject.AddComponent<TextMesh>();
            hpText.anchor = TextAnchor.MiddleCenter;
            hpText.alignment = TextAlignment.Center;
            hpText.fontSize = 36;
            hpText.color = Color.white;
        }

        if (showName)
        {
            GameObject nameObject = new GameObject("NameText");
            nameObject.transform.SetParent(transform);
            nameObject.transform.localPosition = new Vector3(0f, barHeight * 5f, 0f);
            nameObject.transform.localScale = Vector3.one * 0.12f;
            nameText = nameObject.AddComponent<TextMesh>();
            nameText.anchor = TextAnchor.MiddleCenter;
            nameText.alignment = TextAlignment.Center;
            nameText.fontSize = 28;
            nameText.color = Color.white;
            nameText.text = displayName;
        }

        if (showOnlyWhenDamaged && barBG != null)
        {
            barBG.SetActive(false);
        }
    }

    public void Initialize(float maxH, string name)
    {
        maxHP = maxH;
        currentHP = maxH;
        displayName = name;
        displayedHP = 1f;
        if (nameText != null)
        {
            nameText.text = name;
        }
    }

    public void SetHealth(float cur, float max)
    {
        currentHP = cur;
        maxHP = max;
        hasBeenDamaged = true;
        damageShowTimer = 3f;
        if (showOnlyWhenDamaged && barBG != null)
        {
            barBG.SetActive(true);
        }
    }

    private void SetInitialHealth(float cur, float max)
    {
        currentHP = cur;
        maxHP = max;
        displayedHP = maxHP > 0f ? currentHP / maxHP : 0f;
        hasBeenDamaged = false;
        damageShowTimer = 0f;
        if (showOnlyWhenDamaged && barBG != null)
        {
            barBG.SetActive(false);
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = target.position + offset;
        if (Camera.main != null)
        {
            transform.forward = Camera.main.transform.forward;
        }

        float targetHP = maxHP > 0f ? currentHP / maxHP : 0f;
        displayedHP = Mathf.Lerp(displayedHP, targetHP, lerpSpeed * Time.deltaTime);
        if (barFG != null)
        {
            float width = Mathf.Clamp01(displayedHP);
            barFG.transform.localScale = new Vector3(width, 1f, 1f);
            barFG.transform.localPosition = new Vector3(-0.5f + width * 0.5f, 0f, -0.01f);
        }

        if (hpText != null)
        {
            hpText.text = "HP: " + Mathf.CeilToInt(currentHP) + "/" + Mathf.CeilToInt(maxHP);
        }

        if (showOnlyWhenDamaged && hasBeenDamaged)
        {
            damageShowTimer -= Time.deltaTime;
            if (damageShowTimer <= 0f && barBG != null)
            {
                barBG.SetActive(false);
                hasBeenDamaged = false;
            }
        }
    }

    private void OnDestroy()
    {
        if (barBG != null)
        {
            Destroy(barBG);
        }

        if (barFG != null)
        {
            Destroy(barFG);
        }
    }
}
