using System;
using System.Linq;
using System.Text;

namespace SocialMediaSentimentApp;

public sealed class ReportGenerator
{
    private readonly string _assetsDir;

    public ReportGenerator(string assetsDir)
    {
        _assetsDir = assetsDir;
    }

    public string BuildHtml(ReportSummary s)
    {
        // Use file:/// absolute paths for charts (WebBrowser control)
        string ToFileUri(string fileName)
        {
            var path = System.IO.Path.Combine(_assetsDir, fileName);
            return new Uri(path).AbsoluteUri;
        }

        var sb = new StringBuilder();
        sb.AppendLine("<!doctype html>");
        sb.AppendLine("<html lang='en'><head><meta charset='utf-8'/>");
        sb.AppendLine("<meta name='viewport' content='width=device-width, initial-scale=1'/>");
        sb.AppendLine("<title>social-media-sentiment-analysis-results</title>");
        sb.AppendLine("<style>");
        sb.AppendLine("body{font-family:Segoe UI,Arial,sans-serif;margin:0;background:#fafafa;color:#111;}");
        sb.AppendLine(".wrap{max-width:980px;margin:0 auto;padding:28px;}");
        sb.AppendLine("h1{margin:0 0 8px 0;font-size:28px;}");
        sb.AppendLine("h2{margin-top:28px;font-size:18px;border-bottom:1px solid #ddd;padding-bottom:6px;}");
        sb.AppendLine(".meta{color:#555;font-size:13px;margin-bottom:18px;}");
        sb.AppendLine(".kpi{display:flex;gap:14px;flex-wrap:wrap;margin:16px 0;}");
        sb.AppendLine(".card{background:#fff;border:1px solid #e6e6e6;border-radius:12px;padding:14px;min-width:210px;}");
        sb.AppendLine(".big{font-size:26px;font-weight:700;}");
        sb.AppendLine("img{max-width:100%;border-radius:10px;border:1px solid #eee;background:#fff;}");
        sb.AppendLine("ul{margin:8px 0 0 18px;}");
        sb.AppendLine("code{background:#fff;border:1px solid #eee;padding:2px 6px;border-radius:6px;}");
        sb.AppendLine("</style></head><body><div class='wrap'>");

        sb.AppendLine("<h1>social-media-sentiment-analysis-results</h1>");
        sb.AppendLine($"<div class='meta'><div><b>Brand:</b> {Escape(s.Brand)}</div>");
        sb.AppendLine($"<div><b>Scope:</b> {Escape(s.Scope)}</div>");
        sb.AppendLine($"<div><b>Generated at:</b> {Escape(s.GeneratedAt)}</div></div>");

        sb.AppendLine("<h2>Background & Objective</h2>");
        sb.AppendLine("<p>This report summarizes public sentiment from social media text data to monitor perception, detect issues early, and translate signals into actionable insights for Product, Marketing, and Customer Support.</p>");

        sb.AppendLine("<h2>Data & Processing</h2>");
        sb.AppendLine("<p>Texts are cleaned by removing URLs and mentions, normalizing whitespace, and tokenizing. A lightweight rule-based sentiment scorer is applied using bilingual (ID+EN) lexicons, negation handling, and intensity cues.</p>");

        sb.AppendLine("<h2>Results</h2>");
        sb.AppendLine("<div class='kpi'>");
        sb.AppendLine($"<div class='card'><div>Total posts</div><div class='big'>{s.Overall.Total}</div></div>");
        sb.AppendLine($"<div class='card'><div>Negative</div><div class='big'>{s.Overall.NegativeShare:0.0}%</div><div>{s.Overall.Counts.Negative} posts</div></div>");
        sb.AppendLine($"<div class='card'><div>Neutral</div><div class='big'>{s.Overall.NeutralShare:0.0}%</div><div>{s.Overall.Counts.Neutral} posts</div></div>");
        sb.AppendLine($"<div class='card'><div>Positive</div><div class='big'>{s.Overall.PositiveShare:0.0}%</div><div>{s.Overall.Counts.Positive} posts</div></div>");
        sb.AppendLine("</div>");

        sb.AppendLine($"<p><img src='{ToFileUri("sentiment_overall.png")}' alt='Overall sentiment chart'/></p>");
        sb.AppendLine($"<p><img src='{ToFileUri("sentiment_by_platform.png")}' alt='Sentiment by platform chart'/></p>");
        sb.AppendLine($"<p><img src='{ToFileUri("negative_share_trend.png")}' alt='Negative share trend chart'/></p>");

        sb.AppendLine("<h2>Topic Signals (Keywords)</h2>");
        sb.AppendLine("<p>Keyword-based topic signals are extracted to explain what drives sentiment changes (approximate, not full topic modeling).</p>");
        foreach (var k in s.Keywords)
        {
            sb.AppendLine($"<div class='card'><b>{Escape(k.Sentiment)}</b><div style='margin-top:6px'>{Escape(string.Join(", ", k.TopKeywords))}</div></div>");
        }

        sb.AppendLine("<h2>Insights</h2><ul>");
        foreach (var i in s.KeyInsights) sb.AppendLine("<li>" + Escape(i) + "</li>");
        sb.AppendLine("</ul>");

        sb.AppendLine("<h2>Recommendations</h2><ul>");
        foreach (var r in s.Recommendations) sb.AppendLine("<li>" + Escape(r) + "</li>");
        sb.AppendLine("</ul>");

        sb.AppendLine("<h2>Limitations</h2><ul>");
        foreach (var l in s.Limitations) sb.AppendLine("<li>" + Escape(l) + "</li>");
        sb.AppendLine("</ul>");

        sb.AppendLine("<h2>Ethics & Compliance</h2><ul>");
        foreach (var e in s.Ethics) sb.AppendLine("<li>" + Escape(e) + "</li>");
        sb.AppendLine("</ul>");

        sb.AppendLine("</div></body></html>");
        return sb.ToString();
    }

