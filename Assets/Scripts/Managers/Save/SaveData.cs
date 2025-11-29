using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

[Serializable]
public struct Vector3Data
{
    public float x;
    public float y;
    public float z;

    public Vector3Data(Vector3 v)
    {
        x = v.x;
        y = v.y;
        z = v.z;
    }
    public Vector3Data(float _x, float _y, float _z)
    {
        x = _x;
        y = _y;
        z = _z;
    }

    public Vector3 ToVector3() => new Vector3(x, y, z);

    public static implicit operator Vector3(Vector3Data v)
    => new Vector3(v.x, v.y, v.z);

    public static implicit operator Vector3Data(Vector3 v)
        => new Vector3Data(v.x, v.y, v.z);
}

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
    public Vector3Data pos;

    public PlayerSaveData()
    {
        pos = new Vector3Data();
    }
}

[Serializable]
public class GameSaveData
{
    public int saveNumber; 
    public int SceneIndex;
    //public Vector3 savePointPosition;

    public GameSaveData()
    {
        saveNumber = 1;
        SceneIndex = 1;
        //savePointPosition = Vector3.zero;
    }
}

[Serializable]
public class SettingData
{

    public SettingData()
    {
    }
}