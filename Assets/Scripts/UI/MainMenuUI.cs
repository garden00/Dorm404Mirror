using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    //enum Frame
    //{
    //    Main,
    //    Select,
    //    Setteing
    //}

    //Frame currentFrame;

    private void Start()
    {
        Hide_SelecFrame();
        Show_MainFrame();
    }

    // === main frame ===

    [SerializeField]
    GameObject MainFrame;

    private void Show_MainFrame()
    {
        //currentFrame = Frame.Main;

        MainFrame.SetActive(true);
    }

    private void Hide_MainFrame()
    {
        //currentFrame = Frame.Main;

        MainFrame.SetActive(false);
    }

    // --- button ---

    public void OnClick_StartButton()
    {
        // show UI
        Hide_MainFrame();
        Show_SelectFrame();


        //GameManager.Instance.StartGame();
    }

    public void OnClick_ExitButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
    }

    // === Select frame ===

    [SerializeField]
    GameObject SelectFrame;

    private void Show_SelectFrame()
    {
        //currentFrame = Frame.Select;

        SelectFrame.SetActive(true);
    }

    private void Hide_SelecFrame()
    {
        //currentFrame = Frame.Select;

        SelectFrame.SetActive(false);
    }

    // --- button ---
    public void OnClick_SelectButton(int number)
    {
        if (GameManager.Instance != null)
        {
            SaveData data;


            if (SaveSystem.Exists("Save" + number))
            {
                data = SaveSystem.Load<SaveData>("Save" + number);
            }
            else
            {
                data = new SaveData();
                data.GameSaveData.saveNumber = number;
            }

            GameManager.Instance.StartGame(data);
        }
    }

    public void OnClick_DataDeleteButton(int number)
    {
        if (GameManager.Instance != null)
        {
            if (SaveSystem.Exists("Save" + number))
            {
                Debug.Log("Deleted " + number);
                SaveSystem.Delete("Save" + number);
            }
        }
    }

    public void OnClick_SelectFrameBackButton()
    {
        Hide_SelecFrame();
        Show_MainFrame();
    }

    // Setteing frame

    [SerializeField]
    GameObject SetteingFrame;


}
