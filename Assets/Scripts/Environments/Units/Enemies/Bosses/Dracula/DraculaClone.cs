using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DraculaClone : MonoBehaviour, IAttacker
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float fadeSpeed = 2f;



    private bool isFadingOut = false;

    [SerializeField] private int damage = 10;
    public int Damage => damage;

    // 분신 초기화
    public void Setup(Sprite sprite, bool flipX)
    {
        isFadingOut = false;
    }

    // 사라지기 시작
    public void Vanish()
    {
        isFadingOut = true;
    }

    private void Update()
    {
        if (isFadingOut && spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a -= fadeSpeed * Time.deltaTime;
            spriteRenderer.color = color;

            if (color.a <= 0f)
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            DamageInfo info = new DamageInfo(this, AttackType.Melee, Damage);
            collision.GetComponent<IDamageable>()?.ReceiveAttack(info);
        }
    }
}
