using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BoseDraculaPhase1 : MonoBehaviour, IDamageable, IAttacker
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 1000;
    [SerializeField] private UnitHealthBar healthBar;
    private int currentHealth;

    [Header("Rage Settings")]
    [SerializeField] private float maxRage = 100f;
    [SerializeField] private float ragePerLightning = 20f; // 번개 반사 피격 시 차는 게이지
    private float currentRage = 0f;
    private bool isBerserk = false;

    [Header("Attack Settings")]
    [SerializeField] private int damage = 20;
    public int Damage => damage;

    [Header("Prefabs & References")]
    [SerializeField] private GameObject creaturePrefab; 
    [SerializeField] private GameObject clonePrefab;
    [SerializeField] private GameObject reflectableProjectile; 
    [SerializeField] private GameObject wideProjectile;  
    [SerializeField] private Transform firePoint;

    [Header("hmm")]
    [SerializeField] private BoxCollider2D mapBoundary;// 텔포 범위, 맵에서 지정해줘야함
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float bodyRadius = 1.0f;

    [Header("Effect")]
    [SerializeField] private DraculaEffectPlayer effectPlayer;

    [SerializeField] private SpriteRenderer bodySprite; // 패턴2에서 색 잠깐 어둡게 하기
    [SerializeField] private Color dashColor = new Color(0.6f, 0.6f, 0.6f, 1f);
    private Color originalColor;

    private Coroutine berserkEffectRoutine;


    private Transform playerTransform;
    private Animator animator; // 애니메이션 제어용

    // 상태 관리
    private bool isDead = false;
    private bool isPatternRunning = false;

    private List<GameObject> activeMinions = new List<GameObject>();

    private void Start()
    {
        currentHealth = maxHealth;
        if (healthBar) healthBar.UpdateHealth(currentHealth, maxHealth);

        playerTransform = PlayerManager.Instance.transform; // 싱글톤 가정
        animator = GetComponent<Animator>();
        originalColor = bodySprite.color;

        StartCoroutine(PatternRoutine());
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            DamageInfo info = new DamageInfo(this, AttackType.Melee, Damage);
            collision.GetComponent<IDamageable>()?.ReceiveAttack(info);
        }
    }

    // --- IDamageable 구현 ---
    public void ReceiveAttack(DamageInfo damageInfo)
    {
        if (isDead) return;

        GetComponent<D1_AnimatorController>().PlayHit();

        currentHealth -= damageInfo.damage;
        if (healthBar) healthBar.UpdateHealth(currentHealth, maxHealth);

        AddRage(damageInfo);

        if (currentHealth <= 0) Die();
    }

    private void AddRage(DamageInfo damageInfo)
    {
        if (isBerserk) return; // 이미 폭주 중이면 무시

        if (!damageInfo.elect) return;

        currentRage += damageInfo.damage;
        if (currentRage >= maxRage)
        {
            StartCoroutine(EnterBerserkMode());
        }
    }

    // --- 패턴 루틴 ---
    private IEnumerator PatternRoutine()
    {
        while (Vector3.Distance(playerTransform.position, transform.position) > 6f)
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        while (!isDead)
        {
            if (isBerserk)
            {
                yield return StartCoroutine(BerserkSequence());
                // 시퀀스 종료 후 폭주 해제
                isBerserk = false;
                currentRage = 0f;

                // 폭주 이펙트 루프 종료
                if (berserkEffectRoutine != null)
                {
                    StopCoroutine(berserkEffectRoutine);
                    berserkEffectRoutine = null;
                }

                // 혹시 재생 중 이펙트가 남아있으면 강제 숨김
                effectPlayer?.ForceHide();

                Debug.Log("폭주 종료: 기본 상태 복귀");
            }
            else
            {
                yield return StartCoroutine(ExecuteNormalPattern());
            }

            yield return new WaitForSeconds(1.5f); // 패턴 간 대기 시간
        }
    }

    private IEnumerator BerserkEffectLoop()
    {
        while (isBerserk)
        {
            effectPlayer?.PlayBerserkFlash();
            yield return new WaitForSeconds(Random.Range(0.6f, 1.2f));
        }
    }

    private IEnumerator EnterBerserkMode()
    {
        isBerserk = true;
        // 폭주 진입 연출(포효 등) 시간 대기

        if (berserkEffectRoutine == null)
            berserkEffectRoutine = StartCoroutine(BerserkEffectLoop());
        effectPlayer?.PlayBerserkFlash();


        yield return new WaitForSeconds(1.0f);
    }

    private IEnumerator BerserkSequence()
    {
        for (int i = 0; i < 3; i++)
        {
            yield return StartCoroutine(Pattern3_ReflectableProjectile());
            yield return new WaitForSeconds(0.1f);
            yield return StartCoroutine(Pattern3_ReflectableProjectile());
            yield return new WaitForSeconds(0.1f);
            yield return StartCoroutine(Pattern3_ReflectableProjectile());
            yield return new WaitForSeconds(0.1f);
            yield return StartCoroutine(Pattern3_ReflectableProjectile());

            yield return new WaitForSeconds(0.5f);
            yield return StartCoroutine(Pattern5_TeleportBackAttack());
            yield return new WaitForSeconds(0.5f);
            yield return StartCoroutine(Pattern6_TeleportRandom());
            yield return new WaitForSeconds(0.5f);
        }
    }

    private IEnumerator ExecuteNormalPattern()
    {
        yield return StartCoroutine(Pattern1_SummonCreature());

        int rand = Random.Range(1, 4);
        switch (rand)
        {
            //case 0: yield return StartCoroutine(Pattern1_SummonCreature()); break;

            case 1: yield return StartCoroutine(Pattern2_SplitAndCharge()); break;
            case 2: yield return StartCoroutine(Pattern4_WideProjectile()); break;
            case 3: yield return StartCoroutine(Pattern6_TeleportRandom()); break;
        }
    }

    // --- 개별 패턴 구현 ---

    // 1. 크리처 소환
    private IEnumerator Pattern1_SummonCreature()
    {
        // 리스트에서 죽거나 없어진 하수인 제거
        activeMinions.RemoveAll(x => x == null || !x.activeSelf);

        effectPlayer?.PlayCast();

        if (activeMinions.Count < 1)
        {
            // 범위 안에서만 소환
            Vector3 destPos = transform.position;
            bool foundSafePos = false;
            int maxAttempts = 15;

            Bounds bounds = mapBoundary.bounds;

            for (int i = 0; i < maxAttempts; i++)
            {
                float randX = Random.Range(bounds.min.x, bounds.max.x);
                float randY = Random.Range(bounds.min.y, bounds.max.y);

                Vector3 randomPoint = new Vector3(randX, randY, transform.position.z);

                if (!Physics2D.OverlapCircle(randomPoint, bodyRadius, wallLayer))
                {
                    destPos = randomPoint;
                    foundSafePos = true;
                    break;
                }
            }

            if (!foundSafePos)
            {
                //안전한 위치를 찾지 못함
                destPos = transform.position;
            }

            Debug.Log("add");
            GameObject minion = Instantiate(creaturePrefab, Vector3Int.CeilToInt(destPos), Quaternion.identity);
            Debug.Log(Vector3Int.CeilToInt(destPos));
            activeMinions.Add(minion);
        }

        yield return new WaitForSeconds(0.5f);

        
    }

    // 2. 분신으로 분열 후 돌진
    private IEnumerator Pattern2_SplitAndCharge()
    { 
        // 1. 분신 생성 (시각적 효과)
        Vector3[] offsets = { Vector3.left * 2, Vector3.right * 2 };
        List<GameObject> clones = new List<GameObject>();

        foreach (var offset in offsets)
        {
            var clone = Instantiate(clonePrefab, transform.position + offset, Quaternion.identity);
            clones.Add(clone);
        }

        yield return new WaitForSeconds(1.0f); // 플레이어가 구분할 시간 줌

        // 이펙트
        effectPlayer?.PlayDash();
        if (bodySprite != null) bodySprite.color = dashColor;

        // 2. 본체와 분신 모두 플레이어 방향으로 돌진
        Vector3 targetPos = playerTransform.position;
        Vector3 startPos = transform.position;
        float duration = 0.3f;
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            // 분신들도 평행하게 돌진한다고 가정
            foreach (var clone in clones)
            {
                if (clone) clone.transform.position = Vector3.Lerp(clone.transform.position, targetPos + (clone.transform.position - startPos), t);
            }
            yield return null;
        }

        if (bodySprite != null) bodySprite.color = originalColor;

        // 3. 분신 제거
        foreach (var clone in clones) Destroy(clone);
    }

    // 3. 반사 가능 투사체
    private IEnumerator Pattern3_ReflectableProjectile()
    {
        effectPlayer?.PlayCast();
        FireProjectile(reflectableProjectile);
        yield return new WaitForSeconds(0.5f);
    }

    // 4. 일반 투사체
    private IEnumerator Pattern4_WideProjectile()
    {
        effectPlayer?.PlayCast();
        FireProjectile(wideProjectile);
        yield return new WaitForSeconds(0.3f);
        FireProjectile(wideProjectile);
        yield return new WaitForSeconds(0.3f);
        FireProjectile(wideProjectile);
        yield return new WaitForSeconds(0.3f);
    }

    // 5. 플레이어 뒤로 순간이동 후 공격
    private IEnumerator Pattern5_TeleportBackAttack()
    {
        Vector3 playerPos = playerTransform.position;
        Vector3 backDir = -PlayerManager.Instance.Status.ViewDirection;
        float teleportDist = 2.0f;

        // 1. 일단 가고 싶은 위치 계산
        Vector3 targetPos = playerPos + backDir * teleportDist;

        // 2. [맵 범위 제한] 가려는 곳이 맵 밖인가?
        // mapBoundary.bounds는 콜라이더의 월드 공간 경계 상자(AABB)를 가져옵니다.
        if (!mapBoundary.bounds.Contains(targetPos))
        {
            // 맵 밖이라면, 맵 경계선 중 가장 가까운 안쪽 위치로 변경
            targetPos = mapBoundary.bounds.ClosestPoint(targetPos);
        }

        // 3. [내부 벽 체크] 플레이어와 목표 지점 사이에 장애물(기둥 등)이 있는가?
        float distToTarget = Vector3.Distance(playerPos, targetPos);
        // 목표 지점까지 레이를 쏘되, bodyRadius 만큼의 여유를 두고 검사
        RaycastHit2D hit = Physics2D.Raycast(playerPos, (targetPos - playerPos).normalized, distToTarget, wallLayer);

        if (hit.collider != null)
        {
            // 장애물이 있으면 장애물 바로 앞으로 위치 수정
            targetPos = hit.point - (Vector2)(targetPos - playerPos).normalized * bodyRadius;
        }

        yield return StartCoroutine(TeleportEffect(targetPos));

        // 근접 공격 (히트박스 활성화 등)
        // animator.SetTrigger("MeleeAttack");
        // 공격 범위 내 플레이어에게 데미지 (Physics2D.OverlapCircle 등 사용)
        CheckMeleeAttack();

        yield return new WaitForSeconds(0.5f);
    }

    // 6. 근처 랜덤 순간이동
    private IEnumerator Pattern6_TeleportRandom()
    {
        Vector3 destPos = transform.position;
        bool foundSafePos = false;
        int maxAttempts = 15;

        // 맵 경계 정보 캐싱 (최적화)
        Bounds bounds = mapBoundary.bounds;

        for (int i = 0; i < maxAttempts; i++)
        {
            // 1. [맵 범위 랜덤] BoxCollider2D의 min, max 좌표 사이에서 랜덤 추출
            float randX = Random.Range(bounds.min.x, bounds.max.x);
            float randY = Random.Range(bounds.min.y, bounds.max.y);

            Vector3 randomPoint = new Vector3(randX, randY, transform.position.z);

            // 2. [내부 벽 체크] 생성된 위치가 내부 장애물(Wall)과 겹치는지 확인
            if (!Physics2D.OverlapCircle(randomPoint, bodyRadius, wallLayer))
            {
                destPos = randomPoint;
                foundSafePos = true;
                break;
            }
        }

        if (!foundSafePos)
        {
            Debug.LogWarning("안전한 위치를 찾지 못함 -> 플레이어 근처로 이동");
            destPos = playerTransform.position;
        }

        yield return StartCoroutine(TeleportEffect(destPos));
    }

    // 유틸리티: 투사체 발사
    private void FireProjectile(GameObject prefab)
    {
        Vector3 dir = (playerTransform.position - transform.position).normalized;
        var obj = ObjectPoolingManager.Instance.GetPrefab(prefab);
        obj.GetComponent<IProjectile>().Fire(transform.position, dir, gameObject.tag);
    }

    // 유틸리티: 순간이동 연출
    private IEnumerator TeleportEffect(Vector3 destination)
    {
        Vector3Int aa = Vector3Int.CeilToInt(destination);

        // 사라지는 연출
        effectPlayer?.PlayTeleport();
        yield return new WaitForSeconds(0.5f);
        transform.position = aa;

        // 나타나는 연출
        effectPlayer?.PlayTeleport();
        yield return new WaitForSeconds(0.2f);
    }

    private void CheckMeleeAttack()
    {
        if (Vector3.Distance(transform.position, playerTransform.position) < 2.0f)
        {
            playerTransform.GetComponent<IDamageable>()?.ReceiveAttack(new DamageInfo(this, AttackType.Melee, damage));
        }
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("드라큘라 1페이즈 종료");
        // 2페이즈 오브젝트 활성화 로직 추가 필요

        GetComponent<D1_AnimatorController>().PlayDeath();


        Destroy(gameObject, 1f);
    }
}
