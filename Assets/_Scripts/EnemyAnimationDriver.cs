using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(CharacterAnimationController))]
public class EnemyAnimationDriver : MonoBehaviour
{
    private CharacterAnimationController animationController;
    private EnemyAI enemyAI;
    private EnemyStats enemyStats;
    private NavMeshAgent agent;
    private EnemyAI.AIState lastState;

    private void Awake()
    {
        animationController = GetComponent<CharacterAnimationController>();
        enemyAI = GetComponent<EnemyAI>();
        enemyStats = GetComponent<EnemyStats>();
        agent = GetComponent<NavMeshAgent>();
        lastState = enemyAI != null ? enemyAI.currentState : EnemyAI.AIState.Patrol;
    }

    private void OnEnable()
    {
        if (enemyStats != null)
        {
            enemyStats.OnDamaged.AddListener(HandleDamaged);
            enemyStats.OnDeath.AddListener(HandleDeath);
        }
    }

    private void OnDisable()
    {
        if (enemyStats != null)
        {
            enemyStats.OnDamaged.RemoveListener(HandleDamaged);
            enemyStats.OnDeath.RemoveListener(HandleDeath);
        }
    }

    private void Update()
    {
        if (animationController == null || enemyAI == null || enemyStats == null || enemyStats.IsDead())
        {
            return;
        }

        EnemyAI.AIState state = enemyAI.currentState;
        if (state == EnemyAI.AIState.Attack && lastState != EnemyAI.AIState.Attack)
        {
            animationController.PlayAttack(transform.forward);
        }

        lastState = state;

        if (state == EnemyAI.AIState.Attack)
        {
            return;
        }

        Vector3 velocity = agent != null && agent.enabled ? agent.velocity : Vector3.zero;
        velocity.y = 0f;
        if (state == EnemyAI.AIState.Chase || state == EnemyAI.AIState.Return || velocity.sqrMagnitude > 0.03f)
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
