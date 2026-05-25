using UnityEngine;

public static class GameConfig
{
    public const float PlayerMaxHP = 100f;
    public const float PlayerATK = 15f;
    public const float PlayerDEF = 5f;
    public const float PlayerMoveSpeed = 5f;
    public const float PlayerAttackRange = 1.8f;
    public const float PlayerAttackAngle = 135f;
    public const float PlayerAttackCooldown = 0.48f;

    public const float HPPerLevel = 15f;
    public const float ATKPerLevel = 3f;
    public const float DEFPerLevel = 1f;
    public const int MaxLevel = 10;
    public const float XPFormulaA = 25f;
    public const float XPFormulaB = 5f;
    public const float XPFormulaC = 2f;

    public const float SkeletonMaxHP = 60f;
    public const float SkeletonATK = 10f;
    public const float SkeletonDEF = 2f;
    public const float SkeletonMoveSpeed = 3f;
    public const float SkeletonPatrolSpeed = 1.5f;
    public const float SkeletonAttackRange = 2f;
    public const float SkeletonAlertRange = 8f;
    public const float SkeletonChaseRange = 15f;
    public const float SkeletonAttackCooldown = 1.2f;
    public const float SkeletonPatrolDistance = 4f;
    public const float SkeletonPatrolPause = 1f;
    public const float SkeletonDetectionInterval = 0.5f;
    public const int SkeletonXPReward = 20;
    public const int SkeletonGoldMin = 5;
    public const int SkeletonGoldMax = 15;
    public const float SkeletonPotionRate = 0.2f;

    public const float SlimeMaxHP = 30f;
    public const float SlimeATK = 6f;
    public const float SlimeDEF = 0f;
    public const float SlimeMoveSpeed = 4f;
    public const float SlimePatrolSpeed = 2f;
    public const float SlimeAttackRange = 1f;
    public const float SlimeAlertRange = 6f;
    public const float SlimeChaseRange = 12f;
    public const float SlimeAttackCooldown = 0.8f;
    public const float SlimePatrolRadius = 3f;
    public const float SlimeDirChangeInterval = 3f;
    public const float SlimePatrolPause = 0.5f;
    public const float SlimeDetectionInterval = 0.5f;
    public const int SlimeXPReward = 10;
    public const int SlimeGoldMin = 2;
    public const int SlimeGoldMax = 8;
    public const float SlimePotionRate = 0.1f;

    public const int MinDamage = 1;
    public const float PlayerKnockback = 0.1f;
    public const float EnemyKnockback = 0.28f;
    public const float HitFlashDuration = 0.15f;
    public const float ScreenShakeMagnitude = 0.05f;
    public const float ScreenShakeDuration = 0.1f;

    public const float DamageFloatHeight = 1.5f;
    public const float DamageNumberDuration = 1f;
    public const int DamageNumberStartSize = 24;
    public const int DamageNumberEndSize = 12;
    public const float PotionHealAmount = 30f;

    public const float GoldPickupRange = 1.5f;
    public const float PotionPickupRange = 0.8f;
    public const float GoldFlySpeed = 8f;
    public const float GoldRotationSpeed = 90f;
    public const float PotionFloatAmplitude = 0.1f;
    public const float PotionFloatPeriod = 2f;
    public const float ItemLifetime = 60f;
    public const float ItemPickupCooldown = 0.3f;

    public const float SceneSize = 40f;
    public const float WallHeight = 4f;
    public const float WallThickness = 0.5f;

    public static readonly Vector3 CameraOffset = new Vector3(0f, 15f, -8f);
    public static readonly Vector3 CameraRotation = new Vector3(60f, 0f, 0f);
    public const float CameraFOV = 60f;
    public const float CameraFollowSpeed = 5f;
    public const float CameraNearClip = 0.3f;
    public const float CameraFarClip = 100f;

    public const int Wave1SkeletonCount = 3;
    public const int Wave1SlimeCount = 4;
    public const int Wave2SkeletonCount = 4;
    public const int Wave2SlimeCount = 5;
    public const int SkeletonIncrement = 1;
    public const int SlimeIncrement = 1;
    public const float WaveHPScaling = 0.06f;
    public const float WaveATKScaling = 0.035f;
    public const float WaveSpawnDelay = 1.6f;

    public const float WaveClearHealPercent = 0.2f;
    public const int WaveClearGoldBonusBase = 12;
    public const int WaveClearGoldBonusPerWave = 4;
    public const float EliteHealthMultiplier = 1.8f;
    public const float EliteAttackMultiplier = 1.4f;
    public const float EliteScaleMultiplier = 1.2f;
    public const float BossHealthMultiplier = 4f;
    public const float BossAttackMultiplier = 1.9f;
    public const float BossScaleMultiplier = 1.7f;
    public const int BossGoldBonus = 45;
    public const int BossXPBonus = 80;

    public const float DodgeDistance = 5f;
    public const float DodgeDuration = 0.2f;
    public const float DodgeCooldown = 2f;
    public const float ComboTimeWindow = 2f;

    public static int GetXPRequired(int level)
    {
        return Mathf.FloorToInt(XPFormulaA + XPFormulaB * level + XPFormulaC * Mathf.Pow(level, 1.5f));
    }
}
