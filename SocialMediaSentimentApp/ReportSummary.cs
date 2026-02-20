using System;
using System.Collections.Generic;

namespace SocialMediaSentimentApp;

public sealed class SentimentCounts
{
    public int Negative { get; set; }
    public int Neutral { get; set; }
    public int Positive { get; set; }
}

public sealed class OverallSummary
{
    public SentimentCounts Counts { get; set; } = new();
    public int Total { get; set; }
    public double NegativeShare { get; set; }
    public double NeutralShare { get; set; }
    public double PositiveShare { get; set; }
}

public sealed class PlatformSummary
{
    public string Platform { get; set; } = "";
    public SentimentCounts Counts { get; set; } = new();
    public int Total { get; set; }
}

public sealed class DailyTrendPoint
{
    public string Date { get; set; } = "";
    public int Total { get; set; }
    public int Negative { get; set; }
    public double NegativeShare { get; set; }
}

public sealed class KeywordSummary
{
    public string Sentiment { get; set; } = "overall";
    public List<string> TopKeywords { get; set; } = new();
}

public sealed class ReportSummary
{
    public string Title { get; set; } = "social-media-sentiment-analysis-results";
    public string Brand { get; set; } = "";
    public string Scope { get; set; } = "";
    public string GeneratedAt { get; set; } = "";
    public OverallSummary Overall { get; set; } = new();
    public List<PlatformSummary> ByPlatform { get; set; } = new();
    public List<DailyTrendPoint> DailyTrend { get; set; } = new();
    public List<KeywordSummary> Keywords { get; set; } = new();
    public List<string> KeyInsights { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public List<string> Limitations { get; set; } = new();
    public List<string> Ethics { get; set; } = new();
}
