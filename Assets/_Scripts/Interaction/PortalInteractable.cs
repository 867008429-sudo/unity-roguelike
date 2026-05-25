using UnityEngine;

public enum PortalType
{
    NextBattleWave,
    SecretShop
}

public class PortalInteractable : MonoBehaviour, IInteractable
{
    public PortalType portalType = PortalType.NextBattleWave;
    public WaveManager waveManager;
    public float interactDistance = 2.5f;
    public int interactionPriority = 80;
    public string promptSpriteResourcePath = "Prompts/Keyboard_E";
    public string promptOverride = "";

    private InteractableHint hint;

    public Transform InteractionTransform => transform;
    public float InteractionRange => interactDistance;
    public int InteractionPriority => interactionPriority;
    public string InteractionPrompt => string.IsNullOrEmpty(promptOverride) ? GetDefaultPrompt() : promptOverride;
    public string PromptSpriteResourcePath => promptSpriteResourcePath;
    public bool AllowsAttackInteraction => false;
    public bool AllowsMouseInteraction => false;

    private void Awake()
    {
        EnsureHint();
    }

    private void OnEnable()
    {
        PlayerInteractionDetector.Register(this);
    }

    private void Start()
    {
        PlayerInteractionDetector.Register(this);
        EnsureHint();
    }

    private void OnDisable()
    {
        PlayerInteractionDetector.Unregister(this);
    }

    public bool CanInteract(GameObject interactor)
    {
        return waveManager != null;
    }

    public void Interact(GameObject interactor)
    {
        if (waveManager == null)
        {
            return;
        }

        waveManager.HandlePortalSelected(portalType);
    }

    public void SetInteractionFocus(bool focused)
    {
        EnsureHint();
        if (hint == null)
        {
            return;
        }

        hint.SetContent(InteractionPrompt, promptSpriteResourcePath);
        hint.SetFocused(focused);
    }

    private void EnsureHint()
    {
        if (hint == null)
        {
            hint = GetComponent<InteractableHint>();
        }

        if (hint == null)
        {
            hint = gameObject.AddComponent<InteractableHint>();
        }

        hint.SetContent(InteractionPrompt, promptSpriteResourcePath);
        hint.showDistance = interactDistance;
        hint.useDistanceFallback = false;
    }

    private string GetDefaultPrompt()
    {
        return portalType == PortalType.SecretShop
            ? "[E] 进入神秘商店整备"
            : "[E] 进入下一波战斗";
    }
}
