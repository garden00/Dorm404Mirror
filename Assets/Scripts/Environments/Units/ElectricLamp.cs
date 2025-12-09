using UnityEngine;

public class ElectricLamp : MonoBehaviour
{
    public enum FireDir
    {
        Right,
        Left,
        Up,
        Down
    }

    [Header("Beam Settings")]
    [SerializeField] private GameObject beamPrefab;   // SingularBeam 프리팹
    [SerializeField] private float fireInterval = 1f; // 몇 초마다 발사할지

    [Header("Direction")]
    [SerializeField] private FireDir fireDirection = FireDir.Right; // 인스펙터에서 선택할 방향
    [SerializeField] private bool useFirePointDirection = false;    // true면 FirePoint의 방향 사용
    [SerializeField] private Transform firePoint;                   // 발사 위치 (선택사항)

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
            Debug.LogWarning("[ElectricLamp] beamPrefab이 설정되지 않았습니다.", this);
            return;
        }

        // 발사 위치
        Vector3 origin = firePoint != null ? firePoint.position : transform.position;

        //  발사 방향 결정
        Vector3 dir = GetDirection();

        // 풀에서 빔 꺼내기
        var obj = ObjectPoolingManager.Instance.GetPrefab(beamPrefab);
        if (obj == null)
        {
            Debug.LogWarning("[ElectricLamp] 풀에서 빔 프리팹을 가져오지 못했습니다.", this);
            return;
        }

        var proj = obj.GetComponent<IProjectile>();
        if (proj == null)
        {
            Debug.LogWarning("[ElectricLamp] IProjectile 구현이 없습니다.", obj);
            return;
        }

        proj.Fire(origin, dir, gameObject.tag);
    }

    private Vector3 GetDirection()
    {
        // FirePoint 방향을 쓰고 싶고, FirePoint가 있으면 그 방향 우선
        if (useFirePointDirection && firePoint != null)
        {
            Vector3 d = firePoint.right;
            if (d.sqrMagnitude < 0.0001f) d = Vector3.right;
            return d.normalized;
        }

        // 그 외에는 Inspector에서 선택한 enum 방향 사용
        switch (fireDirection)
        {
            case FireDir.Left: return Vector3.left;
            case FireDir.Up: return Vector3.up;
            case FireDir.Down: return Vector3.down;
            case FireDir.Right:
            default: return Vector3.right;
        }
    }

    // 방향 디버깅용 기즈모
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Vector3 origin = firePoint != null ? firePoint.position : transform.position;
        Vector3 dir = GetDirection();

        Gizmos.DrawLine(origin, origin + dir * 1.5f);
    }
}
