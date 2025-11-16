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

        if (anim != null)
            anim.PlayIdle();
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
            Debug.LogWarning($"{name} : projectilePrefab�� ����");
            return;
        }

        var projObj = ObjectPoolingManager.Instance.GetPrefab(projectilePrefab);
        var proj = projObj.GetComponent<IProjectile>();

        if (proj == null)
        {
            Debug.LogWarning($"{name} : IProjectile ����");
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

    public void ApplyDirection(GhostDirection dir)
    {
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
}

public enum GhostDirection
{
    Down,
    Left,
    Up,
    Right
}
