using System.Collections;
using UnityEngine;

public class Frog : MonoBehaviour, IDamageable, IAttacker
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 0.7f; // 느린 추적
    [SerializeField] private LayerMask obstacleMask;

    [Header("Attack")]
    [SerializeField] private Transform player;
    [SerializeField] private int tongueDamage = 1;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 1f;

    [SerializeField]
    private int damage;
    int IAttacker.Damage => damage;

    private bool isAttacking = false;

    [Header("Health")]
    [SerializeField] private int maxHealth = 1;
    [SerializeField] private UnitHealthBar healthBar;  // ← 추가됨

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

    private int lookDir = 1;
    private Animator anim;
    private Collider2D col;

    private FrogAnimatorController frogAnim;

    private void Start()
    {
        CurrentHealth = maxHealth;

        anim = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
        frogAnim = GetComponent<FrogAnimatorController>();

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    private void Update()
    {
        if (IsDead) return;

        UpdateLookDirection();

        if (player != null && CanAttackPlayer())
        {
            DoTongueAttack();
            return;
        }

        FollowPlayerSlow();
    }

    private void FollowPlayerSlow()
    {
        if (player == null) return;

        Vector3 dir = (player.position - transform.position).normalized;

        if (IsObstacleAhead(dir)) return;

        transform.position += dir * moveSpeed * Time.deltaTime;

        if (dir.x != 0)
        {
            lookDir = dir.x < 0 ? -1 : 1;
            frogAnim.FaceDirection(lookDir);
        }

        frogAnim.PlayMove();
    }

    private bool IsObstacleAhead(Vector3 dir)
    {
        return Physics2D.Raycast(transform.position, dir, 0.4f, obstacleMask);
    }

    private bool CanAttackPlayer()
    {
        float dist = Vector3.Distance(player.position, transform.position);
        return dist < attackRange;
    }

    private void DoTongueAttack()
    {
        if (isAttacking) return;

        isAttacking = true;
        frogAnim.FaceDirection(lookDir);
        frogAnim.PlayAttack();

        Invoke(nameof(ApplyTongueDamage), 0.15f);
        Invoke(nameof(ResetAttack), attackCooldown);
    }

    private void ApplyTongueDamage()
    {
        if (player != null)
        {
            var combat = player.GetComponent<PlayerCombat>();
            if (combat != null)
            {
                Vector3 dir = (player.position - transform.position).normalized;
                combat.ReceiveMeleeDamage(tongueDamage, dir);
            }
        }
    }

    private void ResetAttack()
    {
        isAttacking = false;
    }

    private void UpdateLookDirection()
    {
        if (player == null) return;

        lookDir = (player.position.x < transform.position.x) ? -1 : 1;
        frogAnim.FaceDirection(lookDir);
    }

    public void ReceiveAttack(DamageInfo damageInfo)
    {
        if (IsDead) return;

        CurrentHealth -= damageInfo.damage;

        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            Die();
        }
        else
        {
            frogAnim.PlayHit();
        }
    }

    private void Die()
    {
        IsDead = true;
        frogAnim.PlayDeath();

        if (col) col.enabled = false;

        if (healthBar != null)
            healthBar.gameObject.SetActive(false);

        Destroy(gameObject, 0.7f);
    }
}
