using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    /// <summary>
    /// 이 객체가 발사체로부터 공격을 받습니다.
    /// </summary>
    /// <param name="_projectile">공격을 가한 발사체</param>
    public void ReceiveAttack(IProjectile _projectile);

}