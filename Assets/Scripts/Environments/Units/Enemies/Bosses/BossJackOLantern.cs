using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BossJackOLantern : MonoBehaviour, IDamageable, IProjectile
{
    // (이전과 동일한 변수들...)
    [Header("Prefabs")]
    [SerializeField]
    private GameObject projectilePrefab;
    [SerializeField]
    private GameObject vinePrefab;

    // [필수] 여기에 보스 패턴 enum을 정의합니다.
    private enum BossPattern
    {
        Pattern1,
        Pattern2,
        Pattern3,
        Pattern4
    }
    // 이 변수가 위의 enum을 사용할 수 있게 됩니다.
    private BossPattern currentPattern = BossPattern.Pattern1;
    private int patternCount;

    [Header("Pattern Settings")]
    public float patternInterval = 10f;
    [SerializeField] private float dashSpeed = 20f; // 돌진 속도
    [SerializeField] private float dashDuration = 1.5f; // 돌진 최대 지속 시간

    private Transform playerTransform;
    private bool isDashing = false;

    private Rigidbody2D rigid; // Rigidbody 참조

    [SerializeField]
    private int maxHealth = 100;
    private int currentHealth;
    public int CurrentHealth
    {
        get => currentHealth;
        set
        {
            currentHealth = value;

            if (currentHealth <= 0)
            {
                currentHealth = 0;
                Die();
            }
            else if (currentHealth > maxHealth) currentHealth = maxHealth;


            //Debug.Log("enemy health : " + currentHealth);
        }
    }

    [SerializeField]
    int damage = 30;
    int IProjectile.Damage => damage;

    EightDirection IProjectile.MoveDirection => EightDirection.None;

    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        CurrentHealth = maxHealth;

        // 전체 패턴 개수 계산 (예: 4)
        patternCount = 4;//System.Enum.GetNames(typeof(BossPattern)).Length;

        // 보스 패턴 사이클 시작
        playerTransform = PlayerManager.Instance.gameObject.transform;
        StartCoroutine(BossPatternCycle());
    }

    /// <summary>
    /// 보스의 핵심 패턴 관리 코루틴
    /// </summary>
    IEnumerator BossPatternCycle()
    {
        Debug.Log("보스 패턴 시작.");
        while (true) // (보스가 살아있는 동안)
        {
            // 1. 현재 패턴 실행
            switch (currentPattern)
            {
                case BossPattern.Pattern1:
                    StartCoroutine(ExecutePattern1());
                    break;
                case BossPattern.Pattern2:
                    StartCoroutine(ExecutePattern2());
                    break;
                case BossPattern.Pattern3:
                    StartCoroutine(ExecutePattern3());
                    break;
                case BossPattern.Pattern4:
                    StartCoroutine(ExecutePattern4());
                    break;
            }

            // 2. 다음 패턴이 시작되기까지 대기
            yield return new WaitForSeconds(patternInterval);

            // 3. 다음 패턴으로 인덱스 변경 (순환)
            // ( (0+1) % 4 = 1, (1+1) % 4 = 2, (3+1) % 4 = 0 )
            int nextPatternIndex = ((int)currentPattern + 1) % patternCount;
            currentPattern = (BossPattern)nextPatternIndex;

            Debug.Log($"다음 패턴 준비: {currentPattern}");
        }
    }


    // [ IDamageable 인터페이스 구현 ]
    public void ReceiveAttack(IProjectile _projectile)
    {
        CurrentHealth -= _projectile.Damage;
    }

    // --- 패턴 1: 플레이어 방향으로 투사체 여러 발 발사 ---
    IEnumerator ExecutePattern1()
    {
        Debug.Log("보스: 패턴 1 시작");
        int shotCount = 5;

        for (int i = 0; i < shotCount; i++)
        {
            Vector3 shootPos = transform.position;
            Vector3 throwDirectionVec = (playerTransform.position - shootPos).normalized;

            // Vector3를 EightDirection으로 변환
            EightDirection shootDir = EightDirection.FromVector3(throwDirectionVec);

            ObjectPoolingManager.Instance.GetPrefab(projectilePrefab)
                .GetComponent<IProjectile>().
                Fire(shootPos, shootDir, gameObject.tag);

            yield return new WaitForSeconds(0.3f);
        }
    }

    // --- 패턴 2: 플레이어 방향으로 돌진 ---
    IEnumerator ExecutePattern2()
    {
        Debug.Log("보스: 패턴 2 시작 (돌진)");

        // 1. 돌진 상태 활성화 (OnCollisionEnter가 이 값을 참조)
        isDashing = true;

        // 2. 돌진 시작 시점의 플레이어 방향으로 방향 고정
        Vector3 dashDirection = (playerTransform.position - transform.position).normalized;

        // 3. Rigidbody에 힘(속도) 적용
        rigid.velocity = dashDirection * dashSpeed;

        // 4. 돌진 지속 시간동안 대기 (혹은 충돌할 때까지)
        float dashTimer = 0f;

        // isDashing이 true이고(아직 충돌 안함) && 설정된 시간이 다 안됐을 때
        while (isDashing && dashTimer < dashDuration)
        {
            dashTimer += Time.deltaTime;
            yield return null; // 1프레임 대기
        }

        // 5. 돌진 종료 (시간이 다 됐거나, OnCollisionEnter에서 isDashing이 false로 바뀜)
        rigid.velocity = Vector3.zero; // 돌진 속도 초기화
        isDashing = false; // 상태 확실하게 종료
        Debug.Log("보스: 돌진 종료");
    }

    // [ 패턴 2: 돌진 충돌 처리 ]
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDashing && collision.gameObject.CompareTag("Player"))
        {
            if (collision.gameObject.TryGetComponent<IDamageable>(out var playerDamageable))
            {
                playerDamageable.ReceiveAttack(this);
            }
        }
    }

    // --- 패턴 3: 플레이어 주변에서 투사체 발사 ---
    IEnumerator ExecutePattern3()
    {
        Debug.Log("보스: 패턴 3 시작");
        Vector3 playerPos = playerTransform.position;
        // ... (좌우 스폰 위치 계산) ...
        Vector3 spawnPosLeft = playerPos - playerTransform.right * 10f;

        Vector3 dirVecLeft = (playerPos - spawnPosLeft).normalized;
        EightDirection eightDirLeft = EightDirection.FromVector3(dirVecLeft);

        ObjectPoolingManager.Instance.GetPrefab(projectilePrefab)
            .GetComponent<IProjectile>()
            .Fire(spawnPosLeft, eightDirLeft, gameObject.tag);

        // ... (오른쪽도 동일) ...
        yield return null;
    }

    // --- 패턴 4: 덩쿨 소환 ---
    IEnumerator ExecutePattern4()
    {
        Debug.Log("보스: 패턴 4 시작 (덩쿨 소환)");
        // 덩쿨(Vine)은 'IDamageable'을 구현한 프리팹이어야 합니다.
        Instantiate(vinePrefab, playerTransform.position + Vector3.up * 2f, Quaternion.identity);
        yield return null;
    }


    public void Die()
    {
        Destroy(gameObject);
    }

    void IProjectile.Fire(Vector3 _position, EightDirection _direction, string _ownerTag)
    {
        throw new System.NotImplementedException();
    }

    void IProjectile.Reflect(Vector3 _position, EightDirection _direction, string _ownerTag)
    {
        throw new System.NotImplementedException();
    }
}
