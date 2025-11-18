using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class PlayerCombat : MonoBehaviour, IDamageable
{
    [NonSerialized] public PlayerStatus status;

    [SerializeField] private float invincibleTime;
    private float invincibleTimer;

    [SerializeField] private GameObject ChargingAttackProjectilePrefab;

    private ShieldController shield;
    private bool isCharging;

    void Awake()
    {
        shield = GetComponent<ShieldController>();
    }

    void Update()
    {
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

    public void ReceiveAttack(IProjectile projectile)
    {
        StartCoroutine(CameraManager.Instance.WobbleEffect(
            projectile.MoveDirection,
            0.1f * projectile.Damage
        ));

        if (IsReflectable(projectile))
        {
            if (isCharging)
            {
                Charging(projectile);
            }
            else
            {
                Reflect(projectile);
            }
        }
        else
        {
            TakeDamage(projectile);
        }
    }

    private bool IsReflectable(IProjectile projectile)
    {
        if (projectile.MoveDirection == Vector3.zero) return false;

        float angle = Vector2.SignedAngle(-projectile.MoveDirection, shield.Direction);
        return angle >= -45f && angle <= 45f;
    }

    private void Reflect(IProjectile projectile)
    {
        Vector3 d = projectile.MoveDirection;
        Vector3 n = shield.Direction.normalized;

        Vector3 R = 2f * Vector3.Dot(d, n) * n - d;

        projectile.Reflect(transform.position, -R, gameObject.tag);

        GetComponent<PlayerAnimationController>().PlayReflectAnimation();
    }

    private void TakeDamage(IProjectile projectile)
    {
        if (invincibleTimer > 0) return;

        status.CurrentHealth -= projectile.Damage;
        invincibleTimer = invincibleTime;

        GetComponent<PlayerAnimationController>().PlayHitAnimation();

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

                ObjectPoolingManager.Instance
                    .GetPrefab(ChargingAttackProjectilePrefab)
                    .GetComponent<IProjectile>()
                    .Reflect(transform.position, shield.Direction, gameObject.tag);
            }
        }
    }
}
