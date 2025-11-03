using System;
using System.Windows;

namespace MPD.Windows;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainContextLogic _logic = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _logic;
    }

    private void ZoomIn(object sender, RoutedEventArgs e)
    {
        TextContent.FontSize = Math.Floor(TextContent.FontSize) + 1;
    }

    private void ZoomOut(object sender, RoutedEventArgs e)
    {
        TextContent.FontSize = Math.Ceiling(TextContent.FontSize) - 1;
    }

    private void ResetZoom(object sender, RoutedEventArgs e)
    {
        TextContent.FontSize = 16;
    }
}