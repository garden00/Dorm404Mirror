using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoseDraculaPhase2 : MonoBehaviour, IDamageable, IAttacker
{
    [Header("Phase 2 Settings")]
    [SerializeField] private int maxHealth = 1500;
    [SerializeField] private UnitHealthBar healthBar;
    private int currentHealth;

    [Header("Minion Control")]
    [SerializeField] private GameObject creaturePrefab;
    [SerializeField] private List<GameObject> activeMinions = new List<GameObject>();

    [Header("Attack Patterns")]
    [SerializeField] private GameObject wideProjectile; // 1번 패턴

    private Transform playerTransform;
    private bool isDead = false;

    [SerializeField] private int damage = 30;
    public int Damage => damage;

    private void Start()
    {
        currentHealth = maxHealth;
        if (healthBar) healthBar.UpdateHealth(currentHealth, maxHealth);
        playerTransform = PlayerManager.Instance.transform;

        StartCoroutine(Phase2PatternRoutine());
    }

    private void Update()
    {
        if (isDead) return;
        ManageMinions();
    }

    // --- IDamageable 구현 (무적 기믹) ---
    public void ReceiveAttack(DamageInfo damageInfo)
    {
        // 2페이즈는 일반 투사체/공격에 데미지를 입지 않고 통과시킴
        // 빛 공격(특수 공격)만 허용
        Debug.Log("공격이 드라큘라를 투과했습니다 (무적).");

        // 투과 연출(Ghost effect) 등을 여기에 추가 가능
    }

    // 가로등(StreetLight)에서 호출할 데미지 함수
    public void TakeLightDamage(int amount)
    {
        currentHealth -= amount;
        if (healthBar) healthBar.UpdateHealth(currentHealth, maxHealth);

        // 경직(Stun) 애니메이션 재생
        Debug.Log($"드라큘라가 빛에 의해 {amount} 피해를 입었습니다!");

        if (currentHealth <= 0) Die();
    }

    // --- 하수인 관리 (항상 2마리 유지) ---
    private void ManageMinions()
    {
        // 리스트에서 죽거나 없어진 하수인 제거
        activeMinions.RemoveAll(x => x == null || !x.activeSelf);

        if (activeMinions.Count < 2)
        {
            Vector3 spawnPos = transform.position + (Vector3)Random.insideUnitCircle * 4f;
            GameObject minion = ObjectPoolingManager.Instance.GetPrefab(creaturePrefab);
            minion.transform.position = spawnPos;
            minion.SetActive(true);
            activeMinions.Add(minion);
        }
    }

    // --- 패턴 루틴 ---
    private IEnumerator Phase2PatternRoutine()
    {
        while (!isDead)
        {
            int rand = Random.Range(0, 3);
            // 0: 광범위 투사체, 1: 배후 습격, 2: 랜덤 이동

            switch (rand)
            {
                case 0:
                    Debug.Log("2페이즈 패턴1: 광범위 투사체");
                    FireProjectile(wideProjectile);
                    yield return new WaitForSeconds(1.5f);
                    break;
                case 1:
                    Debug.Log("2페이즈 패턴2: 배후 습격");
                    yield return StartCoroutine(TeleportBackAttack());
                    break;
                case 2:
                    Debug.Log("2페이즈 패턴3: 위치 재조정");
                    yield return StartCoroutine(TeleportRandom());
                    break;
            }
            yield return new WaitForSeconds(2.0f);
        }
    }

    private void FireProjectile(GameObject prefab)
    {
        Vector3 dir = (playerTransform.position - transform.position).normalized;
        var obj = ObjectPoolingManager.Instance.GetPrefab(prefab);
        obj.GetComponent<IProjectile>().Fire(transform.position, dir, gameObject.tag);
    }

    private IEnumerator TeleportBackAttack()
    {
        Vector3 backDir = -playerTransform.right;
        Vector3 destPos = playerTransform.position + backDir * 2.0f;

        // 순간이동
        transform.position = destPos;
        yield return new WaitForSeconds(0.2f);

        // 근접 공격 로직
        if (Vector3.Distance(transform.position, playerTransform.position) < 2.5f)
        {
            playerTransform.GetComponent<IDamageable>()?.ReceiveAttack(new DamageInfo(this, AttackType.Melee, damage));
        }
        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator TeleportRandom()
    {
        Vector3 randomPos = playerTransform.position + (Vector3)Random.insideUnitCircle.normalized * 6f;
        transform.position = randomPos;
        yield return new WaitForSeconds(0.5f);
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("드라큘라 사망");
        Destroy(gameObject); // 또는 엔딩 연출
    }
}
