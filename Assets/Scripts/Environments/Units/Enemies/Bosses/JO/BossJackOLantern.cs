using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;

[RequireComponent(typeof(Rigidbody2D))]
public class BossJackOLantern : MonoBehaviour, IDamageable, IAttacker
{
    [Header("Prefabs")]
    [SerializeField]
    private GameObject projectilePrefab;
    [SerializeField]
    private GameObject vinePrefab;

    private enum BossPattern
    {
        Pattern1,
        Pattern2,
        Pattern3,
        Pattern4
    }

    private BossPattern currentPattern = BossPattern.Pattern1;
    private int patternCount;

    [Header("Pattern Settings")]
    public float patternInterval = 4f;
    [SerializeField] private float dashDuration = 1.5f; // 돌진 최대 지속 시간 ( 돌진속도의 역수 )
    [SerializeField] private float dashLen = 10f; // 돌진 길이

    private Transform playerTransform;
    private bool isDashing = false;

    private Rigidbody2D rigid; // Rigidbody 참조

    [SerializeField] private LayerMask wallLayer; // 인스펙터에서 Wall 레이어를 설정해야 함
    [SerializeField] private float bodySize = 1f; // 보스의 반지름 (벽에 파묻히지 않게 여유 공간)


    [Header("Health")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private UnitHealthBar healthBar;

    private int currentHealth;
    public int CurrentHealth
    {
        get => currentHealth;
        set
        {
            currentHealth = value;

            if (healthBar == null)
                healthBar = GetComponentInChildren<UnitHealthBar>();

            if (healthBar != null)
                healthBar.UpdateHealth(currentHealth, maxHealth);

            if (currentHealth <= 0)
            {
                currentHealth = 0;
                Die();
            }
            else if (currentHealth > maxHealth)
                currentHealth = maxHealth;
        }
    }

    [SerializeField]
    int damage = 30;
    int IAttacker.Damage => damage;

    bool isDetected;
    private bool isDead = false;

    private JOAnimatorController bossAnim;

    void Start()
    {
        isDetected = false;

        rigid = GetComponent<Rigidbody2D>();
        if (healthBar == null)
            healthBar = GetComponentInChildren<UnitHealthBar>();
        CurrentHealth = maxHealth;
        
        // 전체 패턴 개수 계산 (예: 4)
        patternCount = 4;//System.Enum.GetNames(typeof(BossPattern)).Length;

        // 보스 패턴 사이클 시작
        playerTransform = PlayerManager.Instance.gameObject.transform;

        bossAnim = GetComponent<JOAnimatorController>();
    }

    void Update()
    {
        if (!isDetected)
        {
            float dir = Vector3.Distance(playerTransform.position, transform.position);

            if (dir < 20f)
            {
                isDetected = true;
                StartCoroutine(BossPatternCycle());
            }
        }

        Vector2 faceDir = playerTransform.position - transform.position;
        bossAnim.SetFacingDirection(faceDir);
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
    public void ReceiveAttack(DamageInfo damageInfo)
    {
        int prev = CurrentHealth;
        CurrentHealth -= damageInfo.damage;

        if (CurrentHealth <= 0)
        {
            return;
        }

        // 살아있는 히트일 때만 hit 재생
        bossAnim.PlayHit();
    }


    // --- 패턴 1: 플레이어 방향으로 투사체 여러 발 발사 ---
    IEnumerator ExecutePattern1()
    {
        Debug.Log("보스: 패턴 1 시작");
        bossAnim.FaceTarget(playerTransform);
        bossAnim.PlayAttack1();

        int shotCount = 5;

        for (int i = 0; i < shotCount; i++)
        {
            Vector3 shootPos = transform.position;
            Vector3 throwDirectionVec = (playerTransform.position - shootPos).normalized;

            // Vector3를 EightDirection으로 변환
            EightDirection shootDir = EightDirection.FromVector3(throwDirectionVec);

            ObjectPoolingManager.Instance.GetPrefab(projectilePrefab)
                .GetComponent<IProjectile>().
                Fire(shootPos, throwDirectionVec, gameObject.tag);

            yield return new WaitForSeconds(0.3f);
        }
    }

    // --- 패턴 2: 플레이어 방향으로 돌진 ---
    IEnumerator ExecutePattern2()
    {
        Debug.Log("보스: 패턴 2 시작 (위치 변경 돌진)");
        isDashing = true;
        bossAnim.SetMoveSpeed(1f);
        bossAnim.FaceTarget(playerTransform);

        // 2. 목표 위치 및 시작 위치 설정
        Vector3 targetPos = playerTransform.position;
        Vector3 startPos = transform.position;

        Vector3 dirPosVec = targetPos - startPos;
        EightDirection dirPos = EightDirection.FromVector3(dirPosVec);

        // 원래 가려고 했던 목표 지점
        Vector3 originalEndPos = startPos + dirPos.VectorGrid * dashLen;
        Vector3 moveDir = dirPos.VectorGrid.normalized; // 이동 방향 단위 벡터

        // --- [수정된 부분: 벽 감지 로직] ---
        Vector3 finalEndPos = originalEndPos;

        // 시작점에서 목표 방향으로 dashLen만큼 레이저를 쏩니다.
        // 2D 게임이라면 Physics2D, 3D라면 Physics를 사용하세요. (아래는 2D 기준)
        RaycastHit2D hit = Physics2D.Raycast(startPos, moveDir, dashLen, wallLayer);

        if (hit.collider != null)
        {
            // 벽에 부딪혔다면, 목표 지점을 '벽 위치 - 몸 크기'로 수정
            Debug.Log("벽 감지됨! 목표 위치 수정");
            finalEndPos = (Vector3)hit.point - (moveDir * bodySize);
        }
        // ------------------------------------

        float dashTimer = 0f;

        // 3. dashDuration 동안 이동 (수정된 finalEndPos 사용)
        while (isDashing && dashTimer < dashDuration)
        {
            float t = dashTimer / dashDuration;

            // (선택사항) 부드러운 움직임
            // t = Mathf.SmoothStep(0, 1, t); 

            transform.position = Vector3.Lerp(startPos, finalEndPos, t);

            dashTimer += Time.deltaTime;
            yield return null;
        }

        // 4. 오차 보정
        if (isDashing)
        {
            transform.position = finalEndPos;
        }

        // 5. 종료
        isDashing = false;
        bossAnim.SetMoveSpeed(0f);

        Debug.Log("보스: 돌진 종료");
    }

    // [ 패턴 2: 돌진 충돌 처리 ]
    private void OnTriggerStay2D(Collider2D other)
    {
        if (isDashing && other.gameObject.CompareTag("Player"))
        {
            DamageInfo info = new DamageInfo(this, AttackType.Melee, damage);

            other.gameObject.GetComponent<IDamageable>().ReceiveAttack(info);
        }
    }

    // --- 패턴 3: 플레이어 주변에서 투사체 발사 ---
    IEnumerator ExecutePattern3()
    {
        Debug.Log("보스: 패턴 3 시작");

        

        Vector3 playerPos = playerTransform.position;
        Vector3 spawnPosDown = playerPos - playerTransform.up * 10f;
        Vector3 spawnPosLeft = playerPos - playerTransform.right * 10f;
        Vector3 spawnPosRight = playerPos + playerTransform.right * 10f;
        Vector3 spawnPosUp = playerPos + playerTransform.up * 10f;

        Vector3 dirVecLeft = (playerPos - spawnPosLeft).normalized;
        Vector3 dirVecDown = (playerPos - spawnPosDown).normalized;
        Vector3 dirVecRight = (playerPos - spawnPosRight).normalized;
        Vector3 dirVecUp = (playerPos - spawnPosUp).normalized;

        ObjectPoolingManager.Instance.GetPrefab(projectilePrefab)
            .GetComponent<IProjectile>()
            .Fire(spawnPosLeft, dirVecLeft, gameObject.tag);
        yield return new WaitForSeconds(0.3f);
        ObjectPoolingManager.Instance.GetPrefab(projectilePrefab)
            .GetComponent<IProjectile>()
            .Fire(spawnPosDown, dirVecDown, gameObject.tag);
        yield return new WaitForSeconds(0.3f);
        ObjectPoolingManager.Instance.GetPrefab(projectilePrefab)
            .GetComponent<IProjectile>()
            .Fire(spawnPosRight, dirVecRight, gameObject.tag);
        yield return new WaitForSeconds(0.3f);
        ObjectPoolingManager.Instance.GetPrefab(projectilePrefab)
            .GetComponent<IProjectile>()
            .Fire(spawnPosUp, dirVecUp, gameObject.tag);

        // ... (오른쪽도 동일) ...
        yield return null;
    }

    // --- 패턴 4: 덩쿨 소환 ---
    IEnumerator ExecutePattern4()
    {
        Debug.Log("보스: 패턴 4 시작 (덩쿨 소환)");

        bossAnim.FaceTarget(playerTransform);
        bossAnim.PlayAttack2();

        Vector3 dirVec = playerTransform.position - transform.position;
        EightDirection dir = EightDirection.FromVector3(dirVec);
        if (dir.x != 0 && dir.y != 0) dir++;

        Instantiate(vinePrefab, playerTransform.position + Quaternion.Euler(0, 0, 90) * dir.VectorNormalized, Quaternion.identity)
            .transform.right = dir.VectorNormalized;
        Instantiate(vinePrefab, playerTransform.position + Quaternion.Euler(0, 0, 90) * dir.VectorNormalized, Quaternion.identity)
            .transform.up = dir.VectorNormalized;
        yield return null;
    }


    public void Die()
    {
        if (isDead) return;
        isDead = true;

        StopAllCoroutines();
        isDashing = false;
        bossAnim.IsDashing = false;

        bossAnim.PlayDeath();

        GateOpenTrigger gateTrigger = FindObjectOfType<GateOpenTrigger>();
        if (gateTrigger != null)
        {
            gateTrigger.SetBossDead();
        }

        EnemyManager.Instance.NotifyBossDead();


        StartCoroutine(DieRoutine());
    }

    private IEnumerator DieRoutine()
    {
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }
}
