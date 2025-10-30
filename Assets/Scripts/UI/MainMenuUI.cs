using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    public void OnClick_StartButton()
    {
        if (GameManager.Instance != null)
        {
            // ���߿� Save/Load System �����ؼ� GameManager�� SceneController.Instance.LoadScene(1)�� ȣ���ϵ��� �� ����
            SceneController.Instance.LoadScene(1);
        }
    }

    public void OnClick_ExitButton()
    {
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }
}
