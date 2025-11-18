using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScarecrowAnimationController : MonoBehaviour, IDamageable
{
    [Header("투사체 발사 관련")]
    [SerializeField] private GameObject projectlie;              // 투사체 프리팹
    [SerializeField] private EightDirection throwDirection;     // 발사 방향 (8방향 열거형)
    [SerializeField] private float throwCycleTime = 2f;         // 발사 주기 (초 단위)
    private float throwTimer = 0f;                               // 투사체 발사 타이머

    public float ThrowCycleTime => throwCycleTime;              // 외부에서 읽기 전용 접근

    [Header("체력 관련")]
    [SerializeField] private int maxHealth = 100;               // 최대 체력
    private int currentHealth;
    private bool isDead = false;                                // 사망 여부 체크

    private Animator animator;

    public int CurrentHealth
    {
        get => currentHealth;
        set
        {
            currentHealth = Mathf.Clamp(value, 0, maxHealth);   // 0 ~ maxHealth 사이로 제한

            if (currentHealth <= 0 && !isDead)
            {
                Die(); // 사망 처리
            }
        }
    }

    void Start()
    {
        CurrentHealth = maxHealth;                              // 시작 시 체력 설정
        animator = GetComponent<Animator>();                    // 애니메이터 컴포넌트 가져오기
    }

    void Update()
    {
        if (!isDead)
        {
            ThrowCycle(); // 주기적으로 투사체 발사 시도
        }
    }

    // 공격을 받았을 때 실행됨
    public void ReceiveAttack(IProjectile projectile)
    {
        if (isDead) return;

        CurrentHealth -= projectile.Damage; // 데미지 적용

        if (animator != null)
            animator.SetTrigger("Hit");     // 피격 애니메이션 실행
    }

    // 일정 주기마다 투사체를 발사하는 루틴
    private void ThrowCycle()
    {
        throwTimer += Time.deltaTime;

        if (throwTimer > throwCycleTime)
        {
            throwTimer = 0f;
            ThrowProjectile();
        }
    }

    // 투사체 발사 로직
    private void ThrowProjectile()
    {
        var obj = ObjectPoolingManager.Instance.GetPrefab(projectlie);
        obj.transform.position = transform.position;

        obj.GetComponent<IProjectile>()?.Fire(transform.position, throwDirection, gameObject.tag);
    }

    // 사망 처리
    private void Die()
    {
        isDead = true;

        if (animator != null)
            animator.SetTrigger("Die"); // 사망 애니메이션 실행

        // 일정 시간 후 오브젝트 비활성화
        StartCoroutine(DeactivateAfterDelay(1f));
    }

    private IEnumerator DeactivateAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false); // 오브젝트 풀링을 위한 비활성화
    }
}
