using UnityEngine;

public class Skeleton : MonoBehaviour, IDamageable, IAttacker
{
    [Header("Attack")]
    [SerializeField] private Transform player;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float attackInterval = 2f;
    [SerializeField] private float projectileSpeed = 8f;
    [SerializeField] private int projectileDamage = 1;

    [SerializeField] private int damage;
    int IAttacker.Damage => damage;

    private float attackTimer = 0f;

    [Header("Health")]
    [SerializeField] private int maxHealth = 2;
    private int currentHealth;
    public bool IsDead { get; private set; } = false;

    private SkeletonAnimatorController anim;

    private void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<SkeletonAnimatorController>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void Update()
    {
        if (IsDead || player == null) return;

        attackTimer += Time.deltaTime;
        if (attackTimer >= attackInterval)
        {
            attackTimer = 0f;
            FireProjectile();
        }

        anim.FaceDirection(player.position.x - transform.position.x);
    }

    private void FireProjectile()
    {
        anim.PlayAttack();

        Vector2 dir = (player.position - firePoint.position).normalized;
        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();
        rb.velocity = dir * projectileSpeed;

        
        Physics2D.IgnoreCollision(proj.GetComponent<Collider2D>(), GetComponent<Collider2D>());
    }

    public void ReceiveAttack(DamageInfo info)
    {
        if (IsDead) return;

        currentHealth -= info.damage;
        if (currentHealth <= 0)
            Die();
        else
            anim.PlayHit();
    }

    private void Die()
    {
        IsDead = true;
        anim.PlayDeath();
        Destroy(gameObject, 1f);
    }
}
