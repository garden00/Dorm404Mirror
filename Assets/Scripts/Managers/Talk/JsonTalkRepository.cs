using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// JSON 파일에서 대화 데이터를 로드합니다. (ITalkRepository 구현)
/// </summary>
public class JsonTalkRepository : MonoBehaviour, ITalkRepository
{
    [SerializeField] private TextAsset talkJsonFile; // 대화 JSON 파일 (Unity Inspector에서 할당)

    private Dictionary<int, TalkData> _talkDatabase;

    void Awake()
    {
        LoadDatabase();
    }

    private void LoadDatabase()
    {
        if (talkJsonFile == null)
        {
            Debug.LogError("Talk JSON file is not assigned!");
            _talkDatabase = new Dictionary<int, TalkData>();
            return;
        }

        try
        {
            var allTalkData = JsonConvert.DeserializeObject<List<TalkData>>(talkJsonFile.text);
            _talkDatabase = allTalkData.ToDictionary(data => data.SceneId, data => data);
            Debug.Log($"Successfully loaded {_talkDatabase.Count} talk scenes.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to parse talk JSON: {ex.Message}");
            _talkDatabase = new Dictionary<int, TalkData>();
        }
    }

    public TalkData LoadTalkData(int sceneId)
    {
        if (_talkDatabase.TryGetValue(sceneId, out TalkData data))
        {
            return data;
        }

        Debug.LogWarning($"TalkData for sceneId {sceneId} not found.");
        return null;
    }
}