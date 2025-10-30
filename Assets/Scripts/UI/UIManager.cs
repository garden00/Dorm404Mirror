using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    #region Singleton
    public static UIManager Instance { get; private set; }

    private void Awake()
    {
        // 3. 비파괴 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬이 바뀌어도 이 오브젝트를 파괴하지 않음
        }
        else
        {
            // 이미 인스턴스가 존재하면 (예: 다른 씬에서 넘어왔을 때)
            // 새로운 UIManager는 파괴함
            Destroy(gameObject);
            return;
        }
    }
    #endregion

    [SerializeField]
    private FadeEffect fadeEffect;
    [SerializeField]
    private SceneLoadingUI loadingUI;

    // --- 외부에서 요청할 공개 함수들 (파사드 역할) ---

    /// <summary>
    /// 화면을 검게 페이드 아웃시킵니다.
    /// </summary>
    /// <param name="duration">페이드아웃에 걸리는 시간</param>
    public void FadeOut(float duration = 1.0f)
    {
        fadeEffect.StartFadeOut(duration);
    }

    /// <summary>
    /// 검은 화면에서 밝게 페이드 인시킵니다.
    /// </summary>
    /// <param name="duration">페이드인에 걸리는 시간</param>
    public void FadeIn(float duration = 1.0f)
    {
        fadeEffect.StartFadeIn(duration);
    }

    /// <summary>
    /// 페이드 아웃 -> (중간 작업) -> 페이드 인을 연속으로 실행합니다.
    /// </summary>
    /// <param name="duration">페이드아웃/인 각각에 걸리는 시간</param>
    /// <param name="middleAction">페이드아웃 직후 실행할 작업 (예: 텔레포트)</param>
    public void FadeInOut(float duration, System.Action middleAction)
    {
        StartCoroutine(FadeInOutCoroutine(duration, middleAction));
    }

    private IEnumerator FadeInOutCoroutine(float duration, System.Action middleAction)
    {
        // 1. 페이드 아웃 시작 (완료될 때까지 대기)
        yield return StartCoroutine(fadeEffect.FadeRoutine(1.0f, duration));

        // 2. 중간 작업 실행 (예: 플레이어 위치 이동)
        middleAction?.Invoke();

        // 3. 페이드 인 시작 (완료될 때까지 대기)
        yield return StartCoroutine(fadeEffect.FadeRoutine(0.0f, duration));
    }

    //

    public void ShowLoadingScreen(bool show)
    {
        loadingUI.ShowLoadingScreen(show);
    }

    public void UpdateLoadingProgress(float progress)
    {
        loadingUI.UpdateLoadingProgress(progress);
    }
}
