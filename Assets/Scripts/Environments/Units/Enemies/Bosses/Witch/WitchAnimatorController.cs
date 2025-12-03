using UnityEngine;

public class WitchBossAnimatorController : MonoBehaviour
{
    private Animator anim;
    private SpriteRenderer sprite;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
    }

    public void SetDirection(Vector2 dir)
    {
        // 왼쪽 바라보는 기본 스프라이트 기준
        sprite.flipX = dir.x >= 0;
    }

    public void PlayAttack()
    {
        anim.ResetTrigger("Hit");
        anim.SetTrigger("Attack");
    }

    public void PlaySummon()
    {
        anim.ResetTrigger("Hit");
        anim.SetTrigger("Sum");
    }

    public void PlayHit()
    {
        anim.ResetTrigger("Attack");
        anim.ResetTrigger("Sum");
        anim.SetTrigger("Hit");
    }

    public void PlayDeath()
    {
        anim.ResetTrigger("Attack");
        anim.ResetTrigger("Sum");
        anim.ResetTrigger("Hit");
        anim.SetTrigger("Death");
    }
}
