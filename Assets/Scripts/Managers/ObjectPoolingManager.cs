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
                objectToPrefab[obj] = prefab; // � prefab���� �Դ��� ���
            }
            poolingDict[prefab] = queue;
        }
    }

    public void Return(GameObject obj)
    {
        if (!objectToPrefab.TryGetValue(obj, out GameObject prefab))
        {
            Debug.LogWarning("��ȯ ���� : �� ������Ʈ�� Ǯ���� �������� �ʾҽ��ϴ�!");
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
            Debug.LogError("Ǯ�� GetPrefab ��û ����: ���޵� prefab�� null�Դϴ�. (FireBall�� selfPrefab�� �Ҵ�Ǿ����� Ȯ���ϼ���)");
            return null;
        }

        if (!poolingDict.ContainsKey(prefab))
        {
            // --- ���Ⱑ �ٽ� ����� �ڵ� ---
            Debug.LogWarning($"[ObjectPool] ���� ����: '{prefab.name}' (ID: {prefab.GetInstanceID()}) prefab�� Ǯ�� �����ϴ�!");

            Debug.LogWarning("--- ���� Ǯ�� ��ϵ� Ű ��� ---");
            foreach (GameObject key in poolingDict.Keys)
            {
                if (key == null)
                {
                    Debug.LogWarning(" - null key�� ��ϵǾ� �ֽ��ϴ�.");
                }
                else
                {
                    Debug.LogWarning($" - Ű: {key.name} (ID: {key.GetInstanceID()})");
                }
            }
            // --- ������� ---

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
            // Ǯ ���� �� ���� ����
            obj = Instantiate(prefab, Vector3.zero, Quaternion.identity, poolParent);
            objectToPrefab[obj] = prefab;
        }

        obj.SetActive(true);
        return obj;
    }

    /// <summary>
    /// ���� �ִ� �ν��Ͻ�(Clone)�� �������
    /// �� ������Ʈ�� ���� �������� ã�� ��ȯ�մϴ�.
    /// </summary>
    /// <param name="instance">���� �ִ� ������Ʈ (Clone)</param>
    /// <returns>���� ������</returns>
    public GameObject GetOriginalPrefab(GameObject instance)
    {
        if (objectToPrefab.ContainsKey(instance))
        {
            return objectToPrefab[instance];
        }

        Debug.LogError($"[ObjectPool] '{instance.name}' (ID: {instance.GetInstanceID()})�� Ǯ �Ŵ����� 'objectToPrefab' ��ųʸ��� ��ϵǾ� ���� �ʽ��ϴ�! Ȥ�� Ǯ�� ������ �ʰ� Instantiate�� �����Ǿ�����?", instance);
        return null;
    }
}