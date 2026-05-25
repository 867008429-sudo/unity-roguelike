using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VisualEffectsManager : MonoBehaviour
{
    private static VisualEffectsManager instance;

    public static VisualEffectsManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = new GameObject("VisualEffectsManager");
                instance = obj.AddComponent<VisualEffectsManager>();
                DontDestroyOnLoad(obj);
            }

            return instance;
        }
    }

    public int pooledParticles = 12;
    public int pooledSlashes = 8;
    public Color enemySlashColor = new Color(1f, 0.18f, 0.08f, 0.82f);
    public Color playerHitColor = new Color(1f, 0.1f, 0.08f, 0.9f);
    public float playerScreenFlashDuration = 0.18f;
    public string enemySlashTextureResource = "Kenney/Particles/slash_01";
    public string hitParticleTextureResource = "Kenney/Particles/spark_01";
    public string levelUpTextureResource = "Kenney/Particles/star_07";
    public string telegraphTextureResource = "Kenney/Particles/scratch_01";

    private readonly Queue<ParticleSystem> particlePool = new Queue<ParticleSystem>();
    private readonly Queue<GameObject> slashPool = new Queue<GameObject>();
    private Material vfxMaterial;
    private Texture2D enemySlashTexture;
    private Texture2D hitParticleTexture;
    private Texture2D levelUpTexture;
    private Texture2D telegraphTexture;
    private Image screenFlash;
    private Coroutine screenFlashRoutine;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        BuildMaterial();
        LoadKenneyTextures();
        WarmPools();
    }

    public void PlayEnemySlash(Vector3 position, Vector3 direction, float length, float width)
    {
        PlaySlash(position, direction, length, width, enemySlashColor, enemySlashTexture, 0.34f, 1.8f, 1.05f);
    }

    public void PlayPlayerSlash(Vector3 position, Vector3 direction, int comboStep, bool empowered)
    {
        int step = Mathf.Clamp(comboStep, 1, 3);
        float length = step == 1 ? 1.55f : step == 2 ? 1.95f : 2.45f;
        float width = step == 1 ? 0.38f : step == 2 ? 0.5f : 0.68f;
        float duration = step == 3 ? 0.42f : 0.3f;
        float height = step == 3 ? 1.12f : 0.98f;
        float expandSpeed = step == 3 ? 2.45f : 1.8f;
        Color color = step == 1
            ? new Color(1f, 0.72f, 0.22f, 0.82f)
            : step == 2
                ? new Color(1f, 0.46f, 0.16f, 0.88f)
                : new Color(1f, 0.92f, 0.32f, 0.96f);

        if (empowered)
        {
            color = Color.Lerp(color, new Color(0.45f, 0.9f, 1f, 0.96f), 0.28f);
        }

        PlaySlash(position, direction, length, width, color, enemySlashTexture, duration, expandSpeed, height);

        if (step == 3)
        {
            PlayGroundPulse(position, color, 1.35f, 0.28f);
        }
    }

    public void PlayGroundPulse(Vector3 position, Color color, float radius, float duration)
    {
        GameObject pulse = GetSlash();
        pulse.transform.position = position + Vector3.up * 0.08f;
        pulse.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        pulse.transform.localScale = Vector3.one * Mathf.Max(0.1f, radius);
        pulse.SetActive(true);

        Renderer renderer = pulse.GetComponent<Renderer>();
        renderer.material.mainTexture = telegraphTexture != null ? telegraphTexture : hitParticleTexture;
        Color pulseColor = color;
        pulseColor.a = Mathf.Min(color.a, 0.58f);
        renderer.material.color = pulseColor;

        StartCoroutine(ReleasePulseAfter(pulse, duration, radius));
    }

    private void PlaySlash(Vector3 position, Vector3 direction, float length, float width, Color color, Texture2D texture, float duration, float expandSpeed, float height)
    {
        if (direction.sqrMagnitude < 0.001f)
        {
            direction = Vector3.forward;
        }

        direction.y = 0f;
        direction.Normalize();

        GameObject slash = GetSlash();
        slash.transform.position = position + direction * (length * 0.4f) + Vector3.up * height;
        OrientBillboard(slash.transform, direction);
        slash.transform.localScale = new Vector3(width * 2.2f, length * 1.7f, 1f);
        slash.SetActive(true);

        Renderer renderer = slash.GetComponent<Renderer>();
        renderer.material.color = color;
        if (texture != null)
        {
            renderer.material.mainTexture = texture;
        }

        StartCoroutine(ReleaseSlashAfter(slash, duration, expandSpeed));
    }

    public void PlayPlayerHitFeedback(GameObject player, Vector3 hitDirection)
    {
        if (player == null)
        {
            return;
        }

        Vector3 hitPosition = player.transform.position + Vector3.up * 0.8f;
        PlayHitBurst(hitPosition, playerHitColor);
        StartCoroutine(FlashRenderers(player, Color.white, 0.08f));
        FlashScreen(playerHitColor, playerScreenFlashDuration);
    }

    public void PlayHitBurst(Vector3 position, Color color)
    {
        ParticleSystem particles = GetParticles();
        particles.transform.position = position;

        ParticleSystem.MainModule main = particles.main;
        main.startColor = color;

        particles.gameObject.SetActive(true);
        particles.Play(true);
        StartCoroutine(ReleaseParticlesAfter(particles, main.duration + main.startLifetime.constantMax));
    }

    public void PlayLevelUpBurst(Transform player)
    {
        if (player == null)
        {
            return;
        }

        StartCoroutine(LevelUpBurstRoutine(player));
    }

    public void ShowAttackWarning(Vector3 position, Vector3 direction, float radius, float duration, bool bossTelegraph)
    {
        if (direction.sqrMagnitude < 0.001f)
        {
            direction = Vector3.forward;
        }

        direction.y = 0f;
        direction.Normalize();

        GameObject warning = GetSlash();
        warning.transform.position = position + direction * (radius * 0.42f) + Vector3.up * 0.12f;
        warning.transform.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(90f, 0f, 0f);
        warning.transform.localScale = new Vector3(radius * 0.95f, radius * 1.55f, 1f);
        warning.SetActive(true);

        Renderer renderer = warning.GetComponent<Renderer>();
        renderer.material.mainTexture = telegraphTexture != null ? telegraphTexture : enemySlashTexture;
        renderer.material.color = bossTelegraph
            ? new Color(0.9f, 0.2f, 1f, 0.38f)
            : new Color(1f, 0.08f, 0.02f, 0.42f);

        StartCoroutine(ReleaseWarningAfter(warning, duration));
    }

    public void FlashScreen(Color color, float duration)
    {
        EnsureScreenFlash();
        if (screenFlash == null)
        {
            return;
        }

        if (screenFlashRoutine != null)
        {
            StopCoroutine(screenFlashRoutine);
        }

        screenFlashRoutine = StartCoroutine(ScreenFlashRoutine(color, duration));
    }

    private void BuildMaterial()
    {
        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null)
        {
            shader = Shader.Find("Unlit/Transparent");
        }

        if (shader == null)
        {
            shader = Shader.Find("Unlit/Color");
        }

        vfxMaterial = new Material(shader);
        vfxMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        vfxMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        vfxMaterial.SetInt("_ZWrite", 0);
        vfxMaterial.renderQueue = 3000;
    }

    private void LoadKenneyTextures()
    {
        enemySlashTexture = Resources.Load<Texture2D>(enemySlashTextureResource);
        hitParticleTexture = Resources.Load<Texture2D>(hitParticleTextureResource);
        levelUpTexture = Resources.Load<Texture2D>(levelUpTextureResource);
        telegraphTexture = Resources.Load<Texture2D>(telegraphTextureResource);
    }

    private void WarmPools()
    {
        for (int i = 0; i < pooledParticles; i++)
        {
            ParticleSystem particles = CreatePooledParticles();
            particlePool.Enqueue(particles);
        }

        for (int i = 0; i < pooledSlashes; i++)
        {
            GameObject slash = CreatePooledSlash();
            slashPool.Enqueue(slash);
        }
    }

    private ParticleSystem GetParticles()
    {
        return particlePool.Count > 0 ? particlePool.Dequeue() : CreatePooledParticles();
    }

    private GameObject GetSlash()
    {
        return slashPool.Count > 0 ? slashPool.Dequeue() : CreatePooledSlash();
    }

    private ParticleSystem CreatePooledParticles()
    {
        GameObject obj = new GameObject("PooledHitParticles");
        obj.transform.SetParent(transform);
        obj.SetActive(false);

        ParticleSystem particles = obj.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = particles.main;
        main.duration = 0.2f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.18f, 0.34f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(2.2f, 5.2f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.16f);
        main.gravityModifier = 0.6f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.useUnscaledTime = true;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 10, 16) });

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.16f;

        ParticleSystemRenderer renderer = obj.GetComponent<ParticleSystemRenderer>();
        renderer.material = vfxMaterial;
        if (hitParticleTexture != null)
        {
            renderer.material.mainTexture = hitParticleTexture;
        }

        return particles;
    }

    private GameObject CreatePooledSlash()
    {
        GameObject slash = GameObject.CreatePrimitive(PrimitiveType.Quad);
        slash.name = "PooledEnemySlash";
        slash.transform.SetParent(transform);
        RemoveCollider(slash);
        slash.GetComponent<Renderer>().material = new Material(vfxMaterial);
        slash.SetActive(false);
        return slash;
    }

    private static void RemoveCollider(GameObject target)
    {
        Collider collider = target.GetComponent<Collider>();
        if (collider == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(collider);
        }
        else
        {
            DestroyImmediate(collider);
        }
    }

    private IEnumerator ReleaseParticlesAfter(ParticleSystem particles, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        if (particles == null)
        {
            yield break;
        }

        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        particles.gameObject.SetActive(false);
        particlePool.Enqueue(particles);
    }

    private IEnumerator ReleaseSlashAfter(GameObject slash, float duration, float expandSpeed)
    {
        Renderer renderer = slash.GetComponent<Renderer>();
        Color startColor = renderer.material.color;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            Color color = startColor;
            color.a = Mathf.Lerp(startColor.a, 0f, elapsed / duration);
            renderer.material.color = color;
            slash.transform.localScale *= 1f + Time.unscaledDeltaTime * expandSpeed;
            yield return null;
        }

        slash.SetActive(false);
        slashPool.Enqueue(slash);
    }

    private IEnumerator ReleasePulseAfter(GameObject pulse, float duration, float radius)
    {
        Renderer renderer = pulse.GetComponent<Renderer>();
        Color startColor = renderer.material.color;
        Vector3 startScale = Vector3.one * Mathf.Max(0.1f, radius * 0.45f);
        Vector3 endScale = Vector3.one * Mathf.Max(0.1f, radius * 1.85f);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            pulse.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            Color color = startColor;
            color.a = Mathf.Lerp(startColor.a, 0f, t);
            renderer.material.color = color;
            yield return null;
        }

        pulse.SetActive(false);
        slashPool.Enqueue(pulse);
    }

    private IEnumerator ReleaseWarningAfter(GameObject warning, float duration)
    {
        Renderer renderer = warning.GetComponent<Renderer>();
        Color startColor = renderer.material.color;
        Vector3 startScale = warning.transform.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            warning.transform.localScale = Vector3.Lerp(startScale * 0.75f, startScale * 1.18f, t);

            Color color = startColor;
            color.a = Mathf.Lerp(startColor.a * 0.35f, startColor.a, Mathf.Sin(t * Mathf.PI));
            renderer.material.color = color;
            yield return null;
        }

        warning.SetActive(false);
        slashPool.Enqueue(warning);
    }

    private IEnumerator LevelUpBurstRoutine(Transform player)
    {
        const int count = 14;
        List<GameObject> stars = new List<GameObject>();
        float duration = 1.15f;

        for (int i = 0; i < count; i++)
        {
            float angle = i * (360f / count);
            Vector3 direction = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0f, Mathf.Sin(angle * Mathf.Deg2Rad));
            GameObject star = GetSlash();
            star.transform.position = player.position + Vector3.up * 1.0f + direction * 0.35f;
            OrientBillboard(star.transform, direction);
            star.transform.localScale = Vector3.one * 0.55f;

            Renderer renderer = star.GetComponent<Renderer>();
            renderer.material.mainTexture = levelUpTexture != null ? levelUpTexture : hitParticleTexture;
            renderer.material.color = new Color(1f, 0.92f, 0.18f, 0.95f);
            star.SetActive(true);
            stars.Add(star);
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (player == null)
            {
                break;
            }

            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            for (int i = 0; i < stars.Count; i++)
            {
                float angle = i * (360f / count) + elapsed * 160f;
                Vector3 direction = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0f, Mathf.Sin(angle * Mathf.Deg2Rad));
                GameObject star = stars[i];
                star.transform.position = player.position + Vector3.up * (0.65f + t * 1.4f) + direction * Mathf.Lerp(0.4f, 2.0f, t);
                OrientBillboard(star.transform, direction);

                Renderer renderer = star.GetComponent<Renderer>();
                Color color = renderer.material.color;
                color.a = Mathf.Lerp(0.95f, 0f, t);
                renderer.material.color = color;
            }

            yield return null;
        }

        foreach (GameObject star in stars)
        {
            if (star == null)
            {
                continue;
            }

            star.SetActive(false);
            slashPool.Enqueue(star);
        }
    }

    private static void OrientBillboard(Transform target, Vector3 direction)
    {
        Camera camera = Camera.main;
        if (camera != null)
        {
            target.rotation = camera.transform.rotation;
            float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            target.Rotate(0f, 0f, -angle, Space.Self);
            return;
        }

        target.rotation = Quaternion.LookRotation(direction);
    }

    private IEnumerator FlashRenderers(GameObject target, Color flashColor, float duration)
    {
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
        Color[] originalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            originalColors[i] = renderers[i].material.color;
            renderers[i].material.color = flashColor;
        }

        yield return new WaitForSecondsRealtime(duration);

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].material.color = originalColors[i];
            }
        }
    }

    private void EnsureScreenFlash()
    {
        if (screenFlash != null)
        {
            return;
        }

        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("DamageFlashCanvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        GameObject flashObject = new GameObject("DamageScreenFlash");
        flashObject.transform.SetParent(canvas.transform, false);
        screenFlash = flashObject.AddComponent<Image>();
        screenFlash.raycastTarget = false;
        screenFlash.color = Color.clear;

        RectTransform rect = screenFlash.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        flashObject.transform.SetAsLastSibling();
    }

    private IEnumerator ScreenFlashRoutine(Color color, float duration)
    {
        float elapsed = 0f;
        color.a = 0.38f;
        screenFlash.color = color;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            Color next = color;
            next.a = Mathf.Lerp(color.a, 0f, elapsed / duration);
            screenFlash.color = next;
            yield return null;
        }

        screenFlash.color = Color.clear;
        screenFlashRoutine = null;
    }
}
