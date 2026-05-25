using System.Collections;
using UnityEngine;

public enum CharacterAnimState
{
    Idle,
    Move,
    Attack,
    Dash,
    Hurt,
    Death
}

public enum CharacterAnimationPreset
{
    Custom,
    Player,
    Skeleton,
    Slime
}

public class CharacterAnimationController : MonoBehaviour
{
    public Transform visualRoot;
    public CharacterAnimationPreset motionPreset = CharacterAnimationPreset.Player;
    public float idleBobHeight = 0.038f;
    public float idleBobSpeed = 2.15f;
    public float idleBreathScale = 0.03f;
    public float moveSwayAngle = 10.5f;
    public float movePulse = 0.085f;
    public float moveSpeed = 10.8f;
    public Color hurtFlashColor = Color.white;
    public float hurtDuration = 0.28f;
    public float deathDuration = 1.05f;
    public float attackPoseScale = 1.08f;
    public float attackLungeScale = 1.08f;
    public float dashStretchScale = 1.12f;
    public float hurtKnockbackScale = 1.08f;
    public float deathFallScale = 1f;
    public bool slimeMotion;
    public bool skeletonMotion;

    private CharacterAnimState currentState = CharacterAnimState.Idle;
    private Vector3 startLocalPosition;
    private Quaternion startLocalRotation;
    private Vector3 startLocalScale;
    private Renderer[] renderers;
    private Color[] originalColors;
    private Coroutine transientRoutine;
    private bool transientLocked;
    private bool dead;
    private Vector3 moveVelocity;
    private Vector3 facingDirection = Vector3.forward;
    private float moveBlend;
    private float moveBlendVelocity;
    private float stridePhaseOffset;

    private void Awake()
    {
        if (motionPreset == CharacterAnimationPreset.Custom)
        {
            EnemyStats enemyStats = GetComponent<EnemyStats>();
            if (GetComponent<PlayerController>() != null)
            {
                motionPreset = CharacterAnimationPreset.Player;
            }
            else if (enemyStats != null && enemyStats.enemyType == EnemyStats.EnemyType.Skeleton)
            {
                motionPreset = CharacterAnimationPreset.Skeleton;
            }
            else if (enemyStats != null && enemyStats.enemyType == EnemyStats.EnemyType.Slime)
            {
                motionPreset = CharacterAnimationPreset.Slime;
            }
        }

        ApplyPreset(motionPreset);
        stridePhaseOffset = Random.Range(0f, 10f);
        CacheVisualRoot();
        CacheRenderers();
    }

    private void Update()
    {
        if (dead || visualRoot == null || transientLocked)
        {
            return;
        }

        float targetMoveBlend = currentState == CharacterAnimState.Move ? 1f : 0f;
        moveBlend = Mathf.SmoothDamp(moveBlend, targetMoveBlend, ref moveBlendVelocity, 0.08f);

        switch (currentState)
        {
            case CharacterAnimState.Move:
                ApplyMoveLoop();
                break;
            case CharacterAnimState.Idle:
            default:
                ApplyIdleLoop();
                break;
        }
    }

    public void SetMoveContext(Vector3 velocity, Vector3 facing)
    {
        velocity.y = 0f;
        facing.y = 0f;
        moveVelocity = velocity;
        if (facing.sqrMagnitude > 0.001f)
        {
            facingDirection = facing.normalized;
        }
    }

    public void SetMotionFlavor(bool slime, bool skeleton)
    {
        slimeMotion = slime;
        skeletonMotion = skeleton;
        if (slime)
        {
            ApplyPreset(CharacterAnimationPreset.Slime);
        }
        else if (skeleton)
        {
            ApplyPreset(CharacterAnimationPreset.Skeleton);
        }
    }

