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
        //DontDestroyOnLoad(gameObject);

        // Unity의 씬 로드 완료 이벤트에 구독
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    #endregion

    private void OnDestroy()
    {
        // DontDestroyOnLoad 객체라도 파괴될 때를 대비해 이벤트 구독 해제
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
            if (sceneNumber == value) return; // 같은 씬 번호면 이벤트 미발생
            sceneNumber = value;
            SceneChanged?.Invoke(sceneNumber);
        }
    }

    public string GetSceneName(int number)
    {
        string sceneName = SceneUtility.GetScenePathByBuildIndex(0); // 0번 씬
        sceneName = System.IO.Path.GetFileNameWithoutExtension(sceneName); // 파일 확장자 제거

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
        // UIManager에게 로딩 화면 표시 요청
        UIManager.Instance.ShowLoadingScreen(true);

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            UIManager.Instance.UpdateLoadingProgress(progress); // 진행률 전달
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        // UIManager에게 로딩 화면 숨기기 요청
        UIManager.Instance.ShowLoadingScreen(false);
    }
    private IEnumerator LoadSceneAsync(int sceneNumber)
    {
        // UIManager에게 로딩 화면 표시 요청
        UIManager.Instance.ShowLoadingScreen(true);

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneNumber);

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            UIManager.Instance.UpdateLoadingProgress(progress); // 진행률 전달
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        // UIManager에게 로딩 화면 숨기기 요청
        UIManager.Instance.ShowLoadingScreen(false);
    }
}
