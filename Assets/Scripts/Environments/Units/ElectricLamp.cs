using UnityEngine;

public class ElectricLamp : MonoBehaviour
{
    public enum FireDir { Right, Left, Up, Down }   // Inspector에서 보여질 방향

    [Header("Beam Settings")]
    [SerializeField] private GameObject beamPrefab;
    [SerializeField] private float fireInterval = 1f;

    [SerializeField] private FireDir fireDirection = FireDir.Right;   // Inspector에서 방향 설정
    [SerializeField] private Transform firePoint;                     // 있으면 firePoint 우선 사용

    private float fireTimer = 0f;

    private void Update()
    {
        fireTimer += Time.deltaTime;

        if (fireTimer >= fireInterval)
        {
            fireTimer = 0f;
            Fire();
        }
    }

    private void Fire()
    {
        if (beamPrefab == null)
        {
            Debug.LogWarning("[ElectricLamp] Beam Prefab not assigned!");
            return;
        }

        // 발사 위치
        Vector3 origin = firePoint != null ? firePoint.position : transform.position;

        // 방향 결정 
        Vector3 dir;

        if (firePoint != null)
        {
            // FirePoint를 사용 중이라면 FirePoint의 right 방향
            dir = firePoint.right;
        }
        else
        {
            // Inspector에서 선택한 enum 방향 사용
            switch (fireDirection)
            {
                case FireDir.Left: dir = Vector3.left; break;
                case FireDir.Up: dir = Vector3.up; break;
                case FireDir.Down: dir = Vector3.down; break;
                default: dir = Vector3.right; break;
            }
        }

        // 발사체 가져오기
        var obj = ObjectPoolingManager.Instance.GetPrefab(beamPrefab);
        if (obj == null) return;

        var proj = obj.GetComponent<IProjectile>();
        if (proj == null)
        {
            Debug.LogWarning("[ElectricLamp] This prefab has no IProjectile component!");
            return;
        }

        proj.Fire(origin, dir, gameObject.tag);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Vector3 origin = firePoint != null ? firePoint.position : transform.position;

        Vector3 dir;

        if (firePoint != null)
            dir = firePoint.right;
        else
        {
            switch (fireDirection)
            {
                case FireDir.Left: dir = Vector3.left; break;
                case FireDir.Up: dir = Vector3.up; break;
                case FireDir.Down: dir = Vector3.down; break;
                default: dir = Vector3.right; break;
            }
        }

        Gizmos.DrawLine(origin, origin + dir * 1.5f);
    }
}
