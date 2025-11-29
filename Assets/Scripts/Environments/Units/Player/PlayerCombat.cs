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

    public void ReceiveAttack(DamageInfo info)
    {
        // 카메라 흔들림
        // CameraManager.Instance?.WobbleEffect(info.direction, 0.1f * info.damage);

        // info.type이 Projectile이고, source가 IProjectile로 변환 가능하다면 'proj'에 담음
        if (info.type == AttackType.Projectile && info.source is IProjectile proj)
        {
            // 투사체 반사 가능 여부 체크 (기존 함수 재활용)
            if (IsReflectable(proj))
            {
                if (isCharging)
                {
                    AbsorbProjectile(proj); // 흡수
                }
                else
                {
                    ReflectProjectile(proj); // 반사
                }
                return; // 반사/흡수했으면 데미지 안 입고 종료
            }
        }

        // 3. 그 외 (근접 공격이거나, 반사 불가능한 투사체인 경우)
        TakeDamage(info);
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

    private void TakeDamage(DamageInfo damageInfo)
    {
        if (invincibleTimer > 0) return;

        status.CurrentHealth -= damageInfo.damage;
        invincibleTimer = invincibleTime;

        // 피격 위치 전달 (넉백 계산용)
        status.RaiseOnHitEvent(damageInfo.direction == EightDirection.None ? -status.ViewDirection : damageInfo.direction);
    }


    // 개구리 같은 근접 공격용
    public void ReceiveMeleeDamage(int damage, Vector3 attackDir)
    {
        if (invincibleTimer > 0) return;

        status.CurrentHealth -= damage;
        invincibleTimer = invincibleTime;

        // ★★★ 넉백 방향 정수
        Vector3 kbDir = attackDir;

        if (kbDir.sqrMagnitude < 0.001f)
            kbDir = -status.ViewDirection;

        if (Mathf.Abs(kbDir.x) > Mathf.Abs(kbDir.y))
            kbDir = new Vector3(Mathf.Sign(kbDir.x), 0, 0);   // 좌우 고정
        else
            kbDir = new Vector3(0, Mathf.Sign(kbDir.y), 0);   // 상하 고정

        // 넉백 이벤트 전달
        status.RaiseOnHitEvent(kbDir);
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