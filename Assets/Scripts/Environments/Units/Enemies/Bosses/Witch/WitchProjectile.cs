using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ProjectileEffect
{
    // 디버프
    Slow,
    Poison,
    Normal,

    // 버프
    SpeedUp,
    Heal
}

public class WitchProjectile : MonoBehaviour, IProjectile
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private int damage = 10;

    private Vector3 moveDirection;
    private string owner;
    private ProjectileEffect effectType;
    private float lifetime = 5f;
    private float timer = 0;

    public int Damage => damage;
    public Vector3 MoveDirection => moveDirection;

    public void SetupEffect(ProjectileEffect type)
    {
        effectType = type;

        // 효과에 따른 색상 변경
        var sprite = GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            switch(type)
            {
                case ProjectileEffect.Heal:
                    sprite.color = Color.yellow;
                    break;
                case ProjectileEffect.SpeedUp:
                    sprite.color = Color.cyan;
                    break;


                case ProjectileEffect.Slow:
                    sprite.color = Color.green;
                    break;
                case ProjectileEffect.Normal:
                    sprite.color = Color.gray;
                    break;
                case ProjectileEffect.Poison:
                    sprite.color = Color.magenta;
                    break;
            }
        }
    }

    public void Fire(Vector3 _position, Vector3 _direction, string _ownerTag)
    {
        transform.position = _position;
        moveDirection = _direction.normalized;
        owner = _ownerTag;

        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    public void Reflect(Vector3 _position, Vector3 _direction, string _ownerTag)
    {
        GameObject myOriginalPrefab = ObjectPoolingManager.Instance.GetOriginalPrefab(gameObject);

        if (myOriginalPrefab == null)
        {
            Debug.LogError($"[Reflect] {gameObject.name}의 원본 프리팹을 ObjectPoolingManager에서 찾을 수 없습니다.", gameObject);
            return;
        }
        GameObject projObj = ObjectPoolingManager.Instance.GetPrefab(myOriginalPrefab);

        WitchProjectile projScript = projObj.GetComponent<WitchProjectile>();
        projScript.SetupEffect(effectType);
        projScript.Fire(_position, _direction, _ownerTag);
    }

    private void Die()
    {
        ObjectPoolingManager.Instance.Return(gameObject);
    }

    private void Update()
    {
        transform.position += moveDirection * speed * Time.deltaTime;

        if(timer < lifetime)
            timer += Time.deltaTime;
        else
            Die();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(owner)) return;

        bool isBuff = (effectType == ProjectileEffect.SpeedUp || effectType == ProjectileEffect.Heal);


        var damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            DamageInfo info = new DamageInfo(this, AttackType.Projectile, damage, moveDirection);

            if (isBuff)
                info.damage = 0;

            damageable.ReceiveAttack(info);

            Die();
        }
    }
}
