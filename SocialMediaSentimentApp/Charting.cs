using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SocialMediaSentimentApp;

/// <summary>
/// Minimal chart rendering using WPF primitives (no System.Drawing dependency).
/// Produces PNG files used by the in-app previews and HTML report.
/// </summary>
public static class Charting
{
    public static void DrawBarChart(string title, string[] categories, int[] values, string filePath)
    {
        const int width = 1100;
        const int height = 650;
        const double margin = 90;
        const double top = 110;
        const double bottom = 140;

        int n = Math.Min(categories.Length, values.Length);
        if (n <= 0)
        {
            DrawEmpty(title, "No data", width, height, filePath);
            return;
        }

        var canvas = BaseCanvas(width, height);
        AddTitle(canvas, title, margin, 30);

        var plotLeft = margin;
        var plotTop = top;
        var plotRight = width - margin;
        var plotBottom = height - bottom;

        AddAxis(canvas, plotLeft, plotTop, plotRight, plotBottom);

        int maxVal = Math.Max(1, values.Take(n).Max());

        // Gridlines + Y labels
        for (int t = 0; t <= 5; t++)
        {
            double frac = t / 5.0;
            int yVal = (int)Math.Round(maxVal * (1 - frac));
            double y = plotTop + (plotBottom - plotTop) * frac;

            AddLine(canvas, plotLeft, y, plotRight, y, Brushes.Gainsboro, 1);
            AddText(canvas, yVal.ToString(), 11, plotLeft - 10, y, alignRight: true, centerY: true, muted: false);
        }

        double barWidth = (plotRight - plotLeft) / (n * 1.5);

        for (int i = 0; i < n; i++)
        {
            double x = plotLeft + (i * 1.5 + 0.25) * barWidth;
            double barH = (plotBottom - plotTop) * (values[i] / (double)maxVal);
            double y = plotBottom - barH;

            var rect = new Rectangle
            {
                Width = barWidth,
                Height = Math.Max(0, barH),
                Fill = PaletteBrush(i),
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, y);
            canvas.Children.Add(rect);

            // Value label
            AddText(canvas, values[i].ToString(), 11, x + barWidth / 2, y - 6, centerX: true, alignRight: false, centerY: false, muted: false, above: true);
            // Category label
            AddText(canvas, categories[i], 12, x + barWidth / 2, plotBottom + 18, centerX: true, muted: false);
        }

        RenderToPng(canvas, width, height, filePath);
    }

