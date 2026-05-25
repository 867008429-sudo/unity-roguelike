using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractionDetector : MonoBehaviour
{
    private static readonly List<IInteractable> RegisteredInteractables = new List<IInteractable>();
    private static PlayerInteractionDetector instance;

    public Transform player;
    public float playerSearchInterval = 0.25f;

    private IInteractable focusedInteractable;
    private float nextPlayerSearchTime;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        RegisteredInteractables.Clear();
        instance = null;
        EnsureInstance();
    }

    public static void Register(IInteractable interactable)
    {
        if (interactable == null || RegisteredInteractables.Contains(interactable))
        {
            return;
        }

        RegisteredInteractables.Add(interactable);
        EnsureInstance();
    }

    public static void Unregister(IInteractable interactable)
    {
        if (interactable == null)
        {
            return;
        }

        RegisteredInteractables.Remove(interactable);
        if (instance != null && instance.focusedInteractable == interactable)
        {
            instance.SetFocus(null);
        }
    }

    private static void EnsureInstance()
    {
        if (instance != null)
        {
            RegisterSceneInteractables();
            return;
        }

        PlayerInteractionDetector existing = FindObjectOfType<PlayerInteractionDetector>();
        if (existing != null)
        {
            instance = existing;
            RegisterSceneInteractables();
            return;
        }

        GameObject detectorObject = new GameObject("PlayerInteractionDetector");
        instance = detectorObject.AddComponent<PlayerInteractionDetector>();
        RegisterSceneInteractables();
    }

    private static void RegisterSceneInteractables()
    {
        MonoBehaviour[] behaviours = FindObjectsOfType<MonoBehaviour>();
        for (int i = 0; i < behaviours.Length; i++)
        {
            IInteractable interactable = behaviours[i] as IInteractable;
            if (interactable != null && !RegisteredInteractables.Contains(interactable))
            {
                RegisteredInteractables.Add(interactable);
            }
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void Update()
    {
        ResolvePlayer();

        IInteractable best = FindBestInteractable();
        SetFocus(best);

        if (focusedInteractable == null || Time.timeScale <= 0f)
        {
            return;
        }

        bool confirmPressed = Input.GetKeyDown(KeyCode.E);
        bool attackKeyPressed = Input.GetKeyDown(KeyCode.J) && focusedInteractable.AllowsAttackInteraction;
        bool mousePressed = Input.GetMouseButtonDown(0) && focusedInteractable.AllowsMouseInteraction;
        if (confirmPressed || attackKeyPressed || mousePressed)
        {
            focusedInteractable.Interact(player != null ? player.gameObject : null);
        }
    }

    private void ResolvePlayer()
    {
        if (player != null || Time.unscaledTime < nextPlayerSearchTime)
        {
            return;
        }

        nextPlayerSearchTime = Time.unscaledTime + playerSearchInterval;
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        player = playerObject != null ? playerObject.transform : null;
    }

    private IInteractable FindBestInteractable()
    {
        if (player == null)
        {
            return null;
        }

        IInteractable best = null;
        float bestDistanceSqr = float.MaxValue;
        int bestPriority = int.MinValue;

        for (int i = RegisteredInteractables.Count - 1; i >= 0; i--)
        {
            IInteractable interactable = RegisteredInteractables[i];
            Object unityObject = interactable as Object;
            if (interactable == null || unityObject == null)
            {
                RegisteredInteractables.RemoveAt(i);
                continue;
            }

            if (!interactable.CanInteract(player.gameObject) || interactable.InteractionTransform == null)
            {
                continue;
            }

            float range = Mathf.Max(0f, interactable.InteractionRange);
            float distanceSqr = (interactable.InteractionTransform.position - player.position).sqrMagnitude;
            if (distanceSqr > range * range)
            {
                continue;
            }

            int priority = interactable.InteractionPriority;
            if (best == null || priority > bestPriority || (priority == bestPriority && distanceSqr < bestDistanceSqr))
            {
                best = interactable;
                bestPriority = priority;
                bestDistanceSqr = distanceSqr;
            }
        }

        return best;
    }

    private void SetFocus(IInteractable interactable)
    {
        if (focusedInteractable == interactable)
        {
            return;
        }

        if (focusedInteractable != null)
        {
            focusedInteractable.SetInteractionFocus(false);
        }

        focusedInteractable = interactable;

        if (focusedInteractable != null)
        {
            focusedInteractable.SetInteractionFocus(true);
        }
    }
}
