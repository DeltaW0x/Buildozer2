using System.Text.Json;
namespace Buildozer;

public struct ScriptCache
{
    public string Path {get; set;}
    public string PathHash {get; set;}
    public DateTime ModifyTimestamp { get; set; }
    
    public string Serialize()
    {
        return JsonSerializer.Serialize(this);
    }

    public static ScriptCache? Deserialize(string json)
    {
        return JsonSerializer.Deserialize<ScriptCache>(json);
    }
}