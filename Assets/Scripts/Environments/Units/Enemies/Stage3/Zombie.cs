using System.Collections;
using UnityEngine;

public class Zombie : MonoBehaviour, IDamageable, IAttacker
{
    [Header("Movement - Grid")]
    [SerializeField] private float gridSize = 1f;
    [SerializeField] private float moveDuration = 0.2f;
    [SerializeField] private Vector2 moveIntervalRange = new Vector2(0.4f, 0.8f);
    [SerializeField] private float detectRange = 6f;
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private LayerMask obstacleMask;

    private bool isMoving = false;
    private Vector3 moveStartPos;
    private Vector3 moveTargetPos;
    private float moveProgress = 0f;
    private float moveTimer = 0f;
    private float currentMoveWait = 0.5f;

    [Header("Attack")]
    [SerializeField] private int contactDamage = 1;       // 실제 근접 공격 데미지
    [SerializeField] private float attackInterval = 0.8f;

    // IAttacker.Damage는 contactDamage와 일치시키기
    int IAttacker.Damage => contactDamage;

    private float lastAttackTime = -999f;

    [Header("Health")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private UnitHealthBar healthBar;
    private int currentHealth;
    public bool IsDead { get; private set; } = false;

    [Header("References")]
    private Transform player;
    private Rigidbody2D rb;
    private Collider2D col;
    private ZombieAnimatorController zombieAnim;

    private float lastMoveX = 1f; // 마지막 이동 방향 (좌우 플립용)

    private Vector3 SnapToGrid(Vector3 pos)
    {
        float x = Mathf.Round(pos.x / gridSize) * gridSize;
        float y = Mathf.Round(pos.y / gridSize) * gridSize;
        return new Vector3(x, y, pos.z);
    }

    private void Start()
    {
        // 그리드 위치에 정렬
        transform.position = SnapToGrid(transform.position);

        currentHealth = maxHealth;

        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        zombieAnim = GetComponent<ZombieAnimatorController>();

        // 플레이어 찾기
        if (PlayerManager.Instance != null)
            player = PlayerManager.Instance.transform;
        else
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // 이동 간격 랜덤 초기화
        currentMoveWait = Random.Range(moveIntervalRange.x, moveIntervalRange.y);

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }

        // 체력바 초기화 (원한다면 여기서 SetActive(false) 해도 됨)
        if (healthBar != null)
            healthBar.UpdateHealth(currentHealth, maxHealth);
    }

    private void Update()
    {
        if (IsDead) return;
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        // 공격 범위 안이면 이동하지 않고 공격만 시도
        if (dist <= attackRange)
        {
            TryDirectAttack();
            return;
        }

        // 이동 중일 때는 보간 계속
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

        // 대기 타이머
        moveTimer += Time.deltaTime;
        if (moveTimer < currentMoveWait)
            return;

        moveTimer = 0f;
        currentMoveWait = Random.Range(moveIntervalRange.x, moveIntervalRange.y);

        Vector3 origin = SnapToGrid(transform.position);
        transform.position = origin;

        if (dist <= detectRange)
            TryStepTowardPlayer(origin);
        else
            Wander(origin);
    }

    private void TryDirectAttack()
    {
        if (Time.time - lastAttackTime < attackInterval) return;
        lastAttackTime = Time.time;

        zombieAnim?.PlayAttack();
        StartCoroutine(DelayedAttack(0.2f));
    }

    private IEnumerator DelayedAttack(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (player == null || IsDead) yield break;

        Vector3 dir = (player.position - transform.position).normalized;
        DamageInfo info = new DamageInfo(this, AttackType.Melee, contactDamage, dir);

        IDamageable target = player.GetComponent<IDamageable>();
        if (target != null) target.ReceiveAttack(info);
    }

    private void TryStepTowardPlayer(Vector3 origin)
    {
        Vector2 toPlayer = (player.position - origin);
        if (toPlayer.sqrMagnitude < 0.0001f)
        {
            zombieAnim?.PlayIdle();
            return;
        }

        // x축 / y축 중 더 큰 방향으로 한 칸 이동 (맨해튼 추적)
        Vector2 dir;
        if (Mathf.Abs(toPlayer.x) > Mathf.Abs(toPlayer.y))
            dir = new Vector2(Mathf.Sign(toPlayer.x), 0f);
        else
            dir = new Vector2(0f, Mathf.Sign(toPlayer.y));

        if (dir.x != 0) lastMoveX = dir.x;

        Vector3 target = origin + (Vector3)(dir * gridSize);

        RaycastHit2D hit = Physics2D.Linecast(origin, target, obstacleMask);
        if (hit.collider != null)
        {
            // 앞에 장애물이 있으면 제자리
            zombieAnim?.PlayIdle();
            return;
        }

        StartMove(origin, target);
    }

    private void Wander(Vector3 origin)
    {
        int idx = Random.Range(0, 4);
        Vector2 dir = idx switch
        {
            0 => Vector2.down,
            1 => Vector2.left,
            2 => Vector2.up,
            _ => Vector2.right
        };

        if (dir.x != 0) lastMoveX = dir.x;

        Vector3 target = origin + (Vector3)(dir * gridSize);

        RaycastHit2D hit = Physics2D.Linecast(origin, target, obstacleMask);
        if (hit.collider != null)
        {
            zombieAnim?.PlayIdle();
            return;
        }

        StartMove(origin, target);
    }

    private void StartMove(Vector3 origin, Vector3 target)
    {
        isMoving = true;
        moveProgress = 0f;
        moveStartPos = origin;
        moveTargetPos = target;

        zombieAnim?.SetFlipX(lastMoveX < 0);
        zombieAnim?.PlayMove();
    }

    // -------------------------------
    //           IDamageable
    // -------------------------------
    public void ReceiveAttack(DamageInfo info)
    {
        if (IsDead) return;

        currentHealth -= info.damage;

        if (healthBar != null)
            healthBar.UpdateHealth(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
        // 여기서 피격 애니메이션/이펙트 넣고 싶으면
        // else { zombieAnim?.PlayHit(); } 이런 식으로 추가 가능
    }

    private void Die()
    {
        IsDead = true;

        if (rb != null) rb.velocity = Vector2.zero;
        if (col != null) col.enabled = false;

        zombieAnim?.PlayDeath();

        if (healthBar != null)
            healthBar.gameObject.SetActive(false);

        Destroy(gameObject, 0.7f);
    }
}
