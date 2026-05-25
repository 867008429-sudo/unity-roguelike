using UnityEngine;

public static class MetaProgression
{
    private const string SoulStonesKey = "MetaSoulStones";
    private const string MaxHpLevelKey = "MetaMaxHpLevel";
    private const string AttackLevelKey = "MetaAttackLevel";
    private const string DefenseLevelKey = "MetaDefenseLevel";

    public const int MaxUpgradeLevel = 5;

    public static int SoulStones => PlayerPrefs.GetInt(SoulStonesKey, 0);
    public static int MaxHpLevel => PlayerPrefs.GetInt(MaxHpLevelKey, 0);
    public static int AttackLevel => PlayerPrefs.GetInt(AttackLevelKey, 0);
    public static int DefenseLevel => PlayerPrefs.GetInt(DefenseLevelKey, 0);

    public static float MaxHpBonus => MaxHpLevel * 10f;
    public static float AttackBonus => AttackLevel * 1.5f;
    public static float DefenseBonus => DefenseLevel * 0.5f;

    public static int CalculateSoulReward(int kills, int levelReached)
    {
        if (kills <= 0)
        {
            return 0;
        }

        return Mathf.Max(1, kills / 2 + Mathf.Max(0, levelReached - 1));
    }

    public static void AddSoulStones(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        PlayerPrefs.SetInt(SoulStonesKey, SoulStones + amount);
        PlayerPrefs.Save();
    }

    public static int GetCost(int currentLevel)
    {
        return 3 + currentLevel * 3;
    }

    public static bool TryUpgradeMaxHp()
    {
        return TryUpgrade(MaxHpLevelKey);
    }

    public static bool TryUpgradeAttack()
    {
        return TryUpgrade(AttackLevelKey);
    }

    public static bool TryUpgradeDefense()
    {
        return TryUpgrade(DefenseLevelKey);
    }

    private static bool TryUpgrade(string levelKey)
    {
        int currentLevel = PlayerPrefs.GetInt(levelKey, 0);
        if (currentLevel >= MaxUpgradeLevel)
        {
            return false;
        }

        int cost = GetCost(currentLevel);
        if (SoulStones < cost)
        {
            return false;
        }

        PlayerPrefs.SetInt(SoulStonesKey, SoulStones - cost);
        PlayerPrefs.SetInt(levelKey, currentLevel + 1);
        PlayerPrefs.Save();
        return true;
    }
}
