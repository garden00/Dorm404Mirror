using System.Collections;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    private Animator anim;
    private PlayerCombat combat;
    private PlayerMovement movement;
    private PlayerStatusData status;

    [SerializeField] private float reflectAnimDuration = 0.15f;

    private Coroutine hitRoutine;
    private Coroutine reflectRoutine;

    public void Initialize(PlayerStatusData playerStatus)
    {
        status = playerStatus;
        anim = GetComponent<Animator>();
        combat = GetComponent<PlayerCombat>();
        movement = GetComponent<PlayerMovement>();

        combat.OnReflectEvent += PlayReflectAnimation;
        status.OnHit += PlayHitAnimation;
    }

    private void OnDestroy()
    {
        // event 해제
        if(combat != null)
            combat.OnReflectEvent += PlayReflectAnimation;

        if (status != null)
            status.OnHit += PlayHitAnimation;
    }

    void Update()
    {
        if(status != null)
        UpdateDirection();
    }


    private void UpdateDirection()
    {
        Vector3 dir = status.ViewDirection.VectorNormalized;

        float x = dir.x;
        float y = dir.y;

        if (float.IsNaN(x)) x = 0;
        if (float.IsNaN(y)) y = -1;

        anim.SetFloat("lastX", x);
        anim.SetFloat("lastY", y);

        if (status.IsMoving)   // 이동 중이면 moveX/moveY도 갱신
        {
            anim.SetFloat("moveX", x);
            anim.SetFloat("moveY", y);
        }
        else
        {
            anim.SetFloat("moveX", 0);
            anim.SetFloat("moveY", 0);
        }
    }


    public void PlayReflectAnimation()
    {
        if (reflectRoutine != null)
            StopCoroutine(reflectRoutine);

        reflectRoutine = StartCoroutine(ReflectRoutine());
    }

    private IEnumerator ReflectRoutine()
    {
        anim.SetBool("Reflect", true);
        yield return new WaitForSeconds(reflectAnimDuration);
        anim.SetBool("Reflect", false);

        reflectRoutine = null;
    }

    public void PlayHitAnimation(Vector3 v)
    {
        if (hitRoutine != null)
            StopCoroutine(hitRoutine);

        anim.SetBool("Hit", true);
        hitRoutine = StartCoroutine(EndHitRoutine());
    }

    private IEnumerator EndHitRoutine()
    {
        yield return new WaitForSeconds(0.35f);

        anim.SetBool("Hit", false);
        hitRoutine = null;
    }
}