using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossWitch : MonoBehaviour, IDamageable, IAttacker, IEffectable
{
    [Header("Settings")]
    [SerializeField] private int maxHealth = 200;
    [SerializeField] private int contactDamage = 10;
    [SerializeField] private float patternInterval = 2f;

    [Header("Prefabs")]
    [SerializeField] private GameObject projectilePrefab; // WitchProjectile이 붙어있어야 함
    [SerializeField] private GameObject clonePrefab;      // WitchClone이 붙어있어야 함

    [Header("Clone Settings")]
    [SerializeField] private Transform[] cloneSpawnPoints;

    private WitchBossAnimatorController animCtrl;

    // 상태 변수
    private int currentHealth;
    public int CurrentHealth
    {
        get => currentHealth;
        set
        {
            currentHealth = value;
        }
    }
    private Transform playerTransform;
    private List<WitchClone> activeClones = new List<WitchClone>();
    private bool isDead = false;

    private float animationSpeedMultiplier = 1f;

    // IAttacker 구현
    public int Damage => contactDamage;

    private void Start()
    {
        currentHealth = maxHealth;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        StartCoroutine(PatternCycle());

        animCtrl = GetComponent<WitchBossAnimatorController>();
    }

    private void Update()
    {
        if (playerTransform == null) return;
        Vector2 dir = playerTransform.position - transform.position;

        // 애니메이션 방향 반전
        animCtrl.SetDirection(dir);
    }


    // IDamageable 구현
    public void ReceiveAttack(DamageInfo damageInfo)
    {
        if (isDead) return;

        animCtrl.PlayHit();

        currentHealth -= damageInfo.damage;
        Debug.Log($"마녀 체력: {currentHealth}");

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


        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        animCtrl.PlayDeath();

        StopAllCoroutines();

        // 살아있는 분신들 모두 제거
        foreach (var clone in activeClones)
        {
            if (clone != null) Destroy(clone.gameObject);
        }
        activeClones.Clear();

        EnemyManager.Instance.NotifyBossDead();


        Debug.Log("마녀 처치됨");
        Destroy(gameObject); // 혹은 사망 애니메이션 재생
    }

    private IEnumerator PatternCycle()
    {
        yield return new WaitForSeconds(1f);

        while (!isDead)
        {
            yield return StartCoroutine(Pattern_ThrowPotion());
            yield return new WaitForSeconds(patternInterval);

            yield return StartCoroutine(Pattern_SummonClones());
            yield return new WaitForSeconds(patternInterval);

            yield return StartCoroutine(Pattern_ThrowPotion());
            yield return new WaitForSeconds(patternInterval);

            yield return StartCoroutine(Pattern_ClonesCharge());

            yield return new WaitForSeconds(patternInterval);
        }
    }

    private IEnumerator Pattern_SummonClones()
    {
        Debug.Log("패턴 2: 분신 소환");
        animCtrl.PlaySummon();

        // 기존 리스트 정리 (혹시 남아있을 null 제거)
        activeClones.RemoveAll(c => c == null);

        // 분신 2마리 소환
        for (int i = 0; i < 2; i++)
        {
            Vector3 spawnPos = (cloneSpawnPoints.Length > i) ? cloneSpawnPoints[i].position : transform.position + Random.insideUnitSphere * 2f;

            GameObject cloneObj = Instantiate(clonePrefab, spawnPos, Quaternion.identity);
            WitchClone cloneScript = cloneObj.GetComponent<WitchClone>();

            if (cloneScript != null)
            {
                activeClones.Add(cloneScript);
                // 분신이 죽으면 리스트에서 제거하도록 이벤트 연결
                cloneScript.OnDeath += (c) => { if (activeClones.Contains(c)) activeClones.Remove(c); };
            }
        }
        yield return null;
    }

    private IEnumerator Pattern_ThrowPotion()
    {
        animCtrl.PlayAttack();

        ProjectileEffect[] potion = { ProjectileEffect.Slow, ProjectileEffect.Poison, ProjectileEffect.Normal, ProjectileEffect.SpeedUp, ProjectileEffect.Heal };
        ProjectileEffect selectedPotion;
        for (int i = 0; i < 5; i++)
        {
            selectedPotion = potion[Random.Range(0, potion.Length)];

            FireProjectile(selectedPotion);

            yield return new WaitForSeconds(0.5f);
        }

        yield return null;
    }

    private IEnumerator Pattern_ClonesCharge()
    {
        // 리스트 정리
        activeClones.RemoveAll(c => c == null);

        if (activeClones.Count > 0)
        {
            // 살아있는 모든 분신에게 돌진 명령
            foreach (var clone in activeClones)
            {
                if (clone != null && playerTransform != null)
                {
                    clone.StartCharge(playerTransform);
                }
            }

            activeClones.Clear();
        }
        else
        {
            Debug.Log("모든 분신 처치");
        }

        yield return null;
    }

    // 투사체 발사 헬퍼 함수
    private void FireProjectile(ProjectileEffect effect)
    {
        if (playerTransform == null) return;

        GameObject projObj = ObjectPoolingManager.Instance.GetPrefab(projectilePrefab);

        WitchProjectile projScript = projObj.GetComponent<WitchProjectile>();
        if (projScript != null)
        {
            projScript.SetupEffect(effect);
            projScript.Fire(transform.position, (playerTransform.position - transform.position).normalized, gameObject.tag);
        }
    }

    // 몸통박치기 데미지
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            DamageInfo info = new DamageInfo(this, AttackType.Melee, contactDamage);
            other.GetComponent<IDamageable>()?.ReceiveAttack(info);
        }
    }

    // --- IEffectable 구현 ---

    public void ApplyHeal(int amount)
    {
        CurrentHealth += amount;
        Debug.Log($"마녀 회복함, 현재 체력: {CurrentHealth}");
        // 회복 이펙트 재생
    }

    public void ApplySpeedChange(float multiplier, float duration)
    {
        StartCoroutine(BossSpeedRoutine(multiplier, duration));
    }

    private IEnumerator BossSpeedRoutine(float multiplier, float duration)
    {
        float originalInterval = patternInterval;
        patternInterval = originalInterval / multiplier;

        // bossAnim.speed = multiplier; 

        Debug.Log($"마녀 속도 변경: {multiplier}배 (지속 {duration}초)");

        yield return new WaitForSeconds(duration);

        patternInterval = originalInterval;
        // bossAnim.speed = 1f;
        Debug.Log("마녀 속도 정상화");
    }

    public void ApplyPoison(int damagePerTick, float duration)
    {
        StartCoroutine(BossPoisonRoutine(damagePerTick, duration));
    }

    private IEnumerator BossPoisonRoutine(int dmg, float duration)
    {
        float timer = 0f;
        while (timer < duration && !isDead)
        {
            DamageInfo info = new DamageInfo(this, AttackType.Projectile, dmg);
            ReceiveAttack(info);

            Debug.Log("마녀가 독 데미지를 입습니다.");
            yield return new WaitForSeconds(1f);
            timer += 1f;
        }
    }
}