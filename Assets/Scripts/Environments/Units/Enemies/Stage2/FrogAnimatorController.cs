using UnityEngine;

public class FrogAnimatorController : MonoBehaviour
{
    private SpriteRenderer sr;
    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        if (sr == null) sr = GetComponent<SpriteRenderer>();
    }

    public void FaceDirection(int xDir)
    {
        if (xDir == 0) return;
        sr.flipX = xDir < 0;
    }

    public void PlayMove()
    {
        anim.ResetTrigger("Attack");
        anim.ResetTrigger("Hit");
        anim.SetTrigger("Move");
    }

    public void PlayAttack()
    {
        anim.SetTrigger("Attack");
    }

    public void PlayHit()
    {
        anim.SetTrigger("Hit");
    }

    public void PlayDeath()
    {
        anim.SetTrigger("Death");
    }
}
