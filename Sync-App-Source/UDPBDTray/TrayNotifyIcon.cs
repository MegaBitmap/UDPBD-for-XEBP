using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;

namespace UDPBDTray
{
    internal class TrayNotifyIcon : ApplicationContext
    {
        private readonly NotifyIcon notifyIcon;
        private readonly ContextMenuStrip contextMenu;
        private readonly ToolStripMenuItem menuItemOpenSync;
        private readonly ToolStripMenuItem menuItemRestart;
        private readonly ToolStripMenuItem menuItemDebug;
        private readonly ToolStripMenuItem menuItemKill;
        private readonly IContainer components;
        private readonly System.Windows.Forms.Timer timerServerCheck;

        private bool showConsole = false;
        private string serverName = "udpbd-server";
        private string gamePath = "FAILED TO SET GAMEPATH";
        private bool needAdmin = true;
        private bool isActive = false;
        private bool firstStart = true;
        private readonly string syncApp = "UDPBD-for-XEB+-GUI";

        public TrayNotifyIcon()
        {
            components = new Container();
            contextMenu = new();
            menuItemOpenSync = new();
            menuItemRestart = new();
            menuItemDebug = new();
            menuItemKill = new();
            notifyIcon = new(components);
            timerServerCheck = new();

            CheckAlreadyRunning();
            SilentKillServer();
            CheckFiles();
            LoadSettings("UDPBDTraySettings.txt");
            InitNotifyIcon();
            isActive = true;
            InitKeepServerAlive();
        }

