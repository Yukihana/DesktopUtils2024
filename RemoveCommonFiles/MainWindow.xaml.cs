using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace RemoveCommonFiles
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                // Check location validity
                if (!Directory.Exists(Dir1.Text) || !Directory.Exists(Dir2.Text))
                {
                    return;
                }

                // Enumerate files
                List<string> Files1 = new List<string>(Directory.GetFiles(Dir1.Text, "*", SearchOption.AllDirectories));
                List<string> Files2 = new List<string>(Directory.GetFiles(Dir2.Text, "*", SearchOption.AllDirectories));

                // Sanitize directory paths for comparision
                var d1 = Path.GetFullPath(Dir1.Text);
                var d2 = Path.GetFullPath(Dir2.Text);

                // Localize absolute file paths
                for (int i = 0; i < Files1.Count; i++)
                {
                    Files1[i] = Files1[i].Substring(d1.Length);
                }
                for (int i = 0; i < Files2.Count; i++)
                {
                    Files2[i] = Files2[i].Substring(d2.Length);
                }

                // List Common Files
                List<string> common = new List<string>();
                foreach (string filepath in Files1)
                {
                    if (Files2.Contains(filepath))
                    {
                        common.Add(filepath);
                    }
                }

                int n = 0;
                // Remove from Dir1
                if (Remove1.IsChecked == true)
                {
                    foreach (string filepath in common)
                    {
                        var fullpath = Path.Combine(d1, filepath);
                        LogPanel.Children.Add(new TextBlock() { Text = $"Removing {fullpath}" });

                        if(Simulate.IsChecked == false)
                        {
                            File.Delete(fullpath);
                            n++;
                        }
                    }
                }

                // Remove from Dir2
                if (Remove2.IsChecked == true)
                {
                    foreach (string filepath in common)
                    {
                        var fullpath = Path.Combine(d2, filepath);
                        LogPanel.Children.Add(new TextBlock() { Text = $"Removing {fullpath}" });

                        if (Simulate.IsChecked == false)
                        {
                            File.Delete(fullpath);
                            n++;
                        }
                    }
                }

                LogPanel.Children.Add(new TextBlock() { Text = $"Deleted {n} out of {Files1.Count + Files2.Count} duplicates." });
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
            }
    }
    }
}
