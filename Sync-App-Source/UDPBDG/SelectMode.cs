using System.ComponentModel;

namespace UDPBDG;

public partial class SelectMode : Form
{
    public string mode_clicked = "";
    public SelectMode()
    {
        InitializeComponent();
    }

    private void SelectMode_HelpButtonClicked(object sender, CancelEventArgs e)
    {
        About about = new();
        about.ShowDialog();
    }

    private void Udpbd_button_Click(object sender, EventArgs e)
    {
        mode_clicked = "udpbd";
        Close();
    }

    private void Udpfs_button_Click(object sender, EventArgs e)
    {
        mode_clicked = "udpfs";
        Close();
    }

    private void Udpfs_bd_button_Click(object sender, EventArgs e)
    {
        mode_clicked = "udpfs_bd";
        Close();
    }
}
