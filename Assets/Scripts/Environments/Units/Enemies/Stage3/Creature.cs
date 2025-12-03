using UnityEngine;

public class Creature : MonoBehaviour, IDamageable, IAttacker
{
    [Header("Attack")]
    [SerializeField] private Transform player;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float attackRange = 6f;
    [SerializeField] private float attackInterval = 2f;
    [SerializeField] private int laserDamage = 2;

    [SerializeField] private int damage;
    int IAttacker.Damage => damage;

    private float attackTimer = 0f;
    private bool isAttacking = false;

    [Header("Health")]
    [SerializeField] private int maxHealth = 2;
    private int currentHealth;
    public bool IsDead { get; private set; } = false;

    private CreatureAnimatorController anim;

    private void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<CreatureAnimatorController>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void Update()
    {
        if (IsDead || player == null) return;

        attackTimer += Time.deltaTime;

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist <= attackRange && attackTimer >= attackInterval)
        {
            attackTimer = 0f;
            FireLaser();
        }
    }

    private void FireLaser()
    {
        anim.PlayAttack();

        Vector3 dir = (player.position - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, attackRange);

        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, hit.collider != null ? hit.point : transform.position + dir * attackRange);
        lineRenderer.enabled = true;

        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            player.GetComponent<IDamageable>()?.ReceiveAttack(new DamageInfo(this, AttackType.Magic, laserDamage));
        }

        Invoke(nameof(DisableLaser), 0.1f);
    }

    private void DisableLaser()
    {
        lineRenderer.enabled = false;
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
