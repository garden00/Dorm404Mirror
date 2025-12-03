using UnityEngine;

public class CreatureAnimatorController : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private static readonly int MoveX = Animator.StringToHash("moveX");
    private static readonly int MoveY = Animator.StringToHash("moveY");
    private static readonly int IsMoving = Animator.StringToHash("isMoving");
    private static readonly int Attack = Animator.StringToHash("Attack");
    private static readonly int Shoot = Animator.StringToHash("Shoot");
    private static readonly int Hit = Animator.StringToHash("Hit");
    private static readonly int Death = Animator.StringToHash("Death");

    private bool isDead = false;

    private void Reset()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    public void SetMove(Vector2 dir)
    {
        if (isDead || animator == null) return;

        bool moving = dir.sqrMagnitude > 0.0001f;
        animator.SetBool(IsMoving, moving);
        animator.SetFloat(MoveX, dir.x);
        animator.SetFloat(MoveY, dir.y);
    }

    public void PlayIdle()
    {
        if (isDead || animator == null) return;
        animator.SetBool(IsMoving, false);
    }

    // 근접 공격
    public void PlayAttack()
    {
        if (isDead || animator == null) return;
        animator.SetTrigger(Attack);
    }

    public void PlayShoot()
    {
        if (isDead || animator == null) return;

        animator.ResetTrigger(Attack);
        animator.SetTrigger(Shoot);
    }

    // 피격
    public void PlayHit()
    {
        if (isDead || animator == null) return;
        animator.SetTrigger(Hit);
    }

    public void PlayDeath()
    {
        if (isDead || animator == null) return;

        isDead = true;
        animator.SetTrigger(Death);
        animator.SetBool(IsMoving, false);
    }
}
