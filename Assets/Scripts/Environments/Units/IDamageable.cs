using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    /// <summary>
    /// �� ��ü�� �߻�ü�κ��� ������ �޽��ϴ�.
    /// </summary>
    /// <param name="_projectile">������ ���� �߻�ü</param>
    public void ReceiveAttack(IProjectile _projectile);

}