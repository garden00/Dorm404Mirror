using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StreetLamp : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float lightDuration = 10f; // 켜진 후 유지 시간 (옵션)

    [Header("Visuals & Physics")]
    [SerializeField] private GameObject lightObject;   // 실제 Light2D가 달린 자식 오브젝트
    [SerializeField] private Collider2D lightAreaCollider; // 빛의 범위를 담당하는 Trigger Collider
    [SerializeField] private SpriteRenderer lampRenderer;
    [SerializeField] private Sprite offSprite;
    [SerializeField] private Sprite onSprite;

    private bool isCharged = false;
    private float timer = 0f;

    private void Start()
    {
        TurnOff();
    }

    private void Update()
    {
        // 켜진 상태에서 일정 시간이 지나면 자동으로 꺼지게 할 경우 (옵션)
        if (isCharged)
        {
            timer += Time.deltaTime;
            if (timer > lightDuration)
            {
                TurnOff();
            }
        }
    }

    // [IDamageable] : 크리처의 전기 공격을 감지하여 가로등을 켬
    public void ReceiveAttack(DamageInfo damageInfo)
    {
        // 이미 켜져있으면 무시
        if (isCharged) return;

        // 투사체 공격만 허용 (혹은 특정 태그/속성 체크)
        if (damageInfo.type == AttackType.Projectile)
        {
            // 필요하다면 여기서 damageInfo.source가 'Minion'인지 체크
            TurnOn();
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!isCharged) return;

        if (other.CompareTag("Player"))
        {
            PlayerManager.Instance.Charge(Time.deltaTime);
        }
    }

    private void TurnOn()
    {
        isCharged = true;
        timer = 0f;

        if (lampRenderer && onSprite) lampRenderer.sprite = onSprite;

        // 빛 오브젝트와 충돌 범위 활성화
        if (lightObject) lightObject.SetActive(true);
        if (lightAreaCollider) lightAreaCollider.enabled = true;

        Debug.Log("가로등 활성화! 플레이어 접근 대기 중...");
    }

    private void TurnOff()
    {
        isCharged = false;

        if (lampRenderer && offSprite) lampRenderer.sprite = offSprite;

        // 빛 오브젝트와 충돌 범위 비활성화
        if (lightObject) lightObject.SetActive(false);
        if (lightAreaCollider) lightAreaCollider.enabled = false;
    }
}
