using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(EnemyStats))]
public class EnemyResourceAnimationDriver : MonoBehaviour
{
    private const string SkeletonControllerResourcePath = "Animation/SkeletonResource";
    private const float LoopCrossFade = 0.12f;
    private const float ActionCrossFade = 0.045f;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");

    private Animator animator;
    private EnemyAI enemyAI;
    private EnemyStats enemyStats;
    private NavMeshAgent agent;
    private string currentLoopState;
    private bool dead;

    private void Awake()
    {
        enemyAI = GetComponent<EnemyAI>();
        enemyStats = GetComponent<EnemyStats>();
        agent = GetComponent<NavMeshAgent>();
        EnsureAnimator();
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
        if (animator == null || enemyAI == null || enemyStats == null || dead || enemyStats.IsDead())
        {
            return;
        }

        Vector3 velocity = agent != null && agent.enabled ? agent.velocity : Vector3.zero;
        velocity.y = 0f;
        animator.SetFloat(SpeedHash, velocity.magnitude);

        if (enemyAI.currentState == EnemyAI.AIState.Attack)
        {
            return;
        }

        PlayLoop(velocity.sqrMagnitude > 0.03f || enemyAI.currentState == EnemyAI.AIState.Chase || enemyAI.currentState == EnemyAI.AIState.Return
            ? "Move"
            : "Idle");
    }

    public void PlayAttack()
    {
        PlayOneShot("Attack", ActionCrossFade);
    }

    public void PlayChargeWindup()
    {
        PlayOneShot("Charge", ActionCrossFade);
    }

    public void PlaySpitWindup()
    {
        PlayOneShot("Attack", ActionCrossFade);
    }

    private void HandleDamaged()
    {
        if (!dead)
        {
            PlayOneShot("Hurt", ActionCrossFade);
        }
    }

    private void HandleDeath()
    {
        dead = true;
        PlayOneShot("Death", ActionCrossFade);
    }

    private void EnsureAnimator()
    {
        if (enemyStats == null || enemyStats.enemyType != EnemyStats.EnemyType.Skeleton)
        {
            return;
        }

        RuntimeAnimatorController controller = Resources.Load<RuntimeAnimatorController>(SkeletonControllerResourcePath);
        if (controller == null)
        {
            Debug.LogWarning("Skeleton resource animator controller not found at Resources/" + SkeletonControllerResourcePath);
            return;
        }

        Transform modelRoot = transform.Find("KayKitVisual/SkeletonEnemy_KayKit_Model");
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
        if (animator == null || currentLoopState == stateName)
        {
            return;
        }

        currentLoopState = stateName;
        animator.CrossFadeInFixedTime(stateName, fade);
    }

    private void PlayOneShot(string stateName, float fade)
    {
        if (animator == null)
        {
            return;
        }

        currentLoopState = null;
        animator.CrossFadeInFixedTime(stateName, fade, 0, 0f);
    }
}
