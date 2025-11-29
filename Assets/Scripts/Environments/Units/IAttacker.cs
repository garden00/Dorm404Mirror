using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 공격 유형 정의
public enum AttackType
{
    Melee,      // 근접
    Projectile  // 투사체
}

// 공격 정보를 담을 구조체 (택배 상자)
public struct DamageInfo
{
    public AttackType type;       // 공격 유형
    public int damage;            // 데미지 양
    public Vector3 direction;     // 공격 방향 (Wobble 효과용)
    public IAttacker source;      // 공격한 놈 원본 (반사를 위해 필요)

    /// <summary>
    /// 공격 정보를 생성합니다.
    /// </summary>
    /// <param name="_source">공격자 (this)</param>
    /// <param name="_type">공격 타입</param>
    /// <param name="_damage">데미지 량</param>
    /// <param name="_direction">공격 방향</param>
    public DamageInfo(IAttacker _source, AttackType _type, int _damage, Vector3 _direction = default)
    {
        type = _type;
        damage = _damage; 
        direction = _direction;
        source = _source;
    }
}

/* 만약 IAttacker을 상속받아 충돌 공격을 하고 싶다면 밑의 코드를 추가하거나 참고할 것

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            DamageInfo info = new DamageInfo(this, AttackType.Melee, damage);

            other.gameObject.GetComponent<IDamageable>().ReceiveAttack(info);
        }
    }

*/

public interface IAttacker
{
    public int Damage { get; }
}

