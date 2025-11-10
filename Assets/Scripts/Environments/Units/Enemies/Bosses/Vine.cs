using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vine : MonoBehaviour, IDamageable, IProjectile
{

    [SerializeField]
    int damage = 10;
    int IProjectile.Damage => damage;

    Vector3 IProjectile.MoveDirection => Vector3.zero;


    [SerializeField]
    private int maxHealth = 3;
    private int currentHealth;
    public int CurrentHealth
    {
        get => currentHealth;
        set
        {
            currentHealth = value;

            if (currentHealth <= 0)
            {
                currentHealth = 0;
                Die();
            }
            else if (currentHealth > maxHealth) currentHealth = maxHealth;


            //Debug.Log("enemy health : " + currentHealth);
        }
    }

    private void Start()
    {
        CurrentHealth = maxHealth;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        GameObject _object = other.gameObject;

        if (_object.CompareTag("Player"))
        {
            var damageable = _object.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.ReceiveAttack(this);
            }
        }

    }




    // [ IDamageable 인터페이스 구현 ]
    public void ReceiveAttack(IProjectile _projectile)
    {
        CurrentHealth -= 1;
    }

    void IProjectile.Fire(Vector3 _position, Vector3 _direction, string _ownerTag)
    {
        throw new System.NotImplementedException();
    }

    void IProjectile.Reflect(Vector3 _position, Vector3 _direction, string _ownerTag)
    {
        throw new System.NotImplementedException();
    }

    void Die()
    {
        Destroy(gameObject);
    }
}
