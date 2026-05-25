using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = GameConfig.PlayerMoveSpeed;
    public float attackCooldown = GameConfig.PlayerAttackCooldown;
    public float attackRange = GameConfig.PlayerAttackRange;
    public float attackAngle = GameConfig.PlayerAttackAngle;
    public float dodgeCooldown = GameConfig.DodgeCooldown;
    public float dodgeDistance = GameConfig.DodgeDistance;
    public float dodgeDuration = GameConfig.DodgeDuration;
    public int maxTargetsPerAttack = 1;
    public float attackDamageMultiplier = 1f;
    public float comboResetWindow = 0.9f;
    public float attackLungeDistance = 0.85f;
    public float attackWindup = 0.08f;
    public float attackRecoverTime = 0.12f;
    public bool qaInputLogging;

    private CharacterController controller;
    private Vector3 facingDirection = Vector3.forward;
    private float lastAttackTime = -999f;
    private float lastDodgeTime = -999f;
    private bool isDodging;
    private float dodgeEndTime;
    private Vector3 dodgeDirection;
    private PlayerStats stats;
    private TrailRenderer trailRenderer;
    private Collider[] playerColliders;
    private CharacterAnimationController animationController;
    private int comboStep;
    private float lastComboTime = -999f;
    private bool isAttacking;
    private Transform visualRoot;
    private Vector3 visualStartLocalPosition;
    private Quaternion visualStartLocalRotation;
    private Vector3 visualStartLocalScale;
    private float perfectDodgeWindowEndTime;
    private bool perfectDodgeAwarded;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        stats = GetComponent<PlayerStats>();
        trailRenderer = GetComponent<TrailRenderer>();
        EnsureAnimationDriver();
        animationController = GetComponent<CharacterAnimationController>();

        if (trailRenderer == null)
        {
            trailRenderer = gameObject.AddComponent<TrailRenderer>();
            trailRenderer.time = 0.15f;
            trailRenderer.startWidth = 0.4f;
            trailRenderer.endWidth = 0f;
            trailRenderer.material = new Material(Shader.Find("Sprites/Default"));
            trailRenderer.startColor = new Color(0.2f, 0.5f, 1f, 0.8f);
            trailRenderer.endColor = new Color(0.2f, 0.5f, 1f, 0f);
            trailRenderer.enabled = false;
        }

        playerColliders = GetComponentsInChildren<Collider>();
    }

    private void Start()
    {
        CacheVisualRoot();
    }

    private void Update()
    {
        if (qaInputLogging && Time.frameCount % 30 == 0)
        {
            Debug.Log("[QA Input] Move Input: " + Input.GetAxis("Horizontal").ToString("0.00") + ", " + Input.GetAxis("Vertical").ToString("0.00"));
        }

        if (GameManager.Instance != null && GameManager.Instance.isGameOver)
        {
            return;
        }

        if (isDodging)
        {
            if (Time.time >= dodgeEndTime)
            {
                EndDodge();
            }
            else
            {
                controller.Move(dodgeDirection * (dodgeDistance / dodgeDuration) * Time.deltaTime);
                return;
            }
        }

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 moveDir = new Vector3(h, 0f, v);
        if (moveDir.magnitude > 1f)
        {
            moveDir.Normalize();
        }

        Vector3 velocity = moveDir * moveSpeed;
        velocity.y = -9.81f;
        controller.Move(velocity * Time.deltaTime);

        if (!isAttacking && moveDir.magnitude > 0.01f)
        {
            SetFacingDirection(moveDir);
        }

        bool attackInput = Input.GetKeyDown(KeyCode.J) || Input.GetMouseButtonDown(0);
        if (attackInput && !isAttacking && Time.time - lastAttackTime >= attackCooldown)
        {
            lastAttackTime = Time.time;
            Vector3 aimDirection = ResolveAttackDirection(Input.GetMouseButtonDown(0));
            StartCoroutine(AttackRoutine(aimDirection));
        }

        if (Input.GetKeyDown(KeyCode.Space) && Time.time - lastDodgeTime >= dodgeCooldown && !isDodging)
        {
            StartDodge();
        }
    }

    private void StartDodge()
    {
        isDodging = true;
        lastDodgeTime = Time.time;
        dodgeEndTime = Time.time + dodgeDuration;
        perfectDodgeWindowEndTime = Time.time + Mathf.Min(0.22f, dodgeDuration + 0.05f);
        perfectDodgeAwarded = false;
        dodgeDirection = facingDirection.magnitude > 0.01f ? facingDirection : transform.forward;

        if (trailRenderer != null)
        {
            trailRenderer.time = Mathf.Max(trailRenderer.time, 0.2f);
            trailRenderer.startWidth = Mathf.Max(trailRenderer.startWidth, 0.5f);
            trailRenderer.enabled = true;
        }

        if (VisualEffectsManager.Instance != null)
        {
            VisualEffectsManager.Instance.PlayGroundPulse(transform.position + Vector3.up * 0.05f, new Color(0.28f, 0.72f, 1f, 0.72f), 1.05f, 0.18f);
        }

        foreach (Collider col in playerColliders)
        {
            if (col != null && col != controller)
            {
                col.enabled = false;
            }
        }
    }

    private void EndDodge()
    {
        isDodging = false;

        if (trailRenderer != null)
        {
            trailRenderer.enabled = false;
        }

        foreach (Collider col in playerColliders)
        {
            if (col != null && col != controller)
            {
                col.enabled = true;
            }
        }
    }

    public bool IsDodging()
    {
        return isDodging;
    }

    public bool IsAttacking()
    {
        return isAttacking;
    }

    public bool TryPerfectDodge(Vector3 dangerPosition)
    {
        if (!isDodging || perfectDodgeAwarded || Time.time > perfectDodgeWindowEndTime)
        {
            return false;
        }

        perfectDodgeAwarded = true;
        lastAttackTime = Mathf.Min(lastAttackTime, Time.time - attackCooldown + 0.08f);
        lastDodgeTime = Mathf.Min(lastDodgeTime, Time.time - dodgeCooldown + 0.18f);

        Vector3 feedbackPosition = transform.position + Vector3.up * 0.35f;
        if (VisualEffectsManager.Instance != null)
        {
            VisualEffectsManager.Instance.PlayHitBurst(feedbackPosition, new Color(0.35f, 0.9f, 1f, 1f));
            VisualEffectsManager.Instance.PlayGroundPulse(transform.position, new Color(0.35f, 0.85f, 1f, 0.78f), 1.45f, 0.24f);
        }

        DamageTextPool.Instance.ShowText(transform.position, "完美闪避", new Color(0.35f, 0.9f, 1f, 1f), true);
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.TriggerPlayerAttackImpact(dangerPosition, false, false, 2);
        }

        return true;
    }

    public void MultiplyAttackCooldown(float factor)
    {
        attackCooldown = Mathf.Clamp(attackCooldown * factor, 0.18f, 3f);
    }

    public void IncreaseAttackRange(float amount)
    {
        attackRange = Mathf.Clamp(attackRange + amount, 1f, 4f);
    }

    public void IncreaseMoveSpeed(float amount)
    {
        moveSpeed = Mathf.Clamp(moveSpeed + amount, 3f, 12f);
    }

    public void ReduceDodgeCooldown(float amount)
    {
        dodgeCooldown = Mathf.Clamp(dodgeCooldown - amount, 0.4f, 5f);
    }

    public void IncreaseMaxTargets(int amount)
    {
        maxTargetsPerAttack = Mathf.Clamp(maxTargetsPerAttack + amount, 1, 8);
    }

    public void IncreaseDamageMultiplier(float amount)
    {
        attackDamageMultiplier = Mathf.Clamp(attackDamageMultiplier + amount, 0.5f, 3f);
    }

    private System.Collections.IEnumerator AttackRoutine(Vector3 aimDirection)
    {
        isAttacking = true;
        SetFacingDirection(aimDirection);
        CacheVisualRoot();

        if (Time.time - lastComboTime > comboResetWindow)
        {
            comboStep = 0;
        }

        comboStep = (comboStep % 3) + 1;
        lastComboTime = Time.time;
        if (animationController != null)
        {
            animationController.PlayAttack(facingDirection, comboStep);
        }

        if (comboStep == 3 && trailRenderer != null)
        {
            trailRenderer.time = Mathf.Max(trailRenderer.time, 0.22f);
            trailRenderer.startWidth = Mathf.Max(trailRenderer.startWidth, 0.55f);
            trailRenderer.enabled = true;
        }

        float windup = Mathf.Max(0.02f, attackWindup * (comboStep == 3 ? 1.35f : comboStep == 2 ? 1.1f : 0.92f));
        float elapsed = 0f;
        while (elapsed < windup)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        PerformAttackHit();

        float recover = Mathf.Max(0.04f, attackRecoverTime * (comboStep == 3 ? 1.18f : comboStep == 2 ? 1.05f : 0.9f));
        elapsed = 0f;
        while (elapsed < recover)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (!isDodging && trailRenderer != null && comboStep == 3)
        {
            trailRenderer.enabled = false;
        }

        isAttacking = false;
    }

    private void PerformAttackHit()
    {
        float comboDamageMultiplier = comboStep == 1 ? 1f : comboStep == 2 ? 1.15f : 1.35f;
        float comboRangeBonus = comboStep == 3 ? 0.35f : comboStep == 2 ? 0.15f : 0f;
        float comboAngleBonus = comboStep == 3 ? 20f : comboStep == 2 ? 10f : 0f;
        float comboKnockback = comboStep == 3 ? 0.55f : comboStep == 2 ? 0.38f : 0.24f;
        float actualRange = attackRange + comboRangeBonus;
        float actualAngle = attackAngle + comboAngleBonus;

        Vector3 slashPos = transform.position + facingDirection * (0.9f + comboRangeBonus) + Vector3.up * 0.65f;
        bool empoweredSlash = stats != null && (stats.critBuildLevel > 0 || stats.burnBuildLevel > 0 || stats.lightningBuildLevel > 0);
        VisualEffectsManager.Instance.PlayPlayerSlash(slashPos, facingDirection, comboStep, empoweredSlash);
        ParticleEffects.Instance.ShowCooldown(transform, attackCooldown);

        Collider[] hits = Physics.OverlapSphere(transform.position, actualRange);
        System.Collections.Generic.List<EnemyStats> validTargets = new System.Collections.Generic.List<EnemyStats>();

        foreach (Collider hit in hits)
        {
            if (!hit.CompareTag("Enemy"))
            {
                continue;
            }

            Vector3 dir = (hit.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(facingDirection, dir);
            if (angle <= actualAngle / 2f)
            {
                EnemyStats enemy = hit.GetComponent<EnemyStats>();
                if (enemy != null && !enemy.IsDead())
                {
                    validTargets.Add(enemy);
                }
            }
        }

        validTargets.Sort((a, b) =>
            Vector3.Distance(transform.position, a.transform.position).CompareTo(
                Vector3.Distance(transform.position, b.transform.position)));
        if (validTargets.Count > 0)
        {
            Vector3 lungeDirection = (validTargets[0].transform.position - transform.position);
            lungeDirection.y = 0f;
            if (lungeDirection.sqrMagnitude > 0.01f)
            {
                lungeDirection.Normalize();
                controller.Move(lungeDirection * Mathf.Min(attackLungeDistance + comboStep * 0.12f, actualRange * 0.5f));
            }
        }

        int hitCount = 0;
        bool landedHit = false;
        bool landedCrit = false;

        foreach (EnemyStats enemy in validTargets)
        {
            if (hitCount >= maxTargetsPerAttack || stats == null)
            {
                break;
            }

            bool isCrit = Random.value < stats.critChance;
            float rawDamage = stats.atk * attackDamageMultiplier * comboDamageMultiplier;
            if (isCrit)
            {
                rawDamage *= stats.critMultiplier;
            }

            float dealtDamage = enemy.TakeDamage(rawDamage, transform.position, isCrit);
            if (dealtDamage > 0f)
            {
                landedHit = true;
                landedCrit |= isCrit;
                hitCount++;
                if (CombatManager.Instance != null)
                {
                    Vector3 knockbackDirection = enemy.transform.position - transform.position;
                    CombatManager.Instance.ApplyKnockback(enemy.gameObject, knockbackDirection, comboKnockback + hitCount * 0.08f);
                }

                if (stats.lifeStealPercent > 0f)
                {
                    stats.Heal(dealtDamage * stats.lifeStealPercent);
                }

                if (stats.burnChance > 0f && Random.value < stats.burnChance)
                {
                    enemy.ApplyBurn(stats.burnDuration, Mathf.Max(1f, stats.burnDamagePerSecond), transform.position);
                    TriggerBurnSpread(enemy);
                }

                if (stats.shockwaveChance > 0f && Random.value < stats.shockwaveChance)
                {
                    TriggerShockwave(enemy.transform.position, rawDamage * (0.55f + stats.lightningBuildLevel * 0.08f), enemy);
                }

                if (isCrit && stats.critBuildLevel > 0)
                {
                    TriggerCritBurst(enemy.transform.position, rawDamage, enemy);
                }

                if (isCrit && stats.HasThunderStrikeBlessing)
                {
                    TriggerThunderStrike(enemy, rawDamage);
                }
            }
        }

        if (landedHit)
        {
            if (CombatManager.Instance != null)
            {
                CombatManager.Instance.TriggerPlayerAttackImpact(transform.position + facingDirection * 0.7f, landedCrit, hitCount > 1, comboStep);
            }
            CombatManager.PlayAttackSound(transform.position);
        }
    }

    public Vector3 GetFacingDirection()
    {
        return facingDirection;
    }

    private Vector3 ResolveAttackDirection(bool preferMouse)
    {
        if (preferMouse && Camera.main != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane groundPlane = new Plane(Vector3.up, transform.position);
            float distance;
            if (groundPlane.Raycast(ray, out distance))
            {
                Vector3 point = ray.GetPoint(distance);
                Vector3 dir = point - transform.position;
                dir.y = 0f;
                if (dir.sqrMagnitude > 0.04f)
                {
                    return dir.normalized;
                }
            }
        }

        EnemyStats nearest = FindBestFacingTarget();
        if (nearest != null)
        {
            Vector3 targetDirection = nearest.transform.position - transform.position;
            targetDirection.y = 0f;
            if (targetDirection.sqrMagnitude > 0.01f)
            {
                return targetDirection.normalized;
            }
        }

        return facingDirection.sqrMagnitude > 0.01f ? facingDirection : transform.forward;
    }

    private EnemyStats FindBestFacingTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange + 1.2f);
        EnemyStats best = null;
        float bestScore = float.MaxValue;
        foreach (Collider hit in hits)
        {
            if (!hit.CompareTag("Enemy"))
            {
                continue;
            }

            EnemyStats enemy = hit.GetComponent<EnemyStats>();
            if (enemy == null || enemy.IsDead())
            {
                continue;
            }

            Vector3 direction = enemy.transform.position - transform.position;
            direction.y = 0f;
            float distance = direction.magnitude;
            if (distance < 0.05f)
            {
                return enemy;
            }

            float angle = Vector3.Angle(facingDirection, direction.normalized);
            float score = distance + angle * 0.02f;
            if (score < bestScore)
            {
                bestScore = score;
                best = enemy;
            }
        }

        return best;
    }

    private void SetFacingDirection(Vector3 direction)
    {
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.01f)
        {
            return;
        }

        facingDirection = direction.normalized;
        transform.rotation = Quaternion.LookRotation(facingDirection);
    }

    private void CacheVisualRoot()
    {
        if (visualRoot != null)
        {
            return;
        }

        Transform kayKitVisual = transform.Find("KayKitVisual");
        visualRoot = kayKitVisual != null ? kayKitVisual : transform;
        visualStartLocalPosition = visualRoot.localPosition;
        visualStartLocalRotation = visualRoot.localRotation;
        visualStartLocalScale = visualRoot.localScale;
    }

    private void EnsureAnimationDriver()
    {
        if (GetComponent<CharacterAnimationController>() == null)
        {
            gameObject.AddComponent<CharacterAnimationController>();
        }

        if (GetComponent<PlayerAnimationDriver>() == null)
        {
            gameObject.AddComponent<PlayerAnimationDriver>();
        }

        if (GetComponent<PlayerResourceAnimationDriver>() == null)
        {
            gameObject.AddComponent<PlayerResourceAnimationDriver>();
        }
    }

    private void AnimateAttackWindup(float t)
    {
        if (visualRoot == null)
        {
            return;
        }

        float lean = Mathf.Lerp(0f, -14f, t);
        float twist = Mathf.Lerp(0f, comboStep % 2 == 0 ? -24f : 24f, t);
        visualRoot.localPosition = visualStartLocalPosition + new Vector3(0f, -0.08f * t, -0.08f * t);
        visualRoot.localRotation = visualStartLocalRotation * Quaternion.Euler(lean, twist, 0f);
        visualRoot.localScale = Vector3.Lerp(visualStartLocalScale, visualStartLocalScale * 0.94f, t);
    }

    private void AnimateAttackRelease(float t)
    {
        if (visualRoot == null)
        {
            return;
        }

        float swing = Mathf.Sin(t * Mathf.PI);
        float snap = 1f - t;
        float twist = comboStep % 2 == 0 ? 42f : -42f;
        visualRoot.localPosition = visualStartLocalPosition + new Vector3(0f, 0.02f * swing, 0.18f * swing * snap);
        visualRoot.localRotation = visualStartLocalRotation * Quaternion.Euler(8f * swing, twist * swing, -18f * swing);
        visualRoot.localScale = visualStartLocalScale * (1f + 0.06f * swing);
    }

    private void ResetAttackVisual()
    {
        if (visualRoot == null)
        {
            return;
        }

        visualRoot.localPosition = visualStartLocalPosition;
        visualRoot.localRotation = visualStartLocalRotation;
        visualRoot.localScale = visualStartLocalScale;
    }

    private void TriggerShockwave(Vector3 center, float damage, EnemyStats primaryTarget)
    {
        float radius = 2.2f + stats.lightningBuildLevel * 0.28f;
        ParticleEffects.Instance.ShowAttackTelegraph(center, radius, 0.18f, false);
        if (VisualEffectsManager.Instance != null)
        {
            VisualEffectsManager.Instance.PlayHitBurst(center + Vector3.up * 0.45f, new Color(0.35f, 0.78f, 1f, 1f));
            VisualEffectsManager.Instance.PlayGroundPulse(center, new Color(0.25f, 0.75f, 1f, 0.72f), radius, 0.26f);
        }

        Collider[] nearby = Physics.OverlapSphere(center, radius);
        int chained = 0;
        foreach (Collider collider in nearby)
        {
            if (!collider.CompareTag("Enemy"))
            {
                continue;
            }

            EnemyStats enemy = collider.GetComponent<EnemyStats>();
            if (enemy == null || enemy == primaryTarget || enemy.IsDead())
            {
                continue;
            }

            ApplyLightningDamage(enemy, center, damage, false);
            chained++;
            if (stats.lightningBuildLevel < 2 && chained >= 3)
            {
                break;
            }
        }
    }

    private void TriggerCritBurst(Vector3 center, float rawDamage, EnemyStats primaryTarget)
    {
        float radius = 1.05f + stats.critBuildLevel * 0.12f;
        float damage = rawDamage * (0.18f + stats.critBuildLevel * 0.06f);
        if (VisualEffectsManager.Instance != null)
        {
            VisualEffectsManager.Instance.PlayHitBurst(center + Vector3.up * 0.55f, new Color(1f, 0.92f, 0.25f, 1f));
            VisualEffectsManager.Instance.PlayGroundPulse(center, new Color(1f, 0.86f, 0.18f, 0.72f), radius + 0.35f, 0.22f);
        }

        if (stats.critBuildLevel < 2)
        {
            return;
        }

        Collider[] nearby = Physics.OverlapSphere(center, radius);
        foreach (Collider collider in nearby)
        {
            if (!collider.CompareTag("Enemy"))
            {
                continue;
            }

            EnemyStats enemy = collider.GetComponent<EnemyStats>();
            if (enemy == null || enemy == primaryTarget || enemy.IsDead())
            {
                continue;
            }

            enemy.TakeDamage(damage, center, true);
        }
    }

    private void TriggerThunderStrike(EnemyStats primaryTarget, float critRawDamage)
    {
        if (primaryTarget == null || stats == null)
        {
            return;
        }

        Vector3 center = primaryTarget.transform.position;
        float radius = 3.1f + stats.lightningBuildLevel * 0.32f;
        float damage = critRawDamage * (0.22f + stats.critBuildLevel * 0.035f + stats.lightningBuildLevel * 0.035f);
        int maxChains = Mathf.Clamp(2 + stats.lightningBuildLevel / 2, 2, 5);

        if (VisualEffectsManager.Instance != null)
        {
            VisualEffectsManager.Instance.PlayDualBlessingPulse(center, new Color(1f, 0.86f, 0.15f, 0.72f), new Color(0.24f, 0.72f, 1f, 0.72f), radius * 0.55f, 0.24f);
        }

        Collider[] nearby = Physics.OverlapSphere(center, radius);
        int chained = 0;
        foreach (Collider collider in nearby)
        {
            if (!collider.CompareTag("Enemy"))
            {
                continue;
            }

            EnemyStats enemy = collider.GetComponent<EnemyStats>();
            if (enemy == null || enemy == primaryTarget || enemy.IsDead())
            {
                continue;
            }

            if (VisualEffectsManager.Instance != null)
            {
                VisualEffectsManager.Instance.PlayChainLightning(center, enemy.transform.position, new Color(0.55f, 0.86f, 1f, 0.95f), 0.18f);
            }

            ApplyLightningDamage(enemy, center, damage, true);
            chained++;
            if (chained >= maxChains)
            {
                break;
            }
        }
    }

    private void ApplyLightningDamage(EnemyStats enemy, Vector3 sourcePosition, float damage, bool criticalConduct)
    {
        if (enemy == null || enemy.IsDead())
        {
            return;
        }

        bool wasBurning = enemy.IsBurning();
        enemy.TakeDamage(damage, sourcePosition, criticalConduct);
        if (wasBurning && stats != null && stats.HasOverloadExplosionBlessing)
        {
            TriggerOverloadExplosion(enemy, sourcePosition);
        }
    }

    private void TriggerOverloadExplosion(EnemyStats burningTarget, Vector3 sourcePosition)
    {
        if (burningTarget == null || stats == null)
        {
            return;
        }

        Vector3 center = burningTarget.transform.position;
        float radius = 1.85f + stats.burnBuildLevel * 0.18f + stats.lightningBuildLevel * 0.18f;
        float damage = Mathf.Max(4f, stats.burnDamagePerSecond * 0.85f + stats.atk * (0.18f + stats.lightningBuildLevel * 0.025f));
        float burnDuration = stats.burnDuration * (1.0f + stats.lightningBuildLevel * 0.08f);
        float burnDps = Mathf.Max(1f, stats.burnDamagePerSecond * (0.75f + stats.burnBuildLevel * 0.05f));

        if (VisualEffectsManager.Instance != null)
        {
            VisualEffectsManager.Instance.PlayDualBlessingPulse(center, new Color(1f, 0.22f, 0.04f, 0.76f), new Color(0.15f, 0.62f, 1f, 0.72f), radius, 0.32f);
        }

        Collider[] nearby = Physics.OverlapSphere(center, radius);
        foreach (Collider collider in nearby)
        {
            if (!collider.CompareTag("Enemy"))
            {
                continue;
            }

            EnemyStats enemy = collider.GetComponent<EnemyStats>();
            if (enemy == null || enemy.IsDead())
            {
                continue;
            }

            enemy.TakeDamage(damage, sourcePosition);
            enemy.ApplyBurn(burnDuration, burnDps, center);
        }

        DamageTextPool.Instance.ShowText(center, "超载爆炸", new Color(1f, 0.45f, 0.12f, 1f), true);
    }

    private void TriggerBurnSpread(EnemyStats primaryTarget)
    {
        if (stats.burnBuildLevel <= 0 || primaryTarget == null)
        {
            return;
        }

        Vector3 center = primaryTarget.transform.position;
        float radius = 1.7f + stats.burnBuildLevel * 0.22f;
        if (VisualEffectsManager.Instance != null)
        {
            VisualEffectsManager.Instance.PlayHitBurst(center + Vector3.up * 0.35f, new Color(1f, 0.35f, 0.08f, 1f));
            VisualEffectsManager.Instance.PlayGroundPulse(center, new Color(1f, 0.24f, 0.04f, 0.66f), radius, 0.3f);
        }

        Collider[] nearby = Physics.OverlapSphere(center, radius);
        int spreadCount = 0;
        int maxSpread = Mathf.Clamp(stats.burnBuildLevel, 1, 4);
        foreach (Collider collider in nearby)
        {
            if (!collider.CompareTag("Enemy"))
            {
                continue;
            }

            EnemyStats enemy = collider.GetComponent<EnemyStats>();
            if (enemy == null || enemy == primaryTarget || enemy.IsDead())
            {
                continue;
            }

            enemy.ApplyBurn(stats.burnDuration * 0.75f, Mathf.Max(1f, stats.burnDamagePerSecond * 0.55f), center);
            spreadCount++;
            if (spreadCount >= maxSpread)
            {
                break;
            }
        }
    }
}
