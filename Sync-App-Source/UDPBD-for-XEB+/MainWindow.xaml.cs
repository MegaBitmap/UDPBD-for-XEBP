using DiscUtils.Iso9660;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;

namespace UDPBD_for_XEB_
{
    public partial class MainWindow : Window
    {
        readonly string version = $"Version {Assembly.GetExecutingAssembly().GetName().Version} by MegaBitmap";
        const string helpUrl = "https://github.com/MegaBitmap/UDPBD-for-XEBP?tab=readme-ov-file#setup";
        const string artUrl = "https://archive.org/download/OPLM_ART_2024_09/OPLM_ART_2024_09.zip/PS2/SERIALID/SERIALID";
        const string serverName = "udpbd-vexfat";
        const string defaultUdpbdConfig = "bsd-udpbd.toml";
        const string serverExe = "udpbd-vexfat.exe";
        readonly string[] neededFiles = [defaultUdpbdConfig, serverExe];

        const string udpbdConfigXeb = "/mass/0/XEBPLUS/APPS/neutrinoUDPBD/config/bsd-udpbd.toml";
        const string udpbdConfigFolder = "/mass/0/XEBPLUS/APPS/neutrinoUDPBD/config/";
        const string settingsCfg = "settings.cfg";
        const string tempUdpbdConfig = "tempbsd-udpbd.toml";
        const string credits =
            "\n\nawaken1ng - udpbd-vexfat - v0.2.0\n" +
            "https://github.com/awaken1ng/udpbd-vexfat\n\n" +
            "Howling Wolf & Chelsea - XtremeEliteBoot+\n" +
            "https://web.archive.org/web/*/hwc.nat.cu/ps2-vault/hwc-projects/xebplus\n\n" +
            "Rick Gaiser - neutrino - v1.3.1\n" +
            "https://github.com/rickgaiser/neutrino\n\n" +
            "sync-on-luma - neutrinoHDD plugin for XEB+ - forked from v1.0.2\n" +
            "https://github.com/sync-on-luma/xebplus-neutrino-loader-plugin";

        string convertLog = "";
        readonly List<string> gameList = [];
        string gamePath = "";

        public MainWindow()
        {
            InitializeComponent();
            if (!CheckForNeededFiles())
            {
                Application.Current.Shutdown();
            }
            TextBlockVersion.Text = version;
            LoadSettings();
        }

        private void SaveSettings()
        {
            TextWriter settings = new StreamWriter(settingsCfg);
            settings.WriteLine(gamePath);
            settings.WriteLine(TextBoxPS2IP.Text);
            settings.Close();
        }

        private void LoadSettings()
        {
            if (!File.Exists(settingsCfg)) return;
            TextReader settings = new StreamReader(settingsCfg);
            string? tempPath = settings.ReadLine();
            string? tempIP = settings.ReadLine();
            settings.Close();

            if (tempPath != null && Directory.Exists(tempPath))
            {
                GetGameList(tempPath);
                if (gameList.Count > 0) gamePath = tempPath;
            }
            if (tempIP != null) TextBoxPS2IP.Text = tempIP;
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            TestPS2Connection(TextBoxPS2IP.Text);
        }

