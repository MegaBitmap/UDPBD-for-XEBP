using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Windows;

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
            "Rick Gaiser - neutrino - v1.3.1\n" +
            "https://github.com/rickgaiser/neutrino\n\n" +
            "sync-on-luma - neutrino plugin for XEB+ - forked from v2.0.3\n" +
            "https://github.com/sync-on-luma/xebplus-neutrino-loader-plugin";

        readonly List<string> gameList = [];
        string gamePath = "";

        public MainWindow()
        {
            InitializeComponent();
            TextBlockVersion.Text = version;
            LoadIPSetting();
            LoadGamePathSetting();
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            TestPS2Connection(TextBoxPS2IP.Text);
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
            SaveGamePathSetting();
        }

        private void Sync_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateSync() != true) return;
            Process process = new();
            process.StartInfo.FileName = "UDPBD-for-XEB+-CLI.exe";
            process.StartInfo.Arguments = $"-path \"{gamePath}\" -ps2ip \"{TextBoxPS2IP.Text}\"";
            process.Start();
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo { FileName = helpUrl, UseShellExecute = true });
        }

        private void StartServer_Click(object sender, RoutedEventArgs e)
        {
            if (gameList.Count == 0)
            {
                MessageBox.Show("Please first select the game folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Process[] processes = Process.GetProcessesByName("udpbd-vexfat");
            if (!(processes.Length == 0))
            {
                MessageBox.Show("The server is already running.", "Server is running", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            Process process = new();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = $"/K udpbd-vexfat \"{gamePath}\"";
            process.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
            process.Start();
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
            settings.Close();
            if (tempPath != null && Directory.Exists(tempPath))
            {
                GetGameList(tempPath);
                if (gameList.Count > 0) gamePath = tempPath;
            }
        }

        private void SaveGamePathSetting()
        {
            TextWriter settings = new StreamWriter("GamePathSetting.cfg");
            settings.WriteLine(gamePath);
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
            return true;
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
                SaveIPSetting();
                return true;
            }
            catch (WebException ex)
            {
                MessageBox.Show($"Failed to connect to the PS2's FTP server.\n\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }
}
