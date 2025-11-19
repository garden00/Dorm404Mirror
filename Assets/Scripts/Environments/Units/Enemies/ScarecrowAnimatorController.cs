using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScarecrowAnimatorController : MonoBehaviour
{
    private Animator animator;

    // Animator Parameters
    private static readonly int DirHash = Animator.StringToHash("Direction");
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int HitHash = Animator.StringToHash("Hit");
    private static readonly int DieHash = Animator.StringToHash("Die");

    // 0: down, 1: left, 2: up, 3: right
    [SerializeField] private int directionIndex = 0;
    private Scarecrow owner;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        owner = GetComponent<Scarecrow>();
    }

    private void Start()
    {
        // 초기 방향 설정
        animator.SetFloat(DirHash, directionIndex);
    }


    // 방향 설정 함수
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
        if (owner.CurrentHealth <= 0)
        {
            animator.SetTrigger(DieHash); return;
        }
        animator.SetTrigger(HitHash);
    }

    public void PlayDeath()
    {
        animator.SetTrigger(DieHash);
    }
}
