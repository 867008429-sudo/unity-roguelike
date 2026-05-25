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

public class CharacterAnimationController : MonoBehaviour
{
    public Transform visualRoot;
    public float idleBobHeight = 0.035f;
    public float idleBobSpeed = 2.6f;
    public float moveSwayAngle = 5f;
    public float movePulse = 0.045f;
    public float moveSpeed = 8f;
    public Color hurtFlashColor = Color.white;
    public float hurtDuration = 0.16f;
    public float deathDuration = 0.55f;

    private CharacterAnimState currentState = CharacterAnimState.Idle;
    private Vector3 startLocalPosition;
    private Quaternion startLocalRotation;
    private Vector3 startLocalScale;
    private Renderer[] renderers;
    private Color[] originalColors;
    private Coroutine transientRoutine;
    private bool transientLocked;
    private bool dead;

    private void Awake()
    {
        CacheVisualRoot();
        CacheRenderers();
    }

    private void Update()
    {
        if (dead || visualRoot == null || transientLocked)
        {
            return;
        }

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
        PlayTransient(CharacterAnimState.Attack, AttackRoutine(direction));
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
        }

        currentState = state;
        transientRoutine = StartCoroutine(routine);
    }

    private void ApplyIdleLoop()
    {
        float bob = Mathf.Sin(Time.time * idleBobSpeed) * idleBobHeight;
        visualRoot.localPosition = startLocalPosition + Vector3.up * bob;
        visualRoot.localRotation = Quaternion.Slerp(visualRoot.localRotation, startLocalRotation, Time.deltaTime * 10f);
        visualRoot.localScale = Vector3.Lerp(visualRoot.localScale, startLocalScale, Time.deltaTime * 10f);
    }

    private void ApplyMoveLoop()
    {
        float wave = Mathf.Sin(Time.time * moveSpeed);
        visualRoot.localPosition = startLocalPosition + Vector3.up * Mathf.Abs(wave) * idleBobHeight * 0.8f;
        visualRoot.localRotation = startLocalRotation * Quaternion.Euler(0f, 0f, wave * moveSwayAngle);
        visualRoot.localScale = startLocalScale + new Vector3(movePulse * Mathf.Abs(wave), -movePulse * 0.55f * Mathf.Abs(wave), movePulse * 0.25f * Mathf.Abs(wave));
    }

    private IEnumerator AttackRoutine(Vector3 direction)
    {
        transientLocked = true;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.001f)
        {
            direction = transform.forward;
        }

        direction.Normalize();
        float duration = 0.18f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float punch = Mathf.Sin(t * Mathf.PI);
            visualRoot.localPosition = startLocalPosition + direction * punch * 0.12f;
            visualRoot.localRotation = startLocalRotation * Quaternion.Euler(8f * punch, -28f * punch, 0f);
            visualRoot.localScale = startLocalScale * (1f + punch * 0.08f);
            yield return null;
        }

        EndTransient();
    }

    private IEnumerator DashRoutine(Vector3 direction)
    {
        transientLocked = true;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.001f)
        {
            direction = transform.forward;
        }

        direction.Normalize();
        float duration = 0.18f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float stretch = Mathf.Sin(t * Mathf.PI);
            visualRoot.localPosition = startLocalPosition - direction * stretch * 0.05f;
            visualRoot.localScale = new Vector3(startLocalScale.x * (1f - stretch * 0.12f), startLocalScale.y * (1f + stretch * 0.08f), startLocalScale.z * (1f + stretch * 0.22f));
            yield return null;
        }

        EndTransient();
    }

    private IEnumerator HurtRoutine(Vector3 hitDirection)
    {
        transientLocked = true;
        hitDirection.y = 0f;
        if (hitDirection.sqrMagnitude < 0.001f)
        {
            hitDirection = -transform.forward;
        }

        hitDirection.Normalize();
        SetRendererColor(hurtFlashColor);

        float elapsed = 0f;
        while (elapsed < hurtDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / hurtDuration);
            float punch = Mathf.Sin(t * Mathf.PI);
            visualRoot.localPosition = startLocalPosition + hitDirection.normalized * punch * 0.12f;
            visualRoot.localRotation = startLocalRotation * Quaternion.Euler(-10f * punch, 0f, 8f * punch);
            yield return null;
        }

        RestoreRendererColors();
        EndTransient();
    }

    private IEnumerator DeathRoutine()
    {
        transientLocked = true;
        Vector3 startScale = visualRoot.localScale;
        float elapsed = 0f;
        while (elapsed < deathDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / deathDuration);
            visualRoot.localPosition = startLocalPosition + Vector3.down * (0.25f * t);
            visualRoot.localRotation = startLocalRotation * Quaternion.Euler(0f, 0f, 120f * t);
            visualRoot.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            SetRendererAlpha(1f - t);
            yield return null;
        }

        currentState = CharacterAnimState.Death;
    }

    private void EndTransient()
    {
        transientLocked = false;
        transientRoutine = null;
        if (!dead)
        {
            currentState = CharacterAnimState.Idle;
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
}
