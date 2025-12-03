using UnityEngine;

public class Minion : MonoBehaviour, IDamageable, IAttacker
{
    [Header("Attack")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float attackInterval = 2f;
    [SerializeField] private float projectileSpeed = 6f;
    [SerializeField] private int projectileDamage = 2;

    [Header("Detection")]
    [SerializeField] private float attackRange = 7f;
    private float attackTimer = 0f;

    [Header("Health")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;
    public bool IsDead { get; private set; } = false;

    [Header("References")]
    [SerializeField] private MinionAnimatorController animatorController;
    [SerializeField] private Transform player;

    [SerializeField] private int damage;
    int IAttacker.Damage => damage;

    private void Start()
    {
        currentHealth = maxHealth;

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (animatorController == null)
            animatorController = GetComponent<MinionAnimatorController>();
    }

    private void Update()
    {
        if (IsDead || player == null) return;

        Vector2 dir = player.position - transform.position;
        animatorController.SetDirection(dir.x < 0 ? -1 : 1);

        attackTimer += Time.deltaTime;
        if (dir.magnitude <= attackRange && attackTimer >= attackInterval)
        {
            attackTimer = 0f;
            FireProjectile(dir.normalized);
        }
    }

    private void FireProjectile(Vector2 direction)
    {
        animatorController.PlayAttack();

        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();
        rb.velocity = direction * projectileSpeed;
    }

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
            animatorController.PlayHit();
        }
    }

    private void Die()
    {
        IsDead = true;
        animatorController.PlayDeath();
        Destroy(gameObject, 1f);
    }
}
