using System.Text.Json;
namespace Buildozer;

public struct SettingsCache
{
    public string[] EnabledOptions { get; set; }
    public string[] DisabledOptions { get; set; }

    public string Serialize()
    {
        return JsonSerializer.Serialize(this);
    }

    public static SettingsCache? Deserialize(string json)
    {
        return JsonSerializer.Deserialize<SettingsCache>(json);
    }
}