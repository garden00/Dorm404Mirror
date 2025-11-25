using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// 할꺼 : 카메라 관련

public class PlayerCombat : MonoBehaviour, IDamageable
{
    private PlayerStatusData status;
    private ShieldController shield;

    [Header("Settings")]
    [SerializeField] private float invincibleTime = 1.0f;
    [SerializeField] private GameObject chargingAttackProjectilePrefab;

    private float invincibleTimer;
    private bool isCharging;

    public event Action OnReflectEvent; // 애니메이션 연동용

    public void Initialize(PlayerStatusData playerStatus)
    {
        status = playerStatus;
        shield = GetComponent<ShieldController>();
    }

    private void Update()
    {
        HandleCooldowns();
        HandleInput();
    }

    private void HandleCooldowns()
    {
        if (invincibleTimer > 0) invincibleTimer -= Time.deltaTime;
    }

    private void HandleInput()
    {
        if (!status.IsActionable && !isCharging) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            isCharging = true;
            status.SetState(PlayerState.Attacking); // 상태 변경 (선택 사항)
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            isCharging = false;
            TryReleaseChargedAttack();
            if (status.CurrentState == PlayerState.Attacking)
                status.SetState(PlayerState.Idle);
        }
    }

    public void ReceiveAttack(IProjectile projectile)
    {
        //CameraManager.Instance?.WobbleEffect(projectile.MoveDirection, 0.1f * projectile.Damage);

        if (IsReflectable(projectile))
        {
            if (isCharging) AbsorbProjectile(projectile);
            else ReflectProjectile(projectile);
        }
        else
        {
            TakeDamage(projectile);
        }
    }

    private bool IsReflectable(IProjectile projectile)
    {
        if (projectile.MoveDirection == Vector3.zero || shield == null) return false;

        float angle = Vector2.SignedAngle(-projectile.MoveDirection, shield.Direction);
        return Mathf.Abs(angle) <= 45f;
    }

    private void ReflectProjectile(IProjectile projectile)
    {
        Vector3 reflectVec = Vector3.Reflect(projectile.MoveDirection, shield.Direction.normalized);
        projectile.Reflect(transform.position, reflectVec, gameObject.tag);

        OnReflectEvent?.Invoke();
    }

    private void AbsorbProjectile(IProjectile projectile)
    {
        status.CurrentChargingPower += projectile.Damage;
        // 투사체 제거 로직은 보통 projectile.Reflect 혹은 별도 Destroy 호출 필요
        // 여기서는 흡수했으므로 투사체가 사라지는 로직이 필요함 (가정)
        //projectile.Deactivate();
    }

    private void TakeDamage(IProjectile projectile)
    {
        if (invincibleTimer > 0) return;

        status.CurrentHealth -= projectile.Damage;
        invincibleTimer = invincibleTime;

        // 피격 위치 전달 (넉백 계산용)
        status.RaiseOnHitEvent(projectile.MoveDirection == EightDirection.None ? -status.ViewDirection: projectile.MoveDirection);
    }

    private void TryReleaseChargedAttack()
    {
        if (status.IsMaxCharged)
        {
            status.CurrentChargingPower = 0;

            var projectileObj = ObjectPoolingManager.Instance?.GetPrefab(chargingAttackProjectilePrefab);
            if (projectileObj != null)
            {
                var proj = projectileObj.GetComponent<IProjectile>();
                proj?.Reflect(transform.position, shield.Direction, gameObject.tag);
            }
        }
    }
}