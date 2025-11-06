using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public static class SaveSystem
{
    // 저장 위치
    private static readonly string SaveFolder =
        Path.Combine(Application.persistentDataPath, "Save");

    /// <summary>
    /// 객체를 JSON으로 직렬화하여 파일로 저장
    /// </summary>
    public static void Save<T>(T data, string fileName)
    {
        try
        {
            if (!Directory.Exists(SaveFolder))
                Directory.CreateDirectory(SaveFolder);

            string path = Path.Combine(SaveFolder, fileName);
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);

            File.WriteAllText(path, json);
#if UNITY_EDITOR
            Debug.Log($"[SaveSystem] Saved: {path}");
#endif
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveSystem] Save failed: {e.Message}");
        }
    }

    /// <summary>
    /// 파일에서 JSON을 읽어 지정한 타입으로 역직렬화
    /// </summary>
    public static T Load<T>(string fileName)
    {
        string path = Path.Combine(SaveFolder, fileName);

        if (!File.Exists(path))
        {
#if UNITY_EDITOR
            Debug.LogWarning($"[SaveSystem] File not found: {path}");
#endif
            return default;
        }

        try
        {
            string json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<T>(json);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveSystem] Load failed: {e.Message}");
            return default;
        }
    }

    /// <summary>
    /// 파일 존재 여부 확인
    /// </summary>
    public static bool Exists(string fileName)
    {
        string path = Path.Combine(SaveFolder, fileName);
        return File.Exists(path);
    }

    /// <summary>
    /// 저장된 파일 삭제
    /// </summary>
    public static void Delete(string fileName)
    {
        string path = Path.Combine(SaveFolder, fileName);

        if (File.Exists(path))
        {
            File.Delete(path);
#if UNITY_EDITOR
            Debug.Log($"[SaveSystem] Deleted: {path}");
#endif
        }
    }
}
