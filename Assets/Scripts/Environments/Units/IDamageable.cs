using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    /// <summary>
    /// 이 객체가 근접 공격을 받습니다.
    /// </summary>
    /// <param name="attacker">공격을 가한 객체</param>
    public void ReceiveAttack(DamageInfo damageInfo);

}