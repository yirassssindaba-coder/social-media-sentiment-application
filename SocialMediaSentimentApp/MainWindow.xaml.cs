using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace SocialMediaSentimentApp;

public partial class MainWindow : Window
{
    private readonly ObservableCollection<PostItem> _posts = new();
    private readonly ObservableCollection<PredictionItem> _predictions = new();

    private readonly StorageService _storage;
    private readonly SentimentAnalyzer _analyzer = new();

    private ReportSummary? _latestSummary;
    private string? _latestReportPath;

    private Guid? _editingId = null;

    public MainWindow()
    {
        InitializeComponent();

        _storage = new StorageService();
        LoadSettingsToUI();

        FilterSentiment.SelectedIndex = 0;
        InputPlatform.SelectedIndex = 0;

        LoadPostsFromDisk();
        RefreshPostsGrid();
        RefreshStatus($"Loaded {_posts.Count} posts.");

        // Default time
        InputDate.SelectedDate = DateTime.Today;
        InputTime.Text = "12:00";
    }

    // ---------- POSTS CRUD ----------
    private void LoadPostsFromDisk()
    {
        _posts.Clear();
        foreach (var p in _storage.LoadPosts())
            _posts.Add(p);
    }

    private void RefreshPostsGrid()
    {
        GridPosts.ItemsSource = _posts;
    }

    private void GridPosts_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (GridPosts.SelectedItem is not PostItem p) return;

        _editingId = p.Id;

        InputDate.SelectedDate = p.Timestamp.Date;
        InputTime.Text = p.Timestamp.ToString("HH:mm", CultureInfo.InvariantCulture);

        SelectPlatform(p.Platform);

