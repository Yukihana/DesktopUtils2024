using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace MetadataUpdater;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Analyse_Click(object sender, RoutedEventArgs e)
    {
        StringBuilder analysis = new();
        List<bool> results = new();
        foreach (string path in LineSplitterRegex().Split(Mirrors.Text))
        {
            if (string.IsNullOrEmpty(path))
                continue;

            string sourcePath = Path.Combine(path, SourcePath.Text);
            string targetPath = Path.Combine(path, TargetPath.Text);

            analysis.AppendLine("Source: " + AppendFileInfo(sourcePath, out bool sourceOk));
            analysis.AppendLine("Target: " + AppendFileInfo(targetPath, out bool targetOk));
            results.Add(sourceOk);
            results.Add(targetOk);

            analysis.AppendLine();
        }

        // Add conclusion
        analysis.AppendLine("Prediction: " + (results.Contains(false) ? "Failure" : "Success"));
        analysis.AppendLine();

        // Display
        LogAnalysis.Text = analysis.ToString();
    }

    private void Apply_Click(object sender, RoutedEventArgs e)
    {
        StringBuilder analysis = new();
        List<bool> results = new();
        foreach (string path in LineSplitterRegex().Split(Mirrors.Text))
        {
            if (string.IsNullOrEmpty(path))
                continue;

            string sourcePath = Path.Combine(path, SourcePath.Text);
            string targetPath = Path.Combine(path, TargetPath.Text);

            analysis.AppendLine("Source: " + AppendFileInfo(sourcePath, out bool sourceOk));
            analysis.AppendLine("Target: " + AppendFileInfo(targetPath, out bool targetOk));
            results.Add(sourceOk);
            results.Add(targetOk);

            if (sourceOk && targetOk)
            {
                analysis.AppendLine("Modify:");
                analysis.AppendLine(UpdateFile(sourcePath, targetPath, out bool statusOk));
                results.Add(statusOk);
            }

            analysis.AppendLine();
        }

        // Add conclusion
        analysis.AppendLine("Status: " + (results.Contains(false) ? "Partial or Total Failure" : "Success"));
        analysis.AppendLine();

        // Display
        LogApply.Text = analysis.ToString();
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        SourcePath.Clear();
        TargetPath.Clear();
    }

    [GeneratedRegex("\r\n|\r|\n")]
    private static partial Regex LineSplitterRegex();

    private static string AppendFileInfo(string path, out bool statusOk)
    {
        statusOk = false;
        string result;
        try
        {
            if (!File.Exists(path))
                throw new IOException("File dun exist yo");
            FileInfo f = new(path);
            result = f.LastWriteTimeUtc.ToString();
            statusOk = true;
        }
        catch { result = "FAIL"; }

        return result + " : " + path;
    }

    private static string UpdateFile(string source, string target, out bool statusOk)
    {
        statusOk = false;
        string result;
        try
        {
            if (!File.Exists(source))
                throw new IOException("File dun exist yo");
            if (!File.Exists(target))
                throw new IOException("File dun exist yo");

            FileInfo sInfo = new(source);
            FileInfo tInfo = new(target);

            result = tInfo.LastWriteTimeUtc.ToString() + " -> " + sInfo.LastWriteTimeUtc.ToString();

            File.SetLastWriteTimeUtc(tInfo.FullName, sInfo.LastWriteTimeUtc);
            result = "SUCCESS : " + result;
            statusOk = true;
        }
        catch (Exception e)
        {
            result = "Modify LastWriteTime failed: " + e.Message;
        }

        return result;
    }
}