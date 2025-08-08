using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;

namespace UDPBDTray
{
    internal class TrayNotifyIcon : ApplicationContext
    {
        private readonly NotifyIcon notifyIcon;
        private readonly ContextMenuStrip contextMenu;
        private readonly ToolStripMenuItem menuItemRestart;
        private readonly ToolStripMenuItem menuItemDebug;
        private readonly ToolStripMenuItem menuItemKill;
        private readonly IContainer components;
        
        private bool showConsole = false;
        private string serverName = "udpbd-server";
        private string gamePath = "FAILED TO SET GAMEPATH";
        private bool needAdmin = true;

        public TrayNotifyIcon()
        {
            components = new Container();
            contextMenu = new();
            menuItemRestart = new();
            menuItemDebug = new();
            menuItemKill = new();
            notifyIcon = new(components);

            CheckAlreadyRunning();
            SilentKillServer();
            CheckFiles();
            InitNotifyIcon();
            LoadSettings("UDPBDTraySettings.txt");
            StartServer(1000);
        }

        private static void CheckAlreadyRunning()
        {
            string pName = Process.GetCurrentProcess().ProcessName;
            int pCount = Process.GetProcessesByName(pName).Length;
            if (pCount > 1)
            {
                MessageBox.Show("This program is already running.", "Already Running", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Environment.Exit(0);
            }
        }

        private void InitNotifyIcon()
        {
            contextMenu.Items.Add(menuItemRestart);
            contextMenu.Items.Add(menuItemDebug);
            contextMenu.Items.Add(menuItemKill);

            menuItemRestart.Text = "Restart and Show Server Console";
            menuItemRestart.Click += new EventHandler(MenuItemRestart_Click);
            menuItemDebug.Text = "Show PS2Client Debug Console";
            menuItemDebug.Click += new EventHandler(MenuItemDebug_Click);
            menuItemKill.Text = "Stop Server and Exit";
            menuItemKill.Click += new EventHandler(MenuItemKill_Click);

            notifyIcon.Icon = Properties.Resources.Icon;
            notifyIcon.ContextMenuStrip = contextMenu;
            notifyIcon.Text = "UDPBD Tray";
            notifyIcon.Visible = true;
            notifyIcon.MouseUp += new MouseEventHandler(NotifyIcon_Click);
        }

        private static void CheckFiles()
        {
            string[] files = ["ps2client.exe", "udpbd-server.exe", "udpbd-vexfat.exe"];
            foreach (var file in files)
            {
                if (!File.Exists(file))
                {
                    MessageBox.Show($"The file {file} is missing.", "File Missing", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(-1);
                }
            }
        }

        private void LoadSettings(string settingsFile)
        {
            if (!File.Exists(settingsFile))
            {
                MessageBox.Show("Error the settings file 'UDPBDTraySettings.txt' does not exist.", "Error Reading Settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(-1);
            }
            using TextReader settingsReader = new StreamReader(settingsFile);
            string? tempPath = settingsReader.ReadLine();
            string? tempServer = settingsReader.ReadLine();
            if (string.IsNullOrEmpty(tempPath) || string.IsNullOrEmpty(tempServer))
            {
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

        private void MenuItemRestart_Click(object? sender, EventArgs e)
        {
            int waitTime = 1000;
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
            StartServer(waitTime);
        }
        private void MenuItemKill_Click(object? sender, EventArgs e)
        {
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

        private void StartServer(int waitTime)
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
                CheckServerStart(waitTime);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start {serverName}.\r\n{ex.Message}\r\nTry Clicking 'Restart and Show Console' on the tray icon.", $"Error Starting {serverName}", MessageBoxButtons.OK, MessageBoxIcon.Error, 0, MessageBoxOptions.DefaultDesktopOnly);
            }
        }

        private void CheckServerStart(int waitTime)
        {
            Thread.Sleep(waitTime); //wait 1 second for the server to start before checking if it failed
            Process[] processesStarted = Process.GetProcessesByName(serverName);
            if (processesStarted.Length != 0)
            {
                notifyIcon.ShowBalloonTip(10000, $"{serverName} is Active!", "The PS2 game server is now running and ready to Play!", ToolTipIcon.None);
            }
            else
            {
                MessageBox.Show($"Failed to start {serverName}.\r\nTry Clicking 'Restart and Show Console' on the tray icon.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, 0, MessageBoxOptions.DefaultDesktopOnly);
            }
        }
    }
}
