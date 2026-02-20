using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SocialMediaSentimentApp;

public static class TextProcessing
{
    private static readonly Regex Url = new(@"https?://\S+|www\.\S+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex Mention = new(@"@\w+", RegexOptions.Compiled);
    private static readonly Regex Hashtag = new(@"#(\w+)", RegexOptions.Compiled);
    private static readonly Regex NonWord = new(@"[^\p{L}\p{N}\s#]+", RegexOptions.Compiled);

    public static string Clean(string text)
    {
        var t = text ?? "";
        t = Url.Replace(t, " ");
        t = Mention.Replace(t, " ");
        t = Hashtag.Replace(t, " $1 ");
        t = t.Replace("\u200b", " ");
        t = NonWord.Replace(t, " ");
        t = Regex.Replace(t, @"\s+", " ").Trim().ToLowerInvariant();
        return t;
    }

    public static List<string> Tokenize(string cleanText)
    {
        if (string.IsNullOrWhiteSpace(cleanText)) return new List<string>();
        return cleanText.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
    }
}

public static class Stopwords
{
    public static readonly HashSet<string> Words = new(StringComparer.OrdinalIgnoreCase)
    {
        // Indonesian
        "yang","dan","di","ke","dari","untuk","pada","dengan","atau","ini","itu","saya","aku","kamu","dia","mereka",
        "kami","kita","bisa","jadi","udah","sudah","belum","lagi","nih","nya","kok","dong","sih","aja","gitu","gue",
        "ga","gak","nggak","tidak","bukan","ya","yg","tp","tapi","karena","kalau","kalo","dalam","sebagai","lebih",
        "banget","sangat","terlalu","pun","juga","akan","harus","kayak","seperti","bgt","pls","tolong","mohon",
        // English
        "the","a","an","and","or","to","for","of","in","on","with","is","are","was","were","be","been","it","this","that",
        "i","you","he","she","they","we","me","my","your","our","their","not","no","yes","but","so","very","really",
    };
}
