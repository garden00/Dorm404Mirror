using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEffectable
{
    // 이동 속도 버프/디버프 (amount > 1 : 가속, amount < 1 : 감속/슬로우)
    void ApplySpeedChange(float multiplier, float duration);

    // 도트 데미지 (독)
    void ApplyPoison(int damagePerTick, float duration);

    // 체력 회복
    void ApplyHeal(int amount);
}