        private bool TestPS2Connection(string ps2ip)
        {
            if (!IPAddress.TryParse(ps2ip, out IPAddress? address))
            {
                MessageBox.Show($"{ps2ip} is not a valid IP address.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            Ping pingSender = new();
            PingReply reply = pingSender.Send(address);
            if (!(reply.Status == IPStatus.Success))
            {
                MessageBox.Show($"Connection failed: {reply.Status}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"ftp://{address}");
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                using FtpWebResponse response = (FtpWebResponse)request.GetResponse();

                TextBlockConnection.Text = "Connected";
                TextBoxPS2IP.IsEnabled = false;
                ButtonConnect.IsEnabled = false;
                return true;
            }
            catch (WebException ex)
            {
                MessageBox.Show($"Failed to connect to the PS2's FTP server.\n\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
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

        private void GetGameList(string testPath)
        {
            gameList.Clear();
            string[] scanFolders = [$"{testPath}\\CD", $"{testPath}\\DVD"];
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

        private bool ValidateSync()
        {
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
            if (!TestPS2Connection(TextBoxPS2IP.Text))
            {
                TextBlockConnection.Text = "Disconnected";
                TextBoxPS2IP.IsEnabled = true;
                ButtonConnect.IsEnabled = true;
                return false;
            }
            if (!KillServer()) return false;
            return true;
        }

        private static void UpdateUDPConfig(IPAddress address)
        {
            TextReader UDPCFG = new StreamReader(defaultUdpbdConfig);
            string stringUDPCFG = UDPCFG.ReadToEnd().Replace("192.168.1.10", address.ToString());
            UDPCFG.Close();
            TextWriter tempUDPCFG = new StreamWriter(tempUdpbdConfig);
            tempUDPCFG.Write(stringUDPCFG);
            tempUDPCFG.Close();
            FTP.UploadFile($"ftp://{address}{udpbdConfigXeb}", tempUdpbdConfig);
        }

        private static void ResetSyncFolder(IPAddress address)
        {
            string vmcFolder = $"ftp://{address}/mass/0/UDPBD-XEBP-Sync/VMC";
            if (FTP.DirectoryExists(vmcFolder)) FTP.DeleteFolderContents(vmcFolder);
            string syncFolder = $"ftp://{address}/mass/0/UDPBD-XEBP-Sync";
            if (!FTP.DirectoryExists(syncFolder)) FTP.CreateDirectory(syncFolder);
            string[] folders = [$"ftp://{address}/mass/0/UDPBD-XEBP-Sync/DVD", $"ftp://{address}/mass/0/UDPBD-XEBP-Sync/CD"];
            foreach (string folder in folders)
            {
                if (!FTP.DirectoryExists(folder)) FTP.CreateDirectory(folder);
                else if (FTP.GetSize(folder) < 262144) FTP.DeleteFolderContents(folder); // only delete the folder contents if less than 0.25MB. The dummy ISO files should only be 11 bytes each.
            }
        }

        private void Sync_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateSync() != true) return;

            _ = IPAddress.TryParse(TextBoxPS2IP.Text, out IPAddress? address);
            if (address == null) return;
            if (!FTP.DirectoryExists($"ftp://{address}{udpbdConfigFolder}"))
            {
                MessageBox.Show("Please install XtremeEliteBoot and the Neutrino UDPBD plugin first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (EnableBinConvert.IsChecked == true) ConvertBinFolders();

            GetGameList(gamePath);
            if (gameList.Count == 0) return;

            UpdateUDPConfig(address);
            ResetSyncFolder(address);            
            foreach (string game in gameList)
            {
                string serialID = GetSerialID(game);
                TextWriter tempIso = new StreamWriter("tempIso.txt");
                tempIso.Write(serialID);
                tempIso.Close();
                FTP.UploadFile($"ftp://{address}/mass/0/UDPBD-XEBP-Sync/{game.Replace(@"\", "/")}", "tempIso.txt");
                if (EnableArtworkDownload.IsChecked == true && !string.IsNullOrEmpty(serialID))
                {
                    if (GetArtwork(serialID) == true)
                    {
                        FTP.UploadFile($"ftp://{address}/mass/0/XEBPLUS/GME/ART/{serialID}_BG.png", "temp_BG.png");
                        FTP.UploadFile($"ftp://{address}/mass/0/XEBPLUS/GME/ART/{serialID}_ICO.png", "temp_ICO.png");
                    }
                }
            }
            SaveSettings();
            if (!string.IsNullOrEmpty(convertLog)) MessageBox.Show(convertLog);
            MessageBox.Show("Synchronization with the PS2 is now complete!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private static bool KillServer()
        {
            Process[] processes = Process.GetProcessesByName(serverName);
            if (!(processes.Length == 0))
            {
                MessageBoxResult response = MessageBox.Show("The server is currently running.\nClick OK to stop the server and sync.", "The server is running", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                if (response == MessageBoxResult.OK)
                {
                    foreach (var item in processes) item.Kill();
                    return true;
                }
                else return false;
            }
            return true;
        }

        private static bool GetArtwork(string serialID)
        {
            try
            {
                using WebClient client = new();
                client.DownloadFile(new Uri(artUrl.Replace("SERIALID", serialID) + "_BG_00.png"), "temp_BG.png");
                client.DownloadFile(new Uri(artUrl.Replace("SERIALID", serialID) + "_ICO.png"), "temp_ICO.png");
                return true;
            }
            catch (Exception)
            {
                MessageBox.Show($"Failed to download artwork for {serialID}.", "Image Download Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
        }

        private string GetSerialID(string game)
        {
            string fullGamePath = gamePath + @"\" + game;
            try
            {
                string content;
                using (FileStream isoStream = File.Open(fullGamePath, FileMode.Open))
                {
                    CDReader cd = new(isoStream, true);
                    if (!cd.FileExists(@"SYSTEM.CNF"))
                    {
                        MessageBox.Show(game + " Is not a valid PS2 game ISO.\nThe SYSTEM.CNF file is missing.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return "";
                    }
                    using Stream fileStream = cd.OpenFile(@"SYSTEM.CNF", FileMode.Open);
                    using StreamReader reader = new(fileStream);
                    content = reader.ReadToEnd();
                }
                if (!content.Contains("BOOT2"))
                {
                    MessageBox.Show(game + " Is not a valid PS2 game ISO.\nThe SYSTEM.CNF file does not contain BOOT2.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return "";
                }
                string serialID = SerialMask().Replace(content.Split("\n")[0], "");
                return serialID;
            }
            catch (Exception)
            {
                MessageBox.Show(game + " was unable to be read.\nThe ISO file may be corrupt.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return "";
            }
        }

        private void StartServer_Click(object sender, RoutedEventArgs e)
        {
            if (gameList.Count == 0)
            {
                MessageBox.Show("Please first select the game folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Process[] processes = Process.GetProcessesByName(serverName);
            if (!(processes.Length == 0))
            {
                MessageBox.Show("The server is already running.", "Server is running", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            Process process = new();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = $"/K {serverExe} {gamePath}";
            process.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
            process.Start();
        }

        private bool CheckForNeededFiles()
        {
            foreach (string file in neededFiles)
            {
                if (!File.Exists(file))
                {
                    MessageBox.Show($"The file {file} is missing.", "File Missing", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
            return true;
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo { FileName = helpUrl, UseShellExecute = true });
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow aboutWindow = new(this);
            aboutWindow.TextBoxAbout.Text = credits.Replace("https://", "").Replace("http://", "");
            aboutWindow.ShowDialog();
        }

        private void ConvertBinFolders()
        {
            CDBin cDBin = new();
            convertLog = "";
            string[] scanFolders = [$"{gamePath}\\CD", $"{gamePath}\\DVD"];
            foreach (var folder in scanFolders)
            {
                string[] binFiles = Directory.GetFiles(folder, "*.bin", SearchOption.AllDirectories);
                foreach (string binFile in binFiles) convertLog += cDBin.ConvertBin(binFile);
            }
        }

        [GeneratedRegex(@".*\\|;.*")]
        private static partial Regex SerialMask();
    }
}
