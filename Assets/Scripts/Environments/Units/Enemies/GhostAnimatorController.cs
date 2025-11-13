using UnityEngine;

public class GhostAnimatorController : MonoBehaviour
{
    private Animator animator;

    [SerializeField] private string direction = "down";
    // down, left, up, right

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    // ------------------------------
    // 애니메이션 재생 함수들
    // ------------------------------
    public void PlayIdle()
    {
        if (animator == null) return;
        animator.Play($"ghost_{direction}_idle");
    }

    public void PlayAttack()
    {
        if (animator == null) return;
        animator.Play($"ghost_{direction}_attack");
    }

    public void PlayHit()
    {
        if (animator == null) return;
        animator.Play($"ghost_{direction}_hit");
    }

    public void PlayDeath()
    {
        if (animator == null) return;
        animator.Play($"ghost_{direction}_death");
    }

    public void SetDirection(string dir)
    {
        direction = dir;
    }
}
