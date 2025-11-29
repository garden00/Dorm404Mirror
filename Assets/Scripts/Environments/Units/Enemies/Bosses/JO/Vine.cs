using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Vine : MonoBehaviour, IDamageable, IAttacker
{

    [SerializeField]
    int damage = 10;
    int IAttacker.Damage => damage;

    [SerializeField]
    private int maxHealth = 3;
    private int currentHealth;
    public int CurrentHealth
    {
        get => currentHealth;
        set
        {
            currentHealth = value;

            if (currentHealth <= 0)
            {
                currentHealth = 0;
                Die();
            }
            else if (currentHealth > maxHealth) currentHealth = maxHealth;


            //Debug.Log("enemy health : " + currentHealth);
        }
    }

    private void Awake()
    {
        renderers = GetComponentsInChildren<SpriteRenderer>();

        originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            originalColors[i] = renderers[i].color;
    }

    private void Start()
    {
        StartCoroutine(SpawnPreview());

        CurrentHealth = maxHealth;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            DamageInfo info = new DamageInfo(this, AttackType.Melee, damage);

            other.gameObject.GetComponent<IDamageable>().ReceiveAttack(info);
        }
    }

    [SerializeField] float previewTime = 1.0f;
    [SerializeField] float previewAlpha = 0.35f;

    SpriteRenderer[] renderers;
    Color[] originalColors;
    bool active = false;



    IEnumerator SpawnPreview()
    {
        active = false;

        // 모든 콜라이더 비활성화 (부모 + 자식)
        foreach (var col in GetComponentsInChildren<Collider2D>())
            col.enabled = false;

        // 스프라이트 반투명
        for (int i = 0; i < renderers.Length; i++)
        {
            var c = originalColors[i];
            renderers[i].color = new Color(c.r, c.g, c.b, previewAlpha);
        }

        // scale로 땅에서 올라오는 연출
        Vector3 startScale = transform.localScale * 0.6f;
        Vector3 endScale = transform.localScale;
        transform.localScale = startScale;

        float t = 0f;
        while (t < previewTime)
        {
            t += Time.deltaTime;
            float lerp = t / previewTime;

            transform.localScale = Vector3.Lerp(startScale, endScale, lerp);

            yield return null;
        }

        // 정상 상태 복원
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].color = originalColors[i];

        foreach (var col in GetComponentsInChildren<Collider2D>())
            col.enabled = true;

        active = true;
    }




    // [ IDamageable 인터페이스 구현 ]
    void IDamageable.ReceiveAttack(DamageInfo damageInfo)
    {
        if (!active) return;
        CurrentHealth -= 1;
    }

    void Die()
    {
        Destroy(gameObject);
    }
}
