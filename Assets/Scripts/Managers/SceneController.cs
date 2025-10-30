using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{

    #region Singleton

    public static SceneController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Unity�� �� �ε� �Ϸ� �̺�Ʈ�� ����
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    #endregion

    private void OnDestroy()
    {
        // DontDestroyOnLoad ��ü�� �ı��� ���� ����� �̺�Ʈ ���� ����
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        this.SceneNumber = scene.buildIndex;
    }

    private void Start()
    {
        this.SceneNumber = SceneManager.GetActiveScene().buildIndex;
    }

    public event Action<int> SceneChanged;

    int sceneNumber;
    public int SceneNumber
    {
        get => sceneNumber;
        private set
        {
            if (sceneNumber == value) return; // ���� �� ��ȣ�� �̺�Ʈ �̹߻�
            sceneNumber = value;
            SceneChanged?.Invoke(sceneNumber);
        }
    }

    public string GetSceneName(int number)
    {
        string sceneName = SceneUtility.GetScenePathByBuildIndex(0); // 0�� ��
        sceneName = System.IO.Path.GetFileNameWithoutExtension(sceneName); // ���� Ȯ���� ����

        return sceneName;
    }

    public int GetSceneNumber(string name)
    {
        return SceneUtility.GetBuildIndexByScenePath("Assets/Scenes/"+ name + ".unity");
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    public void LoadScene(int sceneNumber)
    {
        StartCoroutine(LoadSceneAsync(sceneNumber));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        // UIManager���� �ε� ȭ�� ǥ�� ��û
        UIManager.Instance.ShowLoadingScreen(true);

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            UIManager.Instance.UpdateLoadingProgress(progress); // ����� ����
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        // UIManager���� �ε� ȭ�� ����� ��û
        UIManager.Instance.ShowLoadingScreen(false);
    }
    private IEnumerator LoadSceneAsync(int sceneNumber)
    {
        // UIManager���� �ε� ȭ�� ǥ�� ��û
        UIManager.Instance.ShowLoadingScreen(true);

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneNumber);

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            UIManager.Instance.UpdateLoadingProgress(progress); // ����� ����
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        // UIManager���� �ε� ȭ�� ����� ��û
        UIManager.Instance.ShowLoadingScreen(false);
    }
}
