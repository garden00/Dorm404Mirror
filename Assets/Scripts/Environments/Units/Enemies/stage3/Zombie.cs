using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie : MonoBehaviour, IDamageable, IAttacker
{
    [Header("Movement - Grid")]
    [SerializeField] private float gridSize = 1f;                // 한 칸 크기
    [SerializeField] private float moveDuration = 0.2f;          // 한 칸 이동 시간
    [SerializeField] private Vector2 moveIntervalRange = new Vector2(0.4f, 0.8f); // 다음 이동까지 대기시간
    [SerializeField] private float detectRange = 6f;             // 플레이어 추적 시작 범위
    [SerializeField] private float stopDistance = 0.5f;          // 너무 가까우면 더 안 다가감
    [SerializeField] private LayerMask obstacleMask;             // 벽/문 레이어

    private bool isMoving = false;
    private Vector3 moveStartPos;
    private Vector3 moveTargetPos;
    private float moveProgress = 0f;

    private float moveTimer = 0f;
    private float currentMoveWait = 0.5f;

    [Header("Attack")]
    [SerializeField] private int contactDamage = 1;       // 접촉 데미지
    [SerializeField] private float attackInterval = 0.6f; // 접촉 중 데미지 간격(초)

    [SerializeField] private int damage;                  // IAttacker용 데미지
    int IAttacker.Damage => damage;

    private float lastAttackTime = -999f;

    [Header("Health")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;
    public bool IsDead { get; private set; } = false;

    private Transform player;
    private Rigidbody2D rb;
    private Collider2D col;

    // 애니메이션 컨트롤러(있으면 사용, 없으면 null이어서 무시)
    private ZombieAnimatorController zombieAnim;

    // --- 유틸: 그리드 스냅 ---
    private Vector3 SnapToGrid(Vector3 pos)
    {
        float x = Mathf.Round(pos.x / gridSize) * gridSize;
        float y = Mathf.Round(pos.y / gridSize) * gridSize;
        return new Vector3(x, y, pos.z);
    }

    private void Start()
    {
        // 시작 위치를 격자에 스냅
        transform.position = SnapToGrid(transform.position);

        currentHealth = maxHealth;

        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        zombieAnim = GetComponent<ZombieAnimatorController>();

        // 플레이어 찾기 (PlayerManager가 있으면 그걸 쓰고, 없으면 태그로 찾기)
        if (PlayerManager.Instance != null)
        {
            player = PlayerManager.Instance.transform;
        }
        else
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        currentMoveWait = Random.Range(moveIntervalRange.x, moveIntervalRange.y);

        // Rigidbody를 쓰더라도 이동은 transform으로 처리하므로
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }
    }

    private void Update()
    {
        if (IsDead) return;
        if (player == null) return;

        // 1) 현재 이동 중이면 Lerp로 한 칸 이동
        if (isMoving)
        {
            moveProgress += Time.deltaTime / moveDuration;
            moveProgress = Mathf.Clamp01(moveProgress);

            transform.position = Vector3.Lerp(moveStartPos, moveTargetPos, moveProgress);

            if (moveProgress >= 1f)
            {
                isMoving = false;
                transform.position = moveTargetPos;
                zombieAnim?.PlayIdle();
            }

            return;
        }

        // 2) 이동 중이 아니라면, 일정 시간 대기 후 한 칸 이동 결정
        moveTimer += Time.deltaTime;
        if (moveTimer < currentMoveWait)
            return;

        moveTimer = 0f;
        currentMoveWait = Random.Range(moveIntervalRange.x, moveIntervalRange.y);

        // 현재 위치를 먼저 스냅
        Vector3 origin = SnapToGrid(transform.position);
        transform.position = origin;

        float dist = Vector2.Distance(origin, player.position);

        if (dist <= detectRange)
        {
            TryStepTowardPlayer(origin, dist);
        }
        else
        {
            Wander(origin);
        }
    }

    // 플레이어 쪽으로 1칸 이동 시도
    private void TryStepTowardPlayer(Vector3 origin, float distToPlayer)
    {
        // 너무 가까우면 더 안 다가감
        if (distToPlayer <= stopDistance)
        {
            zombieAnim?.PlayIdle();
            return;
        }

        Vector2 toPlayer = (player.position - origin);
        if (toPlayer.sqrMagnitude < 0.0001f)
        {
            zombieAnim?.PlayIdle();
            return;
        }

        // 4방향(상하좌우) 중 하나로 스냅
        Vector2 dir;
        if (Mathf.Abs(toPlayer.x) > Mathf.Abs(toPlayer.y))
            dir = new Vector2(Mathf.Sign(toPlayer.x), 0f);
        else
            dir = new Vector2(0f, Mathf.Sign(toPlayer.y));

        Vector3 target = origin + (Vector3)(dir * gridSize);

        // 벽 체크
        if (obstacleMask.value != 0)
        {
            RaycastHit2D hit = Physics2D.Linecast(origin, target, obstacleMask);
            if (hit.collider != null)
            {
                // 앞에 벽 있으면 그냥 이 턴은 제자리
                zombieAnim?.PlayIdle();
                return;
            }
        }

        // 실제 이동 시작
        isMoving = true;
        moveProgress = 0f;
        moveStartPos = origin;
        moveTargetPos = target;

        zombieAnim?.PlayMove();
    }

    // 플레이어가 범위 밖이면 랜덤 4방향 중 한 칸 이동
    private void Wander(Vector3 origin)
    {
        int idx = Random.Range(0, 4);
        Vector2 dir;
        switch (idx)
        {
            case 0: dir = Vector2.down; break;
            case 1: dir = Vector2.left; break;
            case 2: dir = Vector2.up; break;
            default: dir = Vector2.right; break;
        }

        Vector3 target = origin + (Vector3)(dir * gridSize);

        if (obstacleMask.value != 0)
        {
            RaycastHit2D hit = Physics2D.Linecast(origin, target, obstacleMask);
            if (hit.collider != null)
            {
                // 벽이면 이동 취소
                zombieAnim?.PlayIdle();
                return;
            }
        }

        isMoving = true;
        moveProgress = 0f;
        moveStartPos = origin;
        moveTargetPos = target;

        zombieAnim?.PlayMove();
    }

    // --- 플레이어와 겹쳐 있을 때 데미지 주기(Frog 방식과 동일 구조) ---
    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag("Player")) return;
        if (Time.time - lastAttackTime < attackInterval) return;

        lastAttackTime = Time.time;

        // 공격 방향 (좀비 → 플레이어)
        Vector3 dir = (other.transform.position - transform.position).normalized;

        DamageInfo info = new DamageInfo(this, AttackType.Melee, contactDamage, dir);

        IDamageable target = other.gameObject.GetComponent<IDamageable>();
        if (target != null)
        {
            target.ReceiveAttack(info);
        }

        zombieAnim?.PlayAttack();
    }

    // --- IDamageable 구현 (Frog와 동일 패턴) ---
    public void ReceiveAttack(DamageInfo damageInfo)
    {
        if (IsDead) return;

        currentHealth -= damageInfo.damage;

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            zombieAnim?.PlayHit();
        }
    }

    private void Die()
    {
        IsDead = true;

        if (rb != null) rb.velocity = Vector2.zero;
        if (col != null) col.enabled = false;

        zombieAnim?.PlayDeath();

        Destroy(gameObject, 0.7f);
    }
}
