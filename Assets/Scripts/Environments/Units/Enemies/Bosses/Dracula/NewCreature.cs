using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewCreature : MonoBehaviour, IDamageable, IAttacker
{
    [Header("Movement - Grid Wander")]
    [SerializeField] private float gridSize = 1f;                // 한 칸 크기
    [SerializeField] private float moveDuration = 0.25f;         // 한 칸 이동 시간
    [SerializeField] private Vector2 moveIntervalRange = new Vector2(1f, 2f); // 다음 이동까지 대기시간
    [SerializeField] private LayerMask obstacleMask;             // 벽/문 레이어

    private bool isMoving = false;
    private Vector3 moveStartPos;
    private Vector3 moveTargetPos;
    private float moveProgress = 0f;

    private float moveTimer = 0f;
    private float currentMoveWait = 0.5f;
    private Vector2 lastMoveDir = Vector2.zero;

    [Header("Attack (Laser)")]
    [SerializeField] private GameObject laserPrefab;   // SingularBeam
    [SerializeField] private float fireInterval = 2f;  // 레이저 쿨타임
    private float fireTimer = 0f;

    [SerializeField] private int projectileDamage = 2;  // IAttacker 데미지
    int IAttacker.Damage => projectileDamage;

    [Header("Direct Attack (Melee)")]
    [SerializeField] private float directAttackRange = 1f; // gridSize와 동일하게 사용
    [SerializeField] private float attackInterval = 1f;
    [SerializeField] private int contactDamage = 2;
    private float lastAttackTime = -999f;

    [Header("Detection")]
    [SerializeField] private float detectRange = 8f;  // 플레이어 파악 범위

    private Transform player;

    [Header("Health")]
    [SerializeField] private int maxHealth = 5;
    private int currentHealth;
    public bool IsDead { get; private set; } = false;

    [Header("References")]
    [SerializeField] private CreatureAnimatorController creatureAnim;
    [SerializeField] private SpriteRenderer spriteRenderer; // flipX용

    private Rigidbody2D rb;
    private Collider2D col;

    private Coroutine hitFlashRoutine;
    private Color originalColor;


    // 마지막 공격 방향 (스프라이트 변경 용입니다)
    private Vector2 lastAttackDir = Vector2.right;

    // --- 유틸: 그리드 스냅 ---
    private Vector3 SnapToGrid(Vector3 pos)
    {
        float x = Mathf.Round(pos.x / gridSize) * gridSize;
        float y = Mathf.Round(pos.y / gridSize) * gridSize;
        return new Vector3(x, y, pos.z);
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        if (creatureAnim == null)
            creatureAnim = GetComponent<CreatureAnimatorController>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        transform.position = SnapToGrid(transform.position);

        originalColor = spriteRenderer.color;

        currentHealth = maxHealth;

        // 근접 공격 범위는 기본적으로 gridSize 1칸
        directAttackRange = gridSize;

        // 플레이어 자동 탐색
        if (PlayerManager.Instance != null)
            player = PlayerManager.Instance.transform;
        else
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        currentMoveWait = Random.Range(moveIntervalRange.x, moveIntervalRange.y);
    }

    private void Update()
    {
        if (IsDead) return;
        if (player == null) return;

        HandleMovement();       // 그리드 1칸 이동
        HandleDirectAttack();   // 1칸 범위 근접 공격
        HandleLaser();          // 레이저 공격
    }

    // -------------------------------
    //   그리드 기반 랜덤 이동
    // -------------------------------
    private void HandleMovement()
    {
        // 이동 중이면 Lerp
        if (isMoving)
        {
            moveProgress += Time.deltaTime / moveDuration;
            moveProgress = Mathf.Clamp01(moveProgress);

            transform.position = Vector3.Lerp(moveStartPos, moveTargetPos, moveProgress);

            if (moveProgress >= 1f)
            {
                isMoving = false;
                transform.position = moveTargetPos;

                lastMoveDir = Vector2.zero;
                creatureAnim?.SetMove(Vector2.zero);
            }

            return;
        }

        // 이동 중이 아니면 대기
        moveTimer += Time.deltaTime;
        if (moveTimer < currentMoveWait)
            return;

        moveTimer = 0f;
        currentMoveWait = Random.Range(moveIntervalRange.x, moveIntervalRange.y);

        Vector3 origin = SnapToGrid(transform.position);
        transform.position = origin;

        // 4방향 중 랜덤 1칸
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

        // 벽 체크
        if (obstacleMask.value != 0)
        {
            RaycastHit2D hit = Physics2D.Linecast(origin, target, obstacleMask);
            if (hit.collider != null)
            {
                // 벽이면 이동 취소
                return;
            }
        }

        isMoving = true;
        moveProgress = 0f;
        moveStartPos = origin;
        moveTargetPos = target;

        lastMoveDir = dir;
        creatureAnim?.SetMove(dir);
    }

    // -------------------------------
    //       근접 공격 (1칸 범위)
    // -------------------------------
    private void HandleDirectAttack()
    {
        if (player == null) return;

        // 탐지 범위 밖이면 스킵
        float distToPlayer = Vector2.Distance(transform.position, player.position);
        if (distToPlayer > detectRange) return;

        // 1칸(=gridSize) 거리 이내일 때만 근접 공격
        if (distToPlayer > directAttackRange) return;

        if (Time.time - lastAttackTime < attackInterval) return;
        lastAttackTime = Time.time;

        creatureAnim?.PlayAttack();
        StartCoroutine(DelayedAttack(0.2f));
    }

    private IEnumerator DelayedAttack(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (player == null || IsDead) yield break;

        float distToPlayer = Vector2.Distance(transform.position, player.position);
        if (distToPlayer > directAttackRange + 0.1f) yield break;

        Vector3 dir = (player.position - transform.position).normalized;
        if (dir.sqrMagnitude < 0.0001f)
            dir = lastAttackDir == Vector2.zero ? Vector2.right : (Vector3)lastAttackDir;

        lastAttackDir = dir;
        UpdateSpriteFlipByDir(lastAttackDir);

        DamageInfo info = new DamageInfo(this, AttackType.Melee, contactDamage, dir);

        IDamageable target = player.GetComponent<IDamageable>();
        if (target != null) target.ReceiveAttack(info);
    }

    private IEnumerator HitFlash(float duration)
    {
        spriteRenderer.color = Color.red;

        yield return new WaitForSeconds(duration);

        spriteRenderer.color = originalColor;
        hitFlashRoutine = null;
    }


    // -------------------------------
    //       레이저 패턴 
    // -------------------------------
    private void HandleLaser()
    {
        // 탐지 범위 밖이면 쿨타임만 증가시키고 실제 발사는 안 함
        if (player == null) return;

        fireTimer += Time.deltaTime;
        if (fireTimer >= fireInterval)
        {
            fireTimer = 0f;
            FireLaser();
        }
    }

    private void FireLaser()
    {
        if (laserPrefab == null)
        {
            Debug.LogWarning("Creature: Laser prefab not assigned!");
            return;
        }

        if (player == null) return;

        Vector3 toPlayer = (player.position - transform.position);
        float sqrDist = toPlayer.sqrMagnitude;
        if (sqrDist > detectRange * detectRange)
            return;

        Vector3 dir = toPlayer.normalized;
        if (dir.sqrMagnitude < 0.0001f)
            dir = Vector3.right;

        lastAttackDir = dir;
        UpdateSpriteFlipByDir(lastAttackDir);

        var obj = ObjectPoolingManager.Instance.GetPrefab(laserPrefab);
        if (obj == null) return;

        var proj = obj.GetComponent<IProjectile>();
        if (proj == null)
        {
            Debug.LogWarning("Creature: IProjectile 구현이 없습니다.");
            return;
        }

        // transform 회전은 건드리지 않고, 레이저 방향만 벡터로 넘김
        proj.Fire(transform.position, dir, gameObject.tag);

        creatureAnim?.PlayShoot();
    }

    // -------------------------------
    //    스프라이트 방향 (flipX)
    // -------------------------------
    private void UpdateSpriteFlipByDir(Vector2 dir)
    {
        if (spriteRenderer == null) return;

        if (dir.x > 0.01f)
            spriteRenderer.flipX = false;   // 오른쪽 바라봄
        else if (dir.x < -0.01f)
            spriteRenderer.flipX = true;    // 왼쪽 바라봄
        // x가 거의 0이면 기존 방향 유지
    }

    // -------------------------------
    //         IDamageable 구현
    // -------------------------------
    public void ReceiveAttack(DamageInfo info)
    {
        if (IsDead) return;

        currentHealth -= info.damage;

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            creatureAnim?.PlayHit();

            // 스프라이트 색 변경 
            if (hitFlashRoutine != null)
                StopCoroutine(hitFlashRoutine);
            hitFlashRoutine = StartCoroutine(HitFlash(0.2f));
        }
    }

    private void Die()
    {
        IsDead = true;

        if (rb) rb.velocity = Vector2.zero;
        if (col) col.enabled = false;

        creatureAnim?.PlayDeath();

        Destroy(gameObject, 0.7f);
    }
}
