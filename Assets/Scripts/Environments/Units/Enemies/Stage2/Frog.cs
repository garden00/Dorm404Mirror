using UnityEngine;

public class Frog : MonoBehaviour, IDamageable
{
    [Header("Movement")]
    [SerializeField] private float tileSize = 1f;     // 1칸 크기
    [SerializeField] private float moveSpeed = 6f;    // 순간이동처럼 빠르게
    [SerializeField] private float moveInterval = 2f; // 몇 초마다 이동 시도
    [SerializeField] private int moveTiles = 3;       // 3칸 이동
    [SerializeField] private LayerMask obstacleMask;  // 벽 체크용

    private float moveTimer = 0f;
    private int moveDir = 1; // 1 오른쪽 -1 왼쪽
    private int lookDir = 1;


    [Header("Attack")]
    [SerializeField] private Transform player;
    [SerializeField] private int tongueDamage = 1;
    [SerializeField] private float detectRangeX = 2f; // 좌우 2칸
    [SerializeField] private float yAttackThreshold = 0.3f;   // ★★★ 추가


    private bool isAttacking = false;

    [Header("Health")]
    [SerializeField] private int maxHealth = 1;
    private int currentHealth;
    public bool IsDead { get; private set; } = false;


    private Animator anim;
    private Collider2D col;

    private FrogAnimatorController frogAnim;

    private void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
        col = GetComponent<Collider2D>();

        frogAnim = GetComponent<FrogAnimatorController>();

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    private void Update()
    {
        if (IsDead) return;
        UpdateLookDirection(); // 방향 업데이트

        if (player != null && CanAttackPlayer())
        {
            DoTongueAttack();
            return;
        }

        moveTimer += Time.deltaTime;
        if (moveTimer >= moveInterval)
        {
            moveTimer = 0f;
            TryMove();
        }
    }


    // 이동 ★★★
    private void TryMove()
    {
        Vector3 start = transform.position;
        Vector3 target = start + new Vector3(moveDir * moveTiles * tileSize, 0, 0);

        // 1) 벽체크 (한 칸씩 체크)
        for (int i = 1; i <= moveTiles; i++)
        {
            Vector3 stepPos = start + new Vector3(moveDir * tileSize * i, 0, 0);

            // 벽이 있으면 이동 불가 → 방향 전환만 하고 종료
            if (Physics2D.OverlapCircle(stepPos, 0.1f, obstacleMask))
            {
                lookDir *= -1;
                moveDir = lookDir;
                frogAnim.FaceDirection(lookDir);
                return;
            }
        }

        frogAnim.PlayMove();

        StopAllCoroutines();
        StartCoroutine(MoveRoutine(target));
    }


    private System.Collections.IEnumerator MoveRoutine(Vector3 target)
    {
        Vector3 start = transform.position;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;
            transform.position = Vector3.Lerp(start, target, t);
            yield return null;
        }

        transform.position = target;
    }


    // 공격: 개구리 x축 +- 2칸
    private bool CanAttackPlayer()
    {
        float dx = Mathf.Abs(player.position.x - transform.position.x);
        if (dx > detectRangeX) return false;

        // ★★★ Y축 
        float dy = Mathf.Abs(player.position.y - transform.position.y);
        if (dy > yAttackThreshold) return false;

        return true;
    }


    private void DoTongueAttack()
    {
        if (isAttacking) return;

        isAttacking = true;

        frogAnim.FaceDirection(lookDir);
        frogAnim.PlayAttack();

        // 실제 데미지는 애니메이션 이벤트 또는 지연 호출로 처리
        Invoke(nameof(ApplyTongueDamage), 0.15f);
        Invoke(nameof(ResetAttack), 0.5f);
    }

    private void ApplyTongueDamage()
    {
        if (player != null)
        {
            var combat = player.GetComponent<PlayerCombat>();
            if (combat != null)
            {
                Vector3 dir = (player.position - transform.position).normalized;
                combat.ReceiveMeleeDamage(tongueDamage, dir);
            }
        }
    }

    // ★★★ 플레이어 기준으로 방향을 갱신하는 함수
    private void UpdateLookDirection()
    {
        if (player == null) return;

        lookDir = (player.position.x < transform.position.x) ? -1 : 1;

        // flip 반영
        frogAnim.FaceDirection(lookDir);

        // 이동 방향도 동일하게 설정
        moveDir = lookDir;
    }



    private void ResetAttack()
    {
        isAttacking = false;
    }

    
    // 데미지: 유령 투사체 맞았을 때만
    public void ReceiveAttack(IProjectile projectile)
    {
        if (IsDead) return;

        currentHealth -= projectile.Damage;
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            frogAnim.PlayHit();
        }
    }

    private void Die()
    {
        IsDead = true;
        frogAnim.PlayDeath();

        if (col) col.enabled = false;

        Destroy(gameObject, 0.7f);
    }
}
