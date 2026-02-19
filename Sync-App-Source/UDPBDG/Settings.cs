using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;
using static UDPBDG.NotificationTray;

namespace UDPBDG;

public partial class Settings : Form
{
    private bool startServer = false;
    private long openFileTime;

    public Settings()
    {
        InitializeComponent();
        Text = $"UDPBDG v{Assembly.GetExecutingAssembly().GetName().Version} by MegaBitmap";
    }

    private void MountButton_Click(object sender, EventArgs e)
    {
        if (!File.Exists(vhdxFile))
        {
            Stream stream = new MemoryStream(Resources.VHDX_ZIP);
            ZipFile.ExtractToDirectory(stream, Directory.GetCurrentDirectory());
        }
        Process.Start("explorer", vhdxFile);
    }

    private void SetPathButton_Click(object sender, EventArgs e)
    {
        OpenFileDialog openFileDialog = new()
        {
            Filter = "PS2 Games (*.iso;*.bin)|*.iso;*.bin",
            Title = "Select a game from the CD or DVD folder..."
        };
        DialogResult dialogResult = openFileDialog.ShowDialog();
        openFileTime = DateTime.Now.AddSeconds(12).Ticks; // openFileDialog will temporarily restrict device access
        if (dialogResult != DialogResult.OK) return;
        DirectoryInfo? parentFolder = Directory.GetParent(openFileDialog.FileName);
        if (parentFolder == null || parentFolder.Parent == null ||
            parentFolder.Name != "CD" && parentFolder.Name != "DVD")
        {
            MessageBox.Show("Game files need to be in a folder named CD or DVD",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        GamePathLabel.Text = parentFolder.Parent.ToString().TrimEnd('/').TrimEnd('\\');
        GameCountLabel.Text = SetGameList(GamePathLabel.Text);
    }

    private void SyncSNL_Click(object sender, EventArgs e)
    {
        if (!IsGamePathValid()) return;
        SyncSNL syncSNL = new(GamePathLabel.Text);
        syncSNL.ShowDialog();
    }

    private void SyncXEBP_Click(object sender, EventArgs e)
    {
        if (!IsGamePathValid()) return;
        SyncXEBP syncXEBP = new(GamePathLabel.Text);
        syncXEBP.ShowDialog();
    }

    private async void StartServerButton_Click(object sender, EventArgs e)
    {
        if (!IsGamePathValid()) return;
        string server = "udpbd-vexfat";
        bool exFAT = Is_exFAT();
        if (exFAT)
            server = "udpbd-server";

        string outGamepath = GamePathLabel.Text;
        DriveInfo driveInfo = new(GamePathLabel.Text);
        if (File.Exists(vhdxFile) && driveInfo.VolumeLabel == vhdxLabel)
            outGamepath = vhdxFile;

        string startupConsole = $"Console={ShowServerCheckbox.Checked}";
        string outText = $"{outGamepath}\n{server}\n{startupConsole}";
        File.WriteAllText(settingsFile, outText);

        StartServerButton.Text = "Please Wait . . .";
        if (exFAT)
            while (DateTime.Now.Ticks < openFileTime) // OpenFileDialog will temporarily restrict access
                await Task.Delay(1000);

        startServer = true;
        Close();
    }

    private async Task LoadSettings()
    {
        if (!File.Exists(oldSettings)) return;
        using TextReader settingsReader = new StreamReader(oldSettings);
        string? tempPath = settingsReader.ReadLine();
        settingsReader.Close();
        string pathToLoad;
        if (string.IsNullOrEmpty(tempPath)) return;
        if (tempPath.Contains(".vhdx") && File.Exists(tempPath))
        {
            char driveLetter = await InitVHDX();
            if (!char.IsLetter(driveLetter))
            {
                MessageBox.Show($"Failed to mount the disk image '{tempPath}'",
                    "Error Mounting VHDX", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            pathToLoad = $"{driveLetter}:";
        }
        else
        {
            if (!Path.Exists(tempPath)) return;
            pathToLoad = tempPath;
        }
        GamePathLabel.Text = pathToLoad;
        GameCountLabel.Text = SetGameList(GamePathLabel.Text);
    }

    private bool Is_exFAT()
    {
        if (GamePathLabel.Text.Length != 2) return false;
        DriveInfo driveInfo = new(GamePathLabel.Text);
        if (driveInfo.DriveFormat == "exFAT")
            return true;

        return false;
    }

    private static string SetGameList(string gamePath)
    {
        int gameCount = 0;
        List<DirectoryInfo> dirInfos = [];
        string[] folders = ["CD", "DVD"];
        foreach (string folder in folders)
            if (Directory.Exists($"{gamePath}/{folder}"))
                dirInfos.Add(new DirectoryInfo($"{gamePath}/{folder}"));

        foreach (DirectoryInfo dirInfo in dirInfos)
        {
            foreach (FileInfo fileInfo in dirInfo.EnumerateFiles("*.bin"))
            {
                if (!File.Exists(fileInfo.FullName.Replace(fileInfo.Extension, ".iso")))
                {
                    DialogResult binResult = MessageBox.Show($"{fileInfo.Name} has not been converted to ISO.\n\n" +
                        "Do you want to convert all bin games to ISO format?",
                        "Found a bin Game not converted to ISO", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (binResult == DialogResult.Yes)
                        CDBin.ConvertFolder(gamePath);

                    break;
                }
            }
            gameCount += dirInfo.EnumerateFiles("*.iso").Count();
        }
        if (gameCount > 0)
            return $"{gameCount} Games Loaded";

        return "0 Games Found";
    }

    private bool IsGamePathValid()
    {
        if (GameCountLabel.Text[0] == '0')
        {
            MessageBox.Show("The game path is not set or empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
        if (!Path.Exists(GamePathLabel.Text))
        {
            MessageBox.Show("The game path does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
        return true;
    }

    private void Settings_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (!startServer)
            Environment.Exit(0);
    }

    private void Settings_HelpButtonClicked(object sender, System.ComponentModel.CancelEventArgs e)
    {
        About about = new();
        about.ShowDialog();
    }

    private async void Settings_Shown(object sender, EventArgs e)
    {
        await LoadSettings();
    }
}
