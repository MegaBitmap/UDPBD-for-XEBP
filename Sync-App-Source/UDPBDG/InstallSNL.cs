using FluentFTP;
using System.Diagnostics;
using System.Net;

namespace UDPBDG;

internal class InstallSNL
{
    private static readonly string[] enceladusFiles = ["enceladus_pkd.elf", "helloworld.lua", "icon.icn", "icon.sys", "index.lua"];

    public static async Task SNL(string rootFolder, string childFolder, string textIP, bool modifyBootloader, Label logLabel, Panel logPanel)
    {
        IPAddress ps2ip = IPAddress.Parse(textIP);
        AsyncFtpClient client = new(ps2ip.ToString());
        client.Config.LogToConsole = false; // Set to true when debugging FTP commands
        client.Config.DataConnectionType = FtpDataConnectionType.PASVEX;
        client.Config.CheckCapabilities = false;

        bool skipEnceladus = false;
        bool skipSNL = false;
        if (!VerifyLocalFiles(enceladusFiles, "InstallFiles/Enceladus") ||
            !VerifyLocalFiles(SNLFiles.Names, "InstallFiles/SimpleNeutrinoLoader"))
        {
            DialogResult dialogResult = MessageBox.Show($"This release of UDPBDG is missing installation files for Simple Neutrino Loader.\n" +
                "Click Yes to open the SNL release page.", "SNL Installation Files Missing", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
            if (dialogResult == DialogResult.Yes)
                Process.Start(new ProcessStartInfo { FileName = "https://github.com/MegaBitmap/PS2-SNL/releases", UseShellExecute = true });
            return;
        }
        SyncSNL.WriteLine("Starting Installation . . .", logLabel, logPanel);
        if (!await FTP.DirectoryExists(client, $"/{rootFolder}/{childFolder}/Enceladus"))
        {
            await FTP.CreateDirectory(client, $"/{rootFolder}/{childFolder}/Enceladus", logLabel, logPanel);
            await InstallEnceladus(client, $"/{rootFolder}/{childFolder}/Enceladus/", logLabel, logPanel);
        }
        else if (!await VerifyFTPFiles(client, enceladusFiles, $"/{rootFolder}/{childFolder}/Enceladus", "InstallFiles/Enceladus", logLabel, logPanel))
            await InstallEnceladus(client, $"/{rootFolder}/{childFolder}/Enceladus/", logLabel, logPanel);
        else
            skipEnceladus = true;

        if (!await FTP.DirectoryExists(client, $"/{rootFolder}/{childFolder}/SimpleNeutrinoLoader"))
        {
            await FTP.CreateDirectory(client, $"/{rootFolder}/{childFolder}/SimpleNeutrinoLoader", logLabel, logPanel);
            await InternalInstallSNL(client, $"/{rootFolder}/{childFolder}/SimpleNeutrinoLoader/", logLabel, logPanel);
        }
        else if (!await VerifyFTPFiles(client, SNLFiles.Names, $"/{rootFolder}/{childFolder}/SimpleNeutrinoLoader", "InstallFiles/SimpleNeutrinoLoader", logLabel, logPanel))
            await InternalInstallSNL(client, $"/{rootFolder}/{childFolder}/SimpleNeutrinoLoader/", logLabel, logPanel);
        else
            skipSNL = true;

        if (!skipEnceladus)
        {
            SyncSNL.WriteLine("Verifying Enceladus Installation . . .", logLabel, logPanel);
            if (!await VerifyFTPFiles(client, enceladusFiles, $"/{rootFolder}/{childFolder}/Enceladus", "InstallFiles/Enceladus", logLabel, logPanel))
            {
                SyncSNL.WriteLine($"Failed to install Enceladus to {rootFolder}", logLabel, logPanel);
                return;
            }
        }
        if (!skipSNL)
        {
            SyncSNL.WriteLine("Verifying Simple Neutrino Loader Installation . . .", logLabel, logPanel);
            if (!await VerifyFTPFiles(client, SNLFiles.Names, $"/{rootFolder}/{childFolder}/SimpleNeutrinoLoader", "InstallFiles/SimpleNeutrinoLoader", logLabel, logPanel))
            {
                SyncSNL.WriteLine($"Failed to install Simple Neutrino Loader to {rootFolder}", logLabel, logPanel);
                return;
            }
        }
        if (modifyBootloader)
        {
            if (await IsPS2BBLInstalled(client))
            {
                string configTarget = "mc?";
                if (rootFolder.Contains("mass"))
                    configTarget = "mass";

                await UpdateBLConfig(client, $"{configTarget}:/Enceladus/enceladus_pkd.elf", logLabel, logPanel);
            }
            else
                SyncSNL.WriteLine("Skipping PS2BBL configuration update because no installation was found.", logLabel, logPanel);
        }
        SyncSNL.WriteLine($"Enceladus and SimpleNeutrinoLoader were installed to {rootFolder}\n" +
            "Please remember to sync your game list then start the server.", logLabel, logPanel);
    }

    public static async Task<bool> VerifyInstallation(AsyncFtpClient client, string FTPPath)
    {
        string tempDir = await FTP.GetDir(client, $"{FTPPath}/Enceladus");
        foreach (string file in enceladusFiles)
            if (!tempDir.Contains(file))
                return false;

        tempDir = await FTP.GetDir(client, $"{FTPPath}/SimpleNeutrinoLoader");
        foreach (string file in SNLFiles.Names)
            if (!tempDir.Contains(file))
                return false;

        return true;
    }

    private static async Task<bool> VerifyFTPFiles(AsyncFtpClient client, string[] files, string FTPPath, string folder, Label logLabel, Panel logPanel)
    {
        string tempDir = await FTP.GetDir(client, FTPPath);
        foreach (string file in files)
        {
            FileInfo fileInfo = new($"{folder}/{file}");
            if (!tempDir.Contains(file))
                return false;

            if (await FTP.GetSize(client, FTPPath, file, logLabel, logPanel) != fileInfo.Length)
                return false;
        }
        return true;
    }

    private static bool VerifyLocalFiles(string[] files, string folder)
    {
        foreach (string file in files)
            if (!File.Exists($"{folder}/{file}"))
                return false;

        return true;
    }

    private static async Task InstallEnceladus(AsyncFtpClient client, string folder, Label logLabel, Panel logPanel)
    {
        SyncSNL.WriteLine("Starting installation of Enceladus . . .", logLabel, logPanel);
        foreach (string file in enceladusFiles)
        {
            SyncSNL.WriteLine($"Installing {file} to {folder}{file} . . .", logLabel, logPanel);
            await FTP.UploadFile(client, $"InstallFiles/Enceladus/{file}", folder, file, logLabel, logPanel);
        }
    }

    private static async Task InternalInstallSNL(AsyncFtpClient client, string folder, Label logLabel, Panel logPanel)
    {
        SyncSNL.WriteLine("Starting installation of Simple Neutrino Loader . . .", logLabel, logPanel);
        foreach (string file in SNLFiles.Names)
        {
            SyncSNL.WriteLine($"Installing {file} to {folder}{file} . . .", logLabel, logPanel);
            await FTP.UploadFile(client, $"InstallFiles/SimpleNeutrinoLoader/{file}", folder, file, logLabel, logPanel);
        }
    }

    public static async Task<bool> IsPS2BBLInstalled(AsyncFtpClient client)
    {
        string configType = await GetBLConfig(client);
        if (string.IsNullOrEmpty(configType))
            return false;

        return true;
    }

    public static async Task UpdateBLConfig(AsyncFtpClient client, string target, Label logLabel, Panel logPanel)
    {
        string configPath = await GetBLConfig(client);
        string configFile = "";
        if (configPath.Contains("SYS-CONF"))
            configFile = "PS2BBL.INI";
        else
            configFile = "CONFIG.INI";

        string configContents = PS2BBL.Config(target);
        if (!MakeDirectory("temp")) return;
        File.WriteAllText("temp/temp-BL-CFG.txt", configContents);
        string readContent = File.ReadAllText("temp/temp-BL-CFG.txt");
        if (readContent.Length < 300)
        {
            SyncSNL.WriteLine($"Error: Failed to save/load contents of 'temp-BL-CFG.txt' in {Directory.GetCurrentDirectory}", logLabel, logPanel);
            return;
        }
        await FTP.UploadFile(client, "temp/temp-BL-CFG.txt", configPath, configFile, logLabel, logPanel);
        SyncSNL.WriteLine($"The configuration for PS2BBL has been updated in {configPath}{configFile}", logLabel, logPanel);
    }

    private static async Task<string> GetBLConfig(AsyncFtpClient client)
    {
        List<string> folders = ["SYS-CONF", "PS2BBL"];
        List<string> configFiles = ["PS2BBL.INI", "CONFIG.INI"];
        foreach (string folder in folders)
        {
            for (int i = 0; i < 2; i++)
            {
                string testFolder = $"/mc/{i}/{folder}";
                foreach (string configFile in configFiles)
                    if (await FTP.FileExists(client, testFolder, configFile))
                        return testFolder + "/";
            }
        }
        return "";
    }

    public static bool MakeDirectory(string newDirName)
    {
        try
        {
            if (!Directory.Exists(newDirName))
            {
                Directory.CreateDirectory(newDirName);
                if (!Directory.Exists(newDirName))
                {
                    MessageBox.Show($"Failed to create the folder {newDirName} in {Directory.GetCurrentDirectory}");
                    return false;
                }
            }
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to create the folder {newDirName} in {Directory.GetCurrentDirectory}\n" +
                $"{ex.Message}\n{ex}");
            return false;
        }
    }
}
