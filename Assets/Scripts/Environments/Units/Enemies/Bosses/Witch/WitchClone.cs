using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class WitchClone : MonoBehaviour, IDamageable, IAttacker, IEffectable
{
    [SerializeField] private int maxHealth = 30;
    [SerializeField] private int attackDamage = 20;
    [SerializeField] private float baseMoveSpeed = 5f; // 기본 속도

    private int currentHealth;
    private float currentSpeedMultiplier = 1f; // 속도 배율
    private bool isCharging = false;
    private Vector3 chargeDir;

    private float ChargTime = 2f; // ChargTime동안 안맞으면 추적 중지
    private float timer = 0;

    private bool hit;
    Transform target;


    public int Damage => attackDamage;

    public System.Action<WitchClone> OnDeath;

    void Start()
    {
        currentHealth = maxHealth;
        hit = false;
    }

    void Update()
    {
        if (isCharging)
        {
            if(!hit)
            {
                if(timer < ChargTime)
                    timer += Time.deltaTime;
                else
                    hit = true;

                chargeDir = (target.position - transform.position).normalized;
            }
            // 배율이 적용된 속도로 이동
            float finalSpeed = baseMoveSpeed * currentSpeedMultiplier;
            transform.position += chargeDir * finalSpeed * Time.deltaTime;
        }
    }

    public void StartCharge(Transform targetPos)
    {
        isCharging = true;
        target = targetPos;
        // 일반 돌진보다 좀 더 빠르게 설정하고 싶다면 baseMoveSpeed를 높게 잡거나 여기서 조정
        Destroy(gameObject, 5f);
    }

    // --- IDamageable 구현 ---
    public void ReceiveAttack(DamageInfo damageInfo)
    {
        TakeDamage(damageInfo);
    }

    private void TakeDamage(DamageInfo damageInfo)
    {
        currentHealth -= damageInfo.damage;

        switch (damageInfo.projectileEffect)
        {
            case ProjectileEffect.Slow:
                ApplySpeedChange(0.5f, 3f); // 3초간 속도 50%
                break;
            case ProjectileEffect.SpeedUp:
                ApplySpeedChange(1.5f, 3f); // 3초간 속도 150%
                break;
            case ProjectileEffect.Poison:
                ApplyPoison(5, 5f); // 5초간 틱당 5데미지
                break;
            case ProjectileEffect.Heal:
                ApplyHeal(30); // 30 회복
                break;
        }

        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        OnDeath?.Invoke(this);
        Destroy(gameObject);
    }

    // --- IEffectable 구현 ---


    public void ApplyHeal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        Debug.Log($"분신 회복: {amount}, 현재 체력: {currentHealth}");
    }

  
    public void ApplySpeedChange(float multiplier, float duration)
    {
        StartCoroutine(SpeedChangeRoutine(multiplier, duration));
    }

    private IEnumerator SpeedChangeRoutine(float multiplier, float duration)
    {
        currentSpeedMultiplier = multiplier;

        var renderer = GetComponent<SpriteRenderer>();
        Color originalColor = renderer.color;
        renderer.color = (multiplier < 1) ? Color.blue : Color.red;

        yield return new WaitForSeconds(duration);

        currentSpeedMultiplier = 1f; // 원상 복구
        renderer.color = originalColor;
    }


    public void ApplyPoison(int damagePerTick, float duration)
    {
        StartCoroutine(PoisonRoutine(damagePerTick, duration));
    }

    private IEnumerator PoisonRoutine(int dmg, float duration)
    {
        float timer = 0f;
        var renderer = GetComponent<SpriteRenderer>();
        renderer.color = Color.green; // 독 효과

        while (timer < duration)
        {
            DamageInfo info = new DamageInfo(this, AttackType.Projectile, dmg);
            ReceiveAttack(info);

            yield return new WaitForSeconds(1f); // 1초마다 데미지
            timer += 1f;
        }

        renderer.color = Color.white; // 색상 복구
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isCharging && other.CompareTag("Player"))
        {
            DamageInfo info = new DamageInfo(this, AttackType.Melee, attackDamage, chargeDir);
            other.GetComponent<IDamageable>()?.ReceiveAttack(info);
            hit = true;
        }
    }
}