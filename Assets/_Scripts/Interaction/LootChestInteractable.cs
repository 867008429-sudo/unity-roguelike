using UnityEngine;

public class LootChestInteractable : MonoBehaviour, IInteractable
{
    public int goldReward = 20;
    public int xpReward = 15;
    public float interactDistance = 2.6f;
    public int interactionPriority = 10;
    public string closedHint = "Open";
    public string openedHint = "Opened";
    public string promptSpriteResourcePath = "Prompts/Keyboard_E";

    private bool opened;
    private InteractableHint hint;

    public Transform InteractionTransform => transform;
    public float InteractionRange => interactDistance;
    public int InteractionPriority => interactionPriority;
    public string InteractionPrompt => opened ? openedHint : closedHint;
    public string PromptSpriteResourcePath => promptSpriteResourcePath;
    public bool AllowsAttackInteraction => false;
    public bool AllowsMouseInteraction => false;

    private void Awake()
    {
        hint = GetComponent<InteractableHint>();
        if (hint == null)
        {
            hint = gameObject.AddComponent<InteractableHint>();
        }

        if (hint != null)
        {
            hint.SetContent(closedHint, promptSpriteResourcePath);
            hint.showDistance = interactDistance;
            hint.useDistanceFallback = false;
        }
    }

    private void OnEnable()
    {
        PlayerInteractionDetector.Register(this);
    }

    private void OnDisable()
    {
        PlayerInteractionDetector.Unregister(this);
    }

    public bool CanInteract(GameObject interactor)
    {
        return !opened;
    }

    public void Interact(GameObject interactor)
    {
        PlayerStats stats = interactor != null ? interactor.GetComponent<PlayerStats>() : null;
        Open(stats);
    }

    public void SetInteractionFocus(bool focused)
    {
        if (hint == null)
        {
            return;
        }

        hint.SetContent(InteractionPrompt, promptSpriteResourcePath);
        hint.SetFocused(focused);
    }

    private void Open(PlayerStats stats)
    {
        opened = true;

        if (stats != null)
        {
            stats.AddGold(goldReward);
            stats.AddXP(xpReward);
        }

        if (hint != null)
        {
            hint.SetContent(openedHint, promptSpriteResourcePath);
            hint.Hide();
            hint.enabled = false;
        }

        foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
        {
            if (renderer != null)
            {
                renderer.material.color = Color.Lerp(renderer.material.color, new Color(1f, 0.85f, 0.35f), 0.45f);
            }
        }

        VisualEffectsManager.Instance.PlayHitBurst(transform.position + Vector3.up * 0.8f, new Color(1f, 0.85f, 0.2f, 1f));
    }
}
