using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public interface IProjectile
{
    public int Damage {  get; }
    public EightDirection MoveDirection { get; }

    public void Fire(Vector3 _position, EightDirection _direction, string _ownerTag);

    public void Reflect(Vector3 _position, EightDirection _direction, string _ownerTag);

}