    public void ApplyPreset(CharacterAnimationPreset preset)
    {
        motionPreset = preset;
        switch (preset)
        {
            case CharacterAnimationPreset.Skeleton:
                idleBobHeight = 0.025f;
                idleBobSpeed = 1.9f;
                idleBreathScale = 0.016f;
                moveSwayAngle = 7.2f;
                movePulse = 0.045f;
                moveSpeed = 8.4f;
                hurtDuration = 0.22f;
                deathDuration = 0.82f;
                attackPoseScale = 0.92f;
                attackLungeScale = 0.95f;
                dashStretchScale = 0.85f;
                hurtKnockbackScale = 0.9f;
                deathFallScale = 0.9f;
                slimeMotion = false;
                skeletonMotion = true;
                break;
            case CharacterAnimationPreset.Slime:
                idleBobHeight = 0.055f;
                idleBobSpeed = 2.05f;
                idleBreathScale = 0.062f;
                moveSwayAngle = 4.4f;
                movePulse = 0.12f;
                moveSpeed = 8.1f;
                hurtDuration = 0.32f;
                deathDuration = 0.95f;
                attackPoseScale = 1.18f;
                attackLungeScale = 0.72f;
                dashStretchScale = 1.25f;
                hurtKnockbackScale = 1.22f;
                deathFallScale = 0.72f;
                slimeMotion = true;
                skeletonMotion = false;
                break;
            case CharacterAnimationPreset.Player:
                idleBobHeight = 0.038f;
                idleBobSpeed = 2.15f;
                idleBreathScale = 0.03f;
                moveSwayAngle = 10.5f;
                movePulse = 0.085f;
                moveSpeed = 10.8f;
                hurtDuration = 0.28f;
                deathDuration = 1.05f;
                attackPoseScale = 1.08f;
                attackLungeScale = 1.08f;
                dashStretchScale = 1.12f;
                hurtKnockbackScale = 1.08f;
                deathFallScale = 1f;
                slimeMotion = false;
                skeletonMotion = false;
                break;
        }
    }

    public void PlayIdle()
    {
        if (!CanChangeLoopState(CharacterAnimState.Idle))
        {
            return;
        }

        currentState = CharacterAnimState.Idle;
    }

    public void PlayMove()
    {
        if (!CanChangeLoopState(CharacterAnimState.Move))
        {
            return;
        }

        currentState = CharacterAnimState.Move;
    }

    public void PlayAttack(Vector3 direction)
    {
        PlayAttack(direction, 1);
    }

    public void PlayAttack(Vector3 direction, int comboStep)
    {
        PlayTransient(CharacterAnimState.Attack, AttackRoutine(direction, Mathf.Clamp(comboStep, 1, 3)));
    }

    public void PlayDash(Vector3 direction)
    {
        PlayTransient(CharacterAnimState.Dash, DashRoutine(direction));
    }

    public void PlayHurt(Vector3 hitDirection)
    {
        PlayTransient(CharacterAnimState.Hurt, HurtRoutine(hitDirection));
    }

    public void PlayDeath()
    {
        if (dead)
        {
            return;
        }

        dead = true;
        PlayTransient(CharacterAnimState.Death, DeathRoutine());
    }

    public void PlayChargeWindup(Vector3 direction, float duration)
    {
        PlayTransient(CharacterAnimState.Attack, ChargeWindupRoutine(direction, Mathf.Max(0.12f, duration)));
    }

    public void PlaySpitWindup(Vector3 direction, float duration)
    {
        PlayTransient(CharacterAnimState.Attack, SpitWindupRoutine(direction, Mathf.Max(0.12f, duration)));
    }

    public CharacterAnimState CurrentState()
    {
        return currentState;
    }

    private bool CanChangeLoopState(CharacterAnimState nextState)
    {
        return !dead && !transientLocked && currentState != nextState;
    }

    private void PlayTransient(CharacterAnimState state, IEnumerator routine)
    {
        if (visualRoot == null || dead && state != CharacterAnimState.Death)
        {
            return;
        }

        if (transientRoutine != null)
        {
            StopCoroutine(transientRoutine);
            RestoreRendererColors();
        }

        currentState = state;
        transientRoutine = StartCoroutine(routine);
    }

    private void ApplyIdleLoop()
    {
        float wave = Mathf.Sin((Time.time + stridePhaseOffset) * idleBobSpeed);
        float breath = (Mathf.Sin((Time.time + stridePhaseOffset) * idleBobSpeed * 0.62f) + 1f) * 0.5f;
        float slimeExtra = slimeMotion ? 1.45f : 1f;
        Vector3 targetPosition = startLocalPosition + Vector3.up * wave * idleBobHeight * slimeExtra;
        Vector3 targetScale = startLocalScale + new Vector3(idleBreathScale * 0.45f, idleBreathScale, idleBreathScale * 0.45f) * breath * slimeExtra;
        Quaternion targetRotation = startLocalRotation * Quaternion.Euler(0f, 0f, wave * (slimeMotion ? 1.8f : 0.8f));

        visualRoot.localPosition = Vector3.Lerp(visualRoot.localPosition, targetPosition, Time.deltaTime * 10f);
        visualRoot.localRotation = Quaternion.Slerp(visualRoot.localRotation, targetRotation, Time.deltaTime * 10f);
        visualRoot.localScale = Vector3.Lerp(visualRoot.localScale, targetScale, Time.deltaTime * 10f);
    }

