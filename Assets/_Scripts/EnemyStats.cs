using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class EnemyStats : MonoBehaviour
{
    public enum EnemyType
    {
        Skeleton,
        Slime
    }

    public string enemyName = "Enemy";
    public EnemyType enemyType;
    public float maxHP = 60f;
    public float currentHP;
    public float atk = 10f;
    public float def = 2f;
    public int xpReward = 20;
    public int goldMin = 5;
    public int goldMax = 15;
    public float potionDropRate = 0.2f;
    public GameObject goldPickupPrefab;
    public GameObject potionPickupPrefab;
    public bool isElite;
    public bool isBoss;

    public UnityEvent OnDeath = new UnityEvent();
    public UnityEvent<float> OnHealthChanged = new UnityEvent<float>();
    public UnityEvent OnDamaged = new UnityEvent();

    private bool isDead;
    private Renderer[] renderers;
    private EnemyAI enemyAI;
    private Coroutine burnRoutine;
    private float burnExpireTime;

    private void Awake()
    {
        currentHP = maxHP;
        renderers = GetComponentsInChildren<Renderer>();
        enemyAI = GetComponent<EnemyAI>();
    }

    public float TakeDamage(float rawDamage, Vector3 attackerPosition, bool critical = false)
    {
        if (isDead)
        {
            return 0f;
        }

        float finalDamage = CombatManager.CalculateDamage(rawDamage, def);
        currentHP = Mathf.Max(currentHP - finalDamage, 0f);
        OnHealthChanged?.Invoke(currentHP);
        OnDamaged?.Invoke();

        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.TriggerHitEffect(gameObject, finalDamage, attackerPosition, critical);
        }

        if (currentHP <= 0f)
        {
            Die();
        }

        return finalDamage;
    }

    public void ApplyBurn(float duration, float damagePerSecond, Vector3 sourcePosition)
    {
        if (isDead || damagePerSecond <= 0f || duration <= 0f)
        {
            return;
        }

        if (burnRoutine != null)
        {
            StopCoroutine(burnRoutine);
        }

        burnExpireTime = Time.time + duration;
        burnRoutine = StartCoroutine(BurnRoutine(duration, damagePerSecond, sourcePosition));
    }

    private void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        OnDeath?.Invoke();

        if (GameManager.Instance != null && GameManager.Instance.playerStats != null)
        {
            PlayerStats playerStats = GameManager.Instance.playerStats;
            playerStats.AddXP(xpReward);

            int goldAmount = Random.Range(goldMin, goldMax + 1);
            SpawnGold(goldAmount);

            if (Random.value <= potionDropRate)
            {
                SpawnPotion();
            }

            GameManager.Instance.AddKill();
        }

        if (enemyAI != null)
        {
            enemyAI.enabled = false;
        }

        Color deathColor = enemyType == EnemyType.Skeleton
            ? new Color(0.9f, 0.9f, 0.85f, 1f)
            : new Color(0.2f, 0.9f, 0.3f, 1f);

        if (isBoss)
        {
            deathColor = new Color(0.75f, 0.1f, 0.95f, 1f);
        }
        else if (isElite)
        {
            deathColor = new Color(1f, 0.65f, 0.15f, 1f);
        }

        if (enemyType == EnemyType.Slime)
        {
            DisableDeathCollision();
            StartCoroutine(DelayedSlimeDeathVFX(deathColor));
        }
        else
        {
            GameFeelVFXManager.Instance.PlayEnemyDeathFracture(gameObject, deathColor);
            GameFeelVFXManager.Instance.HideLiveEnemy(gameObject);
            StartCoroutine(DestroyAfterFracture());
        }
    }

    private void SpawnGold(int amount)
    {
        Vector3 position = transform.position + new Vector3(Random.Range(-1.5f, 1.5f), 0.1f, Random.Range(-1.5f, 1.5f));
        GameObject goldObject = goldPickupPrefab != null ? Instantiate(goldPickupPrefab, position, Quaternion.identity) : CreateDefaultGold(position);
        GoldPickup pickup = goldObject.GetComponent<GoldPickup>();
        if (pickup == null)
        {
            pickup = goldObject.AddComponent<GoldPickup>();
        }

        pickup.goldAmount = amount;
    }

    private void SpawnPotion()
    {
        Vector3 position = transform.position + new Vector3(Random.Range(-1.5f, 1.5f), 0.1f, Random.Range(-1.5f, 1.5f));
        GameObject potionObject = potionPickupPrefab != null ? Instantiate(potionPickupPrefab, position, Quaternion.identity) : CreateDefaultPotion(position);
        if (potionObject.GetComponent<HealthPotion>() == null)
        {
            potionObject.AddComponent<HealthPotion>();
        }
    }

    private GameObject CreateDefaultGold(Vector3 position)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        obj.name = "Gold";
        obj.transform.position = position + Vector3.up * 0.18f;
        obj.transform.localScale = new Vector3(0.46f, 0.06f, 0.46f);
        obj.GetComponent<Renderer>().material.color = new Color(1f, 0.76f, 0.12f, 1f);
        Destroy(obj.GetComponent<Collider>());
        return obj;
    }

    private GameObject CreateDefaultPotion(Vector3 position)
    {
        GameObject obj = new GameObject("Potion");
        obj.name = "Potion";
        obj.transform.position = position + Vector3.up * 0.12f;
        return obj;
    }

    private System.Collections.IEnumerator DestroyAfterFracture()
    {
        yield return new WaitForSeconds(1.8f);
        Destroy(gameObject);
    }

    private System.Collections.IEnumerator DelayedSlimeDeathVFX(Color deathColor)
    {
        CharacterAnimationController animationController = GetComponent<CharacterAnimationController>();
        float delay = animationController != null ? Mathf.Clamp(animationController.deathDuration * 0.86f, 0.45f, 1.25f) : 0.75f;
        yield return new WaitForSeconds(delay);

        if (this == null || gameObject == null)
        {
            yield break;
        }

        GameFeelVFXManager.Instance.PlayEnemyDeathFracture(gameObject, deathColor);
        GameFeelVFXManager.Instance.HideLiveEnemy(gameObject);
        StartCoroutine(DestroyAfterFracture());
    }

    private void DisableDeathCollision()
    {
        foreach (Collider collider in GetComponentsInChildren<Collider>())
        {
            if (collider != null)
            {
                collider.enabled = false;
            }
        }

        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.enabled = false;
        }
    }

    private System.Collections.IEnumerator DeathAnimation()
    {
        float duration = 0.5f;

        if (enemyType == EnemyType.Slime)
        {
            float elapsed = 0f;
            Vector3 originalScale = transform.localScale;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, elapsed / duration);
                yield return null;
            }
        }
        else
        {
            float elapsed = 0f;
            Vector3 startPosition = transform.position;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                transform.position = startPosition + Vector3.down * progress;

                foreach (Renderer renderer in renderers)
                {
                    if (renderer != null)
                    {
                        Color color = renderer.material.color;
                        color.a = 1f - progress;
                        renderer.material.color = color;
                    }
                }

                yield return null;
            }
        }

        Destroy(gameObject);
    }

    public bool IsDead()
    {
        return isDead;
    }

    public bool IsBurning()
    {
        return !isDead && burnRoutine != null && Time.time < burnExpireTime;
    }

    private System.Collections.IEnumerator BurnRoutine(float duration, float damagePerSecond, Vector3 sourcePosition)
    {
        float elapsed = 0f;
        float tickTimer = 0f;
        while (!isDead && elapsed < duration)
        {
            elapsed += Time.deltaTime;
            tickTimer += Time.deltaTime;
            if (tickTimer >= 0.5f)
            {
                tickTimer = 0f;
                float tickDamage = damagePerSecond * 0.5f;
                currentHP = Mathf.Max(currentHP - tickDamage, 0f);
                OnHealthChanged?.Invoke(currentHP);
                ParticleEffects.Instance.PlayHitParticles(transform.position + Vector3.up * 0.5f, new Color(1f, 0.45f, 0.1f, 1f));
                if (CombatManager.Instance != null)
                {
                    CombatManager.Instance.SpawnDamagePopup(transform.position + Vector3.up * 0.5f, Mathf.CeilToInt(tickDamage));
                }

                if (currentHP <= 0f)
                {
                    Die();
                    yield break;
                }
            }

            yield return null;
        }

        burnRoutine = null;
    }
}