    public static void DrawGroupedBarChart(string title, List<string> groups, Dictionary<string, List<int>> series, string filePath)
    {
        const int width = 1300;
        const int height = 720;
        const double margin = 90;
        const double top = 110;
        const double bottom = 160;

        if (groups.Count == 0 || series.Count == 0)
        {
            DrawEmpty(title, "No data", width, height, filePath);
            return;
        }

        var keys = series.Keys.ToList();
        int groupCount = groups.Count;
        int seriesCount = keys.Count;

        // Ensure all series have enough points
        foreach (var k in keys)
        {
            if (series[k].Count < groupCount)
            {
                while (series[k].Count < groupCount) series[k].Add(0);
            }
        }

        int maxVal = 1;
        foreach (var k in keys)
            maxVal = Math.Max(maxVal, series[k].Take(groupCount).Max());

        var canvas = BaseCanvas(width, height);
        AddTitle(canvas, title, margin, 30);

        var plotLeft = margin;
        var plotTop = top;
        var plotRight = width - margin;
        var plotBottom = height - bottom;

        AddAxis(canvas, plotLeft, plotTop, plotRight, plotBottom);

        // Gridlines + Y labels
        for (int t = 0; t <= 5; t++)
        {
            double frac = t / 5.0;
            int yVal = (int)Math.Round(maxVal * (1 - frac));
            double y = plotTop + (plotBottom - plotTop) * frac;

            AddLine(canvas, plotLeft, y, plotRight, y, Brushes.Gainsboro, 1);
            AddText(canvas, yVal.ToString(), 10, plotLeft - 10, y, alignRight: true, centerY: true, muted: false);
        }

        double groupWidth = (plotRight - plotLeft) / Math.Max(1, groupCount);
        double barWidth = groupWidth / (seriesCount + 1);

        for (int gi = 0; gi < groupCount; gi++)
        {
            double gx = plotLeft + gi * groupWidth;
            for (int si = 0; si < seriesCount; si++)
            {
                string k = keys[si];
                int v = series[k][gi];
                double barH = (plotBottom - plotTop) * (v / (double)maxVal);
                double x = gx + (si + 0.5) * barWidth;
                double y = plotBottom - barH;

                var rect = new Rectangle
                {
                    Width = barWidth,
                    Height = Math.Max(0, barH),
                    Fill = PaletteBrush(si),
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };
                Canvas.SetLeft(rect, x);
                Canvas.SetTop(rect, y);
                canvas.Children.Add(rect);
            }

            // Group label
            AddText(canvas, groups[gi], 10, gx + groupWidth / 2, plotBottom + 18, centerX: true, muted: false);
        }

        // Legend
        double lx = plotLeft;
        double ly = plotBottom + 60;
        for (int i = 0; i < keys.Count; i++)
        {
            var swatch = new Rectangle { Width = 18, Height = 18, Fill = PaletteBrush(i), Stroke = Brushes.Black, StrokeThickness = 1 };
            Canvas.SetLeft(swatch, lx);
            Canvas.SetTop(swatch, ly + i * 24);
            canvas.Children.Add(swatch);

            AddText(canvas, keys[i], 11, lx + 28, ly + i * 24 + 2, muted: false);
        }

        RenderToPng(canvas, width, height, filePath);
    }

    public static void DrawLineChart(string title, List<string> xLabels, List<double> values, string yLabel, string filePath)
    {
        const int width = 1200;
        const int height = 650;
        const double margin = 90;
        const double top = 110;
        const double bottom = 140;

        var canvas = BaseCanvas(width, height);
        AddTitle(canvas, title, margin, 30);

        var plotLeft = margin;
        var plotTop = top;
        var plotRight = width - margin;
        var plotBottom = height - bottom;

        AddAxis(canvas, plotLeft, plotTop, plotRight, plotBottom);

        if (values.Count == 0)
        {
            AddText(canvas, "No data", 12, plotLeft, plotTop + 20, muted: false);
            RenderToPng(canvas, width, height, filePath);
            return;
        }

        double maxVal = Math.Max(10.0, values.Max());

        for (int t = 0; t <= 5; t++)
        {
            double frac = t / 5.0;
            double yV = maxVal * (1 - frac);
            double y = plotTop + (plotBottom - plotTop) * frac;

            AddLine(canvas, plotLeft, y, plotRight, y, Brushes.Gainsboro, 1);
            AddText(canvas, $"{yV:0.#}", 10, plotLeft - 10, y, alignRight: true, centerY: true, muted: false);
        }

        AddText(canvas, yLabel, 11, plotLeft, plotBottom + 80, muted: false);

        if (values.Count == 1)
        {
            string lbl = xLabels.Count > 0 ? xLabels[0] : "day1";
            AddText(canvas, $"{lbl}: {values[0]:0.0}%", 12, plotLeft, plotTop + 20, muted: false);
            RenderToPng(canvas, width, height, filePath);
            return;
        }

        int count = values.Count;
        var poly = new Polyline
        {
            Stroke = new SolidColorBrush(Color.FromRgb(30, 90, 200)),
            StrokeThickness = 3,
            StrokeLineJoin = PenLineJoin.Round
        };

        for (int i = 0; i < count; i++)
        {
            double x = plotLeft + (plotRight - plotLeft) * (i / (double)(count - 1));
            double y = plotBottom - (plotBottom - plotTop) * (values[i] / maxVal);
            poly.Points.Add(new Point(x, y));

            var dot = new Ellipse { Width = 6, Height = 6, Fill = Brushes.Black };
            Canvas.SetLeft(dot, x - 3);
            Canvas.SetTop(dot, y - 3);
            canvas.Children.Add(dot);
        }
        canvas.Children.Add(poly);

        int step = Math.Max(1, count / 8);
        for (int i = 0; i < Math.Min(xLabels.Count, count); i += step)
        {
            string lbl = xLabels[i];
            double x = plotLeft + (plotRight - plotLeft) * (i / (double)(count - 1));
            AddText(canvas, lbl, 10, x, plotBottom + 18, centerX: true, muted: false);
        }

        RenderToPng(canvas, width, height, filePath);
    }

