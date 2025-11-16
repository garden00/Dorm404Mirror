using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 캐릭터 초상화 에셋을 관리하고 제공합니다. (IPortraitProvider 구현)
/// </summary>
public class PortraitManager : MonoBehaviour, IPortraitProvider
{
    // Inspector에서 설정할 초상화 데이터 구조
    [System.Serializable]
    public class PortraitMapping
    {
        public string speakerName; // JSON의 "speaker"와 일치
        public List<Sprite> expressions; // 0번 = 기본, 1번 = 화남 등 (JSON의 "portrait" ID와 매칭)
    }

    [SerializeField] private List<PortraitMapping> portraitDatabase;

    private Dictionary<string, List<Sprite>> _portraitDict;

    void Awake()
    {
        // 빠른 조회를 위해 Dictionary로 변환
        _portraitDict = new Dictionary<string, List<Sprite>>();
        foreach (var mapping in portraitDatabase)
        {
            if (!_portraitDict.ContainsKey(mapping.speakerName))
            {
                _portraitDict[mapping.speakerName] = mapping.expressions;
            }
        }
    }

    public Sprite GetPortrait(string speakerName, int expressionId)
    {
        if (_portraitDict.TryGetValue(speakerName, out List<Sprite> expressions))
        {
            if (expressionId >= 0 && expressionId < expressions.Count)
            {
                return expressions[expressionId];
            }
            Debug.LogWarning($"Expression ID {expressionId} out of range for {speakerName}.");
        }

        Debug.LogWarning($"Speaker name {speakerName} not found in PortraitManager.");
        return null; // 혹은 기본 이미지 반환
    }
}