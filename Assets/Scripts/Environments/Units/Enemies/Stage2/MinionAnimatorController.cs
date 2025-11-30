using UnityEngine;

public class MinionAnimatorController : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private static readonly int HashDirection = Animator.StringToHash("Direction");
    private static readonly int HashAttack = Animator.StringToHash("Attack");
    private static readonly int HashHit = Animator.StringToHash("Hit");
    private static readonly int HashDeath = Animator.StringToHash("Die");

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    public void SetDirection(int dir)
    {
        if (animator == null) return;
        animator.SetInteger(HashDirection, dir);
    }

    public void PlayIdle()
    {
        if (animator == null) return;
        animator.ResetTrigger(HashAttack);
        animator.ResetTrigger(HashHit);
        animator.CrossFade("Idle", 0.1f);
    }

    public void PlayAttack()
    {
        if (animator == null) return;
        animator.ResetTrigger(HashHit);
        animator.SetTrigger(HashAttack);
    }

    public void PlayHit()
    {
        if (animator == null) return;
        animator.ResetTrigger(HashAttack);
        animator.SetTrigger(HashHit);
    }

    public void PlayDeath()
    {
        if (animator == null) return;
        animator.SetTrigger(HashDeath);
    }
}
