using System;
using System.ComponentModel;

namespace SocialMediaSentimentApp;

public sealed class PostItem : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public Guid Id { get; set; }

    private DateTime _timestamp;
    public DateTime Timestamp
    {
        get => _timestamp;
        set { _timestamp = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Timestamp))); }
    }

    private string _platform = "Other";
    public string Platform
    {
        get => _platform;
        set { _platform = value ?? "Other"; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Platform))); }
    }

    private string _text = "";
    public string Text
    {
        get => _text;
        set { _text = value ?? ""; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text))); }
    }
}

public sealed class PredictionItem
{
    public DateTime Timestamp { get; init; }
    public string Platform { get; init; } = "";
    public string Text { get; init; } = "";
    public string CleanText { get; init; } = "";
    public string Sentiment { get; init; } = "neutral"; // negative/neutral/positive
    public double Score { get; init; }
    public double Confidence { get; init; }
}