    private void ApplyMoveLoop()
    {
        float speed01 = Mathf.Clamp01(moveVelocity.magnitude / 6f);
        float phase = (Time.time + stridePhaseOffset) * moveSpeed * Mathf.Lerp(0.7f, 1.25f, speed01);
        float stride = Mathf.Sin(phase);
        float step = Mathf.Abs(stride);
        float weight = Mathf.Clamp01(moveBlend);
        float bounceMultiplier = slimeMotion ? 1.7f : skeletonMotion ? 0.85f : 1f;
        Vector3 localVelocity = transform.InverseTransformDirection(moveVelocity.normalized);
        float sideLean = Mathf.Clamp(localVelocity.x, -1f, 1f) * moveSwayAngle * 0.75f;
        float forwardLean = Mathf.Clamp(localVelocity.z, -1f, 1f) * (skeletonMotion ? 6f : 9f);
        float stopSquash = 1f - Mathf.Clamp01(moveVelocity.magnitude / 2.5f);

        Vector3 targetPosition = startLocalPosition
            + Vector3.up * step * idleBobHeight * 1.25f * bounceMultiplier * weight
            - Vector3.forward * stopSquash * 0.035f * (1f - weight);
        Quaternion targetRotation = startLocalRotation * Quaternion.Euler(
            forwardLean * weight,
            sideLean * 0.35f * weight,
            (-sideLean + stride * moveSwayAngle * 0.45f) * weight);
        Vector3 pulse = new Vector3(
            movePulse * step * (slimeMotion ? 1.2f : 0.75f),
            -movePulse * 0.65f * step,
            movePulse * 0.45f * step) * weight;

        if (skeletonMotion)
        {
            pulse *= 0.55f;
        }

        visualRoot.localPosition = Vector3.Lerp(visualRoot.localPosition, targetPosition, Time.deltaTime * 16f);
        visualRoot.localRotation = Quaternion.Slerp(visualRoot.localRotation, targetRotation, Time.deltaTime * 15f);
        visualRoot.localScale = Vector3.Lerp(visualRoot.localScale, startLocalScale + pulse, Time.deltaTime * 16f);
    }

