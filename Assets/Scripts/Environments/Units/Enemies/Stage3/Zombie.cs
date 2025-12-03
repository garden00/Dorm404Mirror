using System.Collections;
using UnityEngine;

public class Zombie : MonoBehaviour, IDamageable, IAttacker
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float moveInterval = 1.5f;
    [SerializeField] private float attackRange = 0.5f;
    private float moveTimer = 0f;

    [Header("Attack")]
    [SerializeField] private int damage = 1;
    int IAttacker.Damage => damage;

    [Header("Health")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;
    public bool IsDead { get; private set; } = false;

    private Transform player;
    private ZombieAnimatorController zombieAnim;
    private Collider2D col;

    private void Start()
    {
        currentHealth = maxHealth;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        zombieAnim = GetComponent<ZombieAnimatorController>();
        col = GetComponent<Collider2D>();
    }

    private void Update()
    {
        if (IsDead || player == null) return;

        moveTimer += Time.deltaTime;
        if (moveTimer >= moveInterval)
        {
            moveTimer = 0f;
            TryMove();
        }

        if (Vector2.Distance(transform.position, player.position) <= attackRange)
        {
            DoAttack();
        }
    }

    private void TryMove()
    {
        Vector2 dir = (player.position - transform.position).normalized;
        transform.position += (Vector3)(dir * moveSpeed * Time.deltaTime);
        zombieAnim.FaceDirection(dir.x);
        zombieAnim.PlayMove();
    }

    private void DoAttack()
    {
        zombieAnim.PlayAttack();
        player.GetComponent<IDamageable>()?.ReceiveAttack(new DamageInfo(this, AttackType.Melee, damage));
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
            zombieAnim.PlayHit();
        }
    }

    private void Die()
    {
        IsDead = true;
        zombieAnim.PlayDeath();
        col.enabled = false;
        Destroy(gameObject, 1f);
    }
}
