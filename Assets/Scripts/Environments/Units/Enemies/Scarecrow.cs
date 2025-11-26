using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scarecrow : MonoBehaviour, IDamageable
{
    [Header("투사체 발사 관련")]
    [SerializeField] private GameObject projectlie;
    [SerializeField] private EightDirection throwDirection;
    [SerializeField] private float throwCycleTime = 2f;
    private float throwTimer = 0f;

    public float ThrowCycleTime => throwCycleTime;

    [Header("체력 관련")]
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;
    private bool isDead = false;

    private ScarecrowAnimatorController animatorController;

    public int CurrentHealth
    {
        get => currentHealth;
        set
        {
            currentHealth = Mathf.Clamp(value, 0, maxHealth);
            if (currentHealth <= 0 && !isDead)
            {
                Die();
            }
        }
    }

    void Start()
    {
        CurrentHealth = maxHealth;
        animatorController = GetComponent<ScarecrowAnimatorController>();

        // 시작 시 방향 설정 (원하는 방향으로)
        animatorController?.SetDirection("down"); // 또는 "left", "right", "up"
    }

    void Update()
    {
        if (!isDead)
        {
            UpdateDirectionToPlayer();
            ThrowCycle();
        }
    }
    void UpdateDirectionToPlayer()
    {
        if (PlayerManager.Instance == null) return;

        Vector2 toPlayer = (PlayerManager.Instance.transform.position - transform.position);
        if (toPlayer.sqrMagnitude < 0.001f) return;

        int directionIndex = GetDirectionIndex(toPlayer);
        animatorController?.SetDirection(directionIndex);

        switch (directionIndex)
        {
            case 0: throwDirection = EightDirection.Down; break;
            case 1: throwDirection = EightDirection.Left; break;
            case 2: throwDirection = EightDirection.Up; break;
            case 3: throwDirection = EightDirection.Right; break;
        }
    }
    int GetDirectionIndex(Vector2 dir)
    {
        // 0: down, 1: left, 2: up, 3: right
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            return dir.x > 0 ? 3 : 1;
        else
            return dir.y > 0 ? 2 : 0;
    }

    public void ReceiveAttack(IProjectile projectile)
    {
        if (isDead) return;

        CurrentHealth -= projectile.Damage;
        animatorController?.PlayHit();
    }

    private void ThrowCycle()
    {
        throwTimer += Time.deltaTime;

        if (throwTimer > throwCycleTime)
        {
            throwTimer = 0f;
            ThrowProjectile();
            animatorController?.PlayAttack(); // 공격 애니메이션
        }
    }

    private void ThrowProjectile()
    {
        if (projectlie == null)
        {
            Debug.LogWarning($"{name} : projectlie가 설정되지 않았습니다.");
            return;
        }

        var obj = ObjectPoolingManager.Instance.GetPrefab(projectlie);
        if (obj == null) return;

        obj.transform.position = transform.position;

        var proj = obj.GetComponent<IProjectile>();
        if (proj == null) return;

        proj.Fire(transform.position, throwDirection.VectorNormalized, gameObject.tag);
    }
    private void Die()
    {
        isDead = true;
        animatorController?.PlayDeath();
        StartCoroutine(DeactivateAfterDelay(1f));
    }

    private IEnumerator DeactivateAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }

}