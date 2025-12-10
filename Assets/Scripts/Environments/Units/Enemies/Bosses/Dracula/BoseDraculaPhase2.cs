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

    [Header("Effect")]
    [SerializeField] private DraculaEffectPlayer effectPlayer;

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

        //effectPlayer?.PlayCast();

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

            GameObject minion = Instantiate(creaturePrefab, Vector3Int.CeilToInt(destPos), Quaternion.identity);
            activeMinions.Add(minion);
        }
    }

    // --- 패턴 루틴 ---
    private IEnumerator Phase2PatternRoutine()
    {
        while (Vector3.Distance(playerTransform.position, transform.position) > 5f)
        {
            yield return null;
        }

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
        effectPlayer?.PlayCast();

        Vector3 dir = (playerTransform.position - transform.position).normalized;
        var obj = ObjectPoolingManager.Instance.GetPrefab(prefab);
        obj.GetComponent<IProjectile>().Fire(transform.position, dir, gameObject.tag);
    }

    private IEnumerator TeleportBackAttack()
    {
        Vector3 playerPos = playerTransform.position;
        Vector3 backDir = -PlayerManager.Instance.Status.ViewDirection;
        float teleportDist = 2.0f;

        Vector3 targetPos = playerPos + backDir * teleportDist;

        if (!mapBoundary.bounds.Contains(targetPos))
        {
            targetPos = mapBoundary.bounds.ClosestPoint(targetPos);
        }

        float distToTarget = Vector3.Distance(playerPos, targetPos);
        RaycastHit2D hit = Physics2D.Raycast(playerPos, (targetPos - playerPos).normalized, distToTarget, wallLayer);

        if (hit.collider != null)
        {
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
            randomPoint = Vector3Int.CeilToInt(randomPoint);

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
        effectPlayer?.PlayTeleport();
        yield return new WaitForSeconds(0.2f);
        transform.position = aa;

        // 나타나는 연출
        effectPlayer?.PlayTeleport();
        yield return new WaitForSeconds(0.2f);
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("드라큘라 사망");

        effectPlayer?.ForceHide();
        GetComponent<D2_AnimatorController>().PlayDeath();

        Destroy(gameObject); // 또는 엔딩 연출
    }
}
