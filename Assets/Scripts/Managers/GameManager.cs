using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Singleton

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        // 3. ���ı� �̱��� ����
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ���� �ٲ� �� ������Ʈ�� �ı����� ����
        }
        else
        {
            // �̹� �ν��Ͻ��� �����ϸ� (��: �ٸ� ������ �Ѿ���� ��)
            // ���ο� UIManager�� �ı���
            Destroy(gameObject);
            return;
        }
    }

    #endregion

    void Update()
    {
        // test�� �ڵ�
        if (Input.GetKeyDown(KeyCode.P))
        {
            int currentSceneNumber = SceneController.Instance.SceneNumber + 1;
            currentSceneNumber %= 6;
            SceneController.Instance.LoadScene(currentSceneNumber);

        }
    }
}
