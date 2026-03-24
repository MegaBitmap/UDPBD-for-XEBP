using Python.Runtime;
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
    private const string pythonFolderDLL = @"\Python313\python313.dll";
    private const string pythonVersion = "3.13";
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
        bool alreadyRunning = true;
        for (int i = 0; i < 5; i++)
        {
            string pName = Process.GetCurrentProcess().ProcessName;
            int pCount = Process.GetProcessesByName(pName).Length;
            if (pCount == 1)
            {
                alreadyRunning = false;
                break;
            }
            Thread.Sleep(1000);
        }
        if (alreadyRunning)
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
        Func<int> serverFunc = new(() => run_vexfat_server(gamePath));
        if (ServerName.Contains("udpbd-server", StringComparison.CurrentCultureIgnoreCase))
        {
            serverFunc = new Func<int>(() => Run_udpbd_server($"\\\\.\\{gamePath}"));
        }
        else if (ServerName.Contains("udpfs_bd", StringComparison.CurrentCultureIgnoreCase))
        {
            serverFunc = new Func<int>(() => Run_udpfs_server("block-device", gamePath));
        }
        else if (ServerName.Contains("udpfs", StringComparison.CurrentCultureIgnoreCase))
        {
            serverFunc = new Func<int>(() => Run_udpfs_server("root-dir", gamePath));
        }
        isActive = true;
        notifyIcon.Icon = Resources.OKIcon;
        notifyIcon.Text = $"{ServerName} is Running";
        // server starts here v
        int rValue = await Task.Run(serverFunc);
        isActive = false;
        if (rValue == 5)
        {
            if (ServerName.Contains("udpbd-server", StringComparison.CurrentCultureIgnoreCase))
            {
                RestartAdmin();
            }
            else
            {
                MessageBox.Show($"Failed to open '{gamePath}'\n" +
                    "Make sure that it is ejected/detached/unmounted.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
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
        Thread.Sleep(500); // wait for the python engine to shutdown first
        Environment.Exit(rValue);
    }

    private static int Run_udpfs_server(string mode, string path)
    {
        Runtime.PythonDLL = GetPythonDLL();
        PythonEngine.Initialize();
        PythonEngine.BeginAllowThreads();
        var gil = Py.GIL();
        int rVal = -1;
        try
        {
            using PyModule pyScope = Py.CreateScope();
            dynamic sys = pyScope.Import("sys");
            sys.stdout = new PyConsoleWrite();
            sys.stderr = new PyConsoleWrite();
            sys.path.append($"{Directory.GetCurrentDirectory()}/udpfs_server");
            dynamic udpfs_server = pyScope.Import("udpfs_server");
            if (mode == "root-dir")
            {
                udpfs_server.fs_main(path);
            }
            else if (mode == "block-device")
            {
                udpfs_server.bd_main(path);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex);

            if (ex.Message == "5")
            {
                rVal = 5;
            }
        }
        finally
        {
            gil.Dispose();
        }
        return rVal;
    }

    private static string GetPythonDLL()
    {
        string[] python_paths = [
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + pythonFolderDLL,
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
            @"\Programs\Python" + pythonFolderDLL
        ];
        string found_python_path = "";
        foreach (string python_path in python_paths)
        {
            if (File.Exists(python_path))
            {
                found_python_path = python_path;
                break;
            }
        }
        if (string.IsNullOrEmpty(found_python_path))
        {
            isActive = false;
            notifyIcon.Icon = Resources.XIcon;
            notifyIcon.Text = $"{ServerName} failed, python is missing";
            MessageBox.Show($"Failed to find a python {pythonVersion} installation.\n" +
                $"Install python to either:\n{python_paths[0]}\nor\n{python_paths[1]}",
                "Failed to find python", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.Exit(-1);
        }
        return found_python_path;
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
            SelectMode selectMode = new();
            selectMode.ShowDialog();
            Settings settings = new(selectMode.mode_clicked);
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
        Thread.Sleep(500); // wait for the python engine to shutdown first
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
        Thread.Sleep(500); // wait for the python engine to shutdown first
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
                DialogResult response = MessageBox.Show("An error has occured while trying to run as Administrator.\n\n" +
                    "Click YES to reset the server settings.\n\n" +
                    $"{ex.Message}\n\n{ex}", "Error", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Error);
                if (response == DialogResult.Yes)
                {
                    if (File.Exists(settingsFile))
                    {
                        if (File.Exists(oldSettings))
                            File.Delete(oldSettings);

                        File.Move(settingsFile, oldSettings);
                    }
                }
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
