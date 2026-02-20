using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace SocialMediaSentimentApp;

public static class CsvService
{
    public static List<PostItem> ReadPosts(string path)
    {
        var lines = File.ReadAllLines(path, Encoding.UTF8);
        if (lines.Length < 2) return new List<PostItem>();

        var header = ParseLine(lines[0]).Select(h => h.Trim().ToLowerInvariant()).ToList();
        int idxTs = header.IndexOf("timestamp");
        int idxPlatform = header.IndexOf("platform");
        int idxText = header.IndexOf("text");

        if (idxTs < 0 || idxPlatform < 0 || idxText < 0)
            throw new InvalidOperationException("CSV must have headers: timestamp,platform,text");

        var posts = new List<PostItem>();

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            var cols = ParseLine(lines[i]);

            if (idxTs >= cols.Count || idxPlatform >= cols.Count || idxText >= cols.Count) continue;

            var tsRaw = cols[idxTs];
            var platform = (cols[idxPlatform] ?? "").Trim();
            var text = (cols[idxText] ?? "").Trim();

            if (string.IsNullOrWhiteSpace(platform) || string.IsNullOrWhiteSpace(text)) continue;

            if (!TryParseTimestamp(tsRaw, out var ts))
                ts = DateTime.Now;

            posts.Add(new PostItem { Id = Guid.NewGuid(), Timestamp = ts, Platform = platform, Text = text });
        }

        return posts;
    }

    public static void WritePosts(string path, List<PostItem> posts)
    {
        using var sw = new StreamWriter(path, false, Encoding.UTF8);
        sw.WriteLine("timestamp,platform,text");
        foreach (var p in posts.OrderBy(x => x.Timestamp))
        {
            sw.Write(Escape(p.Timestamp.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)));
            sw.Write(",");
            sw.Write(Escape(p.Platform));
            sw.Write(",");
            sw.WriteLine(Escape(p.Text));
        }
    }

    public static void WritePredictions(string path, List<PredictionItem> preds)
    {
        using var sw = new StreamWriter(path, false, Encoding.UTF8);
        sw.WriteLine("timestamp,platform,sentiment,score,confidence,text");
        foreach (var p in preds.OrderBy(x => x.Timestamp))
        {
            sw.Write(Escape(p.Timestamp.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)));
            sw.Write(",");
            sw.Write(Escape(p.Platform));
            sw.Write(",");
            sw.Write(Escape(p.Sentiment));
            sw.Write(",");
            sw.Write(p.Score.ToString("0.####", CultureInfo.InvariantCulture));
            sw.Write(",");
            sw.Write(p.Confidence.ToString("0.####", CultureInfo.InvariantCulture));
            sw.Write(",");
            sw.WriteLine(Escape(p.Text));
        }
    }

    private static bool TryParseTimestamp(string s, out DateTime ts)
    {
        var styles = DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeLocal;
        if (DateTime.TryParse(s, CultureInfo.InvariantCulture, styles, out ts))
            return true;

        if (DateTime.TryParse(s, CultureInfo.GetCultureInfo("en-US"), styles, out ts))
            return true;

        return false;
    }

    private static List<string> ParseLine(string line)
    {
        var result = new List<string>();
        var sb = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    sb.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(sb.ToString());
                sb.Clear();
            }
            else
            {
                sb.Append(c);
            }
        }

        result.Add(sb.ToString());
        return result;
    }

    private static string Escape(string? s)
    {
        s ??= "";
        var needs = s.Contains(',') || s.Contains('"') || s.Contains('\n') || s.Contains('\r');
        if (!needs) return s;
        return "\"" + s.Replace("\"", "\"\"") + "\"";
    }
}
