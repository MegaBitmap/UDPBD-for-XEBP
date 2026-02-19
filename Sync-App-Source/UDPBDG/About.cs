using System.Diagnostics;

namespace UDPBDG;

public partial class About : Form
{
    public About()
    {
        InitializeComponent();
    }

    private void RichTextBox_LinkClicked(object sender, LinkClickedEventArgs e)
    {
        Process.Start(new ProcessStartInfo { FileName = e.LinkText, UseShellExecute = true });
    }
}
