using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolingManager : MonoBehaviour
{

    #region Singleton

    public static ObjectPoolingManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null || Instance.gameObject == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    #endregion

    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private Transform poolParent;

    private Dictionary<GameObject, Queue<GameObject>> poolingDict = new Dictionary<GameObject, Queue<GameObject>>();
    private Dictionary<GameObject, GameObject> objectToPrefab = new Dictionary<GameObject, GameObject>();

    [SerializeField]
    private int initialPoolSize = 20;

    void Start()
    {
        foreach (var prefab in prefabs)
        {
            Queue<GameObject> queue = new Queue<GameObject>();
            for (int i = 0; i < initialPoolSize; i++)
            {
                GameObject obj = Instantiate(prefab, Vector3.zero, Quaternion.identity, poolParent);
                obj.SetActive(false);
                queue.Enqueue(obj);
                objectToPrefab[obj] = prefab; // 어떤 prefab에서 왔는지 기록
            }
            poolingDict[prefab] = queue;
        }
    }

    public void Return(GameObject obj)
    {
        if (!objectToPrefab.TryGetValue(obj, out GameObject prefab))
        {
            Debug.LogWarning("반환 에러 : 이 오브젝트는 풀에서 생성되지 않았습니다!");
            Destroy(obj);
            return;
        }

        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.velocity = Vector2.zero;

        obj.SetActive(false);
        poolingDict[prefab].Enqueue(obj);
    }

    public GameObject GetPrefab(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError("풀링 GetPrefab 요청 실패: 전달된 prefab이 null입니다. (FireBall의 selfPrefab이 할당되었는지 확인하세요)");
            return null;
        }

        if (!poolingDict.ContainsKey(prefab))
        {
            // --- 여기가 핵심 디버깅 코드 ---
            Debug.LogWarning($"[ObjectPool] 생성 에러: '{prefab.name}' (ID: {prefab.GetInstanceID()}) prefab은 풀에 없습니다!");

            Debug.LogWarning("--- 현재 풀에 등록된 키 목록 ---");
            foreach (GameObject key in poolingDict.Keys)
            {
                if (key == null)
                {
                    Debug.LogWarning(" - null key가 등록되어 있습니다.");
                }
                else
                {
                    Debug.LogWarning($" - 키: {key.name} (ID: {key.GetInstanceID()})");
                }
            }
            // --- 여기까지 ---

            return null;
        }

        Queue<GameObject> queue = poolingDict[prefab];

        GameObject obj;
        if (queue.Count > 0)
        {
            obj = queue.Dequeue();
        }
        else
        {
            // 풀 부족 시 새로 생성
            obj = Instantiate(prefab, Vector3.zero, Quaternion.identity, poolParent);
            objectToPrefab[obj] = prefab;
        }

        obj.SetActive(true);
        return obj;
    }

    /// <summary>
    /// 씬에 있는 인스턴스(Clone)를 기반으로
    /// 이 오브젝트의 원본 프리팹을 찾아 반환합니다.
    /// </summary>
    /// <param name="instance">씬에 있는 오브젝트 (Clone)</param>
    /// <returns>원본 프리팹</returns>
    public GameObject GetOriginalPrefab(GameObject instance)
    {
        if (objectToPrefab.ContainsKey(instance))
        {
            return objectToPrefab[instance];
        }

        Debug.LogError($"[ObjectPool] '{instance.name}' (ID: {instance.GetInstanceID()})는 풀 매니저의 'objectToPrefab' 딕셔너리에 등록되어 있지 않습니다! 혹시 풀을 통하지 않고 Instantiate로 생성되었나요?", instance);
        return null;
    }
}