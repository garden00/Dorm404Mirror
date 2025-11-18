using System.Collections.Generic;
using Newtonsoft.Json;

public class TalkData
{
    [JsonProperty("sceneId")]
    public int SceneId { get; set; }

    [JsonProperty("lines")]
    public List<TalkLine> Lines { get; set; }
}
