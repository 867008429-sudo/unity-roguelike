using UnityEngine;
using UnityEngine.Events;

public class PlayerStats : MonoBehaviour
{
    public float maxHP = GameConfig.PlayerMaxHP;
    public float currentHP;
    public float atk = GameConfig.PlayerATK;
    public float def = GameConfig.PlayerDEF;
    public float critChance = 0.1f;
    public float critMultiplier = 1.75f;
    public float lifeStealPercent;
    public float shockwaveChance;
    public float burnChance;
    public float burnDamagePerSecond;
    public float burnDuration = 3f;
    public int critBuildLevel;
    public int burnBuildLevel;
    public int lightningBuildLevel;
    public int level = 1;
    public int currentXP;
    public int gold;
    public int xpToNextLevel;

    public UnityEvent<float> OnHealthChanged = new UnityEvent<float>();
    public UnityEvent<int> OnXPChanged = new UnityEvent<int>();
    public UnityEvent<int> OnGoldChanged = new UnityEvent<int>();
    public UnityEvent<int> OnLevelUp = new UnityEvent<int>();
    public UnityEvent OnDamaged = new UnityEvent();
    public UnityEvent OnDeath = new UnityEvent();

    private bool isDead;

    private void Awake()
    {
        maxHP += MetaProgression.MaxHpBonus;
        atk += MetaProgression.AttackBonus;
        def += MetaProgression.DefenseBonus;
        currentHP = maxHP;
        xpToNextLevel = GameConfig.GetXPRequired(level);
    }

    private void Start()
    {
        if (currentHP > 0f)
        {
            PlayerController controller = GetComponent<PlayerController>();
            if (controller != null && !controller.enabled)
            {
                controller.enabled = true;
            }
        }

        OnHealthChanged?.Invoke(currentHP);
        OnXPChanged?.Invoke(currentXP);
        OnGoldChanged?.Invoke(gold);
    }

    public void TakeDamage(float damage, Vector3 attackerPosition)
    {
        if (isDead)
        {
            return;
        }

        PlayerController controller = GetComponent<PlayerController>();
        if (controller != null && controller.IsDodging())
        {
            controller.TryPerfectDodge(attackerPosition);
            return;
        }

        float finalDamage = Mathf.Max(damage - def, GameConfig.MinDamage);
        currentHP = Mathf.Max(currentHP - finalDamage, 0f);
        OnHealthChanged?.Invoke(currentHP);
        OnDamaged?.Invoke();

        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.TriggerHitEffect(gameObject, finalDamage, attackerPosition);
        }

        if (currentHP <= 0f)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (isDead)
        {
            return;
        }

        float before = currentHP;
        currentHP = Mathf.Min(currentHP + amount, maxHP);
        if (currentHP > before)
        {
            OnHealthChanged?.Invoke(currentHP);
        }
    }

    public void IncreaseMaxHealth(float amount, bool healToFull = false)
    {
        if (isDead)
        {
            return;
        }

        maxHP += amount;
        currentHP = healToFull ? maxHP : Mathf.Min(currentHP + amount, maxHP);
        OnHealthChanged?.Invoke(currentHP);
    }

    public void IncreaseAttack(float amount)
    {
        if (!isDead)
        {
            atk += amount;
        }
    }

    public void IncreaseDefense(float amount)
    {
        if (!isDead)
        {
            def += amount;
        }
    }

    public void IncreaseCritChance(float amount)
    {
        if (!isDead)
        {
            critChance = Mathf.Clamp01(critChance + amount);
        }
    }

    public void IncreaseCritMultiplier(float amount)
    {
        if (!isDead)
        {
            critMultiplier = Mathf.Clamp(critMultiplier + amount, 1.25f, 4f);
        }
    }

    public void IncreaseCritBuild(int amount)
    {
        if (!isDead)
        {
            critBuildLevel = Mathf.Clamp(critBuildLevel + amount, 0, 6);
        }
    }

    public void IncreaseLifeSteal(float amount)
    {
        if (!isDead)
        {
            lifeStealPercent = Mathf.Clamp01(lifeStealPercent + amount);
        }
    }

    public void IncreaseShockwaveChance(float amount)
    {
        if (!isDead)
        {
            shockwaveChance = Mathf.Clamp01(shockwaveChance + amount);
        }
    }

    public void IncreaseLightningBuild(int amount)
    {
        if (!isDead)
        {
            lightningBuildLevel = Mathf.Clamp(lightningBuildLevel + amount, 0, 6);
        }
    }

    public void IncreaseBurnChance(float amount, float dpsIncrease)
    {
        if (!isDead)
        {
            burnChance = Mathf.Clamp01(burnChance + amount);
            burnDamagePerSecond += dpsIncrease;
        }
    }

    public void IncreaseBurnBuild(int amount)
    {
        if (!isDead)
        {
            burnBuildLevel = Mathf.Clamp(burnBuildLevel + amount, 0, 6);
        }
    }

    public void AddXP(int amount)
    {
        if (isDead || level >= GameConfig.MaxLevel)
        {
            return;
        }

        currentXP += amount;
        int safety = 0;
        while (currentXP >= xpToNextLevel && level < GameConfig.MaxLevel)
        {
            if (xpToNextLevel <= 0 || safety++ > GameConfig.MaxLevel + 1)
            {
                xpToNextLevel = Mathf.Max(1, GameConfig.GetXPRequired(level));
                break;
            }

            currentXP -= xpToNextLevel;
            LevelUp();
            xpToNextLevel = GameConfig.GetXPRequired(level);
        }

        if (level >= GameConfig.MaxLevel)
        {
            currentXP = 0;
        }

        OnXPChanged?.Invoke(currentXP);
    }

    public void AddGold(int amount)
    {
        gold += amount;
        OnGoldChanged?.Invoke(gold);
    }

    private void LevelUp()
    {
        float healthRatio = maxHP > 0f ? currentHP / maxHP : 1f;
        level++;
        maxHP += GameConfig.HPPerLevel;
        atk += GameConfig.ATKPerLevel;
        def += GameConfig.DEFPerLevel;
        currentHP = Mathf.Clamp(maxHP * healthRatio, 1f, maxHP);

        ParticleEffects.Instance.PlayLevelUpAura(transform);
        OnLevelUp?.Invoke(level);
        OnHealthChanged?.Invoke(currentHP);
    }

    private void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        OnDeath?.Invoke();

        PlayerController controller = GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.enabled = false;
        }
    }
}
