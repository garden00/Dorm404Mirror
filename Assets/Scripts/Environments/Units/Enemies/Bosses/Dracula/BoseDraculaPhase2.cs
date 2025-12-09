using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoseDraculaPhase2 : MonoBehaviour, IDamageable, IAttacker
{
    [Header("Phase 2 Settings")]
    [SerializeField] private int maxHealth = 1500;
    [SerializeField] private UnitHealthBar healthBar;
    private int currentHealth;

    [Header("Minion Control")]
    [SerializeField] private GameObject creaturePrefab;
    [SerializeField] private List<GameObject> activeMinions = new List<GameObject>();

    [Header("Attack Patterns")]
    [SerializeField] private GameObject wideProjectile; // 1번 패턴

    private Transform playerTransform;
    private bool isDead = false;

    [SerializeField] private int damage = 30;
    public int Damage => damage;

    [Header("hmm")]
    [SerializeField] private BoxCollider2D mapBoundary;// 텔포 범위, 맵에서 지정해줘야함
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float bodyRadius = 1.0f;

    private void Start()
    {
        currentHealth = maxHealth;
        if (healthBar) healthBar.UpdateHealth(currentHealth, maxHealth);
        playerTransform = PlayerManager.Instance.transform;

        StartCoroutine(Phase2PatternRoutine());
    }

    private void Update()
    {
        if (isDead) return;
        ManageMinions();
    }

    // --- IDamageable 구현 (무적 기믹) ---
    public void ReceiveAttack(DamageInfo damageInfo)
    {
        // 2페이즈는 일반 투사체/공격에 데미지를 입지 않고 통과시킴
        if (damageInfo.light)
        {
            TakeDamage(damageInfo.damage);
        }
    }

    private void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (healthBar) healthBar.UpdateHealth(currentHealth, maxHealth);

        if (currentHealth <= 0) Die();
    }

    // --- 하수인 관리 ---
    private void ManageMinions()
    {
        // 리스트에서 죽거나 없어진 하수인 제거
        activeMinions.RemoveAll(x => x == null || !x.activeSelf);

        if (activeMinions.Count < 1)
        {
            Vector3 spawnPos = transform.position + (Vector3)Random.insideUnitCircle * 4f;
            GameObject minion = Instantiate(creaturePrefab);
            minion.transform.position = spawnPos;
            activeMinions.Add(minion);
        }
    }

    // --- 패턴 루틴 ---
    private IEnumerator Phase2PatternRoutine()
    {
        yield return new WaitForSeconds(1.5f);

        while (!isDead)
        {
            int rand = Random.Range(0, 3);
            // 0: 광범위 투사체, 1: 배후 습격, 2: 랜덤 이동

            switch (rand)
            {
                case 0:
                    //Debug.Log("2페이즈 패턴1: 폭발 투사체");
                    FireProjectile(wideProjectile);
                    yield return new WaitForSeconds(1.5f);
                    break;
                case 1:
                    //Debug.Log("2페이즈 패턴2: 배후 습격");
                    yield return StartCoroutine(TeleportBackAttack());
                    break;
                case 2:
                    //Debug.Log("2페이즈 패턴3: 위치 재조정");
                    yield return StartCoroutine(TeleportRandom());
                    break;
            }
            yield return new WaitForSeconds(2.0f);
        }
    }

    private void FireProjectile(GameObject prefab)
    {
        Vector3 dir = (playerTransform.position - transform.position).normalized;
        var obj = ObjectPoolingManager.Instance.GetPrefab(prefab);
        obj.GetComponent<IProjectile>().Fire(transform.position, dir, gameObject.tag);
    }

    private IEnumerator TeleportBackAttack()
    {
        Vector3 playerPos = playerTransform.position;
        Vector3 backDir = -playerTransform.right; // 플레이어 뒤쪽
        float teleportDist = 3.0f;

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

        // 근접 공격 로직
        if (Vector3.Distance(transform.position, playerTransform.position) < 2.5f)
        {
            playerTransform.GetComponent<IDamageable>()?.ReceiveAttack(new DamageInfo(this, AttackType.Melee, damage));
        }
        yield return new WaitForSeconds(0.5f);


    }

    private IEnumerator TeleportRandom()
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

    private IEnumerator TeleportEffect(Vector3 destination)
    {
        Vector3Int aa = Vector3Int.CeilToInt(destination);

        // 사라지는 연출
        yield return new WaitForSeconds(0.2f);
        transform.position = aa;
        // 나타나는 연출
        yield return new WaitForSeconds(0.2f);
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("드라큘라 사망");

        GetComponent<D2_AnimatorController>().PlayDeath();

        Destroy(gameObject); // 또는 엔딩 연출
    }
}
