using System.Collections;
using UnityEngine;

public class Frog : MonoBehaviour, IDamageable, IAttacker
{
    [Header("Movement")]
    [SerializeField] private float gridSize = 1f;
    [SerializeField] private float moveDuration = 0.35f;
    [SerializeField] private float moveInterval = 2f; // 2초 간격
    [SerializeField] private int moveTiles = 3;      // 3칸 이동
    [SerializeField] private LayerMask obstacleMask;

    private bool isMoving = false;
    private float moveTimer = 0f;
    private int moveDir = 1;   // 1 = 오른쪽, -1 = 왼쪽
    private float moveProgress = 0f;

    [Header("Attack")]
    [SerializeField] private int tongueDamage = 1;
    [SerializeField] private Transform player;
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private int damage;

    int IAttacker.Damage => damage;
    private bool isAttacking = false;
    private float lastAttackTime = -999f;

    [Header("Health")]
    [SerializeField] private int maxHealth = 1;
    [SerializeField] private UnitHealthBar healthBar;

    private int currentHealth;
    private int CurrentHealth
    {
        get => currentHealth;
        set
        {
            currentHealth = value;
            if (healthBar != null)
                healthBar.UpdateHealth(currentHealth, maxHealth);
        }
    }

    private bool IsDead = false;
    private Collider2D col;
    private FrogAnimatorController frogAnim;

    private Vector3 SnapToGrid(Vector3 pos)
    {
        float x = Mathf.Round(pos.x / gridSize) * gridSize;
        float y = Mathf.Round(pos.y / gridSize) * gridSize;
        return new Vector3(x, y, pos.z);
    }

    private void Start()
    {
        transform.position = SnapToGrid(transform.position);

        CurrentHealth = maxHealth;
        col = GetComponent<Collider2D>();
        frogAnim = GetComponent<FrogAnimatorController>();

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (healthBar != null)
            healthBar.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (IsDead || isMoving) return;

        moveTimer += Time.deltaTime;
        if (moveTimer >= moveInterval)
        {
            moveTimer = 0f;
            TryMoveFixed();
        }

        if (CanAttackPlayer())
            DoTongueAttack();
    }

    private void TryMoveFixed()
    {
        Vector3 origin = SnapToGrid(transform.position);
        transform.position = origin;

        Vector3 target = origin + new Vector3(moveDir * moveTiles * gridSize, 0, 0);

        // 벽체크
        for (int i = 1; i <= moveTiles; i++)
        {
            Vector3 checkPos = origin + new Vector3(moveDir * gridSize * i, 0);
            if (Physics2D.OverlapCircle(checkPos, 0.1f, obstacleMask))
            {
                moveDir *= -1; // 방향 전환만 하고 이동 취소
                frogAnim.FaceDirection(moveDir);
                return;
            }
        }

        frogAnim.FaceDirection(moveDir);
        frogAnim.PlayMove();

        StopAllCoroutines();
        StartCoroutine(GridMoveRoutine(origin, target));
    }

    private IEnumerator GridMoveRoutine(Vector3 start, Vector3 target)
    {
        isMoving = true;
        moveProgress = 0f;

        while (moveProgress < 1f)
        {
            moveProgress += Time.deltaTime / moveDuration;
            transform.position = Vector3.Lerp(start, target, moveProgress);
            yield return null;
        }

        transform.position = target;
        isMoving = false;
        moveDir *= -1; // 반대방향으로 전환
    }

    private bool CanAttackPlayer()
    {
        return player != null &&
               Vector2.Distance(transform.position, player.position) < attackRange &&
               Time.time - lastAttackTime >= attackCooldown;
    }

    private void DoTongueAttack()
    {
        if (isAttacking) return;

        isAttacking = true;
        lastAttackTime = Time.time;

        frogAnim.FaceDirection(moveDir);
        frogAnim.PlayAttack();

        Invoke(nameof(ApplyTongueDamage), 0.15f);
        Invoke(nameof(ResetAttack), attackCooldown);
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

    private void ResetAttack()
    {
        isAttacking = false;
    }

    public void ReceiveAttack(DamageInfo damageInfo)
    {
        if (IsDead) return;

        if (healthBar != null && !healthBar.gameObject.activeSelf)
            healthBar.gameObject.SetActive(true);

        CurrentHealth -= damageInfo.damage;

        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
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
        if (col) col.enabled = false;
        frogAnim.PlayDeath();

        if (healthBar != null)
            healthBar.gameObject.SetActive(false);

        Destroy(gameObject, 0.7f);
    }
}
