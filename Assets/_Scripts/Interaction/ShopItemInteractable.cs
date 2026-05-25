using UnityEngine;

public enum ShopItemRewardType
{
    AttackBlessing,
    DefenseBlessing,
    MaxHealthBlessing,
    CritAssassinSigil,
    BurnInfernoSigil,
    LightningThunderCrown,
    WindBoots,
    LifeStealSigil,
    HealthPotion
}

public class ShopItemInteractable : MonoBehaviour, IInteractable
{
    public string itemName = "神秘商品";
    public int price = 50;
    public ShopItemRewardType rewardType = ShopItemRewardType.AttackBlessing;
    public float interactDistance = 2.4f;
    public int interactionPriority = 20;
    public string promptSpriteResourcePath = "Prompts/Keyboard_E";
    public bool destroyAfterPurchase = true;
    public string soldOutText = "已售罄";

    private bool sold;
    private InteractableHint hint;

    public Transform InteractionTransform => transform;
    public float InteractionRange => interactDistance;
    public int InteractionPriority => interactionPriority;
    public string InteractionPrompt => sold ? soldOutText : itemName + "：" + price + "金币";
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

        hint.SetContent(InteractionPrompt, promptSpriteResourcePath);
        hint.showDistance = interactDistance;
        hint.useDistanceFallback = false;
    }

    private void OnEnable()
    {
        PlayerInteractionDetector.Register(this);
    }

    private void Start()
    {
        PlayerInteractionDetector.Register(this);
    }

    private void OnDisable()
    {
        PlayerInteractionDetector.Unregister(this);
    }

    public bool CanInteract(GameObject interactor)
    {
        return !sold;
    }

    public void Interact(GameObject interactor)
    {
        if (sold)
        {
            return;
        }

        PlayerStats stats = interactor != null ? interactor.GetComponent<PlayerStats>() : null;
        PlayerController controller = interactor != null ? interactor.GetComponent<PlayerController>() : null;
        if (stats == null || !stats.TrySpendGold(price))
        {
            ShowFloatingText("金币不足", new Color(1f, 0.32f, 0.18f, 1f), true);
            return;
        }

        ApplyReward(stats, controller);
        sold = true;
        ShowFloatingText("购买成功", new Color(1f, 0.86f, 0.28f, 1f), false);

        if (hint != null)
        {
            hint.SetContent(soldOutText, promptSpriteResourcePath);
            hint.Hide();
            hint.enabled = false;
        }

        VisualEffectsManager.Instance.PlayHitBurst(transform.position + Vector3.up * 0.65f, new Color(1f, 0.82f, 0.24f, 1f));

        if (destroyAfterPurchase)
        {
            Destroy(gameObject);
        }
        else
        {
            TintSoldOut();
        }
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

    private void ApplyReward(PlayerStats stats, PlayerController controller)
    {
        switch (rewardType)
        {
            case ShopItemRewardType.DefenseBlessing:
                stats.IncreaseDefense(2f);
                break;
            case ShopItemRewardType.MaxHealthBlessing:
                stats.IncreaseMaxHealth(25f, true);
                break;
            case ShopItemRewardType.CritAssassinSigil:
                stats.IncreaseCritChance(0.15f);
                stats.IncreaseCritMultiplier(0.5f);
                stats.IncreaseCritBuild(2);
                break;
            case ShopItemRewardType.BurnInfernoSigil:
                stats.IncreaseBurnChance(0.35f, 10f);
                stats.IncreaseBurnBuild(2);
                break;
            case ShopItemRewardType.LightningThunderCrown:
                stats.IncreaseShockwaveChance(0.30f);
                stats.IncreaseLightningBuild(2);
                if (controller != null)
                {
                    controller.IncreaseDamageMultiplier(0.20f);
                }
                break;
            case ShopItemRewardType.WindBoots:
                if (controller != null)
                {
                    controller.IncreaseMoveSpeed(2f);
                    controller.ReduceDodgeCooldown(0.4f);
                    controller.MultiplyAttackCooldown(0.9f);
                }
                break;
            case ShopItemRewardType.LifeStealSigil:
                stats.IncreaseLifeSteal(0.12f);
                if (controller != null)
                {
                    controller.IncreaseDamageMultiplier(0.15f);
                }
                break;
            case ShopItemRewardType.HealthPotion:
                stats.Heal(50f);
                break;
            case ShopItemRewardType.AttackBlessing:
            default:
                stats.IncreaseAttack(5f);
                break;
        }
    }

    private void ShowFloatingText(string text, Color color, bool emphasized)
    {
        DamageTextPool.Instance.ShowText(transform.position + Vector3.up * 0.25f, text, color, emphasized);
    }

    private void TintSoldOut()
    {
        foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
        {
            if (renderer != null)
            {
                renderer.material.color = Color.Lerp(renderer.material.color, Color.gray, 0.55f);
            }
        }
    }
}
