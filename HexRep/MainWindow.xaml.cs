using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace HexRep;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(OrigText.Text);
        StringBuilder sb = new();
        byte n = 0;
        foreach (byte b in bytes)
        {
            sb.Append($"{b:X2} ");
            n++;

            if (n >= 8)
            {
                sb.AppendLine();
                n = 0;
            }
        }
        HexText.Text = sb.ToString();
    }
}