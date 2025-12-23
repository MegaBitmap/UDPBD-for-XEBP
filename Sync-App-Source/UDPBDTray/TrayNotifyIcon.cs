using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;

namespace UDPBDTray
{
    internal partial class TrayNotifyIcon : ApplicationContext
    {
        private readonly NotifyIcon notifyIcon;
        private readonly ContextMenuStrip contextMenu = new();
        private readonly ToolStripMenuItem menuItemOpenSync = new();
        public static ToolStripMenuItem menuItemConsoleToggle = new();
        private readonly ToolStripMenuItem menuItemKill = new();
        private readonly IContainer components;

        [LibraryImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool AllocConsole();
        [LibraryImport("kernel32.dll")]
        private static partial nint GetConsoleWindow();
        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool ShowWindow(nint hWnd, int nCmdShow);
        [LibraryImport("udpbd_server.dll", StringMarshalling = StringMarshalling.Utf8, SetLastError = true)]
        private static partial int Run_udpbd_server(string path);
        [LibraryImport("udpbd_vexfat.dll", StringMarshalling = StringMarshalling.Utf8, SetLastError = true)]
        private static partial int run_vexfat_server(string path);

        public static bool showConsole = false;
        public static string ServerName { get; private set; } = "udpbd-server";
        private string gamePath = "FAILED TO SET GAMEPATH" ;
        private static bool isActive = false;
        private readonly string syncApp = "UDPBD-for-XEB+-GUI";
        private readonly static string edition = "UDPBD-for-XEB+";
        private readonly int listenPort = 0x4712;
        public static StringBuilder conHistory = new();
        public CustomConsole? customConsole;
        public static bool flagAskShutdown = true;

        public TrayNotifyIcon()
        {
            components = new Container();
            notifyIcon = new(components);
            CheckAlreadyRunning();
            CheckFiles();
            LoadSettings("UDPBDTraySettings.txt");
            GetConsole();
            InitNotifyIcon();
        }

        private static void CheckAlreadyRunning()
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
            contextMenu.Items.Add(menuItemConsoleToggle);
            contextMenu.Items.Add(menuItemKill);

            menuItemOpenSync.Text = "Open Sync App/Change Server Settings";
            menuItemOpenSync.Click += new EventHandler(MenuItemOpenSync_Click);
            menuItemConsoleToggle.Text = "Show Server Console";
            menuItemConsoleToggle.Click += new EventHandler(MenuItemConsoleToggle_Click);
            menuItemConsoleToggle.CheckOnClick = true;

            // use menuItem to create events
            menuItemKill.TextChanged += new EventHandler(StartServerAsync);
            menuItemKill.TextChanged += new EventHandler(PS2ListenAsync);
            menuItemKill.TextChanged += new EventHandler(ServerStartBaloonAsync);

            // update menuItemText to start running async events
            menuItemKill.Text = "Stop Server and Exit";
            menuItemKill.Click += new EventHandler(MenuItemKill_Click);

            notifyIcon.Icon = Properties.Resources.Icon;
            notifyIcon.ContextMenuStrip = contextMenu;
            notifyIcon.Text = $"{ServerName} is Running";
            notifyIcon.Visible = true;
            notifyIcon.MouseUp += new MouseEventHandler(NotifyIcon_Click);
        }

