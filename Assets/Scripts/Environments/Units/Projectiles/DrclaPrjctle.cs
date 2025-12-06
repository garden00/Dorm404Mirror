using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrclaPrjctle : MonoBehaviour, IProjectile
{
    [Header("Settings")]
    [SerializeField] private float speed = 6f;
    [SerializeField] private int damage = 20;
    [SerializeField] private float lifeTime = 8f;

    private Vector3 moveDir;
    private string ownerTag;
    private float timer;

    public int Damage => damage;
    public Vector3 MoveDirection => moveDir;

    public void Fire(Vector3 _position, Vector3 _direction, string _ownerTag)
    {
        transform.position = _position;
        moveDir = _direction.normalized;
        ownerTag = _ownerTag;
        timer = 0f;

        // 크기가 크므로 회전이 중요할 수 있음
        float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        gameObject.SetActive(true);
    }

    public void Reflect(Vector3 _position, Vector3 _direction, string _ownerTag)
    {
        // [중요] 반사 불가능 로직
        // 플레이어가 반사를 시도해도 방향이나 주인이 바뀌지 않음.
        Debug.Log("이 투사체는 너무 거대해서 반사할 수 없습니다!");

        // (옵션) 팅! 하는 금속음이나 'Block' 텍스트 띄우기
    }

    private void Update()
    {
        transform.position += moveDir * speed * Time.deltaTime;

        timer += Time.deltaTime;
        if (timer >= lifeTime)
        {
            ObjectPoolingManager.Instance.Return(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(ownerTag)) return;
        if (collision.GetComponent<IProjectile>() != null) return;

        if (collision.TryGetComponent<IDamageable>(out IDamageable target))
        {
            DamageInfo info = new DamageInfo(this, AttackType.Projectile, damage, moveDir);
            target.ReceiveAttack(info);
            // 광범위 투사체는 관통할 것인가? 
            // 관통하지 않는다면 여기서 반환:
            // ObjectPoolingManager.Instance.Return(gameObject);

            // 관통한다면 그냥 통과 (데미지는 줌)
        }
        // 벽에는 사라짐
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            ObjectPoolingManager.Instance.Return(gameObject);
        }
    }
}
