using Microsoft.Win32;
using System.IO;
using System.Windows;

namespace MPD
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
            OpenFileDialog o = new();
            if(o.ShowDialog() == true)
            {
                TextContent.Text = File.ReadAllText(o.FileName);
            };
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            SaveFileDialog o = new();
            if (o.ShowDialog() == true)
            {
                File.WriteAllText(o.FileName, TextContent.Text);
            };
        }
    }
}