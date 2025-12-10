using UnityEngine;

public class MinionAnimatorController : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private static readonly int HashDirection = Animator.StringToHash("Direction");
    private static readonly int HashAttack = Animator.StringToHash("Attack");
    private static readonly int HashHit = Animator.StringToHash("Hit");
    private static readonly int HashDeath = Animator.StringToHash("Death");

    private bool isDead = false;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    public void SetDirection(Vector2 dir)
    {
        if (animator == null || isDead) return;
        if (dir.sqrMagnitude < 0.0001f) return;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        int dirIndex;

        if (angle > -45f && angle <= 45f) { dirIndex = 3; }
        else if (angle > 45f && angle <= 135f) { dirIndex = 2; }
        else if (angle <= -45f && angle > -135f) { dirIndex = 0; }
        else { dirIndex = 1; }

        animator.SetFloat(HashDirection, dirIndex);
    }


    public void PlayIdle()
    {
        if (isDead || animator == null) return;
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
        if (isDead || animator == null) return;
        animator.ResetTrigger(HashAttack);
        animator.SetTrigger(HashHit);
    }

    public void PlayDeath()
    {
        if (animator == null) return;
        isDead = true;
        animator.SetTrigger(HashDeath);
    }
}
