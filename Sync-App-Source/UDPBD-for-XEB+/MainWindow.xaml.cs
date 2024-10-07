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

        readonly byte[] CDROMHeaderReference = [0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00];
        readonly int sector_raw_size = 2352;
        readonly int sector_target_size = 2048;
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
            FtpUploadFile($"ftp://{address}{udpbdConfigXeb}", tempUdpbdConfig);
        }

        private static void ResetSyncFolder(IPAddress address)
        {
            string vmcFolder = $"ftp://{address}/mass/0/UDPBD-XEBP-Sync/VMC";
            if (FtpDirectoryExists(vmcFolder)) DeleteFTPFolderContents(vmcFolder);
            string syncFolder = $"ftp://{address}/mass/0/UDPBD-XEBP-Sync";
            if (!FtpDirectoryExists(syncFolder)) CreateFtpDirectory(syncFolder);
            string[] folders = [$"ftp://{address}/mass/0/UDPBD-XEBP-Sync/DVD", $"ftp://{address}/mass/0/UDPBD-XEBP-Sync/CD"];
            foreach (string folder in folders)
            {
                if (!FtpDirectoryExists(folder)) CreateFtpDirectory(folder);
                else if (GetFtpSize(folder) < 262144) DeleteFTPFolderContents(folder); // only delete the folder contents if less than 0.25MB. The dummy ISO files should only be 11 bytes each.
            }
        }

        private void Sync_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateSync() != true) return;

            _ = IPAddress.TryParse(TextBoxPS2IP.Text, out IPAddress? address);
            if (address == null) return;
            if (!FtpDirectoryExists($"ftp://{address}{udpbdConfigFolder}"))
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
                FtpUploadFile($"ftp://{address}/mass/0/UDPBD-XEBP-Sync/{game.Replace(@"\", "/")}", "tempIso.txt");
                if (EnableArtworkDownload.IsChecked == true && !string.IsNullOrEmpty(serialID))
                {
                    if (GetArtwork(serialID) == true)
                    {
                        FtpUploadFile($"ftp://{address}/mass/0/XEBPLUS/GME/ART/{serialID}_BG.png", "temp_BG.png");
                        FtpUploadFile($"ftp://{address}/mass/0/XEBPLUS/GME/ART/{serialID}_ICO.png", "temp_ICO.png");
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

        private static void FtpUploadFile(string ftpUrl, string filePath)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpUrl);
                request.Method = WebRequestMethods.Ftp.UploadFile;

                using (FileStream fileStream = File.OpenRead(filePath))
                using (Stream requestStream = request.GetRequestStream())
                {
                    fileStream.CopyTo(requestStream);
                }
                using FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            }
            catch (WebException ex) { MessageBox.Show($"Failed to upload file {filePath} to the PS2 via FTP.\n\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private static bool FtpDirectoryExists(string directoryPath)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(directoryPath);
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                using FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                return true;
            }
            catch { return false; }
        }

        private static long GetFtpSize(string ftpUrl)
        {
            long totalSize = 0;
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpUrl);
                request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                using FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                using StreamReader reader = new(response.GetResponseStream());

                string? line = reader.ReadLine();
                while (!string.IsNullOrEmpty(line))
                {
                    string[] tokens = line.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                    if (tokens.Length > 0 && long.TryParse(tokens[4], out long fileSize))
                    {
                        totalSize += fileSize;
                    }
                    line = reader.ReadLine();
                }
            }
            catch (WebException ex) { MessageBox.Show($"Failed to get the size of file {ftpUrl} on the PS2 via FTP.\n\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
            return totalSize;
        }

        private static void CreateFtpDirectory(string directoryPath)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(directoryPath);
                request.Method = WebRequestMethods.Ftp.MakeDirectory;
                using FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            }
            catch (WebException ex) { MessageBox.Show($"Failed to create the directory {directoryPath} on the PS2 via FTP.\n\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private static void DeleteFTPFolderContents(string url)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url);
                request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                using FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                using System.IO.StreamReader reader = new(response.GetResponseStream());
                while (!reader.EndOfStream)
                {
                    string? line = reader.ReadLine();
                    if (line == null) return;
                    string[] tokens = line.Split(" ", 9, StringSplitOptions.RemoveEmptyEntries);
                    string name = tokens[8];
                    string permissions = tokens[0];
                    string fileUrl = url + "/" + name;

                    if (permissions[0] == 'd' && !fileUrl.EndsWith("/.") && !fileUrl.EndsWith("/..")) DeleteFTPFolderContents(fileUrl + "/");
                    else DeleteFtpFile(fileUrl);
                }
            }
            catch (WebException ex) { MessageBox.Show($"Failed to delete the directory {url} on the PS2 via FTP.\n\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        static void DeleteFtpFile(string url)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url);
                request.Method = WebRequestMethods.Ftp.DeleteFile;
                using FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                if (FtpFileExists(url)) MessageBox.Show($"Failed to delete the file {url} on the PS2 via FTP.\n\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); // for some reason going from launchELF_isr(2023-10-23) to launchELF(2019-1-11) throws an error once per file then fixes itself ???
            }
        }

        static bool FtpFileExists(string url)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url);
                request.Method = WebRequestMethods.Ftp.GetFileSize;
                using FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                return true;
            }
            catch { return false; }
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
            convertLog = "";
            string[] scanFolders = [$"{gamePath}\\CD", $"{gamePath}\\DVD"];
            foreach (var folder in scanFolders)
            {
                string[] binFiles = Directory.GetFiles(folder, "*.bin", SearchOption.AllDirectories);
                foreach (string binFile in binFiles) ConvertBin(binFile);
            }
        }

        private string ScanBin(FileStream fileIn)
        {
            int numSectors = (int)(fileIn.Length / sector_raw_size);
            for (int sectorIndex = 0; sectorIndex < numSectors; sectorIndex++)
            {
                fileIn.Position = sectorIndex * sector_raw_size;
                byte[] header = new byte[16];
                fileIn.Read(header, 0, header.Length);
                if (header[0..12].SequenceEqual(CDROMHeaderReference)) return "data";
            }
            return "";
        }

        private void GenerateISO(FileStream fileIn, string outputISO)
        {
            using FileStream fileOutISO = new(outputISO, FileMode.Create, FileAccess.Write);

            int numSectors = (int)(fileIn.Length / sector_raw_size);
            int sector_offset = 0;
            for (int sectorIndex = 0; sectorIndex < numSectors; sectorIndex++)
            {
                fileIn.Position = sectorIndex * sector_raw_size;
                byte[] header = new byte[16];
                fileIn.Read(header, 0, header.Length);
                if (header[0..12].SequenceEqual(CDROMHeaderReference))
                {
                    int mode = header[15];
                    if (mode == 1) sector_offset = 16;
                    else if (mode == 2) sector_offset = 24;
                    else MessageBox.Show($"Unable to decode the file header for {fileIn.Name}");

                    fileIn.Position = sectorIndex * sector_raw_size + sector_offset;
                    byte[] dataOut = new byte[sector_target_size];
                    fileIn.Read(dataOut, 0, dataOut.Length);
                    fileOutISO.Write(dataOut, 0, dataOut.Length);
                }
            }
        }

        private void ConvertBin(string inputBin)
        {
            using FileStream fileIn = new(inputBin, FileMode.Open, FileAccess.Read);

            string outputFile = $"{Path.GetDirectoryName(fileIn.Name)}\\{Path.GetFileNameWithoutExtension(fileIn.Name)}.iso";

            if (ValidateBin(fileIn) != true || File.Exists(outputFile)) return;
            if (ScanBin(fileIn).Contains("data"))
            {
                GenerateISO(fileIn, outputFile);
                convertLog += $"{Path.GetFileName(outputFile)} was created.\n";
            }
        }

        private bool ValidateBin(FileStream fileIn)
        {
            if (fileIn.Length == 0 || fileIn.Length % 2352 != 0)
            {
                convertLog += $"{Path.GetFileName(fileIn.Name)} is unreadable,\nthe length should be divisible by 2352 (0x930) bytes.\n";
                return false;
            }
            return true;
        }

        [GeneratedRegex(@".*\\|;.*")]
        private static partial Regex SerialMask();
    }
}
