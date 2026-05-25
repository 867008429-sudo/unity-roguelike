using UnityEngine;

[RequireComponent(typeof(CharacterAnimationController))]
public class PlayerAnimationDriver : MonoBehaviour
{
    private CharacterAnimationController animationController;
    private PlayerController playerController;
    private PlayerStats playerStats;
    private CharacterController characterController;
    private bool wasAttacking;
    private bool wasDodging;

    private void Awake()
    {
        animationController = GetComponent<CharacterAnimationController>();
        playerController = GetComponent<PlayerController>();
        playerStats = GetComponent<PlayerStats>();
        characterController = GetComponent<CharacterController>();
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
        if (animationController == null || playerController == null)
        {
            return;
        }

        bool isAttacking = playerController.IsAttacking();
        bool isDodging = playerController.IsDodging();
        Vector3 facing = playerController.GetFacingDirection();

        if (isAttacking && !wasAttacking)
        {
            animationController.PlayAttack(facing);
        }

        if (isDodging && !wasDodging)
        {
            animationController.PlayDash(facing);
        }

        wasAttacking = isAttacking;
        wasDodging = isDodging;

        if (isAttacking || isDodging)
        {
            return;
        }

        Vector3 velocity = characterController != null ? characterController.velocity : Vector3.zero;
        velocity.y = 0f;
        if (velocity.sqrMagnitude > 0.04f)
        {
            animationController.PlayMove();
        }
        else
        {
            animationController.PlayIdle();
        }
    }

    private void HandleDamaged()
    {
        if (animationController != null)
        {
            animationController.PlayHurt(-transform.forward);
        }
    }

    private void HandleDeath()
    {
        if (animationController != null)
        {
            animationController.PlayDeath();
        }
    }
}