    public string BuildMarkdown(ReportSummary s)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# social-media-sentiment-analysis-results");
        sb.AppendLine();
        sb.AppendLine($"Brand: **{s.Brand}**");
        sb.AppendLine();
        sb.AppendLine($"Scope: **{s.Scope}**");
        sb.AppendLine();
        sb.AppendLine($"Generated at: **{s.GeneratedAt}**");
        sb.AppendLine();
        sb.AppendLine("## Results");
        sb.AppendLine($"- Total posts: **{s.Overall.Total}**");
        sb.AppendLine($"- Negative: **{s.Overall.NegativeShare:0.0}%** ({s.Overall.Counts.Negative})");
        sb.AppendLine($"- Neutral: **{s.Overall.NeutralShare:0.0}%** ({s.Overall.Counts.Neutral})");
        sb.AppendLine($"- Positive: **{s.Overall.PositiveShare:0.0}%** ({s.Overall.Counts.Positive})");
        sb.AppendLine();
        sb.AppendLine("Charts (generated in app data folder):");
        sb.AppendLine("- sentiment_overall.png");
        sb.AppendLine("- sentiment_by_platform.png");
        sb.AppendLine("- negative_share_trend.png");
        sb.AppendLine();
        sb.AppendLine("## Topic Signals (Keywords)");
        foreach (var k in s.Keywords)
        {
            sb.AppendLine($"- **{k.Sentiment}**: {string.Join(", ", k.TopKeywords)}");
        }
        sb.AppendLine();
        sb.AppendLine("## Insights");
        foreach (var i in s.KeyInsights) sb.AppendLine($"- {i}");
        sb.AppendLine();
        sb.AppendLine("## Recommendations");
        foreach (var r in s.Recommendations) sb.AppendLine($"- {r}");
        sb.AppendLine();
        sb.AppendLine("## Limitations");
        foreach (var l in s.Limitations) sb.AppendLine($"- {l}");
        sb.AppendLine();
        sb.AppendLine("## Ethics & Compliance");
        foreach (var e in s.Ethics) sb.AppendLine($"- {e}");
        return sb.ToString();
    }

    private static string Escape(string s)
        => (s ?? "").Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
}
