using System.Collections.Generic;
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
    [TextArea(2, 4)]
    public string itemDescription = "";
    public int price = 50;
    public ShopItemRewardType rewardType = ShopItemRewardType.AttackBlessing;
    public float interactDistance = 2.4f;
    public int interactionPriority = 20;
    public string promptSpriteResourcePath = "Prompts/Keyboard_E";
    public bool destroyAfterPurchase = true;
    public bool openShopPanelOnInteract = true;
    public bool includeSiblingItemsInShop = true;
    public Transform shopRoot;
    public string shopTitle = "神秘商店";
    public string soldOutText = "已售罄";

    private bool sold;
    private InteractableHint hint;

    public Transform InteractionTransform => transform;
    public float InteractionRange => interactDistance;
    public int InteractionPriority => interactionPriority;
    public string InteractionPrompt => sold ? soldOutText : (openShopPanelOnInteract ? shopTitle + "：按E查看商品" : itemName + "：" + price + "金币");
    public string PromptSpriteResourcePath => promptSpriteResourcePath;
    public bool AllowsAttackInteraction => false;
    public bool AllowsMouseInteraction => false;
    public string DisplayName => itemName;
    public string Description => string.IsNullOrEmpty(itemDescription) ? GetDefaultDescription(rewardType) : itemDescription;
    public int Price => price;
    public ShopItemRewardType RewardType => rewardType;
    public bool IsSold => sold;

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

        if (openShopPanelOnInteract)
        {
            UIManager uiManager = FindObjectOfType<UIManager>();
            if (uiManager != null)
            {
                uiManager.OpenShopPanel(GetShopItemsInGroup(), this);
                return;
            }
        }

        TryPurchaseFromInteractor(interactor, false);
    }

    public bool TryPurchaseFromInteractor(GameObject interactor, bool keepVisible)
    {
        PlayerStats stats = interactor != null ? interactor.GetComponent<PlayerStats>() : null;
        PlayerController controller = interactor != null ? interactor.GetComponent<PlayerController>() : null;
        return TryPurchase(stats, controller, keepVisible);
    }

    public bool TryPurchase(PlayerStats stats, PlayerController controller, bool keepVisible)
    {
        if (sold)
        {
            return false;
        }

        if (stats == null || !stats.TrySpendGold(price))
        {
            ShowFloatingText("金币不足", new Color(1f, 0.32f, 0.18f, 1f), true);
            return false;
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

        if (VisualEffectsManager.Instance != null)
        {
            VisualEffectsManager.Instance.PlayHitBurst(transform.position + Vector3.up * 0.65f, new Color(1f, 0.82f, 0.24f, 1f));
        }

        if (destroyAfterPurchase && !keepVisible)
        {
            Destroy(gameObject);
        }
        else
        {
            TintSoldOut();
        }

        return true;
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

    private ShopItemInteractable[] GetShopItemsInGroup()
    {
        if (!includeSiblingItemsInShop)
        {
            return new[] { this };
        }

        Transform root = shopRoot != null ? shopRoot : transform.parent;
        if (root == null)
        {
            return new[] { this };
        }

        ShopItemInteractable[] items = root.GetComponentsInChildren<ShopItemInteractable>(true);
        if (items == null || items.Length == 0)
        {
            return new[] { this };
        }

        List<ShopItemInteractable> sorted = new List<ShopItemInteractable>(items);
        sorted.Sort((a, b) => string.Compare(a.DisplayName, b.DisplayName, System.StringComparison.Ordinal));
        return sorted.ToArray();
    }

    private string GetDefaultDescription(ShopItemRewardType type)
    {
        switch (type)
        {
            case ShopItemRewardType.DefenseBlessing:
                return "防御 +2，降低受到的伤害。";
            case ShopItemRewardType.MaxHealthBlessing:
                return "最大生命 +25，并回复到满血。";
            case ShopItemRewardType.CritAssassinSigil:
                return "暴击率、暴击伤害提升，并推进暴击流派。";
            case ShopItemRewardType.BurnInfernoSigil:
                return "提高燃烧概率和燃烧伤害，并推进燃烧流派。";
            case ShopItemRewardType.LightningThunderCrown:
                return "提高冲击波概率和伤害，并推进闪电流派。";
            case ShopItemRewardType.WindBoots:
                return "移动更快，闪避冷却更短，攻击节奏更快。";
            case ShopItemRewardType.LifeStealSigil:
                return "获得生命偷取，并提高少量伤害。";
            case ShopItemRewardType.HealthPotion:
                return "立即回复 50 点生命。";
            case ShopItemRewardType.AttackBlessing:
            default:
                return "攻击力 +5。";
        }
    }
}
