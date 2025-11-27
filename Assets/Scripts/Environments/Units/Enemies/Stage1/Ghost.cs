using UnityEngine;

public class Ghost : MonoBehaviour, IDamageable
{
    [Header("Attack")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private EightDirection throwDirection;   // 현재 바라보는 방향
    [SerializeField] private float throwCycleTime = 2f;
    public float ThrowCycleTime => throwCycleTime;

    private float attackTimer = 0f;

    [Header("Health")]
    [SerializeField] private int maxHealth = 1;   
    private int currentHealth;
    public bool IsDead { get; private set; } = false;

    [Header("Animation")]
    private GhostAnimatorController anim;
    [SerializeField] private GhostDirection initialDirection;

    [Header("Player Detect")]
    [SerializeField] private Transform player;        // 비워두면 태그로 자동 찾음
    [SerializeField] private float sightRange = 8f;   // 시야 거리
    [SerializeField] private float sightAngle = 30f;  // 정면 각도(±)
    [SerializeField] private LayerMask obstacleMask;  // 벽/문 타일맵 레이어

    [Header("Random Move")]
    [SerializeField] private float moveSpeed = 2f;          // 이동 속도
    [SerializeField] private float moveDuration = 0.3f;     // 한 번 움직이는 시간
    [SerializeField] private Vector2 moveIntervalRange = new Vector2(1f, 3f); // 다음 이동까지 대기시간 랜덤

    private float moveTimer = 0f;
    private float moveTimeLeft = 0f;
    private float currentMoveWait = 1f;
    private Vector3 currentMoveDir = Vector3.zero;

    // 투사체가 살아있는 동안은 이동/공격 금지
    private bool waitingProjectile = false;
    private GameObject currentProjectile;

    private void Start()
    {
        anim = GetComponent<GhostAnimatorController>();
        currentHealth = maxHealth;

        ApplyDirection(initialDirection);   // 초기 바라보는 방향 설정

        if (anim != null)
            anim.PlayIdle();

        // 플레이어 참조가 비어 있으면 태그로 자동 찾기
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        currentMoveWait = Random.Range(moveIntervalRange.x, moveIntervalRange.y);
    }

    private void Update()
    {
        if (IsDead) return;

        // 1) 투사체가 아직 살아있으면 (반사되든 말든) 가만히 서 있기
        if (waitingProjectile)
        {
            // 풀로 Return 되어 비활성화되면 대기 종료
            if (currentProjectile == null || !currentProjectile.activeInHierarchy)
            {
                waitingProjectile = false;
                currentProjectile = null;
            }
            else
            {
                return; // 계속 대기, 이동/공격 모두 금지
            }
        }

        bool canSeePlayer = CanSeePlayerInFront();

        if (canSeePlayer)
        {
            // 2) 앞에 플레이어가 보이면 공격 사이클만 돌림 (랜덤 이동 X)
            AttackCycle();
        }
        else
        {
            // 3) 플레이어를 못 보면 가끔 랜덤 이동
            attackTimer = 0f;  // 다시 보기 전까지 타이머 리셋
            Wander();
        }
    }

    #region 공격 로직

    private void AttackCycle()
    {
        attackTimer += Time.deltaTime;

        if (attackTimer >= throwCycleTime)
        {
            attackTimer = 0f;

            // 플레이어 쪽 8방향으로 스냅해서 바라보게 변경
            Vector3 dir = throwDirection.VectorNormalized;

            if (player != null)
            {
                Vector3 toPlayer = player.position - transform.position;
                if (toPlayer.sqrMagnitude > 0.01f)
                {
                    EightDirection d8 = EightDirection.FromVector3(toPlayer);
                    throwDirection = d8;
                    dir = d8.VectorNormalized;
                    UpdateAnimatorDirectionByVector(dir);
                }
            }

            ThrowProjectile(dir);

            if (anim != null)
                anim.PlayAttack();
        }
    }

    private void ThrowProjectile(Vector3 fireDir)
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning($"{name} : projectilePrefab가 비어 있습니다.");
            return;
        }

        var projObj = ObjectPoolingManager.Instance.GetPrefab(projectilePrefab);
        if (projObj == null) return;

        var proj = projObj.GetComponent<IProjectile>();
        if (proj == null)
        {
            Debug.LogWarning($"{name} : IProjectile 구현이 없습니다.");
            return;
        }

        proj.Fire(transform.position, fireDir, gameObject.tag);

