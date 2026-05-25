using UnityEngine;

public class DestructiblePropInteractable : MonoBehaviour, IInteractable
{
    public int goldReward = 6;
    public float interactDistance = 2.5f;
    public int interactionPriority = 0;
    public string hintText = "Press E Break";
    public string promptSpriteResourcePath = "Prompts/Keyboard_E";
    public bool allowAttackInteraction = true;
    public bool allowMouseInteraction = true;

    private bool broken;
    private InteractableHint hint;

    public Transform InteractionTransform => transform;
    public float InteractionRange => interactDistance;
    public int InteractionPriority => interactionPriority;
    public string InteractionPrompt => hintText;
    public string PromptSpriteResourcePath => promptSpriteResourcePath;
    public bool AllowsAttackInteraction => allowAttackInteraction;
    public bool AllowsMouseInteraction => allowMouseInteraction;

    private void Awake()
    {
        hint = GetComponent<InteractableHint>();
        if (hint == null)
        {
            hint = gameObject.AddComponent<InteractableHint>();
        }

        hint.SetContent(hintText, promptSpriteResourcePath);
        hint.showDistance = interactDistance;
        hint.useDistanceFallback = false;
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
        return !broken;
    }

    public void Interact(GameObject interactor)
    {
        Break();
    }

    public void SetInteractionFocus(bool focused)
    {
        if (hint == null)
        {
            return;
        }

        hint.SetContent(hintText, promptSpriteResourcePath);
        hint.SetFocused(focused);
    }

    private void Break()
    {
        broken = true;

        if (hint != null)
        {
            hint.Hide();
            hint.enabled = false;
        }

        GameFeelVFXManager.Instance.PlayEnemyDeathFracture(gameObject, new Color(0.58f, 0.36f, 0.18f, 1f));
        SpawnGold();
        Destroy(gameObject);
    }

    private void SpawnGold()
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        obj.name = "Gold";
        obj.transform.position = transform.position + Vector3.up * 0.28f;
        obj.transform.localScale = new Vector3(0.46f, 0.06f, 0.46f);
        obj.GetComponent<Renderer>().material.color = new Color(1f, 0.76f, 0.12f, 1f);

        Collider collider = obj.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }

        GoldPickup pickup = obj.AddComponent<GoldPickup>();
        pickup.goldAmount = goldReward;
    }
}
