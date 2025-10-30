using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(CanvasGroup))]
public class FadeEffect : MonoBehaviour
{
    // 페이드 효과를 줄 대상 (Image 대신 CanvasGroup 사용)
    private CanvasGroup canvasGroup;

    // 현재 실행 중인 코루틴을 저장 (중복 실행 방지)
    private Coroutine currentFadeCoroutine;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            Debug.LogError("FadeEffect: CanvasGroup 컴포넌트가 없습니다!");
        }

        // 처음엔 투명하게 시작
        canvasGroup.alpha = 0;
        // 씬 로딩 시 자동으로 페이드 인 하려면 아래 주석 해제
        // StartFadeIn(1.0f);
        canvasGroup.blocksRaycasts = false;
    }

    /// <summary>
    /// 페이드 아웃을 시작합니다 (밝음 -> 어두움)
    /// </summary>
    public void StartFadeOut(float duration)
    {
        StartFade(1.0f, duration); // 목표 알파: 1 (불투명)
    }

    /// <summary>
    /// 페이드 인을 시작합니다 (어두움 -> 밝음)
    /// </summary>
    public void StartFadeIn(float duration)
    {
        StartFade(0.0f, duration); // 목표 알파: 0 (투명)
    }

    /// <summary>
    /// 페이드 효과를 시작하고, 기존 페이드가 있다면 중지시킵니다.
    /// </summary>
    private void StartFade(float targetAlpha, float duration)
    {
        // 1. 이미 실행 중인 페이드 코루틴이 있다면 중지
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }

        // 2. 새 페이드 코루틴을 시작하고 저장
        currentFadeCoroutine = StartCoroutine(FadeRoutine(targetAlpha, duration));
    }

    /// <summary>
    /// (UIManager에서도 호출할 수 있도록 public으로 변경)
    /// 실제로 알파 값을 변경하는 코루틴
    /// </summary>
    public IEnumerator FadeRoutine(float targetAlpha, float duration)
    {
        // 1. 캔버스 그룹을 활성화 (클릭 방지 등)
        canvasGroup.blocksRaycasts = true;

        float startAlpha = canvasGroup.alpha;
        float elapsedTime = 0f;

        // 2. duration이 0이거나 매우 작으면 즉시 적용
        if (duration <= 0.01f)
        {
            canvasGroup.alpha = targetAlpha;
            elapsedTime = duration;
        }

        // 3. 시간이 다 될 때까지 알파 값을 서서히 변경
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            // 현재 시간 비율에 맞춰 시작 알파와 목표 알파 사이의 값을 계산 (선형 보간)
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);

            canvasGroup.alpha = newAlpha;

            yield return null; // 다음 프레임까지 대기
        }

        // 4. 정확히 목표 알파 값으로 설정
        canvasGroup.alpha = targetAlpha;

        // 5. 투명해졌다면(페이드 인 완료) 클릭 방지 해제
        if (targetAlpha == 0)
        {
            canvasGroup.blocksRaycasts = false;
        }

        currentFadeCoroutine = null;
    }
}