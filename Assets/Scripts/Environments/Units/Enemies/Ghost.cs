using UnityEngine;

public class Ghost : MonoBehaviour, IDamageable
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
    private GhostAnimatorController anim;

    [SerializeField] private GhostDirection initialDirection;

    private void Start()
    {
        anim = GetComponent<GhostAnimatorController>();
        currentHealth = maxHealth;

        ApplyDirection(initialDirection);

        // 시작할 때 기본 idle 재생
        if (anim != null)
            anim.PlayIdle();
    }

    private void Update()
    {
        if (IsDead) return;

        AttackCycle();
    }

    // -------------------------
    // Attack
    // -------------------------
    private void AttackCycle()
    {
        attackTimer += Time.deltaTime;

        if (attackTimer >= throwCycleTime)
        {
            attackTimer = 0f;

            // 1) 탄 발사
            ThrowProjectile();

            // 2) 공격 애니메이션
            if (anim != null)
                anim.PlayAttack();
        }
    }

    private void ThrowProjectile()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning($"{name} : projectilePrefab이 안 올라가 있음");
            return;
        }

        var projObj = ObjectPoolingManager.Instance.GetPrefab(projectilePrefab);
        var proj = projObj.GetComponent<IProjectile>();

        if (proj == null)
        {
            Debug.LogWarning($"{name} : IProjectile 컴포넌트 없음");
            return;
        }

        proj.Fire(transform.position, throwDirection.VectorNormalized, gameObject.tag);
    }

    // -------------------------
    // Damage / Death
    // -------------------------
    public void ReceiveAttack(IProjectile projectile)
    {
        if (IsDead) return;

        currentHealth -= projectile.Damage;

        if (anim != null)
            anim.PlayHit();

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
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

    // -------------------------
    // Direction
    // -------------------------
    public void ApplyDirection(GhostDirection dir)
    {
        switch (dir)
        {
            case GhostDirection.Down:
                if (anim != null) anim.SetDirection("down");
                throwDirection = EightDirection.Down;
                break;
            case GhostDirection.Left:
                if (anim != null) anim.SetDirection("left");
                throwDirection = EightDirection.Left;
                break;
            case GhostDirection.Up:
                if (anim != null) anim.SetDirection("up");
                throwDirection = EightDirection.Up;
                break;
            case GhostDirection.Right:
                if (anim != null) anim.SetDirection("right");
                throwDirection = EightDirection.Right;
                break;
        }
    }
}

public enum GhostDirection
{
    Down,
    Left,
    Up,
    Right
}