    private IEnumerator AttackRoutine(Vector3 direction, int comboStep)
    {
        transientLocked = true;
        direction = NormalizeFlatDirection(direction);
        Vector3 localDirection = transform.InverseTransformDirection(direction);
        localDirection.y = 0f;
        if (localDirection.sqrMagnitude < 0.001f)
        {
            localDirection = Vector3.forward;
        }

        localDirection.Normalize();

        float windup = comboStep == 3 ? 0.13f : comboStep == 2 ? 0.09f : 0.055f;
        float release = comboStep == 3 ? 0.13f : comboStep == 2 ? 0.095f : 0.065f;
        float recover = comboStep == 3 ? 0.16f : comboStep == 2 ? 0.095f : 0.07f;
        float twistSign = comboStep == 2 ? -1f : 1f;
        float sweep = comboStep == 3 ? 62f : comboStep == 2 ? 48f : 28f;
        float lunge = (comboStep == 3 ? 0.24f : comboStep == 2 ? 0.16f : 0.09f) * attackLungeScale;
        float poseScale = attackPoseScale;

        yield return AnimatePhase(windup, t =>
        {
            float eased = EaseOutCubic(t);
            visualRoot.localPosition = startLocalPosition - localDirection * (0.08f + comboStep * 0.025f) * poseScale * eased + Vector3.down * 0.055f * poseScale * eased;
            visualRoot.localRotation = startLocalRotation * Quaternion.Euler(-10f * poseScale * eased, -sweep * 0.42f * twistSign * poseScale * eased, 8f * twistSign * poseScale * eased);
            visualRoot.localScale = Vector3.Lerp(startLocalScale, new Vector3(startLocalScale.x * (1f + 0.08f * poseScale), startLocalScale.y * (1f - 0.1f * poseScale), startLocalScale.z * (1f + 0.08f * poseScale)), eased);
        });

        if (comboStep == 3)
        {
            if (CombatManager.Instance != null)
            {
                CombatManager.Instance.TriggerPlayerAttackImpact(transform.position + direction * 0.7f, false, false, 3);
            }

            yield return new WaitForSeconds(0.035f);
        }

        yield return AnimatePhase(release, t =>
        {
            float snap = Mathf.Sin(t * Mathf.PI);
            float travel = EaseOutBack(t);
            visualRoot.localPosition = startLocalPosition + localDirection * lunge * snap + Vector3.up * 0.035f * snap;
            visualRoot.localRotation = startLocalRotation * Quaternion.Euler(12f * poseScale * snap, sweep * twistSign * poseScale * travel, -18f * twistSign * poseScale * snap);
            visualRoot.localScale = startLocalScale * (1f + (comboStep == 3 ? 0.13f : 0.08f) * snap);
        });

        yield return AnimatePhase(recover, t =>
        {
            float eased = EaseOutCubic(t);
            visualRoot.localPosition = Vector3.Lerp(startLocalPosition + localDirection * 0.045f, startLocalPosition, eased);
            visualRoot.localRotation = Quaternion.Slerp(startLocalRotation * Quaternion.Euler(0f, -10f * twistSign, 4f * twistSign), startLocalRotation, eased);
            visualRoot.localScale = Vector3.Lerp(startLocalScale * 1.04f, startLocalScale, eased);
        });

        EndTransient();
    }

    private IEnumerator DashRoutine(Vector3 direction)
    {
        transientLocked = true;
        direction = NormalizeFlatDirection(direction);
        Vector3 localDirection = transform.InverseTransformDirection(direction);
        localDirection.y = 0f;
        if (localDirection.sqrMagnitude < 0.001f)
        {
            localDirection = Vector3.forward;
        }

        localDirection.Normalize();

        yield return AnimatePhase(0.055f, t =>
        {
            float squash = EaseOutCubic(t);
            visualRoot.localPosition = startLocalPosition - localDirection * 0.06f * squash + Vector3.down * 0.04f * squash;
            visualRoot.localScale = new Vector3(startLocalScale.x * (1f + 0.2f * dashStretchScale), startLocalScale.y * (1f - 0.22f * dashStretchScale + 0.04f * squash), startLocalScale.z * (1f + 0.18f * dashStretchScale));
            visualRoot.localRotation = startLocalRotation * Quaternion.Euler(-10f * squash, 0f, 0f);
        });

        yield return AnimatePhase(0.13f, t =>
        {
            float stretch = Mathf.Sin(t * Mathf.PI);
            visualRoot.localPosition = startLocalPosition + localDirection * 0.1f * stretch + Vector3.up * 0.025f * stretch;
            visualRoot.localScale = new Vector3(startLocalScale.x * (1f - 0.22f * dashStretchScale + 0.08f * stretch), startLocalScale.y * (1f + 0.08f * dashStretchScale + 0.04f * stretch), startLocalScale.z * (1f + 0.38f * dashStretchScale + 0.08f * stretch));
            visualRoot.localRotation = startLocalRotation * Quaternion.Euler(8f * stretch, 0f, 0f);
        });

        yield return AnimatePhase(0.11f, t =>
        {
            float eased = EaseOutBack(t);
            visualRoot.localPosition = Vector3.Lerp(startLocalPosition + localDirection * 0.055f, startLocalPosition, eased);
            visualRoot.localScale = Vector3.Lerp(new Vector3(startLocalScale.x * 1.08f, startLocalScale.y * 0.94f, startLocalScale.z * 1.04f), startLocalScale, eased);
            visualRoot.localRotation = Quaternion.Slerp(startLocalRotation * Quaternion.Euler(-4f, 0f, 0f), startLocalRotation, eased);
        });

        EndTransient();
    }

