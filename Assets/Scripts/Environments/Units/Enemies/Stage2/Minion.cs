using UnityEngine;

public class Minion : MonoBehaviour, IDamageable, IAttacker
{
    [Header("Attack")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float attackInterval = 2f;
    [SerializeField] private float attackRange = 5f;

    private float attackTimer = 0f;

    [Header("Health")]
    [SerializeField] private int maxHealth = 3;
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

    [Header("References")]
    [SerializeField] private MinionAnimatorController animatorController;
    [SerializeField] private Transform player;

    [SerializeField]
    private int damage;
    int IAttacker.Damage => damage;

    // 투사체가 살아있는 동안 공격 중지
    private bool waitingProjectile = false;
    private GameObject currentProjectile;


    private void Start()
    {
        CurrentHealth = maxHealth;

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (animatorController == null)
            animatorController = GetComponent<MinionAnimatorController>();
    }


    private void Update()
    {
        if (IsDead || player == null) return;

        Vector2 dir = player.position - transform.position;

        // 투사체가 아직 존재하면 공격 금지
        if (waitingProjectile)
        {
            if (currentProjectile == null || !currentProjectile.activeInHierarchy)
            {
                waitingProjectile = false;
                currentProjectile = null;
            }
            else return;
        }

        // 바라보는 방향 설정
        animatorController.SetDirection(dir.x < 0 ? -1 : 1);

        attackTimer += Time.deltaTime;

        if (dir.magnitude <= attackRange && attackTimer >= attackInterval)
        {
            attackTimer = 0f;
            ThrowProjectile(dir.normalized);

            animatorController.PlayAttack();
        }
    }


    private void ThrowProjectile(Vector3 fireDir)
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning($"{name} : projectilePrefab이 비어있습니다!");
            return;
        }

        var projObj = ObjectPoolingManager.Instance.GetPrefab(projectilePrefab);
        if (projObj == null) return;

        var proj = projObj.GetComponent<IProjectile>();
        if (proj == null)
        {
            Debug.LogWarning($"{name} : IProjectile 구현 없음.");
            return;
        }

        proj.Fire(transform.position, fireDir, gameObject.tag);

        currentProjectile = projObj;
        waitingProjectile = true;
    }


    public void ReceiveAttack(DamageInfo info)
    {
        if (IsDead) return;

        CurrentHealth -= info.damage;

        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
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

        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Destroy(gameObject, 0.7f);

        if (healthBar != null)
            healthBar.gameObject.SetActive(false);
    }
}
