using System.IO;
using System.Text;
using System.Text.Json;

namespace SocialMediaSentimentApp;

public static class JsonUtil
{
    public static void WriteJson<T>(string path, T obj)
    {
        var opts = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(obj, opts);
        File.WriteAllText(path, json, Encoding.UTF8);
    }
}
