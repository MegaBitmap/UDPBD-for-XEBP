﻿using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using FluentFTP;
using Microsoft.Win32;

namespace UDPBD_for_XEB__GUI
{
    public partial class MainWindow : Window
    {
        readonly string version = $"Version {Assembly.GetExecutingAssembly().GetName().Version} by MegaBitmap";
        const string helpUrl = "https://github.com/MegaBitmap/UDPBD-for-XEBP?tab=readme-ov-file#setup";

        const string credits =
            "\n\nawaken1ng - udpbd-vexfat - v0.2.0\n" +
            "https://github.com/awaken1ng/udpbd-vexfat\n\n" +
            "Howling Wolf & Chelsea - XtremeEliteBoot+\n" +
            "https://web.archive.org/web/*/hwc.nat.cu/ps2-vault/hwc-projects/xebplus\n\n" +
            "Rick Gaiser - neutrino - v1.6.1\n" +
            "https://github.com/rickgaiser/neutrino\n\n" +
            "sync-on-luma - neutrino plugin for XEB+ - forked from v2.8.4\n" +
            "https://github.com/sync-on-luma/xebplus-neutrino-loader-plugin";

        readonly List<string> gameList = [];
        string gamePath = "";

        public MainWindow()
        {
            InitializeComponent();
            TextBlockVersion.Text = version;
            KillServer();
            LoadIPSetting();
            LoadGamePathSetting();
            CheckFiles();
        }

        private async void Connect_Click(object sender, RoutedEventArgs e)
        {
            ButtonConnect.IsEnabled = false;
            TextBlockConnection.Text = "Please Wait . . .";
            string tempIP = TextBoxPS2IP.Text;
            await PS2ConnectAsync(tempIP);
        }

