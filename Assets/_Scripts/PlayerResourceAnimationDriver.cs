using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerResourceAnimationDriver : MonoBehaviour
{
    private const string ControllerResourcePath = "Animation/PlayerKnightResource";
    private const float AttackCrossFade = 0.035f;
    private const float LoopCrossFade = 0.12f;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");

    private Animator animator;
    private PlayerController playerController;
    private PlayerStats playerStats;
    private CharacterController characterController;
    private bool wasAttacking;
    private bool wasDodging;
    private bool dead;
    private int attackIndex;
    private string currentLoopState;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        playerStats = GetComponent<PlayerStats>();
        characterController = GetComponent<CharacterController>();
        EnsureAnimator();
    }

    private void OnEnable()
    {
        if (playerStats != null)
        {
            playerStats.OnDamaged.AddListener(HandleDamaged);
            playerStats.OnDeath.AddListener(HandleDeath);
        }
    }

    private void OnDisable()
    {
        if (playerStats != null)
        {
            playerStats.OnDamaged.RemoveListener(HandleDamaged);
            playerStats.OnDeath.RemoveListener(HandleDeath);
        }
    }

    private void Update()
    {
        if (animator == null || playerController == null || dead)
        {
            return;
        }

        bool attacking = playerController.IsAttacking();
        bool dodging = playerController.IsDodging();
        Vector3 velocity = characterController != null ? characterController.velocity : Vector3.zero;
        velocity.y = 0f;
        animator.SetFloat(SpeedHash, velocity.magnitude);

        if (attacking && !wasAttacking)
        {
            attackIndex = attackIndex % 3 + 1;
            PlayOneShot("Attack" + attackIndex, AttackCrossFade);
        }

        if (dodging && !wasDodging)
        {
            PlayOneShot("Dash", 0.025f);
        }

        wasAttacking = attacking;
        wasDodging = dodging;

        if (attacking || dodging)
        {
            return;
        }

        PlayLoop(velocity.sqrMagnitude > 0.04f ? "Move" : "Idle");
    }

    private void HandleDamaged()
    {
        if (animator != null && !dead)
        {
            PlayOneShot("Hurt", 0.035f);
        }
    }

    private void HandleDeath()
    {
        dead = true;
        if (animator != null)
        {
            PlayOneShot("Death", 0.035f);
        }
    }

    private void EnsureAnimator()
    {
        RuntimeAnimatorController controller = Resources.Load<RuntimeAnimatorController>(ControllerResourcePath);
        if (controller == null)
        {
            Debug.LogWarning("Player resource animator controller not found at Resources/" + ControllerResourcePath);
            return;
        }

        Transform modelRoot = transform.Find("KayKitVisual/Player_Knight_Model");
        if (modelRoot == null)
        {
            Transform visualRoot = transform.Find("KayKitVisual");
            modelRoot = visualRoot != null && visualRoot.childCount > 0 ? visualRoot.GetChild(0) : null;
        }

        if (modelRoot == null)
        {
            return;
        }

        animator = modelRoot.GetComponent<Animator>();
        if (animator == null)
        {
            animator = modelRoot.gameObject.AddComponent<Animator>();
        }

        animator.runtimeAnimatorController = controller;
        animator.applyRootMotion = false;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        PlayLoop("Idle", 0f);
    }

    private void PlayLoop(string stateName, float fade = LoopCrossFade)
    {
        if (currentLoopState == stateName)
        {
            return;
        }

        currentLoopState = stateName;
        animator.CrossFadeInFixedTime(stateName, fade);
    }

    private void PlayOneShot(string stateName, float fade)
    {
        currentLoopState = null;
        animator.CrossFadeInFixedTime(stateName, fade, 0, 0f);
    }
}