    // ----------------- helpers -----------------

    private static Canvas BaseCanvas(int width, int height)
    {
        return new Canvas
        {
            Width = width,
            Height = height,
            Background = Brushes.White,
            SnapsToDevicePixels = true,
            UseLayoutRounding = true
        };
    }

    private static void AddTitle(Canvas canvas, string title, double x, double y)
    {
        AddText(canvas, title, 18, x, y, bold: true, muted: false);
    }

    private static void AddAxis(Canvas canvas, double left, double top, double right, double bottom)
    {
        AddLine(canvas, left, bottom, right, bottom, Brushes.Black, 1.5);
        AddLine(canvas, left, top, left, bottom, Brushes.Black, 1.5);
    }

    private static void AddLine(Canvas canvas, double x1, double y1, double x2, double y2, Brush stroke, double thickness)
    {
        var line = new Line
        {
            X1 = x1,
            Y1 = y1,
            X2 = x2,
            Y2 = y2,
            Stroke = stroke,
            StrokeThickness = thickness,
            SnapsToDevicePixels = true
        };
        canvas.Children.Add(line);
    }

    private static void AddText(
        Canvas canvas,
        string text,
        double fontSize,
        double x,
        double y,
        bool centerX = false,
        bool alignRight = false,
        bool centerY = false,
        bool bold = false,
        bool muted = true,
        bool above = false)
    {
        var tb = new TextBlock
        {
            Text = text,
            FontFamily = new FontFamily("Segoe UI"),
            FontSize = fontSize,
            FontWeight = bold ? FontWeights.SemiBold : FontWeights.Normal,
            Foreground = muted ? new SolidColorBrush(Color.FromRgb(85, 85, 85)) : Brushes.Black
        };

        // Force measure so we can align
        tb.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        double w = tb.DesiredSize.Width;
        double h = tb.DesiredSize.Height;

        double left = x;
        if (centerX) left = x - w / 2;
        else if (alignRight) left = x - w;

        double top = y;
        if (centerY) top = y - h / 2;
        if (above) top = y - h; // used for value labels above bars

        Canvas.SetLeft(tb, left);
        Canvas.SetTop(tb, top);
        canvas.Children.Add(tb);
    }

    private static Brush PaletteBrush(int i)
    {
        // Similar palette to previous implementation
        return i switch
        {
            0 => new SolidColorBrush(Color.FromRgb(220, 50, 47)),
            1 => new SolidColorBrush(Color.FromRgb(133, 153, 0)),
            2 => new SolidColorBrush(Color.FromRgb(38, 139, 210)),
            3 => new SolidColorBrush(Color.FromRgb(203, 75, 22)),
            _ => new SolidColorBrush(Color.FromRgb(88, 110, 117))
        };
    }

    private static void DrawEmpty(string title, string message, int width, int height, string filePath)
    {
        var canvas = BaseCanvas(width, height);
        AddTitle(canvas, title, 60, 30);
        AddText(canvas, message, 14, 60, 120, muted: false);
        RenderToPng(canvas, width, height, filePath);
    }

    private static void RenderToPng(FrameworkElement element, int width, int height, string filePath)
    {
        element.Measure(new Size(width, height));
        element.Arrange(new Rect(0, 0, width, height));
        element.UpdateLayout();

        var rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        rtb.Render(element);

        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filePath) ?? ".");
        using var fs = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        var enc = new PngBitmapEncoder();
        enc.Frames.Add(BitmapFrame.Create(rtb));
        enc.Save(fs);
    }
}
