using UnityEngine;

public class WitchCloneAnimatorController : MonoBehaviour
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
        if (dir.x == 0) return;

        if (dir.x < 0)
            sprite.flipX = false; // 왼쪽 기본
        else
            sprite.flipX = true;
    }

    public void PlayAttack()  // ← Attack을 Charge 대용으로 사용
    {
        anim.SetTrigger("Attack");
    }

/*    public void PlayHit()
    {
        anim.SetTrigger("Hit");
    }*/

    public void PlayDeath()
    {
        anim.SetTrigger("Death");
    }
}
