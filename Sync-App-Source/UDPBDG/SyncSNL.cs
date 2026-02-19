using FluentFTP;
using System.Net;
using System.Net.NetworkInformation;

namespace UDPBDG;

public partial class SyncSNL : Form
{
    private static bool foundMC0 = false;
    private static bool foundMC1 = false;
    private static bool foundMass = false;
    private static string gamePath = "";

    public SyncSNL(string tempGamePath)
    {
        InitializeComponent();
        gamePath = tempGamePath;
        SetVMCCheckbox(VMCCheckbox);
        LoadIP(PS2IPTextBox);
    }

    private async void ConnectButton_Click(object sender, EventArgs e)
    {
        ConnectButton.Enabled = false;
        ConnectLabel.Text = "Please Wait . . .";
        bool isConnected = await ConnectAsync(PS2IPTextBox.Text, ConnectLabel, ConnectButton, PS2IPTextBox);
        if (isConnected)
        {
            await GetStorageDevices(PS2IPTextBox.Text);
            SyncButton.Enabled = true;
            if (InstallSNL.MakeDirectory("temp"))
            {
                File.WriteAllText("temp/IPConfig.ini", PS2IPTextBox.Text);
            }
        }
    }

    private async void InstallMC0Button_Click(object sender, EventArgs e)
    {
        string startText = InstallMC0Button.Text;
        InstallMC0Button.Text = "Please Wait . . .";
        DisableButtons();
        await InstallSNL.SNL("mc", "0", PS2IPTextBox.Text, AutoStartCheckbox.Checked, LogLabel, LogPanel);
        EnableButtons();
        InstallMC0Button.Text = startText;
    }

    private async void InstallMC1Button_Click(object sender, EventArgs e)
    {
        string startText = InstallMC1Button.Text;
        InstallMC1Button.Text = "Please Wait . . .";
        DisableButtons();
        await InstallSNL.SNL("mc", "1", PS2IPTextBox.Text, AutoStartCheckbox.Checked, LogLabel, LogPanel);
        EnableButtons();
        InstallMC1Button.Text = startText;
    }

    private async void InstallMassButton_Click(object sender, EventArgs e)
    {
        string startText = InstallMassButton.Text;
        InstallMassButton.Text = "Please Wait . . .";
        DisableButtons();
        await InstallSNL.SNL("mass", "0", PS2IPTextBox.Text, AutoStartCheckbox.Checked, LogLabel, LogPanel);
        EnableButtons();
        InstallMassButton.Text = startText;
    }

    private async void SyncButton_Click(object sender, EventArgs e)
    {
        string startText = SyncButton.Text;
        SyncButton.Text = "Please Wait . . .";
        DisableButtons();
        await InternalSyncSNL();
        EnableButtons();
        SyncButton.Text = startText;
    }

    private void DisableButtons()
    {
        InstallMC0Button.Enabled = false;
        InstallMC1Button.Enabled = false;
        InstallMassButton.Enabled = false;
        SyncButton.Enabled = false;
    }

    private void EnableButtons()
    {
        InstallMC0Button.Enabled = foundMC0;
        InstallMC1Button.Enabled = foundMC1;
        InstallMassButton.Enabled = foundMass;
        SyncButton.Enabled = true;
    }

