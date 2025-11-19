using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class JOAnimatorController : MonoBehaviour
{
    private Animator anim;
    private SpriteRenderer sr;

    private float lastDir = -1f;

    public bool IsDashing { get; set; }
    public bool IsAttacking { get; set; }
    public bool IsHit { get; set; }

    void Awake()
    {
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    public void FaceTarget(Transform target)
    {
        if (IsDashing) return;

        float dx = target.position.x - transform.position.x;

        if (dx < 0f) lastDir = -1f;
        else if (dx > 0f) lastDir = 1f;

        UpdateFacing();
    }


    public void SetMoveDirection(Vector2 moveDir)
    {
        // 이동 속도는 넣지 않는다. (보스는 이동 기반이 아님)
        // Speed를 Idle로 강제로 바꾸지 않는다.
        if (IsDashing)
        {
            anim.Play("JO_move");
        }

        // 방향만 갱신
        if (moveDir.x < -0.1f) lastDir = -1f;
        else if (moveDir.x > 0.1f) lastDir = 1f;

        UpdateFacing();
    }

    private void UpdateFacing()
    {
        sr.flipX = (lastDir == 1f);
    }

    public void PlayAttack1()
    {
        IsAttacking = true;
        anim.SetTrigger("Attack1");
    }

    public void PlayAttack2()
    {
        IsAttacking = true;
        anim.SetTrigger("Attack2");
    }

    public void PlayHit()
    {
        IsHit = true;
        anim.SetTrigger("Hit");
    }

    public void PlayDeath()
    {
        anim.SetTrigger("Death");
    }
}
