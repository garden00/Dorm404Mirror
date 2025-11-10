using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerCombat : MonoBehaviour, IDamageable
{
    [NonSerialized]
    public PlayerStatus status;

    [SerializeField]
    private float invincibleTime;//무적시간
    private float invincibleTimer;

    [SerializeField]
    private GameObject ChargingAttackProjectilePrefab;

    private Vector3 shildDirection;

    private bool isCharging;


    // view 이므로 나중에 분리될 수 있음
    [SerializeField]
    private GameObject ShildObject;

    private void Update()
    {
        MoveShild();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            isCharging = true;
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            isCharging = false;
            ReleaseAttack();
        }


        if (invincibleTimer > 0)
        {
            invincibleTimer -= Time.deltaTime;
        }
    }

    private void MoveShild()
    {

        float horizontal = (Input.GetKey(KeyCode.A) ? -1 : 0) + (Input.GetKey(KeyCode.D) ? 1 : 0);
        float vertical = (Input.GetKey(KeyCode.S) ? -1 : 0) + (Input.GetKey(KeyCode.W) ? 1 : 0);
        if(horizontal == 0 && vertical == 0) shildDirection = status.viewDirection;
        else shildDirection = EightDirection.FromVector3(horizontal, vertical, 0);

        // view 이므로 나중에 분리될 수 있음
        ShildObject.transform.localPosition = shildDirection/2;
        float angle = Mathf.Atan2(shildDirection.y, shildDirection.x) * Mathf.Rad2Deg;

        ShildObject.transform.localRotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    public void ReceiveAttack(IProjectile projectile)
    {
        StartCoroutine(CameraManager.Instance.WobbleEffect(projectile.MoveDirection, 0.1f * projectile.Damage));

        if (IsReflectable(projectile))
        {
            if (isCharging)
            {
                Charging(projectile);
                return;
            }
            else
            {
                Reflect(projectile);
                return;
            }
        }
        else
        {
            TakeDamage(projectile);
            return;
        }
    }

    private bool IsReflectable(IProjectile projectile)
    {
        if (projectile.MoveDirection == Vector3.zero) return false;

        Vector3 porjectileDirection = projectile.MoveDirection;
        float angle = Vector2.SignedAngle(-projectile.MoveDirection, shildDirection);
        bool isReflectable = angle >= -45f && angle <= 45f;

        return isReflectable;
    }

    private void Reflect(IProjectile projectile)
    {
        Vector3 porjectileDirection = projectile.MoveDirection;
        Vector3 R = 2f * Vector3.Dot(porjectileDirection, shildDirection.normalized) * shildDirection.normalized - porjectileDirection;

        projectile.Reflect(transform.position, -R, gameObject.tag);

    }

    private void TakeDamage(IProjectile projectile)
    {
        if (invincibleTimer > 0)
        {
            return;
        }

        status.CurrentHealth = status.CurrentHealth - projectile.Damage;
        invincibleTimer = invincibleTime;
    }

    private void Charging(IProjectile projectile)
    {
        if (isCharging)
        {
            status.CurrentChargingPower += projectile.Damage;
        }
    }

    private void ReleaseAttack()
    {
        if (!isCharging)
        {
            if (status.ChargedMax)
            {
                status.CurrentChargingPower = 0;

                ObjectPoolingManager.Instance.GetPrefab(ChargingAttackProjectilePrefab)
                    .GetComponent<IProjectile>().
                    Reflect(transform.position, shildDirection, gameObject.tag);
            }
        }

    }
}
