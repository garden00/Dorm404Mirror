using UnityEngine;

public class Bat : MonoBehaviour, IDamageable, IAttacker
{
    [Header("Movement")]
    [SerializeField] private float speed = 3f;
    [SerializeField] private float attackRange = 0.6f;

    [Header("Attack")]
    [SerializeField] private int damage = 1;
    int IAttacker.Damage => damage;

    [Header("Health")]
    [SerializeField] private int maxHealth = 1;
    private int currentHealth;
    public bool IsDead { get; private set; } = false;

    private Transform player;
    private BatAnimatorController anim;
    private Collider2D col;

    private void Start()
    {
        currentHealth = maxHealth;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        anim = GetComponent<BatAnimatorController>();
        col = GetComponent<Collider2D>();
    }

    private void Update()
    {
        if (IsDead || player == null) return;

        Vector3 dir = (player.position - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;

        anim.FaceDirection(dir.x);
        anim.PlayMove();

        if (Vector2.Distance(transform.position, player.position) <= attackRange)
        {
            AttackPlayer();
        }
    }

    private void AttackPlayer()
    {
        anim.PlayAttack();
        player.GetComponent<IDamageable>()?.ReceiveAttack(new DamageInfo(this, AttackType.Melee, damage));
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
        col.enabled = false;
        Destroy(gameObject, 1f);
    }
}