        private void CheckAlreadyRunning()
        {
            string pName = Process.GetCurrentProcess().ProcessName;
            int pCount = Process.GetProcessesByName(pName).Length;
            if (pCount > 1)
            {
                isActive = false;
                MessageBox.Show("This program is already running.", "Already Running", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Environment.Exit(0);
            }
        }

        private void InitNotifyIcon()
        {
            contextMenu.Items.Add(menuItemOpenSync);
            contextMenu.Items.Add(menuItemRestart);
            contextMenu.Items.Add(menuItemDebug);
            contextMenu.Items.Add(menuItemKill);

            menuItemOpenSync.Text = "Open Sync App/Change Server Settings";
            menuItemOpenSync.Click += new EventHandler(MenuItemOpenSync_Click);
            menuItemRestart.Text = "Restart and Show Server Console";
            menuItemRestart.Click += new EventHandler(MenuItemRestart_ClickAsync);
            menuItemDebug.Text = "Show PS2Client Debug Console";
            menuItemDebug.Click += new EventHandler(MenuItemDebug_Click);
            menuItemKill.Text = "Stop Server and Exit";
            menuItemKill.Click += new EventHandler(MenuItemKill_Click);

            notifyIcon.Icon = Properties.Resources.Icon;
            notifyIcon.ContextMenuStrip = contextMenu;
            notifyIcon.Text = $"{serverName.ToUpper()} is Running";
            notifyIcon.Visible = true;
            notifyIcon.MouseUp += new MouseEventHandler(NotifyIcon_Click);
        }

        private void MenuItemOpenSync_Click(object? sender, EventArgs e)
        {
            if (!File.Exists($"{syncApp}.exe"))
            {
                MessageBox.Show($"Unable to locate {syncApp}");
                return;
            }
            else
            {
                Process syncProcess = new();
                syncProcess.StartInfo.FileName = syncApp;
                syncProcess.Start();
            }
        }

        private void InitKeepServerAlive()
        {
            timerServerCheck.Tick += new EventHandler(KeepServerAliveAsync);
            timerServerCheck.Interval = 100;
            timerServerCheck.Start();
        }

        private async void KeepServerAliveAsync(object? sender, EventArgs e)
        {
            timerServerCheck.Interval = 30000;
            Process[] UCLIProcess = Process.GetProcessesByName("UDPBD-for-XEB+-CLI");
            Process[] SCLIProcess = Process.GetProcessesByName("SNL-CLI");
            if (UCLIProcess.Length != 0 || SCLIProcess.Length != 0)
            {
                notifyIcon.ShowBalloonTip(10000, "CLI is Running", "Please close UDPBD-for-XEB+-CLI and SNL-CLI while UDPBDTray is running.", ToolTipIcon.Warning);
            }
            else if (isActive)
            {
                Process[] serverProcess = Process.GetProcessesByName(serverName);
                if (serverProcess.Length == 0)
                {
                    int wait = 1000;
                    if (!firstStart)
                    {
                        wait = 10000;
                        notifyIcon.ShowBalloonTip(10000, "Server Down", "The Server stopped unexpectedly.\r\n" +
                            "If that was intentional please close UDPBDTray as well.", ToolTipIcon.Warning);
                    }
                    firstStart = false;
                    await StartServerAsync(wait);
                }
            }
        }

        private void CheckFiles()
        {
            string[] files = ["ps2client.exe", "udpbd-server.exe", "udpbd-vexfat.exe"];
            foreach (var file in files)
            {
                if (!File.Exists(file))
                {
                    isActive = false;
                    MessageBox.Show($"The file {file} is missing.", "File Missing", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(-1);
                }
            }
        }

        private void LoadSettings(string settingsFile)
        {
            if (!File.Exists(settingsFile))
            {
                isActive = false;
                MessageBox.Show("Error the settings file 'UDPBDTraySettings.txt' does not exist.", "Error Reading Settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(-1);
            }
            using TextReader settingsReader = new StreamReader(settingsFile);
            string? tempPath = settingsReader.ReadLine();
            string? tempServer = settingsReader.ReadLine();
            if (string.IsNullOrEmpty(tempPath) || string.IsNullOrEmpty(tempServer))
            {
                isActive = false;
                MessageBox.Show("Failed to read the settings file 'UDPBDTraySettings.txt' .", "Error Reading Settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(-1);
            }
            serverName = tempServer;
            if (tempPath.Contains(".vhdx") && File.Exists(tempPath))
            {
                if (!IsDiskImageMounted(tempPath))
                {
                    MountDiskImage(tempPath);
                }
                string driveLetter = GetDiskImageDriveLetter(tempPath);
                gamePath = $"{driveLetter}:";
                needAdmin = false;
            }
            else
            {
                gamePath = tempPath;
                needAdmin = true;
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

        private string GetDiskImageDriveLetter(string fileName)
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
                isActive = false;
                MessageBox.Show("Error Mounting VHDX", $"Failed to get a valid DriveLetter for '{fileName}'.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SilentKillServer();
                Environment.Exit(-1);
            }
            return process.StandardOutput.ReadLine() + "";
        }

        private void NotifyIcon_Click(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                MethodInfo? mi = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
                mi?.Invoke(notifyIcon, null);
            }
        }

        private void MenuItemDebug_Click(object? sender, EventArgs e)
        {
            if (!File.Exists("IPSetting.cfg"))
            {
                MessageBox.Show($"The file IPSetting.cfg is missing.", "File Missing", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            using TextReader cfgIP = new StreamReader("IPSetting.cfg");
            string? tempIP = cfgIP.ReadLine();
            if (string.IsNullOrEmpty(tempIP))
            {
                MessageBox.Show($"The file IPSetting.cfg is unable to be read or empty.", "File Empty", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Process ps2client = new();
            ps2client.StartInfo.FileName = "cmd";
            ps2client.StartInfo.Arguments = $"/k ps2client -h {tempIP} listen";
            ps2client.Start();
        }

        private async void MenuItemRestart_ClickAsync(object? sender, EventArgs e)
        {
            int waitTime = 1000;
            isActive = false;
            QuickKillServer();
            if (showConsole == false)
            {
                showConsole = true;
                menuItemRestart.Text = "Restart and Hide Console";
            }
            else
            {
                showConsole = false;
                menuItemRestart.Text = "Restart and Show Console";
                waitTime = 10000; // Takes longer to start the terminal
            }
            await StartServerAsync(waitTime);
        }
        private void MenuItemKill_Click(object? sender, EventArgs e)
        {
            isActive = false;
            QuickKillServer();
            Environment.Exit(0);
        }

        private void QuickKillServer()
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
                notifyIcon.ShowBalloonTip(10000, $"{serverName} was not Running", "The server was already stopped by another program.", ToolTipIcon.Warning);
            }
        }

        private static void SilentKillServer()
        {
            string[] serverNames = ["udpbd-server", "udpbd-vexfat"];
            foreach (var server in serverNames)
            {
                Process[] processes = Process.GetProcessesByName(server);
                if (!(processes.Length == 0))
                {
                    foreach (var item in processes) item.Kill();
                }
            }
        }

        private async Task StartServerAsync(int waitTime)
        {
            Process process = new();
            process.StartInfo.FileName = "cmd";
            if (serverName.Contains("vexfat"))
            {
                process.StartInfo.Arguments = $"/k {serverName} \"{gamePath}\"";
                if (showConsole != true)
                {
                    process.StartInfo.FileName = serverName;
                    process.StartInfo.Arguments = $"\"{gamePath}\"";
                    process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                }
            }
            else
            {
                process.StartInfo.Arguments = $"/k \"{Path.GetFullPath(serverName)}\" \\\\.\\{gamePath}";
                if (showConsole != true)
                {
                    process.StartInfo.FileName = serverName;
                    process.StartInfo.Arguments = $"\\\\.\\{gamePath}";
                    process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                }
                if (needAdmin)
                {
                    process.StartInfo.UseShellExecute = true;
                    process.StartInfo.Verb = "runas";
                }
            }
            try
            {
                process.Start();
                await CheckServerStartAsync(waitTime);
            }
            catch (Exception ex)
            {
                isActive = false;
                MessageBox.Show($"Failed to start {serverName}.\r\n{ex.Message}\r\n" +
                    "Try Clicking 'Restart and Show Console' on the tray icon.", $"Error Starting {serverName}", MessageBoxButtons.OK, MessageBoxIcon.Error, 0, MessageBoxOptions.DefaultDesktopOnly);
            }
        }

        private async Task CheckServerStartAsync(int waitTime)
        {
            await Task.Delay(waitTime); // wait for the server to start before checking if it failed
            Process[] processesStarted = Process.GetProcessesByName(serverName);
            if (processesStarted.Length != 0)
            {
                isActive = true;
                notifyIcon.ShowBalloonTip(10000, $"{serverName} is Active!", "The PS2 game server is ready to Play!", ToolTipIcon.None);
            }
            else
            {
                isActive = false;
                notifyIcon.Text = $"{serverName.ToUpper()} is Stopped";
                MessageBox.Show($"Failed to start {serverName}.\r\n" +
                    "Try Clicking 'Restart and Show Console' on the notification tray icon.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, 0, MessageBoxOptions.DefaultDesktopOnly);
            }
        }
    }
}
