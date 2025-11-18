using System.Collections;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    private Animator anim;
    private PlayerCombat combat;
    private PlayerMovement movement;
    private PlayerStatus status;

    [SerializeField] private float reflectAnimDuration = 0.15f;

    void Awake()
    {
        anim = GetComponent<Animator>();
        combat = GetComponent<PlayerCombat>();
        movement = GetComponent<PlayerMovement>();
    }

    void Start()
    {
        status = PlayerManager.Instance.Status;
    }


    void Update()
    {
        UpdateDirection();
    }


    private void UpdateDirection()
    {
        Vector3 dir = status.viewDirection.VectorNormalized;

        float x = dir.x;
        float y = dir.y;

        anim.SetFloat("lastX", x);
        anim.SetFloat("lastY", y);

        if (status.isAction)   // 이동 중이면 moveX/moveY도 갱신
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
        StopAllCoroutines();
        StartCoroutine(ReflectRoutine());
    }

    private IEnumerator ReflectRoutine()
    {
        anim.SetBool("Reflect", true);
        yield return new WaitForSeconds(reflectAnimDuration);
        anim.SetBool("Reflect", false);
    }

    public void PlayHitAnimation()
    {
        anim.SetBool("Hit", true);
        StartCoroutine(EndHitRoutine());
    }

    private IEnumerator EndHitRoutine()
    {
        yield return new WaitForSeconds(0.25f);
        anim.SetBool("Hit", false);
    }
}