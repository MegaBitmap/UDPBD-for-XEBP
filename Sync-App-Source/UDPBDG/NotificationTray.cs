using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;

namespace UDPBDG;

internal partial class NotificationTray : ApplicationContext
{
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

    private static readonly IContainer components = new Container();
    private static readonly NotifyIcon notifyIcon = new(components);
    private static readonly ContextMenuStrip contextMenu = new();
    private static readonly ToolStripMenuItem menuItemOpenSync = new();
    public static ToolStripMenuItem menuItemConsoleToggle = new();
    private static readonly ToolStripMenuItem menuItemAbout = new();
    private static readonly ToolStripMenuItem menuItemKill = new();

    public static string ServerName = "udpbd-vexfat";
    private string gamePath = "FAILED TO SET GAMEPATH";
    public const string settingsFile = "Settings.ini";
    public const string oldSettings = "OldSettings.ini";
    public const string vhdxFile = "PS2-Games-exFAT-udpbd.vhdx";
    public const string vhdxLabel = "PS2-exFAT";
    private const int listenPort = 0x4712;
    public static StringBuilder conHistory = new();
    private CustomConsole? customConsole;
    public static bool showConsole = false;
    private static bool isActive = false;
    private static bool okLoad = false;

    public NotificationTray()
    {
        CheckAlreadyRunning();
        CheckFiles();
        GetConsole();
        InitNotifyIcon();
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
        contextMenu.Items.Add(menuItemOpenSync);
        contextMenu.Items.Add(menuItemConsoleToggle);
        contextMenu.Items.Add(menuItemAbout);
        contextMenu.Items.Add(menuItemKill);

        menuItemOpenSync.Text = "Settings / Sync Game List";
        menuItemOpenSync.Click += new EventHandler(MenuItemSettings_Click);
        menuItemConsoleToggle.Text = "Show Server Console";
        menuItemConsoleToggle.Click += new EventHandler(MenuItemConsoleToggle_Click);
        menuItemConsoleToggle.CheckOnClick = true;
        menuItemAbout.Text = "About";
        menuItemAbout.Click += new EventHandler(About_Click);

        notifyIcon.Icon = Resources.Icon;
        notifyIcon.ContextMenuStrip = contextMenu;
        notifyIcon.Visible = true;
        notifyIcon.MouseUp += new MouseEventHandler(NotifyIcon_Click);

        menuItemKill.Click += new EventHandler(MenuItemKill_Click);
        menuItemKill.TextChanged += new EventHandler(LoadSettings);
        menuItemKill.Text = "Exit";
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
        if (ServerName.Contains("vexfat", StringComparison.CurrentCultureIgnoreCase))
            serverFunc = new Func<int>(() => run_vexfat_server(gamePath));
        else
            serverFunc = new Func<int>(() => Run_udpbd_server($"\\\\.\\{gamePath}"));

        isActive = true;
        notifyIcon.Icon = Resources.OKIcon;
        notifyIcon.Text = $"{ServerName} is Running";
        // server starts here v
        int rValue = await Task.Run(serverFunc);
        isActive = false;
        if (rValue == 5)
            RestartAdmin();

        notifyIcon.Icon = Resources.XIcon;
        notifyIcon.Text = $"{ServerName} CRASHED!";
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

    private void MenuItemSettings_Click(object? sender, EventArgs e)
    {
        ResetSettings();
    }

    private static void CheckFiles()
    {
        string[] files = ["udpbd_server.dll", "udpbd_vexfat.dll"];
        foreach (var file in files)
        {
            if (!File.Exists(file))
            {
                MessageBox.Show($"The file {file} is missing.", "File Missing", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(-1);
            }
        }
    }

    private async void LoadSettings(object? sender, EventArgs e)
    {
        if (okLoad) return;
        if (!File.Exists(settingsFile))
        {
            notifyIcon.Icon = Resources.XIcon;
            notifyIcon.Text = "Settings Not Found";
            Settings settings = new();
            settings.ShowDialog();
        }
        using TextReader settingsReader = new StreamReader(settingsFile);
        string? tempPath = settingsReader.ReadLine();
        string? tempServer = settingsReader.ReadLine();
        string? startupConsole = settingsReader.ReadLine();
        settingsReader.Close();
        if (string.IsNullOrEmpty(tempPath) || string.IsNullOrEmpty(tempServer))
        {
            MessageBox.Show($"Failed to read the settings file {settingsFile}",
                "Error Reading Settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
            ResetSettings();
            return;
        }
        ServerName = tempServer;
        if (tempPath.Contains(".vhdx") && File.Exists(tempPath))
        {
            char driveLetter = await InitVHDX();
            if (!char.IsLetter(driveLetter))
            {
                MessageBox.Show($"Failed to mount the disk image '{tempPath}'",
                    "Error Mounting VHDX", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ResetSettings();
                return;
            }
            gamePath = $"{driveLetter}:";
        }
        else
        {
            if (!Path.Exists(tempPath))
            {
                MessageBox.Show($"Error the file path '{tempPath}' does not exist.",
                    "Error finding path", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ResetSettings();
                return;
            }
            gamePath = tempPath;
        }
        if (!string.IsNullOrEmpty(startupConsole) && startupConsole.Contains("Console=True"))
        {
            showConsole = true;
            menuItemConsoleToggle.Checked = true;
            customConsole = new();
            customConsole.Show();
        }
        okLoad = true;
        menuItemKill.TextChanged += new EventHandler(StartServerAsync);
        menuItemKill.TextChanged += new EventHandler(PS2ListenAsync);
        menuItemKill.TextChanged += new EventHandler(ServerStartBaloonAsync);
        menuItemKill.Text = "Stop Server and Exit"; // update menuItemText to start running async events
    }

    public async static Task<char> InitVHDX()
    {
        foreach (DriveInfo drive in DriveInfo.GetDrives())
            if (drive.VolumeLabel == vhdxLabel)
                return drive.RootDirectory.ToString()[0];

        Process.Start("explorer", vhdxFile);
        await Task.Delay(2000); // wait for explorer to mount the vhdx

        foreach (DriveInfo drive in DriveInfo.GetDrives())
            if (drive.VolumeLabel == vhdxLabel)
                return drive.RootDirectory.ToString()[0];

        return ' ';
    }

    private static void ResetSettings()
    {
        if (File.Exists(settingsFile))
        {
            if (File.Exists(oldSettings))
                File.Delete(oldSettings);

            File.Move(settingsFile, oldSettings);
        }
        Process process = new();
        process.StartInfo.FileName = Environment.ProcessPath;
        process.StartInfo.UseShellExecute = true;
        process.Start();
        Environment.Exit(0);
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
            customConsole?.Close();
    }

    private void MenuItemKill_Click(object? sender, EventArgs e)
    {
        Environment.Exit(0);
    }

    private async void ServerStartBaloonAsync(object? sender, EventArgs e)
    {
        await Task.Delay(4000); // wait for the server to start before checking if it failed
        if (isActive)
            notifyIcon.ShowBalloonTip(10000, $"{ServerName} is Active!", "The PS2 game server is ready to Play!", ToolTipIcon.None);
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
        Console.WriteLine($"UDPBDG v{Assembly.GetExecutingAssembly().GetName().Version} by MegaBitmap");
    }

    private void About_Click(object? sender, EventArgs e)
    {
        About about = new();
        about.Show();
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
