using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoseDracula : MonoBehaviour, IDamageable, IAttacker
{
    [Header("Health")]
    [SerializeField] private UnitHealthBar healthBar;
    [SerializeField] private int maxHealth = 5;
    private int currentHealth;
    private int CurrentHealth
    {
        get => currentHealth;
        set
        {
            currentHealth = value;

            if (healthBar == null)
                healthBar = GetComponentInChildren<UnitHealthBar>();

            if (healthBar != null)
                healthBar.UpdateHealth(currentHealth, maxHealth);

            if (currentHealth <= 0)
            {
                currentHealth = 0;
                Die();
            }
            else if (currentHealth > maxHealth)
                currentHealth = maxHealth;
        }
    }
    public bool IsDead { get; private set; } = false;

    bool isDetected = false;
    [SerializeField] int damage;
    int IAttacker.Damage => damage;

    private enum BossPattern
    {
        Pattern1,
        Pattern2,
        Pattern3,
        Pattern4
    }

    private BossPattern currentPattern = BossPattern.Pattern1;
    private int patternCount;

    [Header("Pattern Settings")]
    public float patternInterval = 4f;

    private Transform playerTransform;

    void Start()
    {
        if (healthBar == null)
            healthBar = GetComponentInChildren<UnitHealthBar>();
        CurrentHealth = maxHealth;

        patternCount = System.Enum.GetNames(typeof(BossPattern)).Length;

        playerTransform = PlayerManager.Instance.gameObject.transform;
    }

    void Update()
    {
        if (!isDetected)
        {
            float dir = Vector3.Distance(playerTransform.position, transform.position);

            if (dir < 20f)
            {
                isDetected = true;
                StartCoroutine(BossPatternCycle());
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            DamageInfo info = new DamageInfo(this, AttackType.Melee, damage);

            other.gameObject.GetComponent<IDamageable>().ReceiveAttack(info);
        }
    }


    //

    IEnumerator BossPatternCycle()
    {
        yield return new WaitForSeconds(2f);
        Debug.Log("보스 패턴 시작.");
        while (true)
        {
            switch (currentPattern)
            {
                case BossPattern.Pattern1:
                    StartCoroutine(ExecutePattern1());
                    break;
                case BossPattern.Pattern2:
                    StartCoroutine(ExecutePattern2());
                    break;
                case BossPattern.Pattern3:
                    StartCoroutine(ExecutePattern3());
                    break;
                case BossPattern.Pattern4:
                    StartCoroutine(ExecutePattern4());
                    break;
            }

            yield return new WaitForSeconds(patternInterval);

            int nextPatternIndex = ((int)currentPattern + 1) % patternCount;
            currentPattern = (BossPattern)nextPatternIndex;

            Debug.Log($"다음 패턴 준비: {currentPattern}");
        }
    }

    IEnumerator ExecutePattern1()
    {
        yield return null;
    }

    IEnumerator ExecutePattern2()
    {
        yield return null;
    }

    IEnumerator ExecutePattern3()
    {
        yield return null;
    }

    IEnumerator ExecutePattern4()
    {
        yield return null;
    }



    //

    void IDamageable.ReceiveAttack(DamageInfo damageInfo)
    {
        CurrentHealth -= damageInfo.damage;
    }

    private void Die()
    {
        IsDead = true;

        //if (rb) rb.velocity = Vector2.zero;
        //if (col) col.enabled = false;

        //creatureAnim?.PlayDeath();

        Destroy(gameObject, 0.7f);
    }
}
