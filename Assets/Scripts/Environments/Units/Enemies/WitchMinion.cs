using UnityEngine;

public class WitchMinion : MonoBehaviour, IDamageable, IAttacker
{
    [Header("Attack")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private EightDirection lookDirection;    // 현재 바라보는 방향
    [SerializeField] private float attackCycleTime = 2f;
    public float AttackCycleTime => attackCycleTime;

    private float attackTimer = 0f;

    // 투사체가 살아있는 동안은 또 안 쏘게 하고 싶다면 사용
    private bool waitingProjectile = false;
    private GameObject currentProjectile;

    [Header("Health")]
    [SerializeField] private int maxHealth = 1;
    private int currentHealth;
    public bool IsDead { get; private set; } = false;

    [SerializeField]
    int damage;
    int IAttacker.Damage => damage;

    [Header("Animation")]
    private GhostAnimatorController anim;
    [SerializeField] private GhostDirection initialDirection;

    [Header("Player Detect")]
    [SerializeField] private Transform player;          // 비워두면 태그로 자동 찾음
    [SerializeField] private float sightRange = 8f;     // 시야 거리
    [SerializeField] private float sightAngle = 40f;    // 정면 각도(±)
    [SerializeField] private LayerMask obstacleMask;    // 벽/문 타일맵 레이어

    private void Start()
    {
        currentHealth = maxHealth;

        anim = GetComponent<GhostAnimatorController>();
        if (anim == null)
        {
            anim = gameObject.AddComponent<GhostAnimatorController>();
        }
        ApplyDirection(initialDirection);

        anim.PlayIdle();

        if (anim != null)
            anim.PlayIdle();

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    private void Update()
    {
        if (IsDead) return;

        // 투사체가 아직 살아있으면 대기
        if (waitingProjectile)
        {
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
            AttackCycle();
        }
        else
        {
            attackTimer = 0f;
            anim?.PlayIdle();
        }
    }

    #region 공격 로직

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            DamageInfo info = new DamageInfo(this, AttackType.Melee, damage);

            other.gameObject.GetComponent<IDamageable>().ReceiveAttack(info);
        }
    }
    private void AttackCycle()
    {
        attackTimer += Time.deltaTime;

        if (attackTimer >= attackCycleTime)
        {
            attackTimer = 0f;

            Vector3 dir = lookDirection.VectorNormalized;

            if (player != null)
            {
                Vector3 toPlayer = player.position - transform.position;
                if (toPlayer.sqrMagnitude > 0.01f)
                {
                    EightDirection d8 = EightDirection.FromVector3(toPlayer);
                    lookDirection = d8;
                    dir = d8.VectorNormalized;
                    UpdateAnimatorDirectionByVector(dir);
                }
            }

            ThrowProjectile(dir);

            anim?.PlayAttack();
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

        Vector2 forward = lookDirection.VectorNormalized;
        if (forward.sqrMagnitude < 0.0001f)
            forward = Vector2.up;

        float angle = Vector2.Angle(forward, toPlayer.normalized);
        if (angle > sightAngle) return false;

        if (obstacleMask.value != 0)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, toPlayer.normalized, dist, obstacleMask);
            if (hit.collider != null)
            {
                return false;
            }
        }

        return true;
    }

    #endregion

    #region 애니 방향, 체력/피격, 사망

    private void UpdateAnimatorDirectionByVector(Vector2 dir)
    {
        if (anim == null) return;

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            if (dir.x >= 0) anim.SetDirection(3);   // Right
            else anim.SetDirection(1);              // Left
        }
        else
        {
            if (dir.y >= 0) anim.SetDirection(2);   // Up
            else anim.SetDirection(0);              // Down
        }
    }

    public void ApplyDirection(GhostDirection dir)
    {
        if (anim == null)
            anim = GetComponent<GhostAnimatorController>();
        if (anim == null)
            return;


        switch (dir)
        {
            case GhostDirection.Down:
                anim.SetDirection(0);
                lookDirection = EightDirection.Down;
                break;
            case GhostDirection.Left:
                anim.SetDirection(1);
                lookDirection = EightDirection.Left;
                break;
            case GhostDirection.Up:
                anim.SetDirection(2);
                lookDirection = EightDirection.Up;
                break;
            case GhostDirection.Right:
                anim.SetDirection(3);
                lookDirection = EightDirection.Right;
                break;
        }
    }

    public void ReceiveAttack(DamageInfo damageInfo)
    {
        if (IsDead) return;

        currentHealth -= damageInfo.damage;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
        else
        {
            anim?.PlayHit();
        }
    }

    private void Die()
    {
        if (IsDead) return;
        IsDead = true;

        anim?.PlayDeath();

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

    #endregion
}
