using System.Collections;
using UnityEngine;

// 2D 카메라이므로 Camera 컴포넌트가 반드시 필요하다고 명시
[RequireComponent(typeof(Camera))]
public class CameraManager : MonoBehaviour
{
    #region Scene Singleton

    public static CameraManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        cam = GetComponent<Camera>();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;  // 씬 전환 시 정리
    }
    #endregion

    public Transform target;
    public float smoothSpeed = 5f;
    public Vector3 offset;

    private Camera cam;
    private float camHalfHeight;
    private float camHalfWidth;

    // 맵 경계 변수들은 이제 private입니다. (외부에서 주입받음)
    private float camMinX, camMaxX, camMinY, camMaxY;

    // 경계가 설정되었는지 확인하는 플래그
    private bool hasBounds = false;

    void Start()
    {


        offset = Vector3.zero;

        //  [수정] cam 초기화 코드를 Awake()로 이동했습니다.
        // cam = GetComponent<Camera>(); 
    }

    /// <summary>
    ///  [추가] 외부에서 이 카메라의 경계를 새로 설정하는 함수
    /// </summary>
    /// <param name="newMapBounds">새로운 방(Room)의 경계 콜라이더</param>
    public void UpdateCameraBounds(Bounds newMapBounds)
    {
        // 1. 카메라의 절반 크기를 구합니다.
        camHalfHeight = cam.orthographicSize;
        camHalfWidth = camHalfHeight * cam.aspect;

        // 2. 새로운 경계값을 기준으로 카메라의 실제 이동 한계를 다시 계산합니다.
        camMinX = newMapBounds.min.x + camHalfWidth;
        camMaxX = newMapBounds.max.x - camHalfWidth;
        camMinY = newMapBounds.min.y + camHalfHeight;
        camMaxY = newMapBounds.max.y - camHalfHeight;

        // 3. 경계가 설정되었음을 알림
        hasBounds = true;
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPos = new Vector3(target.position.x, target.position.y, transform.position.z);

        //  [수정] 경계가 설정된 경우에만 Clamp를 적용합니다.
        if (hasBounds)
        {
            float clampedX = Mathf.Clamp(desiredPos.x, camMinX, camMaxX);
            float clampedY = Mathf.Clamp(desiredPos.y, camMinY, camMaxY);
            desiredPos = new Vector3(clampedX, clampedY, desiredPos.z);
        }

        transform.position = Vector3.Lerp(transform.position, desiredPos + offset, smoothSpeed * Time.deltaTime);
    }

    // ---  수정된 WobbleEffect ---
    public IEnumerator WobbleEffect(EightDirection dir, float magnitude = 0.005f)
    {
        float timer = 0f;
        float time = 0.3f;

        if(dir == EightDirection.None)
        {
            dir = EightDirection.Right;
        }

        while (timer < time)
        {
            offset = new Vector3(dir.x * magnitude * Mathf.Cos(30 * timer), dir.y * magnitude * Mathf.Cos(30 * timer), 0);

            timer += Time.deltaTime;
            yield return null;
        }

        // 흔들림이 끝나면 offset을 0으로 초기화
        offset = Vector3.zero;
    }
}