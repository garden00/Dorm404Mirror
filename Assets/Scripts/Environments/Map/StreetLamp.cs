using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StreetLamp : MonoBehaviour, IDamageable
{
    [Header("설정")]
    [SerializeField] private float lightDuration = 10f; // 켜짐 유지 시간

    [Header("연결할 오브젝트")]
    // 중요: 자식 오브젝트인 'LightGroup'을 여기에 넣으세요.
    [SerializeField] private GameObject lightGroupObject;

    [Header("비주얼")]
    [SerializeField] private SpriteRenderer lampRenderer; // 부모에 있는 렌더러
    [SerializeField] private Sprite offSprite; // 꺼진 이미지
    [SerializeField] private Sprite onSprite;  // 켜진 이미지

    private bool isCharged = false;
    private float timer = 0f;

    private void Start()
    {
        // 게임 시작 시 꺼진 상태로 초기화
        TurnOff();
    }

    private void Update()
    {
        // 켜져 있다면 시간 체크 후 끄기
        if (isCharged)
        {
            timer += Time.deltaTime;
            if (timer > lightDuration)
            {
                TurnOff();
            }
        }
    }

    // [IDamageable 구현] 레이저(전기) 공격을 받으면 호출됨
    public void ReceiveAttack(DamageInfo damageInfo)
    {
        Debug.Log("dd");
        // 이미 켜져있으면 무시
        if (isCharged) return;

        // 전기 속성인지 확인
        if (damageInfo.elect)
        {
            Debug.Log("aa");
            TurnOn();
        }
    }

    private void TurnOn()
    {
        isCharged = true;
        timer = 0f;

        // 이미지 변경
        if (lampRenderer != null && onSprite != null)
            lampRenderer.sprite = onSprite;

        // 자식 오브젝트(빛+충전범위) 통째로 켜기
        if (lightGroupObject != null)
            lightGroupObject.SetActive(true);
    }

    private void TurnOff()
    {
        isCharged = false;

        // 이미지 변경
        if (lampRenderer != null && offSprite != null)
            lampRenderer.sprite = offSprite;

        // 자식 오브젝트(빛+충전범위) 통째로 끄기
        if (lightGroupObject != null)
            lightGroupObject.SetActive(false);
    }
}