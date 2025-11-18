using UnityEngine;

public class ScarecrowAnimatorController : MonoBehaviour
{
    private Animator animator;

    // Animator Parameters
    private static readonly int DirHash = Animator.StringToHash("Direction");
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int HitHash = Animator.StringToHash("Hit");
    private static readonly int DieHash = Animator.StringToHash("Die");

    [SerializeField] private int directionIndex = 0;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        animator.SetFloat(DirHash, directionIndex);
    }

    public void SetDirection(int dirIndex)
    {
        directionIndex = dirIndex;
        animator.SetFloat(DirHash, directionIndex);
    }

    public void SetDirection(string dir)
    {
        switch (dir)
        {
            case "down": SetDirection(0); break;
            case "left": SetDirection(1); break;
            case "up": SetDirection(2); break;
            case "right": SetDirection(3); break;
        }
    }

    public void PlayIdle()
    {
        animator.SetFloat(DirHash, directionIndex);
    }

    public void PlayAttack()
    {
        animator.SetTrigger(AttackHash);
    }

    public void PlayHit()
    {
        animator.SetTrigger(HitHash);
    }

    public void PlayDie()
    {
        animator.SetTrigger(DieHash);
    }
}
