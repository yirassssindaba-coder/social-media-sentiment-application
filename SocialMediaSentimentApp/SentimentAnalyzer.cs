using System;
using System.Collections.Generic;
using System.Linq;

namespace SocialMediaSentimentApp;

public sealed class SentimentAnalyzer
{
    private static readonly HashSet<string> Pos = new(StringComparer.OrdinalIgnoreCase)
    {
        // ID
        "bagus","mantap","keren","suka","love","senang","puas","cepat","mudah","helpful","terbaik","oke","ok","lancar",
        "ramah","solutif","rekomendasi","berhasil","fixed","aman","stabil","worth","recommended","membantu","membantuin",
        // EN
        "good","great","awesome","amazing","love","liked","happy","fast","easy","helpful","best","smooth","works","fixed",
        "stable","perfect","nice","excellent","thanks","thank"
    };

    private static readonly HashSet<string> Neg = new(StringComparer.OrdinalIgnoreCase)
    {
        // ID
        "buruk","jelek","parah","lemot","lambat","error","bug","gagal","kecewa","marah","ribet","susah","payah",
        "down","crash","hang","ngadat","bohong","penipuan","mahal","delay","batal","refund","spam","rusak",
        // EN
        "bad","terrible","awful","slow","error","bug","failed","disappointed","angry","hard","confusing","worse",
        "down","crash","broken","hate","scam","expensive"
    };

    private static readonly HashSet<string> Negations = new(StringComparer.OrdinalIgnoreCase)
    {
        "not","no","never","dont","don't","cant","can't","tidak","bukan","gak","ga","nggak","tak","jangan"
    };

    private static readonly HashSet<string> Intensifiers = new(StringComparer.OrdinalIgnoreCase)
    {
        "banget","bgt","sangat","parah","super","sekali","really","very","so","extremely"
    };

    public PredictionItem Predict(PostItem p)
    {
        var clean = TextProcessing.Clean(p.Text);
        var tokens = TextProcessing.Tokenize(clean);

        var score = ScoreTokens(tokens, p.Text);
        var sentiment = score switch
        {
            <= -0.20 => "negative",
            >= 0.20 => "positive",
            _ => "neutral"
        };

        var confidence = Math.Min(1.0, Math.Abs(score) * 1.3 + 0.2);
        confidence = Math.Max(0.35, confidence);

        return new PredictionItem
        {
            Timestamp = p.Timestamp,
            Platform = p.Platform.Trim(),
            Text = p.Text,
            CleanText = clean,
            Sentiment = sentiment,
            Score = score,
            Confidence = confidence
        };
    }

    private static double ScoreTokens(List<string> tokens, string rawText)
    {
        if (tokens.Count == 0) return 0.0;

        double s = 0.0;

        for (int i = 0; i < tokens.Count; i++)
        {
            var t = tokens[i];

            int val = 0;
            if (Pos.Contains(t)) val = 1;
            else if (Neg.Contains(t)) val = -1;

            if (val != 0)
            {
                for (int j = Math.Max(0, i - 2); j < i; j++)
                {
                    if (Negations.Contains(tokens[j]))
                    {
                        val *= -1;
                        break;
                    }
                }

                if (i > 0 && Intensifiers.Contains(tokens[i - 1]))
                    val *= 2;

                s += val;
            }
        }

        var exclam = rawText.Count(c => c == '!');
        if (exclam >= 2) s *= 1.15;

        var norm = Math.Max(4.0, Math.Sqrt(tokens.Count));
        var score = s / norm;

        if (score > 1) score = 1;
        if (score < -1) score = -1;
        return score;
    }
}
