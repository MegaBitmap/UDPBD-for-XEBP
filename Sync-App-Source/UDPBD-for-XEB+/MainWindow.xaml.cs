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
        readonly string[] neededFiles = [defaultUdpbdConfig , serverExe];

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
            if (tempPath != null)
            {
                if (Directory.Exists(tempPath))
                {
                    GetGameList(tempPath);
                    if (gameList.Count > 0)
                    {
                        gamePath = tempPath;
                    }
                }
            }
            if (tempIP != null)
            {
                TextBoxPS2IP.Text = tempIP;
            }
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            TestPS2Connection(TextBoxPS2IP.Text);
        }

        private bool TestPS2Connection(string ps2ip)
        {
            if (!IPAddress.TryParse(ps2ip, out IPAddress? address))
            {
                MessageBox.Show($"{ps2ip} is not a valid IP address.");
                return false;
            }
            Ping pingSender = new();
            PingReply reply = pingSender.Send(address);

            if (!(reply.Status == IPStatus.Success))
            {
                MessageBox.Show($"Connection failed: {reply.Status}");
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
                MessageBox.Show($"Failed to connect to the PS2's FTP server.\n\n{ex.Message}");
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
                MessageBox.Show("Game ISOs need to be in a folder named DVD or CD");
                return;
            }
            gamePath = dialog.FileName.Replace(@"\DVD\" + dialog.SafeFileName, "").Replace(@"\CD\" + dialog.SafeFileName, "");
            GetGameList(gamePath);
        }

        private void GetGameList(string testPath)
        {
            gameList.Clear();
            if (Directory.Exists(testPath + @"\CD"))
            {
                IEnumerable<string> CDFiles = Directory.EnumerateFiles(testPath + @"\CD", "*.iso", SearchOption.TopDirectoryOnly);
                foreach (string file in CDFiles)
                {
                    gameList.Add(file.Replace(testPath + @"\", ""));
                }
            }
            if (Directory.Exists(testPath + @"\DVD"))
            {
                IEnumerable<string> DVDFiles = Directory.EnumerateFiles(testPath + @"\DVD", "*.iso", SearchOption.TopDirectoryOnly);
                foreach (string file in DVDFiles)
                {
                    gameList.Add(file.Replace(testPath + @"\", ""));
                }
            }
            if (gameList.Count == 0) return;
            else if (gameList.Count == 1)
            {
                TextBlockGameList.Text = gameList.Count + " Game Loaded";
            }
            else TextBlockGameList.Text = gameList.Count + " Games Loaded";
        }

        private void Sync_Click(object sender, RoutedEventArgs e)
        {
            if (!TextBlockConnection.Text.Contains("Connected"))
            {
                MessageBox.Show("Please first connect to the PS2.");
                return;
            }
            if (gameList.Count == 0)
            {
                MessageBox.Show("Please first select the game folder.");
                return;
            }
            if (!TestPS2Connection(TextBoxPS2IP.Text))
            {
                TextBlockConnection.Text = "Disconnected";
                TextBoxPS2IP.IsEnabled = true;
                ButtonConnect.IsEnabled = true;
                return;
            }
            _ = IPAddress.TryParse(TextBoxPS2IP.Text, out IPAddress? address);
            if (address == null) return;
            if (!FtpDirectoryExists($"ftp://{address}{udpbdConfigFolder}"))
            {
                MessageBox.Show("Please install XtremeEliteBoot and the Neutrino UDPBD plugin first.");
                return;
            }
            GetGameList(gamePath);
            if (gameList.Count == 0) return;
            TextReader UDPCFG = new StreamReader(defaultUdpbdConfig);
            string stringUDPCFG = UDPCFG.ReadToEnd().Replace("192.168.1.10", address.ToString());
            UDPCFG.Close();

            TextWriter tempUDPCFG = new StreamWriter(tempUdpbdConfig);
            tempUDPCFG.Write(stringUDPCFG);
            tempUDPCFG.Close();

            FtpUploadFile($"ftp://{address}{udpbdConfigXeb}", tempUdpbdConfig);

            string[] folders = [$"ftp://{address}/mass/0/DVD", $"ftp://{address}/mass/0/CD"];
            foreach (string folder in folders)
            {
                if (!FtpDirectoryExists(folder))
                {
                    CreateFtpDirectory(folder);
                }
                else if (GetFtpSize(folder) < 50000)
                {
                    DeleteFtpDirectory(folder);
                }
            }
            foreach (string game in gameList)
            {
                string serialID = GetSerialID(game);
                TextWriter tempIso = new StreamWriter("tempIso.txt");
                tempIso.Write(serialID);
                tempIso.Close();
                FtpUploadFile($"ftp://{address}/mass/0/{game.Replace(@"\", "/")}", "tempIso.txt");
                if (EnableArtworkDownload.IsChecked == true)
                {
                    if (GetArtwork(serialID) == true)
                    {
                        FtpUploadFile($"ftp://{address}/mass/0/XEBPLUS/GME/ART/{serialID}_BG.png", "temp_BG.png");
                        FtpUploadFile($"ftp://{address}/mass/0/XEBPLUS/GME/ART/{serialID}_ICO.png", "temp_ICO.png");
                    }
                }
            }
            SaveSettings();
            MessageBox.Show("Synchronization with the PS2 is now complete.");
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
                MessageBox.Show($"Failed to download artwork for {serialID}.");
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
                        MessageBox.Show(game + " Is not a valid PS2 game ISO.\nThe SYSTEM.CNF file is missing.");
                        return "";
                    }
                    using Stream fileStream = cd.OpenFile(@"SYSTEM.CNF", FileMode.Open);
                    using StreamReader reader = new(fileStream);
                    content = reader.ReadToEnd();
                }
                if (!content.Contains("BOOT2"))
                {
                    MessageBox.Show(game + " Is not a valid PS2 game ISO.\nThe SYSTEM.CNF file does not contain BOOT2.");
                    return "";
                }
                string serialID = SerialMask().Replace(content.Split("\n")[0], "");
                return serialID;
            }
            catch (Exception)
            {
                MessageBox.Show(game + " Is not a valid PS2 game ISO.\nThe ISO file may be corrupt.");
                return "";
            }
        }

        private void StartServer_Click(object sender, RoutedEventArgs e)
        {
            if (gameList.Count == 0)
            {
                MessageBox.Show("Please first select the game folder.");
                return;
            }
            Process[] processes = Process.GetProcessesByName(serverName);
            if (!(processes.Length == 0))
            {
                MessageBox.Show("The server is already running.");
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
            catch (WebException ex)
            {
                MessageBox.Show($"Failed to upload file {filePath} to the PS2 via FTP.\n\n{ex.Message}");
            }
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
            catch {return false;}
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
            catch (WebException ex)
            {
                MessageBox.Show($"Failed to get the size of file {ftpUrl} on the PS2 via FTP.\n\n{ex.Message}");
            }
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
            catch (WebException ex)
            {
                MessageBox.Show($"Failed to create the directory {directoryPath} on the PS2 via FTP.\n\n{ex.Message}");
            }
        }

        private static void DeleteFtpDirectory(string url)
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
                    if (permissions[0] == 'd')
                    {
                        if (!fileUrl.EndsWith("/.") && !fileUrl.EndsWith("/.."))
                        {
                            DeleteFtpDirectory(fileUrl + "/");
                        }
                    }
                    else
                    {
                        DeleteFtpFile(fileUrl);
                    }
                }
            }
            catch (WebException ex)
            {
                MessageBox.Show($"Failed to delete the directory {url} on the PS2 via FTP.\n\n{ex.Message}");
            }
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
                if (FtpFileExists(url)) // for some reason going from launchELF_isr(2023-10-23) to launchELF(2019-1-11) throws an error once per file then fixes itself ???
                {
                    MessageBox.Show($"Failed to delete the file {url} on the PS2 via FTP.\n\n{ex.Message}");
                }
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
            catch {return false;}
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = helpUrl,
                UseShellExecute = true
            });
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow aboutWindow = new(this);
            aboutWindow.TextBoxAbout.Text = credits.Replace("https://", "").Replace("http://", "");
            aboutWindow.ShowDialog();
        }

        [GeneratedRegex(@".*\\|;.*")]
        private static partial Regex SerialMask();
    }
}
