using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Singleton

    public static GameManager Instance { get; private set; }

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
    PlayerStatusData playerStatusData;

    public SaveData saveData;


    void Update()
    {
        // test용 코드
        if (Input.GetKeyDown(KeyCode.P))
        {
            int currentSceneNumber = SceneController.Instance.SceneNumber + 1;
            currentSceneNumber %= 6;
            SceneController.Instance.LoadScene(currentSceneNumber);

        }

        if(playerStatusData.CurrentState == PlayerState.Dead)
        {
            playerStatusData.ResetData();

            StartCoroutine(RestartGame());
        }
    }

    private void Start()
    {
        //playerStatusData.OnPlayerDeath += PlayerDeath;
    }

    public void StartGame(SaveData data)
    {
        //
        saveData = data;
        StartCoroutine(ApplyData());
    }

    public void Save(SavePoint point)
    {


        saveData.GameSaveData.SceneIndex = SceneController.Instance.SceneNumber;
        if (point != null)
            saveData.PlayerSaveData.pos = point.transform.position;

        Debug.Log("save scene : " + saveData.GameSaveData.SceneIndex);
        SaveSystem.Save<SaveData>(saveData, "Save" + saveData.GameSaveData.saveNumber);
    }

    private IEnumerator ApplyData()
    {
        // load 까지 기다리기
        yield return SceneController.Instance.LoadSceneAsync(saveData.GameSaveData.SceneIndex);

        PlayerManager.Instance.Status.ResetData();

        if (saveData.PlayerSaveData.pos != Vector3.zero)
            yield return PlayerManager.Instance.transform.position = saveData.PlayerSaveData.pos;


        CameraManager.Instance.Teleport(saveData.PlayerSaveData.pos);

        yield return null;

    }



    private IEnumerator RestartGame()
    {
        UIManager.Instance.FadeOut(1f);
        yield return new WaitForSeconds(1.5f);

        // save data 적용
        yield return ApplyData();

        yield return null;
    }
}
