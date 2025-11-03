using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.IO;

namespace MPD.Windows;

public partial class MainContextLogic : ObservableObject
{
    public RelayCommand<string> DefaultCommand { get; init; }

    [ObservableProperty]
    private string _text = string.Empty;

    [ObservableProperty]
    private string _location = string.Empty;

    public MainContextLogic()
    {
        DefaultCommand = new(Something);
    }

    public void Something(string? parameter)
    {
        if (parameter is null)
            return;
        switch (parameter.ToLowerInvariant())
        {
            case "new":
            case "open":
                Open();
                break;

            case "save_as":
                SaveAs();
                break;

            default:
                break;
        }
    }

    private OpenFileDialog Open()
    {
        OpenFileDialog o = new();
        if (o.ShowDialog() == true)
        {
            Text = File.ReadAllText(o.FileName);
        };
        return o;
    }

    private void SaveAs()
    {
        SaveFileDialog o = new();
        if (o.ShowDialog() == true)
        {
            File.WriteAllText(o.FileName, Text);
        };
    }
}