    private IEnumerator HurtRoutine(Vector3 hitDirection)
    {
        transientLocked = true;
        hitDirection = NormalizeFlatDirection(hitDirection);
        Vector3 localHit = transform.InverseTransformDirection(hitDirection);
        localHit.y = 0f;
        if (localHit.sqrMagnitude < 0.001f)
        {
            localHit = -Vector3.forward;
        }

        localHit.Normalize();
        SetRendererColor(hurtFlashColor);

        yield return AnimatePhase(hurtDuration * 0.42f, t =>
        {
            float punch = EaseOutBack(t);
            visualRoot.localPosition = startLocalPosition + localHit * 0.18f * hurtKnockbackScale * punch + Vector3.down * 0.045f * hurtKnockbackScale * punch;
            visualRoot.localRotation = startLocalRotation * Quaternion.Euler(-15f * hurtKnockbackScale * punch, 0f, 13f * Mathf.Sign(localHit.x + 0.01f) * hurtKnockbackScale * punch);
            visualRoot.localScale = new Vector3(startLocalScale.x * 1.08f, startLocalScale.y * 0.88f, startLocalScale.z * 1.05f);
        }, true);

        RestoreRendererColors();

        yield return AnimatePhase(hurtDuration * 0.58f, t =>
        {
            float eased = EaseOutCubic(t);
            visualRoot.localPosition = Vector3.Lerp(startLocalPosition + localHit * 0.06f * hurtKnockbackScale, startLocalPosition, eased);
            visualRoot.localRotation = Quaternion.Slerp(startLocalRotation * Quaternion.Euler(6f, 0f, -5f), startLocalRotation, eased);
            visualRoot.localScale = Vector3.Lerp(startLocalScale * 1.04f, startLocalScale, eased);
        }, true);

        EndTransient();
    }

    private IEnumerator DeathRoutine()
    {
        transientLocked = true;
        RestoreRendererColors();

        yield return AnimatePhase(deathDuration * 0.28f, t =>
        {
            float collapse = EaseOutCubic(t);
            visualRoot.localPosition = startLocalPosition + Vector3.down * 0.08f * deathFallScale * collapse;
            visualRoot.localRotation = startLocalRotation * Quaternion.Euler(-18f * deathFallScale * collapse, 0f, 24f * collapse);
            visualRoot.localScale = new Vector3(startLocalScale.x * (1f + 0.08f * collapse), startLocalScale.y * (1f - 0.18f * collapse), startLocalScale.z * (1f + 0.06f * collapse));
        });

        yield return AnimatePhase(deathDuration * 0.48f, t =>
        {
            float fall = EaseInOutCubic(t);
            visualRoot.localPosition = startLocalPosition + Vector3.down * (0.08f + 0.18f * deathFallScale * fall) + Vector3.right * 0.05f * deathFallScale * fall;
            visualRoot.localRotation = startLocalRotation * Quaternion.Euler(-18f - 62f * deathFallScale * fall, 0f, 24f + 72f * deathFallScale * fall);
            visualRoot.localScale = Vector3.Lerp(startLocalScale, startLocalScale * 0.74f, fall);
            SetRendererAlpha(1f - 0.35f * fall);
        });

        yield return AnimatePhase(deathDuration * 0.24f, t =>
        {
            float fade = EaseOutCubic(t);
            visualRoot.localPosition = startLocalPosition + Vector3.down * (0.26f * deathFallScale + 0.08f * fade) + Vector3.right * 0.05f * deathFallScale;
            visualRoot.localRotation = startLocalRotation * Quaternion.Euler(-80f * deathFallScale, 0f, 96f * deathFallScale);
            visualRoot.localScale = Vector3.Lerp(startLocalScale * 0.74f, startLocalScale * 0.48f, fade);
            SetRendererAlpha(0.65f * (1f - fade));
        });

        currentState = CharacterAnimState.Death;
    }

    private IEnumerator ChargeWindupRoutine(Vector3 direction, float duration)
    {
        transientLocked = true;
        direction = NormalizeFlatDirection(direction);
        Vector3 localDirection = transform.InverseTransformDirection(direction).normalized;

        yield return AnimatePhase(duration, t =>
        {
            float pulse = Mathf.Sin(t * Mathf.PI * 5f) * (1f - t);
            float charge = EaseOutCubic(t);
            visualRoot.localPosition = startLocalPosition - localDirection * 0.18f * charge + Vector3.down * 0.06f * charge;
            visualRoot.localRotation = startLocalRotation * Quaternion.Euler(-18f * charge, 0f, pulse * 10f);
            visualRoot.localScale = new Vector3(startLocalScale.x * (1f + 0.14f * charge), startLocalScale.y * (1f - 0.18f * charge), startLocalScale.z * (1f + 0.2f * charge));
        });

        EndTransient();
    }

