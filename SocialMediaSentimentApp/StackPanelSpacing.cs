using System;
using System.Windows;
using System.Windows.Controls;

namespace SocialMediaSentimentApp;

/// <summary>
/// Adds consistent spacing between StackPanel children in WPF.
/// Usage: local:StackPanelSpacing.Spacing="10"
/// </summary>
public static class StackPanelSpacing
{
    public static readonly DependencyProperty SpacingProperty =
        DependencyProperty.RegisterAttached(
            "Spacing",
            typeof(double),
            typeof(StackPanelSpacing),
            new PropertyMetadata(0d, OnSpacingChanged));

    private static readonly DependencyProperty OriginalMarginProperty =
        DependencyProperty.RegisterAttached(
            "OriginalMargin",
            typeof(Thickness),
            typeof(StackPanelSpacing),
            new PropertyMetadata(new Thickness(0)));

    public static void SetSpacing(DependencyObject element, double value) =>
        element.SetValue(SpacingProperty, value);

    public static double GetSpacing(DependencyObject element) =>
        (double)element.GetValue(SpacingProperty);

    private static Thickness GetOriginalMargin(DependencyObject element) =>
        (Thickness)element.GetValue(OriginalMarginProperty);

    private static void SetOriginalMargin(DependencyObject element, Thickness value) =>
        element.SetValue(OriginalMarginProperty, value);

    private static void OnSpacingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not StackPanel panel) return;

        panel.Loaded -= Panel_Loaded;
        panel.Loaded += Panel_Loaded;

        panel.LayoutUpdated -= Panel_LayoutUpdated;
        panel.LayoutUpdated += Panel_LayoutUpdated;

        Apply(panel);
    }

    private static void Panel_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is StackPanel panel) Apply(panel);
    }

    private static void Panel_LayoutUpdated(object? sender, EventArgs e)
    {
        if (sender is StackPanel panel) Apply(panel);
    }

    private static void Apply(StackPanel panel)
    {
        var spacing = GetSpacing(panel);
        if (spacing <= 0) return;

        bool horizontal = panel.Orientation == Orientation.Horizontal;

        for (int i = 0; i < panel.Children.Count; i++)
        {
            if (panel.Children[i] is not FrameworkElement fe) continue;

            // Store original margin once, so spacing doesn't accumulate.
            if (fe.ReadLocalValue(OriginalMarginProperty) == DependencyProperty.UnsetValue)
                SetOriginalMargin(fe, fe.Margin);

            var baseM = GetOriginalMargin(fe);

            fe.Margin = horizontal
                ? new Thickness(baseM.Left + (i == 0 ? 0 : spacing), baseM.Top, baseM.Right, baseM.Bottom)
                : new Thickness(baseM.Left, baseM.Top + (i == 0 ? 0 : spacing), baseM.Right, baseM.Bottom);
        }
    }
}
