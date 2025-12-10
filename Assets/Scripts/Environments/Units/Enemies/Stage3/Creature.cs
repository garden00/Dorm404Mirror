using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creature : MonoBehaviour, IDamageable, IAttacker
{
    [Header("Movement - Grid Wander")]
    [SerializeField] private float gridSize = 1f;
    [SerializeField] private float moveDuration = 0.25f;
    [SerializeField] private Vector2 moveIntervalRange = new Vector2(1f, 2f);
    [SerializeField] private LayerMask obstacleMask;

    private bool isMoving = false;
    private Vector3 moveStartPos;
    private Vector3 moveTargetPos;
    private float moveProgress = 0f;
    private float moveTimer = 0f;
    private float currentMoveWait = 0.5f;
    private Vector2 lastMoveDir = Vector2.zero;

    [Header("Attack (Laser)")]
    [SerializeField] private GameObject laserPrefab;
    [SerializeField] private float fireInterval = 2f;
    private float fireTimer = 0f;

    [SerializeField] private int projectileDamage = 2;
    int IAttacker.Damage => projectileDamage;

    [Header("Direct Attack (Melee)")]
    [SerializeField] private float directAttackRange = 1f;
    [SerializeField] private float attackInterval = 1f;
    [SerializeField] private int contactDamage = 2;
    private float lastAttackTime = -999f;

    [Header("Detection")]
    [SerializeField] private float detectRange = 8f;
    private Transform player;

    [Header("Health")]
    [SerializeField] private int maxHealth = 5;
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

    public bool IsDead { get; private set; } = false;

    [Header("References")]
    [SerializeField] private CreatureAnimatorController creatureAnim;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private Rigidbody2D rb;
    private Collider2D col;
    private Coroutine hitFlashRoutine;
    private Color originalColor;

    private Vector2 lastAttackDir = Vector2.right;

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
        // 그리드에 스냅
        transform.position = Vector3Int.CeilToInt(transform.position);

        originalColor = spriteRenderer.color;

        // 체력/체력바 동기화
        CurrentHealth = maxHealth;

        // 근접 공격 사거리를 그리드 크기에 맞춤
        directAttackRange = gridSize;

        // 플레이어 찾기
        if (PlayerManager.Instance != null)
            player = PlayerManager.Instance.transform;
        else
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        currentMoveWait = Random.Range(moveIntervalRange.x, moveIntervalRange.y);

        // 처음에는 체력바 숨김
        if (healthBar != null)
        {
            healthBar.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (IsDead || player == null) return;

        HandleMovement();
        HandleDirectAttack();
        HandleLaser();
    }

    private void HandleMovement()
    {
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

        moveTimer += Time.deltaTime;
        if (moveTimer < currentMoveWait)
            return;

        moveTimer = 0f;
        currentMoveWait = Random.Range(moveIntervalRange.x, moveIntervalRange.y);

        Vector3 origin = Vector3Int.CeilToInt(transform.position);
        transform.position = origin;

        int idx = Random.Range(0, 4);
        Vector2 dir = idx switch
        {
            0 => Vector2.down,
            1 => Vector2.left,
            2 => Vector2.up,
            _ => Vector2.right
        };

        Vector3 target = origin + (Vector3)(dir * gridSize);

        if (obstacleMask.value != 0)
        {
            RaycastHit2D hit = Physics2D.Linecast(origin, target, obstacleMask);
            if (hit.collider != null)
                return;
        }

        isMoving = true;
        moveProgress = 0f;
        moveStartPos = origin;
        moveTargetPos = target;

        lastMoveDir = dir;
        creatureAnim?.SetMove(dir);
    }

    private void HandleDirectAttack()
    {
        if (player == null) return;

        float distToPlayer = Vector2.Distance(transform.position, player.position);
        if (distToPlayer > detectRange || distToPlayer > directAttackRange) return;
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
        target?.ReceiveAttack(info);
    }

    private IEnumerator HitFlash(float duration)
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(duration);
        spriteRenderer.color = originalColor;
        hitFlashRoutine = null;
    }

    private void HandleLaser()
    {
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
        if (sqrDist > detectRange * detectRange) return;

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

        proj.Fire(transform.position, dir, gameObject.tag);

        creatureAnim?.PlayShoot();
    }

    private void UpdateSpriteFlipByDir(Vector2 dir)
    {
        if (spriteRenderer == null) return;

        if (dir.x > 0.01f)
            spriteRenderer.flipX = false;
        else if (dir.x < -0.01f)
            spriteRenderer.flipX = true;
    }

    public void ReceiveAttack(DamageInfo info)
    {
        if (IsDead) return;

        if (healthBar != null && !healthBar.gameObject.activeSelf)
        {
            healthBar.gameObject.SetActive(true);
        }

        CurrentHealth -= info.damage;

        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            Die();
        }
        else
        {
            creatureAnim?.PlayHit();

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

        if (healthBar != null)
            healthBar.gameObject.SetActive(false);

        Destroy(gameObject, 0.7f);
    }
}
