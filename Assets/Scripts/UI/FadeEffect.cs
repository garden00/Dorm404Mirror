using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(CanvasGroup))]
public class FadeEffect : MonoBehaviour
{
    // ���̵� ȿ���� �� ��� (Image ��� CanvasGroup ���)
    private CanvasGroup canvasGroup;

    // ���� ���� ���� �ڷ�ƾ�� ���� (�ߺ� ���� ����)
    private Coroutine currentFadeCoroutine;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            Debug.LogError("FadeEffect: CanvasGroup ������Ʈ�� �����ϴ�!");
        }

        // ó���� �����ϰ� ����
        canvasGroup.alpha = 0;
        // �� �ε� �� �ڵ����� ���̵� �� �Ϸ��� �Ʒ� �ּ� ����
        // StartFadeIn(1.0f);
        canvasGroup.blocksRaycasts = false;
    }

    /// <summary>
    /// ���̵� �ƿ��� �����մϴ� (���� -> ��ο�)
    /// </summary>
    public void StartFadeOut(float duration)
    {
        StartFade(1.0f, duration); // ��ǥ ����: 1 (������)
    }

    /// <summary>
    /// ���̵� ���� �����մϴ� (��ο� -> ����)
    /// </summary>
    public void StartFadeIn(float duration)
    {
        StartFade(0.0f, duration); // ��ǥ ����: 0 (����)
    }

    /// <summary>
    /// ���̵� ȿ���� �����ϰ�, ���� ���̵尡 �ִٸ� ������ŵ�ϴ�.
    /// </summary>
    private void StartFade(float targetAlpha, float duration)
    {
        // 1. �̹� ���� ���� ���̵� �ڷ�ƾ�� �ִٸ� ����
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }

        // 2. �� ���̵� �ڷ�ƾ�� �����ϰ� ����
        currentFadeCoroutine = StartCoroutine(FadeRoutine(targetAlpha, duration));
    }

    /// <summary>
    /// (UIManager������ ȣ���� �� �ֵ��� public���� ����)
    /// ������ ���� ���� �����ϴ� �ڷ�ƾ
    /// </summary>
    public IEnumerator FadeRoutine(float targetAlpha, float duration)
    {
        // 1. ĵ���� �׷��� Ȱ��ȭ (Ŭ�� ���� ��)
        canvasGroup.blocksRaycasts = true;

        float startAlpha = canvasGroup.alpha;
        float elapsedTime = 0f;

        // 2. duration�� 0�̰ų� �ſ� ������ ��� ����
        if (duration <= 0.01f)
        {
            canvasGroup.alpha = targetAlpha;
            elapsedTime = duration;
        }

        // 3. �ð��� �� �� ������ ���� ���� ������ ����
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            // ���� �ð� ������ ���� ���� ���Ŀ� ��ǥ ���� ������ ���� ��� (���� ����)
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);

            canvasGroup.alpha = newAlpha;

            yield return null; // ���� �����ӱ��� ���
        }

        // 4. ��Ȯ�� ��ǥ ���� ������ ����
        canvasGroup.alpha = targetAlpha;

        // 5. ���������ٸ�(���̵� �� �Ϸ�) Ŭ�� ���� ����
        if (targetAlpha == 0)
        {
            canvasGroup.blocksRaycasts = false;
        }

        currentFadeCoroutine = null;
    }
}