    public static List<string> ScanFolder(string scanPath)
    {
        List<string> tempList = [];
        string[] scanFolders = [$"{scanPath}/CD", $"{scanPath}/DVD"];
        foreach (var folder in scanFolders)
        {
            if (Directory.Exists(folder))
            {
                string[] ISOFiles = Directory.GetFiles(folder, "*.iso", SearchOption.TopDirectoryOnly);
                foreach (string file in ISOFiles)
                    tempList.Add(file.Replace(scanPath, "").Replace(@"\", "/"));
            }
        }
        return tempList;
    }

    private void CreateGameList(string gamePath, List<string> gameList)
    {
        List<string> gameListWithID = [];
        foreach (var game in gameList)
        {
            string serialGameID = ISO.GetSerialID(gamePath + game, LogLabel, LogPanel);
            string friendlyName = Path.GetFileNameWithoutExtension(gamePath + game);
            if (!string.IsNullOrEmpty(serialGameID))
            {
                gameListWithID.Add($"{friendlyName}|{serialGameID}|-bsd=udpbd|-dvd=mass:{game}");
                WriteLine($"Loaded {game}");
            }
            else
                WriteLine($"Unable to find a serial Game ID for {game}");
        }
        if (!InstallSNL.MakeDirectory("temp")) return;
        File.WriteAllLines("temp/UDPBDList.txt", gameListWithID);
    }

    private async Task InternalSyncSNL()
    {
        List<string> gameList = ScanFolder(gamePath);
        if (gameList.Count < 1)
        {
            WriteLine($"No games found in {gamePath}/CD or {gamePath}/DVD");
            WriteLine("Try adding -bin2iso to the command to convert BIN format games to iso.");
        }
        WriteLine($"{gameList.Count} games loaded");
        CreateGameList(gamePath, gameList);

        AsyncFtpClient client = new(PS2IPTextBox.Text);
        client.Config.LogToConsole = false; // Set to true when debugging FTP commands
        client.Config.DataConnectionType = FtpDataConnectionType.PASVEX;
        client.Config.CheckCapabilities = false;

        string syncTarget = await GetInstallLocation(client);
        if (string.IsNullOrEmpty(syncTarget)) return;

        string udpConf = BSDConf.Config(PS2IPTextBox.Text);
        if (!InstallSNL.MakeDirectory("temp")) return;
        File.WriteAllText("temp/temp-bsd-udpbd.toml", udpConf);
        await FTP.UploadFile(client, "temp/temp-bsd-udpbd.toml", $"{syncTarget}/SimpleNeutrinoLoader/", "bsd-udpbd.toml", LogLabel, LogPanel);
        WriteLine($"Set {syncTarget}/SimpleNeutrinoLoader/bsd-udpbd.toml to ip={PS2IPTextBox.Text}");

        if (VMCCheckbox.Checked)
        {
            if (await VMC.Sync(gamePath, gameList, "SNL", LogLabel, LogPanel))
                WriteLine("Virtual Memory Cards are now enabled.");
            else
                WriteLine("Virtual Memory Cards are now disabled.");
        }
        else
            WriteLine("Virtual Memory Cards are now disabled.");

        if (!ValidateList("temp/UDPBDList.txt", LogLabel, LogPanel)) return;
        await FTP.UploadFile(client, "temp/UDPBDList.txt", $"{syncTarget}/SimpleNeutrinoLoader/", "UDPBDList.txt", LogLabel, LogPanel);
        WriteLine($"Updated game list at {syncTarget}/SimpleNeutrinoLoader/UDPBDList.txt");
        WriteLine("Synchronization with the PS2 is now complete!");
        WriteLine("Please make sure to start the server before launching a game.");
    }

    private async Task<string> GetInstallLocation(AsyncFtpClient client)
    {
        int numTargets = 0;
        List<string> targets = [];
        if (await InstallSNL.VerifyInstallation(client, "/mc/0"))
        {
            numTargets++;
            targets.Add("/mc/0");
        }
        if (await InstallSNL.VerifyInstallation(client, "/mc/1"))
        {
            numTargets++;
            targets.Add("/mc/1");
        }
        if (await InstallSNL.VerifyInstallation(client, "/mass/0"))
        {
            numTargets++;
            targets.Add("/mass/0");
        }
        if (numTargets < 1)
        {
            WriteLine("Error: Unable to find a valid Simple Neutrino Loader installation.");
            return "";
        }
        else if (numTargets == 1) return targets.First();
        else
        {
            WriteLine("Error: Found multiple installations.\n" +
                "Please disconnect one and try again.");
            return "";
        }
    }

    public static async Task<bool> ConnectAsync(string ps2ip, Label connectLabel, Button connectButton, TextBox ps2IPTextBox)
    {
        if (!IPAddress.TryParse(ps2ip, out IPAddress? address))
        {
            connectLabel.Text = "Disconnected";
            connectButton.Enabled = true;
            MessageBox.Show($"{ps2ip} is not a valid IP address.",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
        try
        {
            bool pingSuccess = false;
            for (int i = 0; i < 2; i++)
            {
                Ping pingSender = new();
                PingReply reply = await pingSender.SendPingAsync(address, 3000);
                if (reply.Status == IPStatus.Success)
                    pingSuccess = true;
            }
            if (!pingSuccess)
            {
                connectLabel.Text = "Disconnected";
                connectButton.Enabled = true;
                MessageBox.Show("Failed to receive a ping reply:\n\n" +
                    "Please verify that your network settings are configured properly and all cables are connected. " +
                    "Try adjusting the IP address settings in launchELF.\n\n",
                    "Connection Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        catch (Exception ex)
        {
            connectLabel.Text = "Disconnected";
            connectButton.Enabled = true;
            MessageBox.Show("The network location cannot be reached:\n\n" +
                "Please verify that your network settings are configured properly and all cables are connected. " +
                "Try manually assigning an IPv4 address and subnet mask to this PC.\n\n" +
                $"{ex.Message}", "Connection Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
        FtpListItem[] ftpList;
        try
        {
            AsyncFtpClient client = new(address.ToString());
            await client.Connect();
            ftpList = await client.GetListing();
            await client.Disconnect();
        }
        catch (Exception ex)
        {
            connectLabel.Text = "Disconnected";
            connectButton.Enabled = true;
            MessageBox.Show("Failed to connect to the PS2's FTP server.\n\n" +
                $"{ex.Message}", "Connection Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
        foreach (var item in ftpList)
        {
            if (item.Name.Contains("mc"))
            {
                connectLabel.Text = "Connected";
                ps2IPTextBox.Enabled = false;
                connectButton.Enabled = false;
                return true;
            }
        }
        connectLabel.Text = "Disconnected";
        connectButton.Enabled = true;
        MessageBox.Show("Failed to connect to the PS2's FTP server.\n\n" +
            "No exceptions were raised.", "Connection Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return false;
    }

    private async Task GetStorageDevices(string address)
    {
        AsyncFtpClient client = new(address);
        string tempDir = await FTP.GetDir(client, "/mc/");
        if (tempDir.Contains('0'))
        {
            foundMC0 = true;
            InstallMC0Button.Enabled = true;
        }
        if (tempDir.Contains('1'))
        {
            foundMC1 = true;
            InstallMC1Button.Enabled = true;
        }
        if (await FTP.DirectoryExists(client, "/mass/0/"))
        {
            foundMass = true;
            InstallMassButton.Enabled = true;
        }
    }

    public static void LoadIP(TextBox ipBox)
    {
        if (!File.Exists("temp/IPConfig.ini")) return;
        using TextReader settingsReader = new StreamReader("temp/IPConfig.ini");
        string? tempIP = settingsReader.ReadLine();
        settingsReader.Close();
        if (string.IsNullOrEmpty(tempIP)) return;
        ipBox.Text = tempIP;
    }

    public static void SetVMCCheckbox(CheckBox vmcCheckbox)
    {
        if (gamePath.Length != 2) return; // the CD and DVD folders must be in the root of and exFAT drive
        DriveInfo driveInfo = new(gamePath);
        if (driveInfo.DriveFormat == "exFAT")
            vmcCheckbox.Enabled = true;
    }

    public static bool ValidateList(string fileName, Label logLabel, Panel logPanel)
    {
        string combinedList = File.ReadAllText(fileName);
        if (combinedList.Length < 20)
        {
            WriteLine($"Failed to save game list to {fileName}", logLabel, logPanel);
            WriteLine("The sync was not able to be completed.", logLabel, logPanel);
            return false;
        }
        return true;
    }

    private void WriteLine(string line)
    {
        LogLabel.Text += $"{line}\n";
        LogPanel.VerticalScroll.Value = LogPanel.VerticalScroll.Maximum;
        LogPanel.PerformLayout();
    }

    public static void WriteLine(string line, Label logLabel, Panel logPanel)
    {
        logLabel.Text += $"{line}\n";
        logPanel.VerticalScroll.Value = logPanel.VerticalScroll.Maximum;
        logPanel.PerformLayout();
    }

    private void SyncSNL_HelpButtonClicked(object sender, System.ComponentModel.CancelEventArgs e)
    {
        About about = new();
        about.ShowDialog();
    }
}
