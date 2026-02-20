using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SocialMediaSentimentApp;

public static class Aggregation
{
    public static ReportSummary BuildSummary(List<PredictionItem> preds, string brand, string scope)
    {
        var summary = new ReportSummary
        {
            Brand = brand,
            Scope = scope,
            GeneratedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
        };

        var overallCounts = new SentimentCounts
        {
            Negative = preds.Count(p => p.Sentiment == "negative"),
            Neutral = preds.Count(p => p.Sentiment == "neutral"),
            Positive = preds.Count(p => p.Sentiment == "positive"),
        };
        var total = preds.Count;

        summary.Overall = new OverallSummary
        {
            Counts = overallCounts,
            Total = total,
            NegativeShare = total == 0 ? 0 : overallCounts.Negative * 100.0 / total,
            NeutralShare = total == 0 ? 0 : overallCounts.Neutral * 100.0 / total,
            PositiveShare = total == 0 ? 0 : overallCounts.Positive * 100.0 / total,
        };

        summary.ByPlatform = preds
            .GroupBy(p => NormalizePlatform(p.Platform))
            .Select(g => new PlatformSummary
            {
                Platform = g.Key,
                Total = g.Count(),
                Counts = new SentimentCounts
                {
                    Negative = g.Count(x => x.Sentiment == "negative"),
                    Neutral = g.Count(x => x.Sentiment == "neutral"),
                    Positive = g.Count(x => x.Sentiment == "positive"),
                }
            })
            .OrderByDescending(x => x.Total)
            .ToList();

        summary.DailyTrend = preds
            .GroupBy(p => p.Timestamp.Date)
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var t = g.Count();
                var neg = g.Count(x => x.Sentiment == "negative");
                return new DailyTrendPoint
                {
                    Date = g.Key.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    Total = t,
                    Negative = neg,
                    NegativeShare = t == 0 ? 0 : neg * 100.0 / t
                };
            })
            .ToList();

        summary.Keywords = new List<KeywordSummary>
        {
            new() { Sentiment = "overall", TopKeywords = TopKeywords(preds.Select(p => p.CleanText), 12) },
            new() { Sentiment = "negative", TopKeywords = TopKeywords(preds.Where(p => p.Sentiment=="negative").Select(p => p.CleanText), 12) },
            new() { Sentiment = "neutral", TopKeywords = TopKeywords(preds.Where(p => p.Sentiment=="neutral").Select(p => p.CleanText), 12) },
            new() { Sentiment = "positive", TopKeywords = TopKeywords(preds.Where(p => p.Sentiment=="positive").Select(p => p.CleanText), 12) },
        };

        var negTop = summary.Keywords.First(k => k.Sentiment == "negative").TopKeywords.Take(5).ToList();
        var posTop = summary.Keywords.First(k => k.Sentiment == "positive").TopKeywords.Take(5).ToList();

        summary.KeyInsights = new List<string>
        {
            $"Overall sentiment share: negative {summary.Overall.NegativeShare:0.0}%, neutral {summary.Overall.NeutralShare:0.0}%, positive {summary.Overall.PositiveShare:0.0}%.",
            $"Top negative drivers (keywords): {string.Join(", ", negTop)}.",
            $"Top positive drivers (keywords): {string.Join(", ", posTop)}.",
            $"Platforms with highest volume: {string.Join(", ", summary.ByPlatform.Take(3).Select(x => x.Platform))}."
        };

        summary.Recommendations = new List<string>
        {
            "Set an alert when negative share rises above baseline (e.g., +10% vs 7-day average).",
            "Create quick-response templates for top negative topics and route them to the right owner (CS, Product, Ops).",
            "Amplify what drives positive sentiment via content and community engagement.",
            "Review lexicon weekly for slang, negation patterns, and sarcasm failure cases."
        };

        summary.Limitations = new List<string>
        {
            "Rule-based sentiment may miss sarcasm/irony and context from threads or images/video.",
            "Platform sampling bias: what appears in the dataset may not represent the whole audience.",
            "Keyword-based topics are approximate; deeper topic modeling needs richer methods and more data."
        };

        summary.Ethics = new List<string>
        {
            "Use aggregated reporting; avoid exposing personal data and keep identifiers anonymized.",
            "Respect platform TOS and local regulations; collect only what is necessary.",
            "Treat sentiment as perception signals, not objective truth."
        };

        return summary;
    }

    private static string NormalizePlatform(string p)
    {
        var t = (p ?? "").Trim().ToLowerInvariant();
        if (t.Contains("twitter") || t == "x") return "X/Twitter";
        if (t.Contains("instagram")) return "Instagram";
        if (t.Contains("tiktok")) return "TikTok";
        if (t.Contains("youtube")) return "YouTube";
        if (t.Contains("reddit")) return "Reddit";
        if (string.IsNullOrWhiteSpace(t)) return "Unknown";
        return char.ToUpperInvariant(t[0]) + t.Substring(1);
    }

    private static List<string> TopKeywords(IEnumerable<string> texts, int k)
    {
        var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var text in texts)
        {
            var tokens = TextProcessing.Tokenize(text);
            foreach (var tok in tokens)
            {
                if (tok.Length < 3) continue;
                if (Stopwords.Words.Contains(tok)) continue;
                if (tok.All(char.IsDigit)) continue;

                counts.TryGetValue(tok, out var v);
                counts[tok] = v + 1;
            }
        }

        return counts
            .OrderByDescending(kv => kv.Value)
            .ThenBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase)
            .Take(k)
            .Select(kv => kv.Key)
            .ToList();
    }
}
