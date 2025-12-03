using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creature : MonoBehaviour, IDamageable, IAttacker
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

    [Header("Attack (Laser)")]
    [SerializeField] private GameObject laserPrefab;   // SingularBeam
    [SerializeField] private float fireInterval = 2f;  // 레이저 쿨타임
    private float fireTimer = 0f;

    [SerializeField] private int projectileDamage = 2;  // IAttacker 데미지
    int IAttacker.Damage => projectileDamage;

    [Header("Detection")]
    [SerializeField] private float detectRange = 8f;  // 플레이어 파악 범위

    private Transform player;

    [Header("Health")]
    [SerializeField] private int maxHealth = 5;
    private int currentHealth;
    public bool IsDead { get; private set; } = false;

    private Rigidbody2D rb;
    private Collider2D col;

    //private CreatureAnimatorController creatureAnim; // 애니 쓰고 싶으면 나중에 활성화

    // --- 유틸: 그리드 스냅 ---
    private Vector3 SnapToGrid(Vector3 pos)
    {
        float x = Mathf.Round(pos.x / gridSize) * gridSize;
        float y = Mathf.Round(pos.y / gridSize) * gridSize;
        return new Vector3(x, y, pos.z);
    }

    private void Start()
    {
        transform.position = SnapToGrid(transform.position);

        currentHealth = maxHealth;

        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        //rb.isKinematic = true;

        //creatureAnim = GetComponent<CreatureAnimatorController>();

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

        HandleMovement();    // 그리드 1칸 이동
        RotateTowardPlayer();
        HandleLaser();
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
                //creatureAnim?.PlayIdle();
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

        //creatureAnim?.PlayMove();
    }

    // -------------------------------
    //       레이저 패턴 
    // -------------------------------
    private void HandleLaser()
    {
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

        var obj = ObjectPoolingManager.Instance.GetPrefab(laserPrefab);
        if (obj == null) return;

        var proj = obj.GetComponent<IProjectile>();
        if (proj == null)
        {
            Debug.LogWarning("Creature: IProjectile 구현이 없습니다.");
            return;
        }

        // 레이저 방향은 creature의 오른쪽 방향
        Vector3 dir = transform.right;

        proj.Fire(transform.position, dir, gameObject.tag);

        //creatureAnim?.PlayAttack();
    }

    // -------------------------------
    //       플레이어 바라보기
    // -------------------------------
    private void RotateTowardPlayer()
    {
        if (player == null) return;

        Vector3 dir = (player.position - transform.position).normalized;

        if (dir.x != 0 || dir.y != 0)
            transform.right = dir;   // 오른쪽 방향을 플레이어 쪽으로

        // 애니메이션 방향이 필요하면 여기서 creatureAnim 사용
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
            //creatureAnim?.PlayHit();
        }
    }

    private void Die()
    {
        IsDead = true;

        if (rb) rb.velocity = Vector2.zero;
        if (col) col.enabled = false;

        //creatureAnim?.PlayDeath();

        Destroy(gameObject, 0.7f);
    }
}
