using FluentFTP;
using System.Security.Cryptography;
using System.Text;

namespace UDPBDG;

public partial class SyncXEBP : Form
{
    private static string gamePath = "";
    private const string artUrl =
        "https://archive.org/download/OPLM_ART_2024_09/OPLM_ART_2024_09.zip/PS2/SERIALID/SERIALID";
    private readonly string mode = "";

    public SyncXEBP(string tempGamePath, string input_mode)
    {
        InitializeComponent();
        gamePath = tempGamePath;
        SyncSNL.SetVMCCheckbox(gamePath, VMCCheckbox, input_mode);
        SyncSNL.LoadIP(PS2IPTextBox);
        mode = input_mode;
        Text += $" ({mode} mode)";
    }

    private async void ConnectButton_Click(object sender, EventArgs e)
    {
        ConnectButton.Enabled = false;
        ConnectLabel.Text = "Please Wait . . .";
        bool isConnected = await SyncSNL.ConnectAsync(PS2IPTextBox.Text, ConnectLabel, ConnectButton, PS2IPTextBox);
        if (isConnected)
        {
            SyncButton.Enabled = true;
            if (InstallSNL.MakeDirectory("temp"))
            {
                File.WriteAllText("temp/IPConfig.ini", PS2IPTextBox.Text);
            }
        }
    }

    private async void SyncButton_Click(object sender, EventArgs e)
    {
        string startText = SyncButton.Text;
        SyncButton.Text = "Please Wait . . .";
        SyncButton.Enabled = false;
        await InternalSyncXEBP();
        SyncButton.Enabled = true;
        SyncButton.Text = startText;
    }

    private void CreateGameList(string gamePath, List<string> gameList)
    {
        List<string> gameListWithID = [];
        foreach (var game in gameList)
        {
            WriteLine($"Loading {game}");
            string serialGameID = GameID.Get(gamePath + game, LogLabel, LogPanel);
            if (!string.IsNullOrEmpty(serialGameID))
            {
                gameListWithID.Add($"{serialGameID} {game}");
            }
            else
                WriteLine($"Unable to find a serial Game ID for {game}");
        }
        string hash = ComputeMD5(string.Join("\n", gameListWithID));
        gameListWithID.Add(hash);
        if (!InstallSNL.MakeDirectory("temp")) return;
        File.WriteAllLines("temp/neutrinoUDPBD.list", gameListWithID);
    }

    private async Task InternalSyncXEBP()
    {
        List<string> gameList = SyncSNL.ScanFolder(gamePath);
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

        if (!await FTP.DirectoryExists(client, "/mass/0/XEBPLUS/CFG/"))
        {
            WriteLine($"Unable to detect XtremeEliteBoot+ on the PS2's USB flash drive.");
            return;
        }
        if (!await FTP.DirectoryExists(client, "/mass/0/XEBPLUS/APPS/neutrinoLauncher/config"))
        {
            WriteLine($"Unable to detect the neutrino launcher plugin on the PS2's USB flash drive.");
            return;
        }
        if (!await FTP.DirectoryExists(client, "/mass/0/XEBPLUS/CFG/neutrinoLauncher/"))
        {
            await FTP.CreateDirectory(client, "/mass/0/XEBPLUS/CFG/neutrinoLauncher/", LogLabel, LogPanel);
            WriteLine("Created the folder mass:/XEBPLUS/CFG/neutrinoLauncher/");
        }
        if (mode == "udpbd" || mode == "udpfs_bd")
        {
            string udpConf = BSDConf.Udpbd(PS2IPTextBox.Text, mode);
            if (!InstallSNL.MakeDirectory("temp")) return;
            File.WriteAllText("temp/temp-bsd-udpbd.toml", udpConf);
            File.WriteAllText("temp/temp-loadUDPBD.lua", XEBPConf.Set("udpbd"));
            await FTP.UploadFile(client, "temp/temp-bsd-udpbd.toml", "/mass/0/XEBPLUS/APPS/neutrinoLauncher/config/", "bsd-udpbd.toml", LogLabel, LogPanel);
            await FTP.UploadFile(client, "temp/temp-loadUDPBD.lua", "/mass/0/XEBPLUS/APPS/neutrinoLauncher/", "loadUDPBD.lua", LogLabel, LogPanel);
            WriteLine($"Updated XEBPLUS/APPS/neutrinoLauncher/config/bsd-udpbd.toml to ip={PS2IPTextBox.Text}\n" +
                $"Updated udp driver to {mode}.irx\n" +
                "Updated XEBPLUS/APPS/neutrinoLauncher/loadUDPBD.lua to use udpbd");
        }
        else if (mode == "udpfs")
        {
            string udpConf = BSDConf.Udpfs(PS2IPTextBox.Text);
            if (!InstallSNL.MakeDirectory("temp")) return;
            File.WriteAllText("temp/temp-bsd-udpfs.toml", udpConf);
            File.WriteAllText("temp/temp-loadUDPBD.lua", XEBPConf.Set("udpfs"));
            await FTP.UploadFile(client, "temp/temp-bsd-udpfs.toml", "/mass/0/XEBPLUS/APPS/neutrinoLauncher/config/", "bsd-udpfs.toml", LogLabel, LogPanel);
            await FTP.UploadFile(client, "temp/temp-loadUDPBD.lua", "/mass/0/XEBPLUS/APPS/neutrinoLauncher/", "loadUDPBD.lua", LogLabel, LogPanel);
            WriteLine($"Updated XEBPLUS/APPS/neutrinoLauncher/config/bsd-udpfs.toml to ip={PS2IPTextBox.Text}\n" +
                "Updated XEBPLUS/APPS/neutrinoLauncher/loadUDPBD.lua to use udpfs");
        }
        if (ArtCheckbox.Checked)
            await DownloadArtList(gameList, client);

        if (VMCCheckbox.Checked)
        {
            if (await VMC.Sync(gamePath, gameList, "XEBP", LogLabel, LogPanel))
                WriteLine("Virtual Memory Cards are now enabled.");
            else
                WriteLine("Virtual Memory Cards are now disabled.");
        }
        else
            WriteLine("Virtual Memory Cards are now disabled.");

        if (!SyncSNL.ValidateList("temp/neutrinoUDPBD.list", LogLabel, LogPanel)) return;
        await FTP.UploadFile(client, "temp/neutrinoUDPBD.list", "/mass/0/XEBPLUS/CFG/neutrinoLauncher/", "neutrinoUDPBD.list", LogLabel, LogPanel);
        if (AutoStartCheckbox.Checked)
        {
            if (await InstallSNL.IsPS2BBLInstalled(client))
            {
                await InstallSNL.UpdateBLConfig(client, "mass:/XEBPLUS/XEBPLUS_XMAS.ELF", LogLabel, LogPanel);
            }
            else
                WriteLine("Skipping PS2BBL configuration update because no installation was found.");
        }
        WriteLine("Updated game list at XEBPLUS/CFG/neutrinoLauncher/neutrinoUDPBD.list");
        WriteLine("Synchronization with the PS2 is now complete!");
        WriteLine("Please make sure to start the server before launching a game.");
    }

