using Newtonsoft.Json;

public class TalkLine
{
    [JsonProperty("speaker")]
    public string Speaker { get; set; }

    [JsonProperty("portrait")]
    public int PortraitId { get; set; } // 초상화 ID (표정 등)

    [JsonProperty("effect")]
    public int EffectId { get; set; } // 텍스트 효과 (0: 기본, 1: 흔들림 등)

    [JsonProperty("text")]
    public string Text { get; set; }
}