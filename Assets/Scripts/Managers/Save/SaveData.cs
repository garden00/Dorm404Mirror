using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveData
{
    // 데이터 버전 관리 (나중에 세이브 파일 구조가 바뀔 때 유용)
    //public string Version;
    public string LastSaveTime;

    // 데이터 그룹화
    public PlayerSaveData PlayerSaveData;
    public GameSaveData GameSaveData;
    public SettingData SettingData;

    // 생성자: 여기서 '기본값'을 설정합니다.
    // 파일이 없어서 새로 만들 때 이 값들이 사용됩니다.
    public SaveData()
    {
        //Version = Application.version;
        LastSaveTime = DateTime.Now.ToString();

        PlayerSaveData = new PlayerSaveData();
        GameSaveData = new GameSaveData();
        SettingData = new SettingData();
    }
}

[Serializable]
public class PlayerSaveData
{
    public int currentHp;

    public PlayerSaveData()
    {
        currentHp = 100;
    }
}

[Serializable]
public class GameSaveData
{
    public int saveNumber;
    public int SceneIndex;
    public Vector3? savePointPosition;

    public GameSaveData()
    {
        saveNumber = 1;
        SceneIndex = 1;
        savePointPosition = null;
    }
}

[Serializable]
public class SettingData
{

    public SettingData()
    {
    }
}