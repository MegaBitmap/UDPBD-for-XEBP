using System.Diagnostics;
using System.IO;
using System.IO.Compression;
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
            "\n\nawaken1ng - udpbd-vexfat - 2025-4-14\n" +
            "https://github.com/awaken1ng/udpbd-vexfat\n\n" +
            "Howling Wolf & Chelsea - XtremeEliteBoot+\n" +
            "https://web.archive.org/web/*/hwc.nat.cu/ps2-vault/hwc-projects/xebplus\n\n" +
            "Rick Gaiser - neutrino - v1.7.0-21-gc063dbc\n" +
            "https://github.com/rickgaiser/neutrino\n\n" +
            "sync-on-luma - neutrino plugin for XEB+ - forked from v2.9.3\n" +
            "https://github.com/sync-on-luma/xebplus-neutrino-loader-plugin";

        readonly List<string> gameList = [];
        string gamePath = "";
        readonly string VHDXNameZip = "PS2-Games-exFAT-udpbd.zip";
        readonly string VHDXName = "PS2-Games-exFAT-udpbd.vhdx";
        readonly string traySettingsFile = "UDPBDTraySettings.txt";

        public MainWindow()
        {
            InitializeComponent();
            CheckAlreadyRunning();
            TextBlockVersion.Text = version;
            KillServer();
            LoadIPSetting();
            LoadGamePathSetting();
            if (File.Exists(VHDXName))
            {
                if (!IsDiskImageMounted(VHDXName))
                {
                    MountDiskImage(VHDXName);
                }
            }
            CheckFiles();
            if (!CheckForExFat())
            {
                SelectVexfat();
            }
        }

        private static void CheckAlreadyRunning()
        {
            string pName = Process.GetCurrentProcess().ProcessName;
            int pCount = Process.GetProcessesByName(pName).Length;
            if (pCount > 1)
            {
                MessageBox.Show("This program is already running.", "Already Running", MessageBoxButton.OK, MessageBoxImage.Information);
                Environment.Exit(-1);
            }
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
            if (ComboBoxServer.SelectedIndex == 0 && CheckBoxEnableVMC.IsChecked == true)
            {
                extraArgs += " -enablevmc";
            }
            Process process = new();
            process.StartInfo.FileName = "UDPBD-for-XEB+-CLI.exe";
            process.StartInfo.Arguments = $"-path \"{gamePath}\" -ps2ip \"{TextBoxPS2IP.Text}\"{extraArgs}";
            process.Start();
        }

        private void CheckBoxEnableVMC_Checked(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("After checking this box you will need to sync.\r\nThen in the XEB+ neutrino Launcher with a game in view, press the ⏹ square button to open the context menu.\r\nIn the context menu you can enable or disable VMCs globally or individually per game.");
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
            if (ComboBoxServer.SelectedIndex == 1)
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
            SaveTraySettings(serverName);

            Process process = new();
            process.StartInfo.FileName = "UDPBDTray.exe";
            process.Start();
            ServerButton.Content = "Stop Server";
        }

        private void SaveTraySettings(string serverName)
        {
            string trayMountPath = gamePath;
            if (File.Exists(VHDXName) && IsDiskImageMounted(VHDXName))
            {
                string driveLetter = GetDiskImageDriveLetter(VHDXName);
                if (!string.IsNullOrEmpty(driveLetter) && gamePath.Contains(driveLetter))
                {
                    trayMountPath = VHDXName;
                }
            }
            using TextWriter traySettings = new StreamWriter(traySettingsFile);
            traySettings.WriteLine(trayMountPath);
            traySettings.WriteLine(serverName);
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
            using TextReader settings = new StreamReader("GamePathSetting.cfg");
            string? tempPath = settings.ReadLine();
            string? serveVMC = settings.ReadLine();
            if (tempPath != null && Directory.Exists(tempPath))
            {
                GetGameList(tempPath);
                if (gameList.Count > 0) gamePath = tempPath;

                if (!string.IsNullOrEmpty(serveVMC) && serveVMC.Contains("VMCServer"))
                {
                    ComboBoxServer.SelectedIndex = 0;
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
            if (CheckBoxEnableVMC.IsChecked == true && ComboBoxServer.SelectedIndex == 0)
            {
                settings.WriteLine("VMCServer");
            }
            settings.Close();
        }

        private void LoadIPSetting()
        {
            if (!File.Exists("IPSetting.cfg")) return;
            using TextReader settings = new StreamReader("IPSetting.cfg");
            string? tempIP = settings.ReadLine();
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
            if (ComboBoxServer.SelectedIndex == 0)
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
                MessageBoxResult result = MessageBox.Show("The program was unable to find an exFAT volume or partition.\r\nDo you want to mount a Virtual Drive?", "exFAT not Found", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result != MessageBoxResult.Yes)
                {
                    return false;
                }
                if (!File.Exists(VHDXName))
                {
                    ZipFile.ExtractToDirectory(VHDXNameZip, Directory.GetCurrentDirectory());
                }
                MountDiskImage(VHDXName);
                if (!IsDiskImageMounted(VHDXName))
                {
                    MessageBox.Show($"Failed to mount the disk image '{VHDXName}'.", "Error Mounting VHDX", MessageBoxButton.OK, MessageBoxImage.Error);
                    Environment.Exit(-1);
                }
                MessageBox.Show("The virtual drive has been mounted. Add your PS2 game ISOs to the DVD or CD folder then restart this sync app.", "Virtual Drive Mounted", MessageBoxButton.OK, MessageBoxImage.Information);
                Environment.Exit(0);
                return false;
            }
        }

        private static void MountDiskImage(string fileName)
        {
            Process process = new();
            process.StartInfo.FileName = "cmd";
            process.StartInfo.Arguments = $"/c {fileName} && timeout /t 1 /nobreak"; // for some reason a 1 second delay is needed or the vhdx will not be mounted
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.Start();
            process.WaitForExit();
        }

        private static bool IsDiskImageMounted(string fileName)
        {
            Process process = new();
            process.StartInfo.FileName = "powershell";
            process.StartInfo.Arguments = $"-Command (Get-DiskImage (Resolve-Path {fileName})).Attached;";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.Start();
            process.WaitForExit();
            string result = process.StandardOutput.ReadLine() + "";
            if (result.Contains("True"))
            {
                return true;
            }
            return false;
        }

        private static string GetDiskImageDriveLetter(string fileName)
        {
            Process process = new();
            process.StartInfo.FileName = "powershell";
            process.StartInfo.Arguments = $"-Command (Get-Partition ((Get-DiskImage (Resolve-Path {fileName})).DevicePath -replace '....PhysicalDrive', '')).DriveLetter";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.Start();
            process.WaitForExit();
            int testChar = process.StandardOutput.Peek();
            if (testChar == 0)
            {
                return "";
            }
            return process.StandardOutput.ReadLine() + "";
        }

        private static void CheckFiles()
        {
            string[] files = ["bsd-udpbd.toml", "UDPBD-for-XEB+-CLI.exe", "udpbd-server.exe", "udpbd-vexfat.exe", "PS2-Games-exFAT-udpbd.zip", "UDPBDTray.exe"];
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

        private void KillServer()
        {
            bool killAll = false;
            string[] serverNames = ["udpbd-server", "udpbd-vexfat", "UDPBDTray"];
            foreach (var server in serverNames)
            {
                Process[] processes = Process.GetProcessesByName(server);
                if (!(processes.Length == 0))
                {
                    if (killAll)
                    {
                        foreach (var item in processes) item.Kill();
                    }
                    else
                    {
                        MessageBoxResult response = MessageBox.Show("The server is currently running.\nClick OK to stop the server and sync.", "The server is running", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                        if (response == MessageBoxResult.OK)
                        {
                            killAll = true;
                            foreach (var item in processes) item.Kill();
                            ServerButton.Content = "Start Server";
                        }
                        else
                        {
                            Environment.Exit(-1);
                        }
                    }
                }
            }
        }

        private static void QuickKillServer()
        {
            bool hasKilled = false;
            string[] serverNames = ["udpbd-server", "udpbd-vexfat", "UDPBDTray"];
            foreach (var server in serverNames)
            {
                Process[] processes = Process.GetProcessesByName(server);
                if (!(processes.Length == 0))
                {
                    if (!server.Contains("Tray"))
                    {
                        hasKilled = true;
                    }
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
            if (ComboBoxServer.SelectedIndex == 1)
            {
                SelectVexfat();
            }
            else
            {
                if (!CheckForExFat())
                {
                    SelectVexfat();
                    return;
                }
                SelectUServer();
            }
        }

        private void SelectVexfat()
        {
            ComboBoxServer.SelectedIndex = 1;
            ButtonSelectGamePath.Visibility = Visibility.Visible;
            TextBlockGameList.Visibility = Visibility.Visible;
            TextBlockSelectExFAT.Visibility = Visibility.Hidden;
            ComboBoxGameVolume.Visibility = Visibility.Hidden;
            CheckBoxEnableVMC.Visibility = Visibility.Hidden;
        }

        private void SelectUServer()
        {
            ButtonSelectGamePath.Visibility = Visibility.Hidden;
            TextBlockGameList.Visibility = Visibility.Hidden;
            TextBlockSelectExFAT.Visibility = Visibility.Visible;
            ComboBoxGameVolume.Visibility = Visibility.Visible;
            CheckBoxEnableVMC.Visibility = Visibility.Visible;
        }

        [GeneratedRegex(@"\\.*")]
        private static partial Regex SelectedVolume();
    }
}