        private async void PS2ListenAsync(object? sender, EventArgs e)
        {
            await Task.Delay(1000);
            if (!isActive) return;
            IPEndPoint ipEndPoint = new(IPAddress.Any, listenPort);
            using Socket ps2sock = new(ipEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            try
            {
                ps2sock.Bind(ipEndPoint);
                Console.WriteLine($"Listening to PS2 console output on port {listenPort} (0x{listenPort:X})");
                while (true)
                {
                    byte[] buffer = new byte[512];
                    int numBytes = await ps2sock.ReceiveAsync(buffer);
                    Console.Write(Encoding.UTF8.GetString(buffer, 0, numBytes));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error has occured in PS2ListenAsync.\n\n" +
                    $"{ex.Message}\n\n{ex}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void StartServerAsync(object? sender, EventArgs e)
        {
            GetInterferingProcess();
            Console.WriteLine("Starting Server . . .");
            Func<int> serverFunc;
            if (ServerName.Contains("vexfat"))
            {
                serverFunc = new Func<int>(() => run_vexfat_server(gamePath));
            }
            else
            {
                serverFunc = new Func<int>(() => Run_udpbd_server($"\\\\.\\{gamePath}"));
            }
            isActive = true;
            // server starts here v
            int rValue = await Task.Run(serverFunc);            
            isActive = false;
            if (rValue == 5)
            {
                RestartAdmin();
            }
            ShowConsoleError();
            string errorMessage = "";
            if (rValue > 0)
            {
                Win32Exception ex = new(rValue);
                errorMessage = $"This might be caused by the following:\n\n{ex.Message}";
            }
            DialogResult response = MessageBox.Show($"Server stopped unexpectedly with a return value of {rValue}\n{errorMessage}\n" +
                "Do you want to save the server console history?",
                "Server Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
            if (response == DialogResult.Yes)
            {
                CustomConsole.CopyConsoleHistory();
                MessageBox.Show("The server history has been copied to your clipboard",
                    "Copied to clipboard", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            Environment.Exit(rValue);
        }

        private void MenuItemOpenSync_Click(object? sender, EventArgs e)
        {
            StartSyncApp();
        }

        private static void CheckFiles()
        {
            string[] files = ["udpbd_server.dll", "udpbd_vexfat.dll"];
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
                DialogResult response = MessageBox.Show("Error the settings file 'UDPBDTraySettings.txt' does not exist.\n" +
                    "Do you want to open the sync app to apply new settings?",
                    "Error Reading Settings", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                if (response == DialogResult.Yes)
                {
                    StartSyncApp();
                }
                Environment.Exit(-1);
            }
            using TextReader settingsReader = new StreamReader(settingsFile);
            string? tempPath = settingsReader.ReadLine();
            string? tempServer = settingsReader.ReadLine();
            if (string.IsNullOrEmpty(tempPath) || string.IsNullOrEmpty(tempServer))
            {
                isActive = false;
                MessageBox.Show("Failed to read the settings file 'UDPBDTraySettings.txt'",
                    "Error Reading Settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(-1);
            }
            ServerName = tempServer;
            if (tempPath.Contains(".vhdx") && File.Exists(tempPath))
            {
                string driveLetter = InitVHDX(tempPath);
                if (string.IsNullOrEmpty(driveLetter))
                {
                    MessageBox.Show($"Failed to mount the disk image '{tempPath}'",
                        "Error Mounting VHDX", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(-1);
                }
                gamePath = $"{driveLetter}:";
            }
            else
            {
                if (!Path.Exists(tempPath))
                {
                    isActive = false;
                    MessageBox.Show($"Error the file path '{tempPath}' does not exist.",
                        "Error finding path", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(-1);
                }
                gamePath = tempPath;
            }
        }

        private static string InitVHDX(string fileName)
        {
            Process process = new();
            process.StartInfo.FileName = "powershell";
            process.StartInfo.Arguments = "-Command " +
                $"$p=Resolve-Path '{fileName}';" +
                "$d=Get-DiskImage $p;" +
                "if(-not$d.Attached){&$p;Start-Sleep .6;$d=Get-DiskImage $p}" +
                "(Get-Partition([string]$d.DevicePath[-1])).DriveLetter";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.Start();
            process.WaitForExit();
            int testChar = process.StandardOutput.Peek();
            if (testChar == 0) return "";
            return process.StandardOutput.ReadLine() + "";
        }

        private void StartSyncApp()
        {
            if (!File.Exists($"{syncApp}.exe"))
            {
                MessageBox.Show($"Unable to locate {syncApp}", "Sync app missing", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else if (Process.GetProcessesByName(syncApp).Length != 0)
            {
                MessageBox.Show("The sync app is already running.", "Already Running", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else
            {
                Process syncProcess = new();
                syncProcess.StartInfo.FileName = syncApp;
                syncProcess.StartInfo.UseShellExecute = true;
                syncProcess.Start();
                Environment.Exit(0);
            }
        }

        private void NotifyIcon_Click(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                MethodInfo? mi = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
                mi?.Invoke(notifyIcon, null);
            }
        }

        private void MenuItemConsoleToggle_Click(object? sender, EventArgs e)
        {
            showConsole = !showConsole;
            if (showConsole)
            {
                customConsole = new();
                customConsole.Show();
            }
            else
            {
                flagAskShutdown = false;
                customConsole?.Close();
            }
        }

        private void MenuItemKill_Click(object? sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private async void ServerStartBaloonAsync(object? sender, EventArgs e)
        {
            await Task.Delay(4000); // wait for the server to start before checking if it failed
            if (isActive)
            {
                notifyIcon.ShowBalloonTip(10000, $"{ServerName} is Active!", "The PS2 game server is ready to Play!", ToolTipIcon.None);
            }
        }

        private static void RestartAdmin()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal pricipal = new(identity);
            if (!pricipal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                try
                {
                    Process process = new();
                    process.StartInfo.Verb = "runas";
                    process.StartInfo.UseShellExecute = true;
                    process.StartInfo.FileName = Environment.ProcessPath;
                    process.Start();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error has occured while trying to run as Administrator.\n\n" +
                        $"{ex.Message}\n\n{ex}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                Environment.Exit(5);
            }
        }

        private static void GetInterferingProcess()
        {
            string[] pNames = ["SNL-CLI", "UDPBD-for-XEB+-CLI", "udpbd-server", "udpbd-vexfat"];
            foreach (string p in pNames)
            {
                if (Process.GetProcessesByName(p).Length != 0)
                {
                    MessageBox.Show($"{p} is currently running.\n Stop the {p} app before starting UDPBDTray.",
                    "Interfering Process is Running", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Environment.Exit(-1);
                }
            }
        }

        private static void GetConsole()
        {
            AllocConsole();
            ShowWindow(GetConsoleWindow(), 0);
            Console.WriteLine($"{edition} UDPBDTray {Assembly.GetExecutingAssembly().GetName().Version} by MegaBitmap");
        }

        private void ShowConsoleError()
        {
            showConsole = true;
            menuItemConsoleToggle.Checked = true;
            if (customConsole == null)
            {
                customConsole = new();
                customConsole.Show();
            }
            ShowWindow(GetConsoleWindow(), 5);
        }
    }
}
