using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public interface IProjectile
{
    public int Damage {  get; }
    public Vector3 MoveDirection { get; }

    public void Fire(Vector3 _position, Vector3 _direction, string _ownerTag);

    public void Reflect(Vector3 _position, Vector3 _direction, string _ownerTag);

}