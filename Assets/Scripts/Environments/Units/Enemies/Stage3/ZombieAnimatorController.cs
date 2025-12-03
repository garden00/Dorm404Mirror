using UnityEngine;

public class ZombieAnimatorController : MonoBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private SpriteRenderer sr;

    private void Awake()
    {
        if (anim == null) anim = GetComponentInChildren<Animator>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
    }

    public void SetFlipX(bool flip)
    {
        sr.flipX = flip;
    }

    public void PlayIdle()
    {
        anim.SetBool("IsMoving", false);
    }

    public void PlayMove()
    {
        anim.SetBool("IsMoving", true);
    }

    public void PlayAttack()
    {
        anim.SetTrigger("IsAttacking");
    }

    public void PlayDeath()
    {
        anim.SetTrigger("Die");
    }
}
