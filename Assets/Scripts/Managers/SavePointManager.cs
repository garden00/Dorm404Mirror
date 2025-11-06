using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavePointManager : MonoBehaviour
{
    #region Scene Singleton
    public static SavePointManager Instance { get; private set; }

    private void Awake()
    {
        // 씬 싱글톤의 표준적인 구현
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void OnDestroy()
    {
        // 자신이 Instance일 경우에만 null로 설정
        if (Instance == this)
            Instance = null;
    }
    #endregion

    public void OnEnterSavePoint(SavePoint savePoint)
    {
        // show UI
    }

    public void OnStaySavePoint(SavePoint savePoint)
    {



        if(Input.GetKeyDown(KeyCode.I))
        {
            //SaveSystem.Save <= GameManager.gameData, player.position, savePoint
            //PlayerManager.playerStatus.Healing()
        }
    }

    public void OnExitSavePoint(SavePoint savePoint)
    {
        // hide UI
    }
}