    private IEnumerator SpitWindupRoutine(Vector3 direction, float duration)
    {
        transientLocked = true;
        direction = NormalizeFlatDirection(direction);
        Vector3 localDirection = transform.InverseTransformDirection(direction).normalized;

        yield return AnimatePhase(duration, t =>
        {
            float swell = Mathf.Sin(t * Mathf.PI);
            float jitter = Mathf.Sin(t * Mathf.PI * 8f) * 0.025f;
            visualRoot.localPosition = startLocalPosition - localDirection * 0.04f * swell + Vector3.up * jitter;
            visualRoot.localRotation = startLocalRotation * Quaternion.Euler(8f * swell, 0f, Mathf.Sin(t * Mathf.PI * 5f) * 7f);
            visualRoot.localScale = new Vector3(startLocalScale.x * (1f + 0.24f * swell), startLocalScale.y * (1f + 0.12f * swell), startLocalScale.z * (1f + 0.24f * swell));
        });

        yield return AnimatePhase(0.12f, t =>
        {
            float eased = EaseOutBack(t);
            visualRoot.localPosition = Vector3.Lerp(startLocalPosition + localDirection * 0.07f, startLocalPosition, eased);
            visualRoot.localRotation = Quaternion.Slerp(startLocalRotation * Quaternion.Euler(-7f, 0f, 0f), startLocalRotation, eased);
            visualRoot.localScale = Vector3.Lerp(new Vector3(startLocalScale.x * 0.86f, startLocalScale.y * 1.1f, startLocalScale.z * 0.86f), startLocalScale, eased);
        });

        EndTransient();
    }

    private IEnumerator AnimatePhase(float duration, System.Action<float> apply, bool unscaled = false)
    {
        float elapsed = 0f;
        duration = Mathf.Max(0.001f, duration);
        while (elapsed < duration)
        {
            elapsed += unscaled ? Time.unscaledDeltaTime : Time.deltaTime;
            apply(Mathf.Clamp01(elapsed / duration));
            yield return null;
        }
    }

    private void EndTransient()
    {
        transientLocked = false;
        transientRoutine = null;
        if (!dead)
        {
            currentState = moveVelocity.sqrMagnitude > 0.04f ? CharacterAnimState.Move : CharacterAnimState.Idle;
            RestoreBaseTransform();
        }
    }

    private void RestoreBaseTransform()
    {
        if (visualRoot == null)
        {
            return;
        }

        visualRoot.localPosition = startLocalPosition;
        visualRoot.localRotation = startLocalRotation;
        visualRoot.localScale = startLocalScale;
    }

    private void CacheVisualRoot()
    {
        if (visualRoot == null)
        {
            Transform kayKitVisual = transform.Find("KayKitVisual");
            visualRoot = kayKitVisual != null ? kayKitVisual : transform;
        }

        startLocalPosition = visualRoot.localPosition;
        startLocalRotation = visualRoot.localRotation;
        startLocalScale = visualRoot.localScale;
    }

    private void CacheRenderers()
    {
        renderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                originalColors[i] = renderers[i].material.color;
            }
        }
    }

    private Vector3 NormalizeFlatDirection(Vector3 direction)
    {
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.001f)
        {
            direction = facingDirection.sqrMagnitude > 0.001f ? facingDirection : transform.forward;
        }

        return direction.normalized;
    }

    private void SetRendererColor(Color color)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && renderers[i].enabled)
            {
                renderers[i].material.color = color;
            }
        }
    }

    private void RestoreRendererColors()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && renderers[i].enabled)
            {
                renderers[i].material.color = originalColors[i];
            }
        }
    }

    private void SetRendererAlpha(float alpha)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null || !renderers[i].enabled)
            {
                continue;
            }

            Color color = renderers[i].material.color;
            color.a = alpha;
            renderers[i].material.color = color;
        }
    }

    private float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }

    private float EaseInOutCubic(float t)
    {
        return t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) * 0.5f;
    }

    private float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}
