using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SceneLoadingUI : MonoBehaviour
{
    [SerializeField]
    private GameObject loadingScreen;
    [SerializeField]
    private Image progressBar;

    private void Awake()
    {
        loadingScreen.SetActive(false);
    }

    public void ShowLoadingScreen(bool show)
    {
        if (loadingScreen != null)
            loadingScreen.SetActive(show);
        else
            Debug.LogWarning("UIManager.loadingScreen�� ������� �ʾҽ��ϴ�.");
    }

    public void UpdateLoadingProgress(float progress)
    {
        if (progressBar != null)
            progressBar.fillAmount = progress;
        else
            Debug.LogWarning("UIManager.progressBar�� ������� �ʾҽ��ϴ�.");
    }
}
