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

    [SerializeField]
    private FadeEffect fadeEffect;
    [SerializeField]
    private SceneLoadingUI loadingUI;

    // --- �ܺο��� ��û�� ���� �Լ��� (�Ļ�� ����) ---

    /// <summary>
    /// ȭ���� �˰� ���̵� �ƿ���ŵ�ϴ�.
    /// </summary>
    /// <param name="duration">���̵�ƿ��� �ɸ��� �ð�</param>
    public void FadeOut(float duration = 1.0f)
    {
        fadeEffect.StartFadeOut(duration);
    }

    /// <summary>
    /// ���� ȭ�鿡�� ��� ���̵� �ν�ŵ�ϴ�.
    /// </summary>
    /// <param name="duration">���̵��ο� �ɸ��� �ð�</param>
    public void FadeIn(float duration = 1.0f)
    {
        fadeEffect.StartFadeIn(duration);
    }

    /// <summary>
    /// ���̵� �ƿ� -> (�߰� �۾�) -> ���̵� ���� �������� �����մϴ�.
    /// </summary>
    /// <param name="duration">���̵�ƿ�/�� ������ �ɸ��� �ð�</param>
    /// <param name="middleAction">���̵�ƿ� ���� ������ �۾� (��: �ڷ���Ʈ)</param>
    public void FadeInOut(float duration, System.Action middleAction)
    {
        StartCoroutine(FadeInOutCoroutine(duration, middleAction));
    }

    private IEnumerator FadeInOutCoroutine(float duration, System.Action middleAction)
    {
        // 1. ���̵� �ƿ� ���� (�Ϸ�� ������ ���)
        yield return StartCoroutine(fadeEffect.FadeRoutine(1.0f, duration));

        // 2. �߰� �۾� ���� (��: �÷��̾� ��ġ �̵�)
        middleAction?.Invoke();

        // 3. ���̵� �� ���� (�Ϸ�� ������ ���)
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