    private async Task DownloadArtList(List<string> gameList, AsyncFtpClient client)
    {
        WriteLine("Checking for Game Artwork . . .");
        int failCount = 0;
        var artList = await FTP.GetDir(client, "/mass/0/XEBPLUS/GME/ART/");
        foreach (var game in gameList)
        {
            string serialID = GameID.Get(gamePath + game, LogLabel, LogPanel);
            if (string.IsNullOrEmpty(serialID)) continue;
            if (!artList.Contains($"{serialID}_BG.png"))
            {
                if (await GetArtBG(serialID, game))
                {
                    await FTP.UploadFile(client, "temp/temp_BG.png", "/mass/0/XEBPLUS/GME/ART/", $"{serialID}_BG.png", LogLabel, LogPanel);
                    WriteLine($"Downloaded Background Artwork for {game}");
                    failCount = 0;
                }
                else
                    failCount++;
            }
            if (!artList.Contains($"{serialID}_ICO.png"))
            {
                if (await GetArtICO(serialID, game))
                {
                    await FTP.UploadFile(client, "temp/temp_ICO.png", "/mass/0/XEBPLUS/GME/ART/", $"{serialID}_ICO.png", LogLabel, LogPanel);
                    WriteLine($"Downloaded Disc Artwork for {game}");
                    failCount = 0;
                }
                else
                    failCount++;
            }
            if (failCount > 4)
            {
                WriteLine("Automatic Artwork Download has been skipped as it has failed 5 times.");
                return;
            }
        }
    }

    private async Task<bool> GetArtBG(string serialID, string gameName)
    {
        try
        {
            using HttpClient client = new();
            byte[] fileDownload =
                await client.GetByteArrayAsync(new Uri(artUrl.Replace("SERIALID", serialID) + "_BG_00.png"));
            if (!InstallSNL.MakeDirectory("temp")) return false;
            File.WriteAllBytes("temp/temp_BG.png", fileDownload);
            if (File.Exists("temp/temp_BG.png")) return true;
            WriteLine($"Failed to download artwork for {gameName} {serialID}.\n" +
                $"The downloaded png image is missing.");
            return false;
        }
        catch (Exception ex)
        {
            WriteLine($"Failed to download background artwork for {gameName} {serialID}.\n" +
                $"{artUrl.Replace("SERIALID", serialID)}_BG.png\n{ex.Message}");
            return false;
        }
    }

    private async Task<bool> GetArtICO(string serialID, string gameName)
    {
        try
        {
            using HttpClient client = new();
            byte[] fileDownload =
                await client.GetByteArrayAsync(new Uri(artUrl.Replace("SERIALID", serialID) + "_ICO.png"));
            if (!InstallSNL.MakeDirectory("temp")) return false;
            File.WriteAllBytes("temp/temp_ICO.png", fileDownload);
            if (File.Exists("temp/temp_ICO.png")) return true;
            WriteLine($"Failed to download artwork for {gameName} {serialID}.\n" +
                $"The downloaded png image is missing.");
            return false;
        }
        catch (Exception ex)
        {
            WriteLine($"Failed to download disc artwork for {gameName} {serialID}.\n" +
                $"{artUrl.Replace("SERIALID", serialID)}_ICO.png\n{ex.Message}");
            return false;
        }
    }

    public static string ComputeMD5(string input)
    {
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);
        byte[] hashBytes = MD5.HashData(inputBytes);

        // Convert the byte array to a hexadecimal string
        StringBuilder sb = new();
        for (int i = 0; i < hashBytes.Length; i++)
            sb.Append(hashBytes[i].ToString("X2"));

        return sb.ToString().ToLowerInvariant();
    }

    private void WriteLine(string line)
    {
        LogLabel.Text += $"{line}\n";
        LogPanel.VerticalScroll.Value = LogPanel.VerticalScroll.Maximum;
        LogPanel.PerformLayout();
    }

    private void SyncXEBP_HelpButtonClicked(object sender, System.ComponentModel.CancelEventArgs e)
    {
        About about = new();
        about.ShowDialog();
    }
}
