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

        curSavePoint = null;
    }

    void OnDestroy()
    {
        // 자신이 Instance일 경우에만 null로 설정
        if (Instance == this)
            Instance = null;
    }
    #endregion

    SavePoint curSavePoint;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {


            // player heal
            PlayerManager.Instance.Status.Healing();

            // save
            Debug.Log("Saved f");
            GameManager.Instance.Save(curSavePoint);

        }

        if (curSavePoint && Input.GetKeyDown(KeyCode.I))
        {


            // player heal
            PlayerManager.Instance.Status.Healing();

            // save
            Debug.Log("Saved");
            GameManager.Instance.Save(curSavePoint);

        }
    }

    public void OnEnterSavePoint(SavePoint savePoint)
    {
        // show UI
        curSavePoint = savePoint;
    }

    //public void OnStaySavePoint(SavePoint savePoint)
    //{

    //}

    public void OnExitSavePoint(SavePoint savePoint)
    {
        // hide UI
        curSavePoint = null;
    }
}
