using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleEffects : MonoBehaviour
{
    private static ParticleEffects instance;

    public static ParticleEffects Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = new GameObject("ParticleEffectsManager");
                instance = obj.AddComponent<ParticleEffects>();
                DontDestroyOnLoad(obj);
            }

            return instance;
        }
    }

    private Material transparentMaterial;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        Shader shader = Shader.Find("Unlit/Color");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        transparentMaterial = new Material(shader);
        transparentMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        transparentMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        transparentMaterial.SetInt("_ZWrite", 0);
        transparentMaterial.renderQueue = 3000;
    }

    public void PlaySlashArc(Vector3 position, Vector3 direction)
    {
        VisualEffectsManager.Instance.PlayEnemySlash(position, direction, 1.7f, 0.45f);
    }

    public void PlayHitParticles(Vector3 position, Color particleColor)
    {
        VisualEffectsManager.Instance.PlayHitBurst(position, particleColor);
    }

    public void PlayDeathExplosion(Vector3 position, Color particleColor)
    {
        StartCoroutine(DeathExplosionRoutine(position, particleColor));
    }

    public void PlayLevelUpAura(Transform player)
    {
        VisualEffectsManager.Instance.PlayLevelUpBurst(player);
    }

    public void ShowCooldown(Transform parent, float cooldownTime)
    {
        StartCoroutine(CooldownIndicatorRoutine(parent, cooldownTime));
    }

    public void ShowAttackTelegraph(Vector3 position, float radius, float duration, bool bossTelegraph)
    {
        Vector3 direction = Vector3.forward;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            direction = player.transform.position - position;
            direction.y = 0f;
        }

        VisualEffectsManager.Instance.ShowAttackWarning(position, direction, radius, duration, bossTelegraph);
    }

    private IEnumerator SlashArcRoutine(Vector3 position, Vector3 direction)
    {
        int segments = 10;
        float totalAngle = 140f;
        float radius = 1.6f;
        float duration = 0.2f;

        GameObject root = new GameObject("SlashArc");
        root.transform.position = position;
        if (direction.sqrMagnitude > 0.001f)
        {
            root.transform.rotation = Quaternion.LookRotation(direction);
        }

        List<GameObject> quads = new List<GameObject>();
        for (int i = 0; i < segments; i++)
        {
            float t = (float)i / (segments - 1);
            float angle = -totalAngle * 0.5f + totalAngle * t;
            float radians = angle * Mathf.Deg2Rad;

            GameObject quad = CreatePrimitiveQuad();
            quad.transform.SetParent(root.transform);
            quad.transform.localPosition = new Vector3(Mathf.Sin(radians) * radius, 0.6f, Mathf.Cos(radians) * radius);
            quad.transform.localRotation = Quaternion.Euler(90f, 0f, -angle);
            quad.transform.localScale = new Vector3(0.25f, 0.9f, 1f);

            Material mat = new Material(transparentMaterial);
            float brightness = 1f - Mathf.Abs(t - 0.5f) * 2f;
            mat.color = new Color(1f, 0.65f, 0.15f, 0.75f * brightness);
            quad.GetComponent<Renderer>().material = mat;
            quads.Add(quad);
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - elapsed / duration;
            root.transform.position = position + direction.normalized * (elapsed * 1.2f);

            for (int i = 0; i < quads.Count; i++)
            {
                Renderer renderer = quads[i].GetComponent<Renderer>();
                float t = (float)i / (segments - 1);
                float brightness = 1f - Mathf.Abs(t - 0.5f) * 2f;
                renderer.material.color = new Color(1f, 0.65f, 0.15f, alpha * 0.75f * brightness);
            }

            yield return null;
        }

        Destroy(root);
    }

    private IEnumerator HitParticlesRoutine(Vector3 position, Color color)
    {
        int count = Random.Range(3, 6);
        List<GameObject> particles = new List<GameObject>();
        List<Vector3> velocities = new List<Vector3>();

        for (int i = 0; i < count; i++)
        {
            GameObject particle = CreatePrimitiveSphere();
            particle.transform.position = position + Random.insideUnitSphere * 0.2f;
            particle.transform.localScale = Vector3.one * Random.Range(0.08f, 0.15f);
            Material mat = new Material(transparentMaterial);
            mat.color = color;
            particle.GetComponent<Renderer>().material = mat;
            particles.Add(particle);

            Vector3 velocity = Random.onUnitSphere * Random.Range(3f, 6f);
            velocity.y = Mathf.Abs(velocity.y) + 1.5f;
            velocities.Add(velocity);
        }

        float duration = 0.4f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            for (int i = 0; i < particles.Count; i++)
            {
                velocities[i] += Vector3.down * 12f * Time.deltaTime;
                particles[i].transform.position += velocities[i] * Time.deltaTime;
                particles[i].transform.localScale = Vector3.Lerp(particles[i].transform.localScale, Vector3.zero, Time.deltaTime * 8f);

                Renderer renderer = particles[i].GetComponent<Renderer>();
                Color particleColor = renderer.material.color;
                particleColor.a = Mathf.Lerp(particleColor.a, 0f, Time.deltaTime * 5f);
                renderer.material.color = particleColor;
            }

            yield return null;
        }

        foreach (GameObject particle in particles)
        {
            if (particle != null)
            {
                Destroy(particle);
            }
        }
    }

    private IEnumerator DeathExplosionRoutine(Vector3 position, Color color)
    {
        int count = 18;
        float duration = 0.55f;
        List<GameObject> particles = new List<GameObject>();
        List<Vector3> velocities = new List<Vector3>();

        for (int i = 0; i < count; i++)
        {
            GameObject particle = CreatePrimitiveSphere();
            particle.transform.position = position;
            particle.transform.localScale = Vector3.one * Random.Range(0.1f, 0.28f);
            Material mat = new Material(transparentMaterial);
            mat.color = color;
            particle.GetComponent<Renderer>().material = mat;
            particles.Add(particle);

            Vector3 velocity = Random.onUnitSphere * Random.Range(3f, 8f);
            velocity.y = Mathf.Abs(velocity.y) + 2f;
            velocities.Add(velocity);
        }

        GameObject flash = CreatePrimitiveSphere();
        flash.transform.position = position;
        flash.transform.localScale = Vector3.one * 0.4f;
        Material flashMat = new Material(transparentMaterial);
        flashMat.color = new Color(color.r, color.g, color.b, 0.9f);
        flash.GetComponent<Renderer>().material = flashMat;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            flash.transform.localScale = Vector3.one * (0.4f + t * 2.5f);
            Color flashColor = flashMat.color;
            flashColor.a = Mathf.Lerp(0.8f, 0f, t);
            flashMat.color = flashColor;

            for (int i = 0; i < particles.Count; i++)
            {
                velocities[i] += Vector3.down * 6f * Time.deltaTime;
                particles[i].transform.position += velocities[i] * Time.deltaTime;
                particles[i].transform.localScale = Vector3.Lerp(particles[i].transform.localScale, Vector3.zero, Time.deltaTime * 4f);

                Renderer renderer = particles[i].GetComponent<Renderer>();
                Color particleColor = renderer.material.color;
                particleColor.a = Mathf.Lerp(particleColor.a, 0f, Time.deltaTime * 4f);
                renderer.material.color = particleColor;
            }

            yield return null;
        }

        foreach (GameObject particle in particles)
        {
            if (particle != null)
            {
                Destroy(particle);
            }
        }

        Destroy(flash);
    }

    private IEnumerator LevelUpAuraRoutine(Transform player)
    {
        float duration = 2f;
        int count = 16;
        float radius = 1.6f;
        List<GameObject> particles = new List<GameObject>();
        List<float> baseAngles = new List<float>();
        List<float> yOffsets = new List<float>();

        for (int i = 0; i < count; i++)
        {
            GameObject particle = CreatePrimitiveSphere();
            particle.transform.localScale = Vector3.one * 0.1f;
            Material mat = new Material(transparentMaterial);
            mat.color = new Color(1f, 0.85f, 0.1f, 0.9f);
            particle.GetComponent<Renderer>().material = mat;
            particles.Add(particle);
            baseAngles.Add((360f / count) * i);
            yOffsets.Add(Random.Range(0f, 2.2f));
        }

        GameObject ring = CreatePrimitiveQuad();
        ring.transform.localScale = new Vector3(3.5f, 3.5f, 1f);
        Material ringMat = new Material(transparentMaterial);
        ringMat.color = new Color(1f, 0.85f, 0.1f, 0.4f);
        ring.GetComponent<Renderer>().material = ringMat;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            if (player == null)
            {
                break;
            }

            ring.transform.position = player.position + new Vector3(0f, 0.05f, 0f);
            ring.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            for (int i = 0; i < particles.Count; i++)
            {
                float radians = (baseAngles[i] + elapsed * 200f) * Mathf.Deg2Rad;
                float x = Mathf.Cos(radians) * radius;
                float z = Mathf.Sin(radians) * radius;
                float y = yOffsets[i] + Mathf.Sin(elapsed * 2.5f + baseAngles[i]) * 0.4f;
                particles[i].transform.position = player.position + new Vector3(x, y, z);

                Color color = particles[i].GetComponent<Renderer>().material.color;
                color.a = Mathf.Lerp(0.9f, 0f, t);
                particles[i].GetComponent<Renderer>().material.color = color;
            }

            yield return null;
        }

        foreach (GameObject particle in particles)
        {
            if (particle != null)
            {
                Destroy(particle);
            }
        }

        Destroy(ring);
    }

    private IEnumerator CooldownIndicatorRoutine(Transform parent, float cooldownTime)
    {
        int count = 16;
        float radius = 0.4f;
        GameObject root = new GameObject("CooldownIndicator");
        root.transform.SetParent(parent);
        root.transform.localPosition = new Vector3(0f, 2.6f, 0f);

        GameObject[] segments = new GameObject[count];
        for (int i = 0; i < count; i++)
        {
            float angle = (360f / count) * i;
            float radians = angle * Mathf.Deg2Rad;
            GameObject segment = CreatePrimitiveQuad();
            segment.transform.SetParent(root.transform);
            segment.transform.localPosition = new Vector3(Mathf.Cos(radians) * radius, 0f, Mathf.Sin(radians) * radius);
            segment.transform.localRotation = Quaternion.Euler(0f, 0f, -angle + 90f);
            segment.transform.localScale = new Vector3(0.06f, 0.18f, 1f);
            Material mat = new Material(transparentMaterial);
            mat.color = new Color(0.4f, 0.8f, 1f, 0.9f);
            segment.GetComponent<Renderer>().material = mat;
            segments[i] = segment;
        }

        float elapsed = 0f;
        while (elapsed < cooldownTime)
        {
            if (parent == null)
            {
                Destroy(root);
                yield break;
            }

            elapsed += Time.deltaTime;
            int hiddenCount = Mathf.FloorToInt((elapsed / cooldownTime) * count);
            for (int i = 0; i < count; i++)
            {
                Renderer renderer = segments[i].GetComponent<Renderer>();
                Color color = renderer.material.color;
                color.a = i < hiddenCount ? 0.1f : 0.9f;
                renderer.material.color = color;
            }

            yield return null;
        }

        Destroy(root);
    }

    private IEnumerator AttackTelegraphRoutine(Vector3 position, float radius, float duration, bool bossTelegraph)
    {
        GameObject disc = CreatePrimitiveQuad();
        disc.transform.position = position + Vector3.up * 0.05f;
        disc.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        disc.transform.localScale = new Vector3(radius * 2f, radius * 2f, 1f);

        Material mat = new Material(transparentMaterial);
        mat.color = bossTelegraph
            ? new Color(0.8f, 0.15f, 0.95f, 0.18f)
            : new Color(1f, 0.15f, 0.15f, 0.16f);
        disc.GetComponent<Renderer>().material = mat;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float pulse = 1f + Mathf.Sin(t * Mathf.PI) * 0.08f;
            disc.transform.localScale = new Vector3(radius * 2f * pulse, radius * 2f * pulse, 1f);

            Color color = mat.color;
            color.a = Mathf.Lerp(bossTelegraph ? 0.18f : 0.16f, bossTelegraph ? 0.5f : 0.42f, t);
            mat.color = color;
            yield return null;
        }

        Destroy(disc);
    }

    private GameObject CreatePrimitiveQuad()
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Quad);
        DestroyImmediate(obj.GetComponent<Collider>());
        return obj;
    }

    private GameObject CreatePrimitiveSphere()
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        DestroyImmediate(obj.GetComponent<Collider>());
        return obj;
    }
}
