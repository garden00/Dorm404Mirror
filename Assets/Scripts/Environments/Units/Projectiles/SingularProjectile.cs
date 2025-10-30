using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingularProjectile : MonoBehaviour, IProjectile
{
    [Header("Stats")]
    [SerializeField]
    private float lifeTimeLimit = 10.0f;
    private float currentLifeTime = 0f;

    [SerializeField]
    private int damage;
    public int Damage { get => damage; }

    [SerializeField]
    private float moveSpeed;

    private EightDirection moveDirection;
    public EightDirection MoveDirection { get => moveDirection; }

    private string ownerTag;

    void Start()
    {

    }

    void Update()
    {
        lifeCycle();
        Move();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        GameObject _object = other.gameObject;

        if (_object.CompareTag(ownerTag) is false)
        {
            var damageable = _object.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.ReceiveAttack(this);
                Die();
            }
        }

    }

    public void Fire(Vector3 _position, EightDirection _direction, string _ownerTag)
    {
        ownerTag = _ownerTag;
        moveDirection = _direction;
        gameObject.transform.position = _position + moveDirection;
    }

    public void Reflect(Vector3 _position, EightDirection _direction, string _ownerTag)
    {
        GameObject myOriginalPrefab = ObjectPoolingManager.Instance.GetOriginalPrefab(gameObject);

        if (myOriginalPrefab == null)
        {
            Debug.LogError($"[Reflect] {gameObject.name}�� ���� �������� ObjectPoolingManager���� ã�� �� �����ϴ�.", gameObject);
            return;
        }

        ObjectPoolingManager.Instance.GetPrefab(myOriginalPrefab)
            .GetComponent<IProjectile>().
            Fire(_position, _direction, _ownerTag);
    }

    private void lifeCycle()
    {
        currentLifeTime += Time.deltaTime;
        if (currentLifeTime > lifeTimeLimit)
            Die();
    }

    private void Die()
    {
        ObjectPoolingManager.Instance.Return(gameObject);
    }

    private void Move()
    {
        transform.position += moveDirection.VectorGrid * moveSpeed * Time.deltaTime;
    }
}
