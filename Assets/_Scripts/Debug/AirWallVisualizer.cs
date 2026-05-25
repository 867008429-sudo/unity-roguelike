using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class AirWallVisualizer : MonoBehaviour
{
    public bool onlyInvisibleColliders = true;
    public bool includeTriggers;
    public Color visibleColor = new Color(1f, 0.25f, 0.05f, 0.85f);
    public Color triggerColor = new Color(1f, 0.85f, 0.1f, 0.6f);

#if UNITY_EDITOR
    [MenuItem("RPG Tools/Debug/Create Air Wall Visualizer", false, 80)]
    public static void CreateVisualizer()
    {
        const string objectName = "AirWallVisualizer";
        GameObject existing = GameObject.Find(objectName);
        GameObject target = existing != null ? existing : new GameObject(objectName);

        if (target.GetComponent<AirWallVisualizer>() == null)
        {
            target.AddComponent<AirWallVisualizer>();
        }

        Selection.activeGameObject = target;
    }
#endif

    private void OnDrawGizmos()
    {
        Collider[] colliders = FindObjectsOfType<Collider>();
        foreach (Collider collider in colliders)
        {
            if (collider == null || !collider.enabled)
            {
                continue;
            }

            if (collider.isTrigger && !includeTriggers)
            {
                continue;
            }

            if (onlyInvisibleColliders && HasVisibleRenderer(collider.gameObject))
            {
                continue;
            }

            Bounds bounds = collider.bounds;
            Gizmos.color = collider.isTrigger ? triggerColor : visibleColor;
            Gizmos.DrawWireCube(bounds.center, bounds.size);

#if UNITY_EDITOR
            Handles.color = Gizmos.color;
            Handles.Label(bounds.center + Vector3.up * (bounds.extents.y + 0.15f), collider.name);
#endif
        }
    }

    private static bool HasVisibleRenderer(GameObject target)
    {
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            if (renderer != null && renderer.enabled)
            {
                return true;
            }
        }

        return false;
    }
}