        currentProjectile = projObj;
        waitingProjectile = true;
    }

    #endregion

    #region 플레이어 감지

    private bool CanSeePlayerInFront()
    {
        if (player == null) return false;

        Vector2 toPlayer = player.position - transform.position;
        float dist = toPlayer.magnitude;
        if (dist > sightRange) return false;

        Vector2 forward = throwDirection.VectorNormalized;
        if (forward.sqrMagnitude < 0.0001f)
            forward = Vector2.up;

        // 정면 각도 체크
        float angle = Vector2.Angle(forward, toPlayer.normalized);
        if (angle > sightAngle) return false;

        // 장애물(벽/문)에 가려졌는지 Raycast로 확인
        if (obstacleMask.value != 0)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, toPlayer.normalized, dist, obstacleMask);
            if (hit.collider != null)
            {
                // 중간에 벽이 있음
                return false;
            }
        }

        return true;
    }

    #endregion

    #region 랜덤 이동

    private void Wander()
    {
        // 현재 이동 중이면 계속 이동
        if (moveTimeLeft > 0f)
        {
            float dt = Time.deltaTime;
            Vector3 nextPos = transform.position + currentMoveDir * moveSpeed * dt;

            // 앞에 벽이 있으면 이동 취소
            if (obstacleMask.value != 0)
            {
                RaycastHit2D hit = Physics2D.Linecast(transform.position, nextPos, obstacleMask);
                if (hit.collider != null)
                {
                    moveTimeLeft = 0f;
                    return;
                }
            }

            transform.position = nextPos;
            moveTimeLeft -= dt;
            return;
        }

        // 이동 중이 아니면, 일정 시간 기다렸다가 랜덤 방향으로 한 번 이동
        moveTimer += Time.deltaTime;
        if (moveTimer >= currentMoveWait)
        {
            moveTimer = 0f;
            currentMoveWait = Random.Range(moveIntervalRange.x, moveIntervalRange.y);

            // 4방향 중 하나 랜덤 선택 (유령 스프라이트가 4방향이니까)
            int idx = Random.Range(0, 4);
            EightDirection d8;
            switch (idx)
            {
                case 0: d8 = EightDirection.Down; break;
                case 1: d8 = EightDirection.Left; break;
                case 2: d8 = EightDirection.Up; break;
                default: d8 = EightDirection.Right; break;
            }

            throwDirection = d8;                      // 바라보는 방향 업데이트
            currentMoveDir = d8.VectorNormalized;     // 실제 이동 방향
            UpdateAnimatorDirectionByVector(currentMoveDir);

            moveTimeLeft = moveDuration;
        }
    }

    // 이동/조준 방향 벡터를 가지고 유령 애니 방향(0:down,1:left,2:up,3:right) 바꾸기
    private void UpdateAnimatorDirectionByVector(Vector2 dir)
    {
        if (anim == null) return;

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            // 좌우
            if (dir.x >= 0) anim.SetDirection(3);    // Right
            else anim.SetDirection(1);              // Left
        }
        else
        {
            // 상하
            if (dir.y >= 0) anim.SetDirection(2);    // Up
            else anim.SetDirection(0);              // Down
        }
    }

    #endregion

    #region IDamageable 구현 및 기존 함수

    public void ReceiveAttack(IProjectile projectile)
    {
        if (IsDead) return;

        currentHealth -= projectile.Damage;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
        else
        {
            if (anim != null)
                anim.PlayHit();
        }
    }

    private void Die()
    {
        if (IsDead) return;
        IsDead = true;

        if (anim != null)
            anim.PlayDeath();

        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }

        Destroy(gameObject, 0.7f);
    }

    public void ApplyDirection(GhostDirection dir)
    {
        if (anim == null) anim = GetComponent<GhostAnimatorController>();

        switch (dir)
        {
            case GhostDirection.Down:
                anim.SetDirection(0);
                throwDirection = EightDirection.Down;
                break;
            case GhostDirection.Left:
                anim.SetDirection(1);
                throwDirection = EightDirection.Left;
                break;
            case GhostDirection.Up:
                anim.SetDirection(2);
                throwDirection = EightDirection.Up;
                break;
            case GhostDirection.Right:
                anim.SetDirection(3);
                throwDirection = EightDirection.Right;
                break;
        }
    }

    #endregion
}

public enum GhostDirection
{
    Down,
    Left,
    Up,
    Right
}
