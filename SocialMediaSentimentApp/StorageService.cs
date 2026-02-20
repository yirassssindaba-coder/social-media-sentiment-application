using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace SocialMediaSentimentApp;

public sealed class StorageService
{
    public string BaseDir { get; }
    public string PostsPath { get; }
    public string SettingsPath { get; }
    public string ReportsDir { get; }
    public string AssetsDir { get; }
    public string OutputsDir { get; }

    public StorageService()
    {
        BaseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SocialMediaSentimentApp");
        PostsPath = Path.Combine(BaseDir, "posts.json");
        SettingsPath = Path.Combine(BaseDir, "settings.json");
        ReportsDir = Path.Combine(BaseDir, "reports");
        AssetsDir = Path.Combine(ReportsDir, "assets");
        OutputsDir = Path.Combine(BaseDir, "outputs");

        Directory.CreateDirectory(BaseDir);
        Directory.CreateDirectory(ReportsDir);
        Directory.CreateDirectory(AssetsDir);
        Directory.CreateDirectory(OutputsDir);
    }

    public List<PostItem> LoadPosts()
    {
        if (!File.Exists(PostsPath)) return new List<PostItem>();

        var json = File.ReadAllText(PostsPath, Encoding.UTF8);
        var obj = JsonSerializer.Deserialize<List<PostItem>>(json);
        return obj ?? new List<PostItem>();
    }

    public void SavePosts(List<PostItem> posts)
    {
        var opts = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(posts, opts);
        File.WriteAllText(PostsPath, json, Encoding.UTF8);
    }

    public AppSettings LoadSettings()
    {
        if (!File.Exists(SettingsPath)) return new AppSettings();

        var json = File.ReadAllText(SettingsPath, Encoding.UTF8);
        var obj = JsonSerializer.Deserialize<AppSettings>(json);
        return obj ?? new AppSettings();
    }

    public void SaveSettings(AppSettings s)
    {
        var opts = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(s, opts);
        File.WriteAllText(SettingsPath, json, Encoding.UTF8);
    }
}
