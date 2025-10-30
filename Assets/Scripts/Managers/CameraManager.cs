using System.Collections;
using UnityEngine;

// 2D ī�޶��̹Ƿ� Camera ������Ʈ�� �ݵ�� �ʿ��ϴٰ� ���
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
            Instance = null;  // �� ��ȯ �� ����
    }
    #endregion

    public Transform target;
    public float smoothSpeed = 5f;
    public Vector3 offset;

    private Camera cam;
    private float camHalfHeight;
    private float camHalfWidth;

    // �� ��� �������� ���� private�Դϴ�. (�ܺο��� ���Թ���)
    private float camMinX, camMaxX, camMinY, camMaxY;

    // ��谡 �����Ǿ����� Ȯ���ϴ� �÷���
    private bool hasBounds = false;

    void Start()
    {


        offset = Vector3.zero;

        //  [����] cam �ʱ�ȭ �ڵ带 Awake()�� �̵��߽��ϴ�.
        // cam = GetComponent<Camera>(); 
    }

    /// <summary>
    ///  [�߰�] �ܺο��� �� ī�޶��� ��踦 ���� �����ϴ� �Լ�
    /// </summary>
    /// <param name="newMapBounds">���ο� ��(Room)�� ��� �ݶ��̴�</param>
    public void UpdateCameraBounds(Bounds newMapBounds)
    {
        // 1. ī�޶��� ���� ũ�⸦ ���մϴ�.
        camHalfHeight = cam.orthographicSize;
        camHalfWidth = camHalfHeight * cam.aspect;

        // 2. ���ο� ��谪�� �������� ī�޶��� ���� �̵� �Ѱ踦 �ٽ� ����մϴ�.
        camMinX = newMapBounds.min.x + camHalfWidth;
        camMaxX = newMapBounds.max.x - camHalfWidth;
        camMinY = newMapBounds.min.y + camHalfHeight;
        camMaxY = newMapBounds.max.y - camHalfHeight;

        // 3. ��谡 �����Ǿ����� �˸�
        hasBounds = true;
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPos = new Vector3(target.position.x, target.position.y, transform.position.z);

        //  [����] ��谡 ������ ��쿡�� Clamp�� �����մϴ�.
        if (hasBounds)
        {
            float clampedX = Mathf.Clamp(desiredPos.x, camMinX, camMaxX);
            float clampedY = Mathf.Clamp(desiredPos.y, camMinY, camMaxY);
            desiredPos = new Vector3(clampedX, clampedY, desiredPos.z);
        }

        transform.position = Vector3.Lerp(transform.position, desiredPos + offset, smoothSpeed * Time.deltaTime);
    }

    // ---  ������ WobbleEffect ---
    public IEnumerator WobbleEffect(EightDirection dir, float magnitude = 0.005f)
    {
        float timer = 0f;
        float time = 0.3f;

        while (timer < time)
        {
            offset = new Vector3(dir.x * magnitude * Mathf.Cos(30 * timer), dir.y * magnitude * Mathf.Cos(30 * timer), 0);

            timer += Time.deltaTime;
            yield return null;
        }

        // ��鸲�� ������ offset�� 0���� �ʱ�ȭ
        offset = Vector3.zero;
    }
}