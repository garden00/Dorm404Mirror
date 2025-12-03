using UnityEngine;

public class BatAnimatorController : MonoBehaviour
{
    private SpriteRenderer sr;
    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    public void FaceDirection(float x)
    {
        sr.flipX = x < 0;
    }

    public void PlayMove() => anim.SetTrigger("Move");
    public void PlayAttack() => anim.SetTrigger("Attack");
    public void PlayHit() => anim.SetTrigger("Hit");
    public void PlayDeath() => anim.SetTrigger("Death");
}
