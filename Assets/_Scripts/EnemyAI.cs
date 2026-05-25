using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    public enum AIState
    {
        Patrol,
        Chase,
        Attack,
        Return,
        Dead
    }

    public AIState currentState = AIState.Patrol;
    public float moveSpeed = 3f;
    public float patrolSpeed = 1.5f;
    public float attackRange = 2f;
    public float alertRange = 8f;
    public float chaseRange = 15f;
    public float attackCooldown = 1.2f;
    public float patrolDistance = 4f;
    public float patrolPause = 1f;
    public float detectionInterval = 0.5f;
    public float attackWindup = 0.3f;
    public float attackConeAngle = 105f;
    public float attackVisualLength = 1.8f;
    public float attackVisualWidth = 0.55f;
    public bool canCharge;
    public bool canSpitProjectile;
    public float specialCooldown = 4.5f;
    public float chargeWindup = 0.55f;
    public float chargeDistance = 4.2f;
    public float chargeDuration = 0.22f;
    public float chargeDamageMultiplier = 1.35f;
    public float projectileWindup = 0.42f;
    public float projectileSpeed = 8f;
    public float projectileDamageMultiplier = 1.05f;
    public bool useRandomPatrol;
    public float patrolRadius = 3f;
    public float directionChangeInterval = 3f;
    public Transform playerTransform;

    private NavMeshAgent agent;
    private EnemyStats stats;
    private Vector3 spawnPoint;
    private float lastAttackTime = -999f;
    private float lastDetectionTime;
    private Vector3 patrolPointA;
    private Vector3 patrolPointB;
    private bool movingToA = true;
    private float patrolWaitTimer;
    private bool isWaiting;
    private Vector3 randomPatrolDir;
    private float randomPatrolTimer;
    private float lastSpecialTime = -999f;
    private bool specialInProgress;
    private Vector3 lockedAttackDirection = Vector3.forward;
    private Vector3 lockedAttackOrigin;
    private bool attackDirectionLocked;
    private CharacterAnimationController animationController;
    private EnemyResourceAnimationDriver resourceAnimationDriver;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        stats = GetComponent<EnemyStats>();
        EnsureAnimationDriver();
        animationController = GetComponent<CharacterAnimationController>();
        resourceAnimationDriver = GetComponent<EnemyResourceAnimationDriver>();
        spawnPoint = transform.position;
        agent.speed = patrolSpeed;
        agent.stoppingDistance = 0.1f;
        agent.updateRotation = false;
    }

    private void Start()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        if (useRandomPatrol)
        {
            randomPatrolDir = Random.insideUnitSphere;
            randomPatrolDir.y = 0f;
            randomPatrolDir.Normalize();
        }
        else
        {
            Vector3 randomDir = Random.insideUnitSphere;
            randomDir.y = 0f;
            randomDir.Normalize();
            patrolPointA = ClampToArena(spawnPoint + randomDir * (patrolDistance * 0.5f));
            patrolPointB = ClampToArena(spawnPoint - randomDir * (patrolDistance * 0.5f));
            agent.SetDestination(patrolPointA);
        }
    }

    private void Update()
    {
        if (stats != null && stats.IsDead())
        {
            currentState = AIState.Dead;
            return;
        }

        if (playerTransform == null)
        {
            return;
        }

        if (Time.time - lastDetectionTime >= detectionInterval)
        {
            lastDetectionTime = Time.time;
            EvaluateState();
        }

        switch (currentState)
        {
            case AIState.Patrol:
                UpdatePatrol();
                break;
            case AIState.Chase:
                UpdateChase();
                break;
            case AIState.Attack:
                UpdateAttack();
                break;
            case AIState.Return:
                UpdateReturn();
                break;
        }
    }

    private void EvaluateState()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        float distanceFromSpawn = Vector3.Distance(transform.position, spawnPoint);

        if (currentState == AIState.Patrol || currentState == AIState.Return)
        {
            if (distanceToPlayer <= alertRange)
            {
                ChangeState(AIState.Chase);
            }
        }
        else if (currentState == AIState.Chase)
        {
            if (distanceToPlayer <= attackRange)
            {
                ChangeState(AIState.Attack);
            }
            else if (distanceFromSpawn > chaseRange)
            {
                ChangeState(AIState.Return);
            }
        }
        else if (currentState == AIState.Attack)
        {
            if (distanceToPlayer > attackRange)
            {
                ChangeState(AIState.Chase);
            }
            else if (distanceFromSpawn > chaseRange)
            {
                ChangeState(AIState.Return);
            }
        }
    }

    private void ChangeState(AIState nextState)
    {
        currentState = nextState;
        switch (nextState)
        {
            case AIState.Patrol:
                agent.speed = patrolSpeed;
                agent.isStopped = false;
                isWaiting = false;
                if (useRandomPatrol)
                {
                    randomPatrolTimer = 0f;
                    randomPatrolDir = Random.insideUnitSphere;
                    randomPatrolDir.y = 0f;
                    randomPatrolDir.Normalize();
                }
                else
                {
                    agent.SetDestination(movingToA ? patrolPointA : patrolPointB);
                }
                break;
            case AIState.Chase:
                agent.speed = moveSpeed;
                agent.isStopped = false;
                break;
            case AIState.Attack:
                agent.isStopped = true;
                break;
            case AIState.Return:
                agent.speed = moveSpeed;
                agent.isStopped = false;
                agent.SetDestination(spawnPoint);
                break;
        }
    }

    private void UpdatePatrol()
    {
        if (useRandomPatrol)
        {
            randomPatrolTimer += Time.deltaTime;
            if (randomPatrolTimer >= directionChangeInterval)
            {
                randomPatrolTimer = 0f;
                randomPatrolDir = Random.insideUnitSphere;
                randomPatrolDir.y = 0f;
                randomPatrolDir.Normalize();
            }

            Vector3 candidate = transform.position + randomPatrolDir * patrolSpeed * Time.deltaTime;
            if (Vector3.Distance(candidate, spawnPoint) <= patrolRadius)
            {
                agent.SetDestination(candidate);
            }
            else
            {
                agent.SetDestination(spawnPoint);
            }

            return;
        }

        if (isWaiting)
        {
            patrolWaitTimer -= Time.deltaTime;
            if (patrolWaitTimer <= 0f)
            {
                isWaiting = false;
                agent.isStopped = false;
                agent.SetDestination(movingToA ? patrolPointA : patrolPointB);
            }

            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
        {
            isWaiting = true;
            patrolWaitTimer = patrolPause;
            movingToA = !movingToA;
            agent.isStopped = true;
        }
    }

    private void UpdateChase()
    {
        if (TryStartCombatSpecial())
        {
            return;
        }

        agent.SetDestination(playerTransform.position);
        Face(playerTransform.position);
    }

    private void UpdateAttack()
    {
        if (!attackDirectionLocked)
        {
            Face(playerTransform.position);
        }
        else
        {
            FaceDirection(lockedAttackDirection);
        }

        if (TryStartCombatSpecial())
        {
            return;
        }

        if (stats != null && stats.isBoss && !specialInProgress && Time.time - lastSpecialTime >= 6f)
        {
            lastSpecialTime = Time.time;
            if (stats.enemyType == EnemyStats.EnemyType.Skeleton)
            {
                StartCoroutine(BoneTyrantChargeRoutine());
            }
            else
            {
                StartCoroutine(GelatinousHorrorNovaRoutine());
            }
            return;
        }

        if (Time.time - lastAttackTime >= attackCooldown)
        {
            lastAttackTime = Time.time;
            LockAttackDirection();
            if (animationController != null)
            {
                animationController.PlayAttack(lockedAttackDirection);
            }
            if (resourceAnimationDriver != null)
            {
                resourceAnimationDriver.PlayAttack();
            }

            float telegraphRadius = stats != null && stats.isBoss ? attackRange + 1.2f : attackRange + 0.35f;
            VisualEffectsManager.Instance.ShowAttackWarning(transform.position, lockedAttackDirection, telegraphRadius, attackWindup, stats != null && stats.isBoss);
            StartCoroutine(AttackDelayed(lockedAttackDirection, lockedAttackOrigin));
        }
    }

    private bool TryStartCombatSpecial()
    {
        if (specialInProgress || stats == null || stats.IsDead() || stats.isBoss || playerTransform == null)
        {
            return false;
        }

        if (Time.time - lastSpecialTime < specialCooldown)
        {
            return false;
        }

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        if (canCharge && distance > attackRange + 0.75f && distance <= Mathf.Max(alertRange, 6f))
        {
            lastSpecialTime = Time.time;
            StartCoroutine(ChargeRoutine());
            return true;
        }

        if (canSpitProjectile && distance > attackRange + 1f && distance <= Mathf.Max(alertRange + 1f, 7f))
        {
            lastSpecialTime = Time.time;
            StartCoroutine(ProjectileRoutine());
            return true;
        }

        return false;
    }

    private System.Collections.IEnumerator AttackDelayed(Vector3 attackDirection, Vector3 attackOrigin)
    {
        float elapsed = 0f;
        while (elapsed < attackWindup)
        {
            elapsed += Time.deltaTime;
            FaceDirection(attackDirection);
            yield return null;
        }

        if (playerTransform == null || stats == null || stats.IsDead())
        {
            attackDirectionLocked = false;
            yield break;
        }

        VisualEffectsManager.Instance.PlayEnemySlash(transform.position, attackDirection, attackVisualLength, attackVisualWidth);

        Vector3 toPlayer = playerTransform.position - attackOrigin;
        toPlayer.y = 0f;
        bool inRange = toPlayer.magnitude <= attackRange + 1f;
        bool inCone = toPlayer.sqrMagnitude < 0.001f || Vector3.Angle(attackDirection, toPlayer.normalized) <= attackConeAngle * 0.5f;

        if (inRange && inCone)
        {
            PlayerStats playerStats = playerTransform.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.TakeDamage(stats.atk, attackOrigin);
            }
        }

        attackDirectionLocked = false;
    }

    private System.Collections.IEnumerator ChargeRoutine()
    {
        specialInProgress = true;
        agent.isStopped = true;

        Vector3 origin = transform.position;
        Vector3 direction = playerTransform != null ? playerTransform.position - origin : transform.forward;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.001f)
        {
            direction = transform.forward;
        }

        direction.Normalize();
        FaceDirection(direction);

        if (animationController != null)
        {
            animationController.PlayChargeWindup(direction, chargeWindup);
        }
        if (resourceAnimationDriver != null)
        {
            resourceAnimationDriver.PlayChargeWindup();
        }

        VisualEffectsManager.Instance.ShowAttackWarning(origin, direction, chargeDistance + 0.8f, chargeWindup, false);

        float elapsed = 0f;
        while (elapsed < chargeWindup)
        {
            elapsed += Time.deltaTime;
            FaceDirection(direction);
            yield return null;
        }

        bool hitPlayer = false;
        elapsed = 0f;
        while (elapsed < chargeDuration)
        {
            elapsed += Time.deltaTime;
            float step = chargeDistance / Mathf.Max(0.01f, chargeDuration) * Time.deltaTime;
            agent.Move(direction * step);
            FaceDirection(direction);

            if (!hitPlayer && playerTransform != null)
            {
                float distanceToPath = DistancePointToSegment(playerTransform.position, origin, transform.position);
                if (distanceToPath <= 0.9f)
                {
                    PlayerStats playerStats = playerTransform.GetComponent<PlayerStats>();
                    if (playerStats != null)
                    {
                        playerStats.TakeDamage(stats.atk * chargeDamageMultiplier, origin);
                        hitPlayer = true;
                    }
                }
            }

            yield return null;
        }

        if (VisualEffectsManager.Instance != null)
        {
            VisualEffectsManager.Instance.PlayHitBurst(transform.position + Vector3.up * 0.3f, new Color(0.95f, 0.75f, 0.45f, 1f));
        }

        yield return new WaitForSeconds(0.18f);
        specialInProgress = false;
        if (currentState != AIState.Dead)
        {
            agent.isStopped = false;
        }
    }

    private System.Collections.IEnumerator ProjectileRoutine()
    {
        specialInProgress = true;
        agent.isStopped = true;

        Vector3 direction = playerTransform != null ? playerTransform.position - transform.position : transform.forward;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.001f)
        {
            direction = transform.forward;
        }

        direction.Normalize();
        FaceDirection(direction);

        Vector3 targetPoint = playerTransform != null ? playerTransform.position : transform.position + direction * 4f;
        if (animationController != null)
        {
            animationController.PlaySpitWindup(direction, projectileWindup);
        }
        if (resourceAnimationDriver != null)
        {
            resourceAnimationDriver.PlaySpitWindup();
        }

        ParticleEffects.Instance.ShowAttackTelegraph(targetPoint, 0.9f, projectileWindup, false);

        float elapsed = 0f;
        while (elapsed < projectileWindup)
        {
            elapsed += Time.deltaTime;
            FaceDirection(direction);
            yield return null;
        }

        if (stats != null && !stats.IsDead())
        {
            Vector3 spawnPosition = transform.position + direction * 0.65f + Vector3.up * 0.45f;
            EnemyProjectile.Spawn(spawnPosition, targetPoint, stats.atk * projectileDamageMultiplier, projectileSpeed);
            if (VisualEffectsManager.Instance != null)
            {
                VisualEffectsManager.Instance.PlayHitBurst(spawnPosition, new Color(0.35f, 1f, 0.28f, 1f));
            }
        }

        yield return new WaitForSeconds(0.15f);
        specialInProgress = false;
        if (currentState != AIState.Dead)
        {
            agent.isStopped = false;
        }
    }

    private System.Collections.IEnumerator BoneTyrantChargeRoutine()
    {
        specialInProgress = true;
        agent.isStopped = true;
        Vector3 targetPosition = playerTransform != null ? playerTransform.position : transform.position;
        float castTime = 0.7f;
        float impactRadius = 2.3f;
        ParticleEffects.Instance.ShowAttackTelegraph(targetPosition, impactRadius, castTime, true);
        yield return new WaitForSeconds(castTime);

        if (stats != null && !stats.IsDead())
        {
            Vector3 dashDirection = (targetPosition - transform.position);
            dashDirection.y = 0f;
            if (dashDirection.sqrMagnitude > 0.01f)
            {
                dashDirection.Normalize();
                transform.position += dashDirection * 3.5f;
                Face(targetPosition);
            }

            if (playerTransform != null && Vector3.Distance(playerTransform.position, targetPosition) <= impactRadius)
            {
                PlayerStats playerStats = playerTransform.GetComponent<PlayerStats>();
                if (playerStats != null) playerStats.TakeDamage(stats.atk * 1.7f, transform.position);
            }
        }

        yield return new WaitForSeconds(0.25f);
        specialInProgress = false;
        if (currentState != AIState.Dead)
        {
            agent.isStopped = false;
        }
    }

    private System.Collections.IEnumerator GelatinousHorrorNovaRoutine()
    {
        specialInProgress = true;
        agent.isStopped = true;

        float[] radii = { attackRange + 1f, attackRange + 2f, attackRange + 3f };
        for (int i = 0; i < radii.Length; i++)
        {
            float castTime = 0.45f;
            ParticleEffects.Instance.ShowAttackTelegraph(transform.position, radii[i], castTime, true);
            yield return new WaitForSeconds(castTime);

            if (playerTransform != null && stats != null && !stats.IsDead())
            {
                if (Vector3.Distance(transform.position, playerTransform.position) <= radii[i])
                {
                    PlayerStats playerStats = playerTransform.GetComponent<PlayerStats>();
                    if (playerStats != null)
                    {
                        playerStats.TakeDamage(stats.atk * (0.8f + i * 0.2f), transform.position);
                    }
                }
            }

            yield return new WaitForSeconds(0.15f);
        }

        specialInProgress = false;
        if (currentState != AIState.Dead)
        {
            agent.isStopped = false;
        }
    }

    private void UpdateReturn()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
        {
            ChangeState(AIState.Patrol);
        }
    }

    private void Face(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0f;
        FaceDirection(direction);
    }

    private void FaceDirection(Vector3 direction)
    {
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    private float DistancePointToSegment(Vector3 point, Vector3 start, Vector3 end)
    {
        point.y = 0f;
        start.y = 0f;
        end.y = 0f;
        Vector3 segment = end - start;
        float segmentLengthSqr = segment.sqrMagnitude;
        if (segmentLengthSqr < 0.001f)
        {
            return Vector3.Distance(point, start);
        }

        float t = Mathf.Clamp01(Vector3.Dot(point - start, segment) / segmentLengthSqr);
        Vector3 closest = start + segment * t;
        return Vector3.Distance(point, closest);
    }

    private void LockAttackDirection()
    {
        lockedAttackOrigin = transform.position;
        lockedAttackDirection = playerTransform != null ? playerTransform.position - transform.position : transform.forward;
        lockedAttackDirection.y = 0f;
        if (lockedAttackDirection.sqrMagnitude < 0.001f)
        {
            lockedAttackDirection = transform.forward;
        }

        lockedAttackDirection.Normalize();
        attackDirectionLocked = true;
        FaceDirection(lockedAttackDirection);
    }

    private Vector3 ClampToArena(Vector3 position)
    {
        float halfSize = 19f;
        position.x = Mathf.Clamp(position.x, -halfSize, halfSize);
        position.z = Mathf.Clamp(position.z, -halfSize, halfSize);
        return position;
    }

    private void EnsureAnimationDriver()
    {
        if (GetComponent<CharacterAnimationController>() == null)
        {
            gameObject.AddComponent<CharacterAnimationController>();
        }

        if (GetComponent<EnemyAnimationDriver>() == null)
        {
            gameObject.AddComponent<EnemyAnimationDriver>();
        }

        if (GetComponent<EnemyResourceAnimationDriver>() == null)
        {
            gameObject.AddComponent<EnemyResourceAnimationDriver>();
        }
    }
}
