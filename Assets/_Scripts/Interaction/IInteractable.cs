using UnityEngine;

public interface IInteractable
{
    Transform InteractionTransform { get; }
    float InteractionRange { get; }
    int InteractionPriority { get; }
    string InteractionPrompt { get; }
    string PromptSpriteResourcePath { get; }
    bool AllowsAttackInteraction { get; }
    bool AllowsMouseInteraction { get; }

    bool CanInteract(GameObject interactor);
    void Interact(GameObject interactor);
    void SetInteractionFocus(bool focused);
}
