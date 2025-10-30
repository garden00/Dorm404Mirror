using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    public void OnClick_StartButton()
    {
        if (GameManager.Instance != null)
        {
            // 나중에 Save/Load System 연결해서 GameManager가 SceneController.Instance.LoadScene(1)을 호출하도록 할 예정
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