        InputText.Text = p.Text;
        TxtStatus.Text = $"Editing: {p.Id}";
    }

    private void BtnNew_Click(object sender, RoutedEventArgs e)
    {
        _editingId = null;
        GridPosts.SelectedItem = null;
        InputDate.SelectedDate = DateTime.Today;
        InputTime.Text = "12:00";
        InputPlatform.SelectedIndex = 0;
        InputText.Text = "";
        TxtStatus.Text = "New post";
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        var ts = BuildTimestampFromInputs();
        if (ts is null) return;

        var platform = GetSelectedPlatform();
        var text = (InputText.Text ?? "").Trim();

        if (string.IsNullOrWhiteSpace(platform))
        {
            RefreshStatus("Platform required.");
            return;
        }
        if (string.IsNullOrWhiteSpace(text))
        {
            RefreshStatus("Text required.");
            return;
        }

        if (_editingId is null)
        {
            var item = new PostItem
            {
                Id = Guid.NewGuid(),
                Timestamp = ts.Value,
                Platform = platform,
                Text = text
            };
            _posts.Insert(0, item);
            _storage.SavePosts(_posts.ToList());
            RefreshStatus("Saved new post.");
        }
        else
        {
            var idx = _posts.ToList().FindIndex(x => x.Id == _editingId.Value);
            if (idx >= 0)
            {
                _posts[idx].Timestamp = ts.Value;
                _posts[idx].Platform = platform;
                _posts[idx].Text = text;
                _storage.SavePosts(_posts.ToList());
                RefreshStatus("Updated post.");
            }
        }
        // Refresh view
        GridPosts.Items.Refresh();
    }

    private void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        if (GridPosts.SelectedItem is not PostItem p)
        {
            RefreshStatus("Select a post to delete.");
            return;
        }

        if (MessageBox.Show("Delete selected post?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            return;

        _posts.Remove(p);
        _storage.SavePosts(_posts.ToList());
        BtnNew_Click(sender, e);
        RefreshStatus("Deleted.");
    }

    private void BtnImportCsv_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Filter = "CSV Files (*.csv)|*.csv|All files (*.*)|*.*",
            Title = "Import posts from CSV"
        };
        if (dlg.ShowDialog() != true) return;

        try
        {
            var imported = CsvService.ReadPosts(dlg.FileName);
            foreach (var p in imported.OrderByDescending(x => x.Timestamp))
                _posts.Insert(0, p);

            _storage.SavePosts(_posts.ToList());
            GridPosts.Items.Refresh();
            RefreshStatus($"Imported {imported.Count} posts.");
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Import error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnExportCsv_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new SaveFileDialog
        {
            Filter = "CSV Files (*.csv)|*.csv",
            Title = "Export posts to CSV",
            FileName = "posts_export.csv"
        };
        if (dlg.ShowDialog() != true) return;

        try
        {
            CsvService.WritePosts(dlg.FileName, _posts.ToList());
            RefreshStatus($"Exported to {dlg.FileName}");
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Export error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnLoadDemo_Click(object sender, RoutedEventArgs e)
    {
        var demo = DemoData.BuildDemoPosts();
        _posts.Clear();
        foreach (var p in demo.OrderByDescending(x => x.Timestamp))
            _posts.Add(p);

        _storage.SavePosts(_posts.ToList());
        GridPosts.Items.Refresh();
        RefreshStatus("Loaded demo dataset.");
    }

    // ---------- ANALYSIS ----------
    private void BtnRunAnalysis_Click(object sender, RoutedEventArgs e)
    {
        if (_posts.Count == 0)
        {
            RefreshStatus("No posts. Add or import CSV first.");
            return;
        }

        var settings = _storage.LoadSettings();
        var preds = _posts.Select(p => _analyzer.Predict(p)).ToList();

        _predictions.Clear();
        foreach (var pr in preds.OrderByDescending(x => x.Timestamp))
            _predictions.Add(pr);

        var summary = Aggregation.BuildSummary(preds, settings.Brand, settings.Scope);
        _latestSummary = summary;

        // Save outputs to app data
        var outputsDir = _storage.OutputsDir;
        Directory.CreateDirectory(outputsDir);

        CsvService.WritePredictions(Path.Combine(outputsDir, "predictions.csv"), preds);
        JsonUtil.WriteJson(Path.Combine(outputsDir, "summary.json"), summary);

        // Charts to reports/assets
        var assetsDir = _storage.AssetsDir;
        Directory.CreateDirectory(assetsDir);

        var overallPng = Path.Combine(assetsDir, "sentiment_overall.png");
        var platformPng = Path.Combine(assetsDir, "sentiment_by_platform.png");
        var trendPng = Path.Combine(assetsDir, "negative_share_trend.png");

        Charting.DrawBarChart(
            "Overall Sentiment Distribution",
            new[] { "negative", "neutral", "positive" },
            new[] { summary.Overall.Counts.Negative, summary.Overall.Counts.Neutral, summary.Overall.Counts.Positive },
            overallPng);

        Charting.DrawGroupedBarChart(
            "Sentiment by Platform (Counts)",
            summary.ByPlatform.OrderByDescending(x => x.Total).Select(x => x.Platform).ToList(),
            new Dictionary<string, List<int>>
            {
                ["negative"] = summary.ByPlatform.OrderByDescending(x => x.Total).Select(x => x.Counts.Negative).ToList(),
                ["neutral"]  = summary.ByPlatform.OrderByDescending(x => x.Total).Select(x => x.Counts.Neutral).ToList(),
                ["positive"] = summary.ByPlatform.OrderByDescending(x => x.Total).Select(x => x.Counts.Positive).ToList(),
            },
            platformPng);

        Charting.DrawLineChart(
            "Negative Share Trend (Daily)",
            summary.DailyTrend.Select(d => d.Date).ToList(),
            summary.DailyTrend.Select(d => d.NegativeShare).ToList(),
            "Negative share (%)",
            trendPng);

        // KPIs
        KpiTotal.Text = summary.Overall.Total.ToString(CultureInfo.InvariantCulture);
        KpiNeg.Text = $"{summary.Overall.NegativeShare:0.0}%";
        KpiNeu.Text = $"{summary.Overall.NeutralShare:0.0}%";
        KpiPos.Text = $"{summary.Overall.PositiveShare:0.0}%";
        KpiNegCount.Text = $"{summary.Overall.Counts.Negative} posts";
        KpiNeuCount.Text = $"{summary.Overall.Counts.Neutral} posts";
        KpiPosCount.Text = $"{summary.Overall.Counts.Positive} posts";

        // Keywords display
        var overallKw = summary.Keywords.FirstOrDefault(k => k.Sentiment == "overall")?.TopKeywords ?? new List<string>();
        TxtKeywords.Text = overallKw.Count == 0 ? "(No keywords)" : string.Join(", ", overallKw);

        // Images
        ImgOverall.Source = LoadBitmap(overallPng);
        ImgPlatform.Source = LoadBitmap(platformPng);
        ImgTrend.Source = LoadBitmap(trendPng);

        // Predictions grid
        ApplyPredictionFilter();

        RefreshStatus("Analysis completed. Outputs saved to data folder.");
    }

    private void ApplyPredictionFilter()
    {
        GridPredictions.ItemsSource = null;
        var filter = GetSelectedFilterSentiment();
        if (filter == "all")
        {
            GridPredictions.ItemsSource = _predictions;
            return;
        }
        GridPredictions.ItemsSource = new ObservableCollection<PredictionItem>(_predictions.Where(p => p.Sentiment == filter));
    }

    private void FilterSentiment_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ApplyPredictionFilter();
    }

    private void BtnGenerateReport_Click(object sender, RoutedEventArgs e)
    {
        if (_latestSummary is null)
        {
            RefreshStatus("Run analysis first.");
            return;
        }

        var reportsDir = _storage.ReportsDir;
        Directory.CreateDirectory(reportsDir);

        var htmlPath = Path.Combine(reportsDir, "social-media-sentiment-analysis-results.html");
        var mdPath = Path.Combine(reportsDir, "social-media-sentiment-analysis-results.md");

        var generator = new ReportGenerator(_storage.AssetsDir);
        File.WriteAllText(htmlPath, generator.BuildHtml(_latestSummary), System.Text.Encoding.UTF8);
        File.WriteAllText(mdPath, generator.BuildMarkdown(_latestSummary), System.Text.Encoding.UTF8);

        _latestReportPath = htmlPath;

        // Preview (Browser can show local file)
        try
        {
            BrowserReport.Navigate(new Uri(htmlPath));
        }
        catch
        {
            // fallback to navigate to string
            BrowserReport.NavigateToString(generator.BuildHtml(_latestSummary));
        }

        RefreshStatus("Report generated and previewed.");
    }

    private void BtnOpenReport_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_latestReportPath) || !File.Exists(_latestReportPath))
        {
            var reportsDir = _storage.ReportsDir;
            var path = Path.Combine(reportsDir, "social-media-sentiment-analysis-results.html");
            if (!File.Exists(path))
            {
                RefreshStatus("Report not found. Generate report first.");
                return;
            }
            _latestReportPath = path;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = _latestReportPath!,
            UseShellExecute = true
        });
    }

    // ---------- SETTINGS ----------
    private void LoadSettingsToUI()
    {
        var s = _storage.LoadSettings();
        InputBrand.Text = s.Brand;
        InputScopeText.Text = s.Scope;
    }

    private void BtnSaveSettings_Click(object sender, RoutedEventArgs e)
    {
        var brand = (InputBrand.Text ?? "").Trim();
        var scope = (InputScopeText.Text ?? "").Trim();

        if (string.IsNullOrWhiteSpace(brand)) brand = "Demo Brand";
        if (string.IsNullOrWhiteSpace(scope)) scope = "Offline app | Multi-platform | ID+EN | 3-class sentiment";

        _storage.SaveSettings(new AppSettings { Brand = brand, Scope = scope });
        TxtSettingsStatus.Text = "Saved.";
        RefreshStatus("Settings saved.");
    }

    private void BtnOpenDataFolder_Click(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = _storage.BaseDir,
            UseShellExecute = true
        });
    }

    // ---------- Helpers ----------
    private DateTime? BuildTimestampFromInputs()
    {
        if (InputDate.SelectedDate is not DateTime d)
        {
            RefreshStatus("Date required.");
            return null;
        }

        var tRaw = (InputTime.Text ?? "").Trim();
        if (!TimeSpan.TryParseExact(tRaw, "hh\\:mm", CultureInfo.InvariantCulture, out var ts))
        {
            RefreshStatus("Time must be HH:mm (e.g., 09:30).");
            return null;
        }

        return new DateTime(d.Year, d.Month, d.Day, ts.Hours, ts.Minutes, 0);
    }

    private string GetSelectedPlatform()
    {
        if (InputPlatform.SelectedItem is ComboBoxItem item)
            return (item.Content?.ToString() ?? "Other").Trim();
        return "Other";
    }

    private void SelectPlatform(string platform)
    {
        platform = (platform ?? "").Trim();
        foreach (ComboBoxItem it in InputPlatform.Items)
        {
            if (string.Equals(it.Content?.ToString(), platform, StringComparison.OrdinalIgnoreCase))
            {
                InputPlatform.SelectedItem = it;
                return;
            }
        }
        InputPlatform.SelectedIndex = 0;
    }

    private string GetSelectedFilterSentiment()
    {
        if (FilterSentiment.SelectedItem is ComboBoxItem item)
            return (item.Content?.ToString() ?? "all").Trim().ToLowerInvariant();
        return "all";
    }

    private void RefreshStatus(string msg)
    {
        TxtStatus.Text = msg;
    }

    private static BitmapImage? LoadBitmap(string path)
    {
        if (!File.Exists(path)) return null;

        var bmp = new BitmapImage();
        bmp.BeginInit();
        bmp.CacheOption = BitmapCacheOption.OnLoad;
        bmp.UriSource = new Uri(path);
        bmp.EndInit();
        bmp.Freeze();
        return bmp;
    }
}
