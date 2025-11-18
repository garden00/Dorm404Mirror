using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scarecrow : MonoBehaviour, IDamageable
{
    [Header("Attack")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private EightDirection throwDirection;
    [SerializeField] private float throwCycleTime = 2f;
    public float ThrowCycleTime => throwCycleTime;

    private float attackTimer = 0f;

    [Header("Health")]
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;
    public bool IsDead { get; private set; } = false;

    [Header("Animation")]
    private ScarecrowAnimatorController anim;

    [SerializeField] private SCDirection initialDirection;

    [Header("Movement")]
    [SerializeField] private bool enableTileMove = true;
    [SerializeField] private bool horizontal = true;    // true=좌우, false=상하
    [SerializeField] private float tileSize = 1f;
    [SerializeField] private float moveInterval = 0.6f;  // 이동 간격(뚝딱 느낌)
    [SerializeField] private float moveSpeed = 6f;       // 타일 이동 속도

    private int moveDir = 1; // 1 또는 -1
    private bool isMoving = false;


    private void Start()
    {
        anim = GetComponent<ScarecrowAnimatorController>();
        currentHealth = maxHealth;

        ApplyDirection(initialDirection);

        if (anim != null)
            anim.PlayIdle();
        if (enableTileMove)
            StartCoroutine(TileMoveRoutine());
    }

    private void Update()
    {
        if (IsDead) return;

        AttackCycle();
    }


    private void AttackCycle()
    {
        attackTimer += Time.deltaTime;

        if (attackTimer >= throwCycleTime)
        {
            attackTimer = 0f;

            ThrowProjectile();
            anim.PlayAttack();
        }
    }

    private void ThrowProjectile()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning($"{name} : projectilePrefab?? ????");
            return;
        }

        var projObj = ObjectPoolingManager.Instance.GetPrefab(projectilePrefab);
        var proj = projObj.GetComponent<IProjectile>();

        if (proj == null)
        {
            Debug.LogWarning($"{name} : IProjectile ????");
            return;
        }

        proj.Fire(transform.position, throwDirection.VectorNormalized, gameObject.tag);
    }


    public void ReceiveAttack(IProjectile projectile)
    {
        if (IsDead) return;

        currentHealth -= projectile.Damage;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();                      // 치명타면 바로 Death만
        }
        else
        {
            if (anim != null)
                anim.PlayHit();         // 살았을 때만 Hit
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

    public void ApplyDirection(SCDirection dir)
    {
        switch (dir)
        {
            case SCDirection.Down:
                anim.SetDirection(0);
                throwDirection = EightDirection.Down;
                break;
            case SCDirection.Left:
                anim.SetDirection(1);
                throwDirection = EightDirection.Left;
                break;
            case SCDirection.Up:
                anim.SetDirection(2);
                throwDirection = EightDirection.Up;
                break;
            case SCDirection.Right:
                anim.SetDirection(3);
                throwDirection = EightDirection.Right;
                break;
        }
    }

    private IEnumerator MoveToTile(Vector3 target)
    {
        isMoving = true;

        while (Vector3.Distance(transform.position, target) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                target,
                moveSpeed * Time.deltaTime
            );
            yield return null;
        }

        transform.position = target;
        isMoving = false;
    }

    private IEnumerator TileMoveRoutine()
    {
        while (!IsDead)
        {
            if (!enableTileMove)
            {
                yield return null;
                continue;
            }

            if (!isMoving)
            {
                Vector3 dir = horizontal
                    ? new Vector3(moveDir, 0, 0)
                    : new Vector3(0, moveDir, 0);

                if (horizontal)
                {
                    if (moveDir == 1) ApplyDirection(SCDirection.Right); // 방향 바꾸기
                    else ApplyDirection(SCDirection.Left);
                }
                else
                {
                    if (moveDir == 1) ApplyDirection(SCDirection.Up);
                    else ApplyDirection(SCDirection.Down);
                }

                Vector3 targetPos = transform.position + dir * tileSize;

                yield return StartCoroutine(MoveToTile(targetPos)); // 이동
                moveDir *= -1; // 방향 반전
                yield return new WaitForSeconds(moveInterval); // delay
            }

            yield return null;
        }
    }



}

public enum SCDirection
{
    Down,
    Left,
    Up,
    Right
}
