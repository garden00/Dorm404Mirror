using System.Collections;
using UnityEngine;

public class Frog : MonoBehaviour, IDamageable, IAttacker
{
    [Header("Movement - Grid Based")]
    [SerializeField] private float gridSize = 1f;
    [SerializeField] private float moveDuration = 0.35f;
    [SerializeField] private float detectRange = 8f;
    [SerializeField] private LayerMask obstacleMask;

    private bool isMoving = false;
    private Vector3 moveStartPos;
    private Vector3 moveTargetPos;
    private float moveProgress = 0f;

    [Header("Attack")]
    [SerializeField] private Transform player;
    [SerializeField] private int tongueDamage = 1;
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private float attackCooldown = 1f;

    [SerializeField] private int damage;
    int IAttacker.Damage => damage;

    private bool isAttacking = false;
    private float lastAttackTime = -999f;

    [Header("Health")]
    [SerializeField] private int maxHealth = 1;
    [SerializeField] private UnitHealthBar healthBar;

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
    private Collider2D col;
    private FrogAnimatorController frogAnim;

    // --- Grid Snap ---
    private Vector3 SnapToGrid(Vector3 pos)
    {
        float x = Mathf.Round(pos.x / gridSize) * gridSize;
        float y = Mathf.Round(pos.y / gridSize) * gridSize;
        return new Vector3(x, y, pos.z);
    }

    private void Start()
    {
        transform.position = SnapToGrid(transform.position);

        CurrentHealth = maxHealth;
        col = GetComponent<Collider2D>();
        frogAnim = GetComponent<FrogAnimatorController>();

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        
        if (healthBar != null)
            healthBar.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (IsDead) return;
        if (player == null) return;
        if (isMoving) return;

        HandleGridMovement();

        if (CanAttackPlayer())
            DoTongueAttack();
    }

    private void HandleGridMovement()
    {
        float dist = Vector2.Distance(transform.position, player.position);
        if (dist > detectRange) return;

        Vector3 origin = SnapToGrid(transform.position);
        transform.position = origin;

        Vector3 toPlayer = player.position - origin;

        if (toPlayer.sqrMagnitude < 0.001f)
            return;

        Vector2 dir;
        if (Mathf.Abs(toPlayer.x) > Mathf.Abs(toPlayer.y))
            dir = new Vector2(Mathf.Sign(toPlayer.x), 0f);
        else
            dir = new Vector2(0f, Mathf.Sign(toPlayer.y));

        lookDir = dir.x < 0 ? -1 : 1;
        frogAnim.FaceDirection(lookDir);
        frogAnim.PlayMove();

        Vector3 target = origin + (Vector3)(dir * gridSize);

        if (obstacleMask.value != 0)
        {
            RaycastHit2D hit = Physics2D.Linecast(origin, target, obstacleMask);
            if (hit.collider != null)
                return;
        }

        StartCoroutine(GridMoveRoutine(origin, target));
    }

    private IEnumerator GridMoveRoutine(Vector3 start, Vector3 target)
    {
        isMoving = true;
        moveProgress = 0f;

        while (moveProgress < 1f)
        {
            moveProgress += Time.deltaTime / moveDuration;
            transform.position = Vector3.Lerp(start, target, moveProgress);
            yield return null;
        }

        transform.position = target;
        isMoving = false;
    }

    private bool CanAttackPlayer()
    {
        return Vector2.Distance(transform.position, player.position) < attackRange
               && Time.time - lastAttackTime >= attackCooldown;
    }

    private void DoTongueAttack()
    {
        if (isAttacking) return;

        isAttacking = true;
        lastAttackTime = Time.time;

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

    public void ReceiveAttack(DamageInfo damageInfo)
    {
        if (IsDead) return;

        
        if (healthBar != null && !healthBar.gameObject.activeSelf)
            healthBar.gameObject.SetActive(true);

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
        if (col) col.enabled = false;
        frogAnim.PlayDeath();

        if (healthBar != null)
            healthBar.gameObject.SetActive(false);

        Destroy(gameObject, 0.7f);
    }
}
