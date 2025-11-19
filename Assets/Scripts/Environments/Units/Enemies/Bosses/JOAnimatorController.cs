using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class JOAnimatorController : MonoBehaviour
{
    private Animator anim;
    private SpriteRenderer sr;

    private float lastDir = -1f;
    public bool IsDashing { get; set; }


    private void Awake()
    {
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    public void SetFacingDirection(Vector2 dir)
    {
        if (dir.x < -0.01f) lastDir = -1f;
        else if (dir.x > 0.01f) lastDir = 1f;

        UpdateFacing();
    }

    private void UpdateFacing()
    {
        sr.flipX = lastDir > 0f;
    }

    public void SetMoveSpeed(float speed)
    {
        anim.SetFloat("Speed", speed);
    }

    public void FaceTarget(Transform target)
    {
        float dx = target.position.x - transform.position.x;

        if (dx < 0f) lastDir = -1f;
        else if (dx > 0f) lastDir = 1f;

        UpdateFacing();
    }

    public void PlayAttack1()
    {
        anim.SetTrigger("Attack1");
    }

    public void PlayAttack2()
    {
        anim.SetTrigger("Attack2");
    }

    public void PlayHit()
    {
        anim.ResetTrigger("Hit");
        anim.SetTrigger("Hit");
    }

    public void PlayDeath()
    {
        anim.SetTrigger("Death");
    }
}
