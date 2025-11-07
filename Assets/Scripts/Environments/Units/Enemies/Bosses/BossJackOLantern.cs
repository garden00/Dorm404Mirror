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
    public float patternInterval = 4f;
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
        yield return new WaitForSeconds(2f);
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
        Debug.Log("보스: 패턴 2 시작 (위치 변경 돌진)");
        isDashing = true;

        // 2. 목표 위치 및 시작 위치 설정
        Vector3 targetPos = playerTransform.position; // 돌진 시작 시점의 플레이어 위치
        Vector3 startPos = transform.position;

        Vector3 dirPosVec = targetPos - startPos;

        int dir = (int)dirPosVec.magnitude;

        EightDirection dirPos = EightDirection.FromVector3(dirPosVec);

        Vector3 endPos = dirPos.VectorGrid * 5f;

        float dashTimer = 0f;

        // 3. dashDuration 동안 startPos에서 targetPos로 이동 (Lerp)
        while (isDashing && dashTimer < dashDuration)
        {
            // 경과 시간을 0~1 사이의 비율(t)로 변환
            float t = dashTimer / dashDuration;

            // (선택적: 돌진을 더 부드럽게)
            // t = Mathf.SmoothStep(0, 1, t); // 처음과 끝을 부드럽게

            // Lerp를 사용하여 현재 프레임의 위치 계산
            transform.position = Vector3.Lerp(startPos, endPos, t);

            dashTimer += Time.deltaTime;
            yield return null; // 1프레임 대기
        }

        // 4. (혹시 모를 오차 보정) 돌진이 끝나면 목표 위치로 정확히 이동
        if (isDashing)
        {
            transform.position = endPos;
        }

        // 5. 돌진 상태 종료 및 Kinematic 해제
        isDashing = false;
        Debug.Log("보스: 돌진 종료");
    }

    // [ 패턴 2: 돌진 충돌 처리 ]
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDashing && other.gameObject.CompareTag("Player"))
        {
            other.gameObject.GetComponent<IDamageable>().ReceiveAttack(this);
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
