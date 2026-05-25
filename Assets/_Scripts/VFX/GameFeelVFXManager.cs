using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class GameFeelVFXManager : MonoBehaviour
{
    public static GameFeelVFXManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = new GameObject("GameFeelVFXManager");
                instance = obj.AddComponent<GameFeelVFXManager>();
                DontDestroyOnLoad(obj);
            }

            return instance;
        }
    }

    public int sparkPoolSize = 16;
    public int bloodPoolSize = 10;
    public Color playerFlashColor = Color.white;
    public Color enemySparkColor = new Color(1f, 0.68f, 0.18f, 1f);
    public Color playerBloodColor = new Color(0.9f, 0.04f, 0.03f, 1f);

    private static GameFeelVFXManager instance;
    private readonly Queue<ParticleSystem> sparkPool = new Queue<ParticleSystem>();
    private readonly Queue<ParticleSystem> bloodPool = new Queue<ParticleSystem>();
    private readonly List<Image> glitchBars = new List<Image>();
    private Material particleMaterial;
    private Canvas overlayCanvas;
    private Image redFlashImage;
    private Coroutine redFlashRoutine;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        BuildSharedMaterial();
        WarmPools();
    }

    public void PlayPlayerDamaged(GameObject player, Vector3 hitDirection)
    {
        if (player == null)
        {
            return;
        }

        Vector3 hitPosition = player.transform.position + Vector3.up * 0.9f;
        PlayBloodBurst(hitPosition, hitDirection);
        FlashWhite(player, 0.075f);
        PunchModel(player.transform, hitDirection, 0.075f, 0.09f);
        FullscreenRedGlitch(0.22f);
    }

    public void PlayEnemyHit(GameObject enemy, Vector3 attackerPosition, bool critical)
    {
        if (enemy == null)
        {
            return;
        }

        Vector3 direction = enemy.transform.position - attackerPosition;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.001f)
        {
            direction = enemy.transform.forward;
        }

        Vector3 hitPosition = enemy.transform.position + Vector3.up * 0.75f - direction.normalized * 0.18f;
        PlaySparkBurst(hitPosition, direction.normalized, critical);
        FlashWhite(enemy, critical ? 0.105f : 0.065f);
    }

    public void PlayEnemyDeathFracture(GameObject enemy, Color baseColor)
    {
        if (enemy == null)
        {
            return;
        }

        Bounds bounds = CalculateBounds(enemy);
        int shardCount = enemy.GetComponent<EnemyStats>() != null && enemy.GetComponent<EnemyStats>().isBoss ? 22 : 12;
        GameObject root = new GameObject(enemy.name + "_FracturedBody");
        root.transform.position = bounds.center;

        for (int i = 0; i < shardCount; i++)
        {
            GameObject shard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shard.name = "FractureShard";
            shard.transform.SetParent(root.transform);

            Vector3 randomOffset = new Vector3(
                Random.Range(-bounds.extents.x, bounds.extents.x),
                Random.Range(-bounds.extents.y * 0.3f, bounds.extents.y),
                Random.Range(-bounds.extents.z, bounds.extents.z));
            shard.transform.position = bounds.center + randomOffset;

            float size = Random.Range(0.12f, 0.32f) * Mathf.Max(0.75f, bounds.size.magnitude * 0.28f);
            shard.transform.localScale = new Vector3(size * Random.Range(0.55f, 1.25f), size * Random.Range(0.35f, 0.9f), size * Random.Range(0.55f, 1.25f));
            shard.transform.rotation = Random.rotation;

            Renderer renderer = shard.GetComponent<Renderer>();
            Material material = new Material(Shader.Find("Standard"));
            material.color = Color.Lerp(baseColor, Random.value > 0.5f ? Color.white : Color.black, Random.Range(0.05f, 0.22f));
            renderer.material = material;

            Rigidbody body = shard.AddComponent<Rigidbody>();
            body.mass = 0.12f;
            body.drag = 1.3f;
            body.angularDrag = 1.6f;
            Vector3 forceDirection = (randomOffset + Vector3.up * Random.Range(0.6f, 1.6f)).normalized;
            body.AddForce(forceDirection * Random.Range(1.6f, 4.2f), ForceMode.Impulse);
            body.AddTorque(Random.insideUnitSphere * Random.Range(1.5f, 5f), ForceMode.Impulse);

            Collider collider = shard.GetComponent<Collider>();
            collider.isTrigger = false;
        }

        PlaySparkBurst(bounds.center + Vector3.up * 0.2f, Vector3.up, true);
        StartCoroutine(FadeAndDestroyFracture(root, 1.35f, 0.65f));
    }

    public void HideLiveEnemy(GameObject enemy)
    {
        if (enemy == null)
        {
            return;
        }

        foreach (Renderer renderer in enemy.GetComponentsInChildren<Renderer>())
        {
            if (renderer != null)
            {
                renderer.enabled = false;
            }
        }

        foreach (Collider collider in enemy.GetComponentsInChildren<Collider>())
        {
            if (collider != null)
            {
                collider.enabled = false;
            }
        }

        NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.enabled = false;
        }
    }

    private void BuildSharedMaterial()
    {
        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null)
        {
            shader = Shader.Find("Particles/Standard Unlit");
        }

        particleMaterial = new Material(shader);
    }

    private void WarmPools()
    {
        for (int i = 0; i < sparkPoolSize; i++)
        {
            sparkPool.Enqueue(CreateParticleObject("PooledHitSpark", true));
        }

        for (int i = 0; i < bloodPoolSize; i++)
        {
            bloodPool.Enqueue(CreateParticleObject("PooledPlayerBlood", false));
        }
    }

    private ParticleSystem CreateParticleObject(string objectName, bool sparks)
    {
        GameObject obj = new GameObject(objectName);
        obj.transform.SetParent(transform);
        obj.SetActive(false);

        ParticleSystem particles = obj.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = particles.main;
        main.loop = false;
        main.duration = sparks ? 0.16f : 0.24f;
        main.startLifetime = sparks
            ? new ParticleSystem.MinMaxCurve(0.12f, 0.28f)
            : new ParticleSystem.MinMaxCurve(0.18f, 0.42f);
        main.startSpeed = sparks
            ? new ParticleSystem.MinMaxCurve(3.5f, 7.5f)
            : new ParticleSystem.MinMaxCurve(1.4f, 4.6f);
        main.startSize = sparks
            ? new ParticleSystem.MinMaxCurve(0.045f, 0.12f)
            : new ParticleSystem.MinMaxCurve(0.065f, 0.18f);
        main.gravityModifier = sparks ? 0.28f : 1.25f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.useUnscaledTime = true;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, sparks ? (short)18 : (short)12, sparks ? (short)26 : (short)18) });

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = sparks ? 28f : 42f;
        shape.radius = 0.08f;

        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        Color start = sparks ? enemySparkColor : playerBloodColor;
        gradient.SetKeys(
            new[] { new GradientColorKey(start, 0f), new GradientColorKey(Color.black, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
        colorOverLifetime.color = gradient;

        ParticleSystemRenderer renderer = obj.GetComponent<ParticleSystemRenderer>();
        renderer.material = particleMaterial;
        renderer.renderMode = ParticleSystemRenderMode.Billboard;

        return particles;
    }

    private void PlaySparkBurst(Vector3 position, Vector3 direction, bool critical)
    {
        ParticleSystem particles = sparkPool.Count > 0 ? sparkPool.Dequeue() : CreateParticleObject("PooledHitSpark", true);
        particles.transform.position = position;
        particles.transform.rotation = Quaternion.LookRotation(direction.sqrMagnitude > 0.001f ? direction : Vector3.forward);

        ParticleSystem.MainModule main = particles.main;
        main.startColor = critical ? new Color(1f, 0.95f, 0.25f, 1f) : enemySparkColor;
        main.startSpeed = critical ? new ParticleSystem.MinMaxCurve(5f, 10f) : new ParticleSystem.MinMaxCurve(3.5f, 7.5f);

        particles.gameObject.SetActive(true);
        particles.Play(true);
        StartCoroutine(ReleaseParticlesAfter(particles, sparkPool, 0.55f));
    }

    private void PlayBloodBurst(Vector3 position, Vector3 direction)
    {
        ParticleSystem particles = bloodPool.Count > 0 ? bloodPool.Dequeue() : CreateParticleObject("PooledPlayerBlood", false);
        particles.transform.position = position;
        particles.transform.rotation = Quaternion.LookRotation(direction.sqrMagnitude > 0.001f ? direction : Vector3.forward);

        ParticleSystem.MainModule main = particles.main;
        main.startColor = playerBloodColor;
        particles.gameObject.SetActive(true);
        particles.Play(true);
        StartCoroutine(ReleaseParticlesAfter(particles, bloodPool, 0.7f));
    }

    private IEnumerator ReleaseParticlesAfter(ParticleSystem particles, Queue<ParticleSystem> pool, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        if (particles == null)
        {
            yield break;
        }

        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        particles.gameObject.SetActive(false);
        pool.Enqueue(particles);
    }

    private void FlashWhite(GameObject target, float duration)
    {
        StartCoroutine(FlashWhiteRoutine(target, duration));
    }

    private IEnumerator FlashWhiteRoutine(GameObject target, float duration)
    {
        if (target == null)
        {
            yield break;
        }

        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
        Color[] originals = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null)
            {
                continue;
            }

            originals[i] = renderers[i].material.color;
            renderers[i].material.color = playerFlashColor;
        }

        yield return new WaitForSecondsRealtime(duration);

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && renderers[i].enabled)
            {
                renderers[i].material.color = originals[i];
            }
        }
    }

    private void PunchModel(Transform target, Vector3 hitDirection, float distance, float duration)
    {
        StartCoroutine(PunchModelRoutine(target, hitDirection, distance, duration));
    }

    private IEnumerator PunchModelRoutine(Transform target, Vector3 hitDirection, float distance, float duration)
    {
        if (target == null)
        {
            yield break;
        }

        Vector3 origin = target.localPosition;
        Vector3 direction = -hitDirection;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.001f)
        {
            direction = -target.forward;
        }

        direction.Normalize();
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float punch = Mathf.Sin(t * Mathf.PI);
            target.localPosition = origin + direction * distance * punch;
            yield return null;
        }

        if (target != null)
        {
            target.localPosition = origin;
        }
    }

    private void FullscreenRedGlitch(float duration)
    {
        EnsureOverlay();
        if (redFlashRoutine != null)
        {
            StopCoroutine(redFlashRoutine);
        }

        redFlashRoutine = StartCoroutine(FullscreenRedGlitchRoutine(duration));
    }

    private void EnsureOverlay()
    {
        if (overlayCanvas != null)
        {
            return;
        }

        GameObject canvasObj = new GameObject("GameFeelOverlayCanvas");
        DontDestroyOnLoad(canvasObj);
        overlayCanvas = canvasObj.AddComponent<Canvas>();
        overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        overlayCanvas.sortingOrder = 900;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject flashObj = new GameObject("FullscreenRedGlitch");
        flashObj.transform.SetParent(canvasObj.transform, false);
        redFlashImage = flashObj.AddComponent<Image>();
        redFlashImage.raycastTarget = false;
        redFlashImage.color = Color.clear;
        RectTransform flashRect = redFlashImage.rectTransform;
        flashRect.anchorMin = Vector2.zero;
        flashRect.anchorMax = Vector2.one;
        flashRect.offsetMin = Vector2.zero;
        flashRect.offsetMax = Vector2.zero;

        for (int i = 0; i < 5; i++)
        {
            GameObject barObj = new GameObject("GlitchBar_" + i);
            barObj.transform.SetParent(canvasObj.transform, false);
            Image bar = barObj.AddComponent<Image>();
            bar.raycastTarget = false;
            bar.color = Color.clear;
            RectTransform rect = bar.rectTransform;
            rect.anchorMin = new Vector2(0f, Random.Range(0.05f, 0.9f));
            rect.anchorMax = new Vector2(1f, rect.anchorMin.y);
            rect.sizeDelta = new Vector2(0f, Random.Range(5f, 18f));
            glitchBars.Add(bar);
        }
    }

    private IEnumerator FullscreenRedGlitchRoutine(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float alpha = Mathf.Lerp(0.36f, 0f, t);
            redFlashImage.color = new Color(1f, 0.03f, 0.02f, alpha);

            for (int i = 0; i < glitchBars.Count; i++)
            {
                Image bar = glitchBars[i];
                RectTransform rect = bar.rectTransform;
                rect.anchoredPosition = new Vector2(Random.Range(-22f, 22f), Random.Range(-360f, 360f));
                rect.sizeDelta = new Vector2(0f, Random.Range(4f, 16f));
                bar.color = new Color(1f, 0.05f, 0.02f, alpha * Random.Range(0.18f, 0.55f));
            }

            yield return null;
        }

        redFlashImage.color = Color.clear;
        foreach (Image bar in glitchBars)
        {
            bar.color = Color.clear;
        }

        redFlashRoutine = null;
    }

    private Bounds CalculateBounds(GameObject target)
    {
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            return new Bounds(target.transform.position + Vector3.up * 0.7f, Vector3.one);
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return bounds;
    }

    private IEnumerator FadeAndDestroyFracture(GameObject root, float hold, float fade)
    {
        yield return new WaitForSeconds(hold);
        if (root == null)
        {
            yield break;
        }

        Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
        float elapsed = 0f;
        while (elapsed < fade)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fade);
            foreach (Renderer renderer in renderers)
            {
                if (renderer == null)
                {
                    continue;
                }

                Color color = renderer.material.color;
                color.a = alpha;
                renderer.material.color = color;
            }

            yield return null;
        }

        Destroy(root);
    }
}
