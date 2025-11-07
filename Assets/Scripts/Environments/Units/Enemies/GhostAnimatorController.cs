using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostAnimatorController : MonoBehaviour
{
    private Animator animator;
    private EnemyExample enemy; // 공격 타이밍 참고, EnemyExample.cs의 EnemyExample class


    private float attackTimer = 0f;
    private bool isAttacking = false;

    [SerializeField] private string direction = "down";
    // down, left, up, right

    void Awake()
    {
        animator = GetComponent<Animator>();
        enemy = GetComponent<EnemyExample>(); // 없어도 됨, 필요시만
    }

    void Update()
    {
        // EnemyExample.ThrowCycleTime 값을 참고해서 타이밍 동기화
        if (enemy != null)
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= enemy.ThrowCycleTime)
            {
                StartAttack();
                attackTimer = 0f;
            }
        }

        // 공격 중이면 Idle로 복귀
        if (isAttacking && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
        {
            isAttacking = false;
            animator.Play($"ghost_{direction}_idle");
        }
    }

    void StartAttack()
    {
        isAttacking = true;
        animator.Play($"ghost_{direction}_attack");
    }
}