        private void SelectPath_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new()
            {
                Filter = "PS2 ISO Files | *.iso",
                Title = "Select a game from the DVD folder..."
            };
            bool? result = dialog.ShowDialog();
            if (!result == true) return;
            if (!dialog.FileName.Contains(@"\DVD\" + dialog.SafeFileName) && !dialog.FileName.Contains(@"\CD\" + dialog.SafeFileName))
            {
                MessageBox.Show("Game ISOs need to be in a folder named DVD or CD", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            gamePath = dialog.FileName.Replace(@"\DVD\" + dialog.SafeFileName, "").Replace(@"\CD\" + dialog.SafeFileName, "");
            GetGameList(gamePath);
        }

        private async void Sync_Click(object sender, RoutedEventArgs e)
        {
            KillServer();
            if (await ValidateSyncAsync() != true) return;
            SaveGamePathSetting();
            string extraArgs = "";
            if (CheckBoxArtworkDownload.IsChecked == true)
            {
                extraArgs += " -downloadart";
            }
            if (CheckBoxBinConvert.IsChecked == true)
            {
                extraArgs += " -bin2iso";
            }
            if (ComboBoxServer.SelectedIndex == 1 && CheckBoxEnableVMC.IsChecked == true)
            {
                extraArgs += " -enablevmc";
            }
            Process process = new();
            process.StartInfo.FileName = "UDPBD-for-XEB+-CLI.exe";
            process.StartInfo.Arguments = $"-path \"{gamePath}\" -ps2ip \"{TextBoxPS2IP.Text}\"{extraArgs}";
            process.Start();
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo { FileName = helpUrl, UseShellExecute = true });
        }

        private void StartServer_Click(object sender, RoutedEventArgs e)
        {
            string? currentState = ServerButton.Content.ToString();
            if (string.IsNullOrEmpty(currentState)) return;
            if (currentState.Contains("Stop"))
            {
                QuickKillServer();
                ServerButton.Content = "Start Server";
                return;
            }
            string serverName;
            if (ComboBoxServer.SelectedIndex == 0)
            {
                serverName = "udpbd-vexfat";
                if (CheckServer(serverName)) return;
            }
            else
            {
                serverName = "udpbd-server";
                if (CheckServer(serverName)) return;
                string? tempGameDrive = ComboBoxGameVolume.SelectedItem.ToString();
                if (tempGameDrive == null) return;
                gamePath = SelectedVolume().Replace(tempGameDrive, "");
                GetGameList(gamePath);
            }
            if (gameList.Count == 0)
            {
                MessageBox.Show("Please first select the game folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Process process = new();
            process.StartInfo.FileName = "cmd.exe";
            if (serverName.Contains("vexfat"))
            {
                process.StartInfo.Arguments = $"/K {serverName} \"{gamePath}\"";
                if (CheckBoxShowConsole.IsChecked != true)
                {
                    process.StartInfo.FileName = serverName;
                    process.StartInfo.Arguments = $"\"{gamePath}\"";
                    process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                }
            }
            else
            {
                process.StartInfo.Arguments = $"/K \"{Path.GetFullPath(serverName)}\" \\\\.\\{gamePath}";
                if (CheckBoxShowConsole.IsChecked != true)
                {
                    process.StartInfo.FileName = serverName;
                    process.StartInfo.Arguments = $"\\\\.\\{gamePath}";
                    process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                }
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.Verb = "runas";
            }
            process.Start();
            if (CheckBoxShowConsole.IsChecked != true)
            {
                CheckServerStart(serverName);
            }
            ServerButton.Content = "Stop Server";
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow aboutWindow = new(this);
            aboutWindow.TextBoxAbout.Text = credits.Replace("https://", "").Replace("http://", "");
            aboutWindow.ShowDialog();
        }

        private void LoadGamePathSetting()
        {
            if (!File.Exists("GamePathSetting.cfg")) return;
            TextReader settings = new StreamReader("GamePathSetting.cfg");
            string? tempPath = settings.ReadLine();
            string? serveVMC = settings.ReadLine();
            settings.Close();
            if (tempPath != null && Directory.Exists(tempPath))
            {
                GetGameList(tempPath);
                if (gameList.Count > 0) gamePath = tempPath;

                if (!string.IsNullOrEmpty(serveVMC) && serveVMC.Contains("VMCServer"))
                {
                    ComboBoxServer.SelectedIndex = 1;
                    CheckBoxEnableVMC.IsChecked = true;
                    int itemNum = 0;
                    foreach (var item in ComboBoxGameVolume.Items)
                    {
                        string? tempItem = item.ToString();
                        if (tempItem != null && tempItem.Contains(tempPath))
                        {
                            ComboBoxGameVolume.SelectedIndex = itemNum;
                            return;
                        }
                        itemNum++;
                    }
                }
            }
        }

        private void SaveGamePathSetting()
        {
            TextWriter settings = new StreamWriter("GamePathSetting.cfg");
            settings.WriteLine(gamePath);
            if (CheckBoxEnableVMC.IsChecked == true && ComboBoxServer.SelectedIndex == 1)
            {
                settings.WriteLine("VMCServer");
            }
            settings.Close();
        }

        private void LoadIPSetting()
        {
            if (!File.Exists("IPSetting.cfg")) return;
            TextReader settings = new StreamReader("IPSetting.cfg");
            string? tempIP = settings.ReadLine();
            settings.Close();
            if (!string.IsNullOrEmpty(tempIP)) TextBoxPS2IP.Text = tempIP;
        }

        private void SaveIPSetting()
        {
            TextWriter settings = new StreamWriter("IPSetting.cfg");
            settings.WriteLine(TextBoxPS2IP.Text);
            settings.Close();
        }

        private async Task<bool> ValidateSyncAsync()
        {
            if (ComboBoxServer.SelectedIndex == 1)
            {
                string? tempGameDrive = ComboBoxGameVolume.SelectedItem.ToString();
                if (tempGameDrive == null) return false;
                gamePath = SelectedVolume().Replace(tempGameDrive, "");
                GetGameList(gamePath);
            }
            if (!TextBlockConnection.Text.Contains("Connected"))
            {
                MessageBox.Show("Please first connect to the PS2.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (gameList.Count == 0)
            {
                MessageBox.Show("Please first select the game folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (!await PS2ConnectAsync(TextBoxPS2IP.Text))
            {
                TextBlockConnection.Text = "Disconnected";
                TextBoxPS2IP.IsEnabled = true;
                ButtonConnect.IsEnabled = true;
                return false;
            }
            return true;
        }

        private void GetGameList(string testPath)
        {
            KillServer();
            TextBlockGameList.Text = "";
            gameList.Clear();
            string[] scanFolders = [$"{testPath}/CD", $"{testPath}/DVD"];
            foreach (var item in scanFolders)
            {
                if (Directory.Exists(item))
                {
                    IEnumerable<string> ISOFiles = Directory.EnumerateFiles(item, "*.iso", SearchOption.TopDirectoryOnly);
                    foreach (string file in ISOFiles) gameList.Add(file.Replace(testPath + @"\", ""));
                }
            }
            if (gameList.Count == 0) return;
            else if (gameList.Count == 1) TextBlockGameList.Text = gameList.Count + " Game Loaded";
            else TextBlockGameList.Text = gameList.Count + " Games Loaded";
        }

        private async Task<bool> PS2ConnectAsync(string ps2ip)
        {
            if (!IPAddress.TryParse(ps2ip, out IPAddress? address))
            {
                TextBlockConnection.Text = "Disconnected";
                ButtonConnect.IsEnabled = true;
                MessageBox.Show($"{ps2ip} is not a valid IP address.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            try
            {
                Ping pingSender = new();
                PingReply reply = await pingSender.SendPingAsync(address, 6000);
                if (!(reply.Status == IPStatus.Success))
                {
                    TextBlockConnection.Text = "Disconnected";
                    ButtonConnect.IsEnabled = true;
                    MessageBox.Show($"Failed to receive a ping reply:\n\nPlease verify that your network settings are configured properly and all cables are connected. Try adjusting the IP address settings in launchELF.\n\n{reply.Status}", "Connection Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                TextBlockConnection.Text = "Disconnected";
                ButtonConnect.IsEnabled = true;
                MessageBox.Show($"The network location cannot be reached:\n\nPlease verify that your network settings are configured properly and all cables are connected. Try manually assigning an IPv4 address and subnet mask to this PC.\n\n{ex.Message}", "Connection Failed", MessageBoxButton.OK, MessageBoxImage.Error);
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
                TextBlockConnection.Text = "Disconnected";
                ButtonConnect.IsEnabled = true;
                MessageBox.Show($"Failed to connect to the PS2's FTP server.\n\n{ex.Message}", "Connection Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            foreach (var item in ftpList)
            {
                if (item.Name.Contains("mass"))
                {
                    TextBlockConnection.Text = "Connected";
                    TextBoxPS2IP.IsEnabled = false;
                    ButtonConnect.IsEnabled = false;
                    SaveIPSetting();
                    return true;
                }
            }
            TextBlockConnection.Text = "Disconnected";
            ButtonConnect.IsEnabled = true;
            MessageBox.Show($"Failed to detect USB storage on the PS2's FTP server.\nPlease make sure the USB drive is plugged in.", "Connection Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        private bool CheckForExFat()
        {
            int numValidVolume = 0;
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.DriveFormat.Equals("exFAT", StringComparison.OrdinalIgnoreCase))
                {
                    GetGameList(drive.ToString());
                    int numGames = gameList.Count;
                    ComboBoxGameVolume.Items.Add($"{drive}    {TextBlockGameList.Text}");
                    numValidVolume++;
                }
            }
            if (numValidVolume >= 1)
            {
                ComboBoxGameVolume.SelectedIndex = 0;
                return true;
            }
            else
            {
                MessageBox.Show("The program was unable to find an exFAT volume or partition.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private static void CheckFiles()
        {
            string[] files = ["bsd-udpbd.toml", "UDPBD-for-XEB+-CLI.exe", "udpbd-server.exe", "udpbd-vexfat.exe"];
            foreach (var file in files)
            {
                if (!File.Exists(file))
                {
                    MessageBox.Show($"The file {file} is missing.", "File Missing", MessageBoxButton.OK, MessageBoxImage.Error);
                    Environment.Exit(-1);
                }
            }
        }

        private static bool CheckServer(string serverName)
        {
            Process[] processes = Process.GetProcessesByName(serverName);
            if (!(processes.Length == 0))
            {
                MessageBox.Show("The server is already running.", "Server is running", MessageBoxButton.OK, MessageBoxImage.Information);
                return true;
            }
            return false;
        }

        private static void CheckServerStart(string serverName)
        {
            Thread.Sleep(500); //wait 0.5 seconds for the server to start before checking if it failed
            Process[] processesStarted = Process.GetProcessesByName(serverName);
            if (!(processesStarted.Length == 0))
            {
                MessageBox.Show("The server is now running and ready to Play!", "Server is running", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else MessageBox.Show("Failed to start the server.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void KillServer()
        {
            string[] serverNames = ["udpbd-server", "udpbd-vexfat"];
            foreach (var server in serverNames)
            {
                Process[] processes = Process.GetProcessesByName(server);
                if (!(processes.Length == 0))
                {
                    MessageBoxResult response = MessageBox.Show("The server is currently running.\nClick OK to stop the server and sync.", "The server is running", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                    if (response == MessageBoxResult.OK)
                    {
                        foreach (var item in processes) item.Kill();
                        ServerButton.Content = "Start Server";
                    }
                    else Environment.Exit(-1);
                }
            }
        }

        private static void QuickKillServer()
        {
            bool hasKilled = false;
            string[] serverNames = ["udpbd-server", "udpbd-vexfat"];
            foreach (var server in serverNames)
            {
                Process[] processes = Process.GetProcessesByName(server);
                if (!(processes.Length == 0))
                {
                    hasKilled = true;
                    foreach (var item in processes) item.Kill();
                }
            }
            if (!hasKilled)
            {
                MessageBox.Show("The server was not running.", "Server is stopped", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else MessageBox.Show("The server was stopped.", "Server is stopped", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ComboBoxGameVolume_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            gameList.Clear();
            gamePath = "";
            if (TextBlockGameList == null) return;
            ComboBoxGameVolume.Items.Clear();
            TextBlockGameList.Text = "";
            if (ComboBoxServer.SelectedIndex == 0)
            {
                ButtonSelectGamePath.Visibility = Visibility.Visible;
                TextBlockGameList.Visibility = Visibility.Visible;
                TextBlockSelectExFAT.Visibility = Visibility.Hidden;
                ComboBoxGameVolume.Visibility = Visibility.Hidden;
                CheckBoxEnableVMC.Visibility = Visibility.Hidden;
            }
            else
            {
                if (!CheckForExFat())
                {
                    ComboBoxServer.SelectedIndex = 0;
                    ButtonSelectGamePath.Visibility = Visibility.Visible;
                    TextBlockGameList.Visibility = Visibility.Visible;
                    TextBlockSelectExFAT.Visibility = Visibility.Hidden;
                    ComboBoxGameVolume.Visibility = Visibility.Hidden;
                    CheckBoxEnableVMC.Visibility = Visibility.Hidden;
                    return;
                }
                ButtonSelectGamePath.Visibility = Visibility.Hidden;
                TextBlockGameList.Visibility = Visibility.Hidden;
                TextBlockSelectExFAT.Visibility = Visibility.Visible;
                ComboBoxGameVolume.Visibility = Visibility.Visible;
                CheckBoxEnableVMC.Visibility = Visibility.Visible;
            }
        }
        [GeneratedRegex(@"\\.*")]
        private static partial Regex SelectedVolume();
    }
}
