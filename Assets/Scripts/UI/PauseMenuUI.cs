using UnityEngine;
using UnityEngine.SceneManagement;  

public class PauseMenuController : MonoBehaviour
{
    [Header("UI")]
    public GameObject pauseMenuPanel;

    [Header("ESC 키")]
    public KeyCode pauseKey = KeyCode.Escape;

    public bool IsPaused { get; private set; } = false;

    private void Start()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        Time.timeScale = 1f;
        IsPaused = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (!IsPaused)
            Pause();
        else
            Resume();
    }

    public void Pause()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);

        Time.timeScale = 0f; 
        IsPaused = true;
    }

    public void Resume()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        Time.timeScale = 1f;  
        IsPaused = false;
    }

    public void OnClickResume()
    {
        Resume();
    }

    public void OnClickSave()
    {
        Debug.Log("[PauseMenu] Save 버튼 클릭");
        Save();
    }

    public void OnClickLoad()
    {
        Debug.Log("[PauseMenu] Load 버튼 클릭");
        Load();
    }


    public void OnClickExit()
    {
        Debug.Log("[PauseMenu] Exit 버튼 클릭");

        Time.timeScale = 1f;
        SceneManager.LoadScene("Title");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    }
#endif
    private void Save()
    {
    }

    private void Load()
    {
    }
}
