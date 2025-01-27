using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using DiscUtils.Iso9660;

namespace UDPBD_for_XEB__CLI
{
    internal partial class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"UDPBD for XEB+ CLI Sync App {Assembly.GetExecutingAssembly().GetName().Version} by MegaBitmap");

            string gamePath = "";
            IPAddress ps2ip = IPAddress.Parse("192.168.0.10");
            bool enableArt = false;
            bool enableBin2ISO = false;
            bool enableVMC = false;
            
            if (args.Length < 2 || !args.Contains("-path"))
            {
                PrintHelp();
                PauseExit(0);
            }
            int argIndex = 0;
            foreach (var arg in args)
            {
                if (arg.Contains("-path"))
                {
                    gamePath = args[argIndex + 1];
                }
                else if (arg.Contains("-ps2ip"))
                {
                    ps2ip = IPAddress.Parse(args[argIndex + 1]);
                }
                else if (arg.Contains("-downloadart"))
                {
                    enableArt = true;
                }
                else if (arg.Contains("-bin2iso"))
                {
                    enableBin2ISO = true;
                }
                else if (arg.Contains("-enablevmc"))
                {
                    enableVMC = true;
                }
                argIndex++;
            }
            if (!File.Exists("bsd-udpbd.toml"))
            {
                Console.WriteLine("Missing file bsd-udpbd.toml");
                PauseExit(1);
            }
            if (!KillServer())
            {
                PauseExit(2);
            }
            if (!Path.Exists(gamePath))
            {
                Console.WriteLine("The path does not exist.");
                PauseExit(3);
            }
            if (!Path.Exists($"{gamePath}/CD") && !Path.Exists($"{gamePath}/DVD"))
            {
                Console.WriteLine($"There must be a DVD or CD folder inside '{gamePath}'.");
                PauseExit(4);
            }
            if (enableBin2ISO)
            {
                BinConvertFolder(gamePath);
            }
            List<string> gameList = ScanFolder(gamePath);
            if (gameList.Count < 1)
            {
                Console.WriteLine($"No games found in {gamePath}/CD or {gamePath}/DVD");
                PauseExit(5);
            }
            Console.WriteLine($"{gameList.Count} games loaded");
            CreateGameList(gamePath, gameList);
            
            if (!FTP.TestConnection(ps2ip))
            {
                PauseExit(6);
            }
            if (!FTP.DirectoryExists($"ftp://{ps2ip}/mass/0/XEBPLUS/CFG/"))
            {
                Console.WriteLine($"Unable to detect XtremeEliteBoot+ on the PS2's USB flash drive.");
                PauseExit(7);
            }
            if (!FTP.DirectoryExists($"ftp://{ps2ip}/mass/0/XEBPLUS/APPS/neutrinoLauncher/config"))
            {
                Console.WriteLine($"Unable to detect the neutrino launcher plugin on the PS2's USB flash drive.");
                PauseExit(8);
            }
            if (!FTP.DirectoryExists($"ftp://{ps2ip}/mass/0/XEBPLUS/CFG/neutrinoLauncher/"))
            {
                FTP.CreateDirectory($"ftp://{ps2ip}/mass/0/XEBPLUS/CFG/neutrinoLauncher/");
            }
            UpdateUDPConfig(ps2ip);
            if (enableArt)
            {
                if (File.Exists("ArtworkURL.cfg"))
                {
                    string artUrl = File.ReadLines("ArtworkURL.cfg").First();
                    DownloadArtList(gamePath, gameList, ps2ip, artUrl);
                }
                else Console.WriteLine("Missing the file ArtworkURL.cfg");
            }
            if (enableVMC)
            {
                if (SyncVMC(gamePath, gameList))
                {
                    Console.WriteLine("Virtual Memory Cards are now enabled.");
                }
                else Console.WriteLine("Virtual Memory Cards are now disabled.");
            }
            else  Console.WriteLine("Virtual Memory Cards are now disabled.");
            FTP.UploadFile($"ftp://{ps2ip}/mass/0/XEBPLUS/CFG/neutrinoLauncher/neutrinoUDPBD.list", "tempNeutrinoUDPBDList.txt");
            Console.WriteLine("Updated game list at mass:/XEBPLUS/CFG/neutrinoLauncher/neutrinoUDPBD.list");
            Console.WriteLine(@" ________       ___    ___ ________   ________  _______   ________     
|\   ____\     |\  \  /  /|\   ___  \|\   ____\|\  ___ \ |\   ___ \    
\ \  \___|_    \ \  \/  / | \  \\ \  \ \  \___|\ \   __/|\ \  \_|\ \   
 \ \_____  \    \ \    / / \ \  \\ \  \ \  \    \ \  \_|/_\ \  \ \\ \  
  \|____|\  \    \/  /  /   \ \  \\ \  \ \  \____\ \  \_|\ \ \  \_\\ \ 
    ____\_\  \ __/  / /      \ \__\\ \__\ \_______\ \_______\ \_______\
   |\_________\\___/ /        \|__| \|__|\|_______|\|_______|\|_______|
   \|_________\|___|/");
            Console.WriteLine("Synchronization with the PS2 is now complete!");
            Console.WriteLine("Please make sure to start the server before launching a game.");
            PauseExit(9);
        }

        static void PrintHelp()
        {
            Console.WriteLine("Usage Example:\n");
            Console.WriteLine(@"dotnet UDPBD-for-XEB+-CLI.dll -path 'C:\PS2\' -ps2ip 192.168.0.10" + "\n");
            Console.WriteLine("-path is the file path to the CD and DVD folder that contain game ISOs.\n");
            Console.WriteLine("-ps2ip is the ip address for connecting to the PS2 with PS2Net.\n");
            Console.WriteLine("-downloadart enables automatic game artwork downloading.\n");
            Console.WriteLine("-bin2iso enables automatic CD-ROM Bin to ISO conversion.\n");
            Console.WriteLine("-enablevmc will assign a virtual memory card for each game or group of games in 'vmc_groups.list'.\n");
        }

        static List<string> ScanFolder(string scanPath)
        {
            List<string> tempList = [];
            string[] scanFolders = [$"{scanPath}/CD", $"{scanPath}/DVD"];
            foreach (var folder in scanFolders)
            {
                if (Directory.Exists(folder))
                {
                    string[] ISOFiles = Directory.GetFiles(folder, "*.iso", SearchOption.TopDirectoryOnly);
                    foreach (string file in ISOFiles)
                    {
                        tempList.Add(file.Replace(scanPath, "").Replace(@"\", "/"));
                    }
                }
            }
            return tempList;
        }

        static void CreateGameList(string gamePath, List<string> gameList)
        {
            List<string> gameListWithID = [];
            foreach (var game in gameList)
            {
                string serialGameID = GetSerialID(gamePath + game);
                if (!string.IsNullOrEmpty(serialGameID))
                {
                    gameListWithID.Add($"{serialGameID} {game}");
                    Console.WriteLine($"Loaded {game}");
                }
                else
                {
                    Console.WriteLine($"Unable to find a serial Game ID for {game}");
                }
            }
            string hash = ComputeMD5(string.Join("\n", gameListWithID));
            gameListWithID.Add(hash);
            File.WriteAllLines("tempNeutrinoUDPBDList.txt", gameListWithID);
        }

        static void BinConvertFolder(string scanPath)
        {
            string[] scanFolders = [$"{scanPath}/CD", $"{scanPath}/DVD"];
            foreach (var folder in scanFolders)
            {
                if (Directory.Exists(folder))
                {
                    string[] binFiles = Directory.GetFiles(folder, "*.bin", SearchOption.TopDirectoryOnly);
                    foreach (string binFile in binFiles)
                    {
                        if (CheckSpace(binFile, folder))
                        {
                            CDBin.ConvertBin(binFile);
                        }
                        else
                        {
                            Console.WriteLine($"There is not enough space to convert {binFile} to ISO.");
                        }
                    }
                }
            }
        }

        static void DownloadArtList(string gamePath, List<string> gameList, IPAddress ps2ip, string artUrl)
        {
            Console.WriteLine("Checking for Game Artwork . . .");
            int failCount = 0;
            foreach (var game in gameList)
            {
                string serialID = GetSerialID(gamePath + game);
                if (!string.IsNullOrEmpty(serialID) && !FTP.FileExists($"ftp://{ps2ip}/mass/0/XEBPLUS/GME/ART/{serialID}_BG.png") && !FTP.FileExists($"ftp://{ps2ip}/mass/0/XEBPLUS/GME/ART/{serialID}_ICO.png"))
                {
                    if (GetArtBG(artUrl, serialID))
                    {
                        FTP.UploadFile($"ftp://{ps2ip}/mass/0/XEBPLUS/GME/ART/{serialID}_BG.png", "temp_BG.png");
                        Console.WriteLine($"Downloaded Background Artwork for {game}");
                        failCount = 0;
                    }
                    else failCount++;
                    if (GetArtICO(artUrl, serialID))
                    {
                        FTP.UploadFile($"ftp://{ps2ip}/mass/0/XEBPLUS/GME/ART/{serialID}_ICO.png", "temp_ICO.png");
                        Console.WriteLine($"Downloaded Disc Artwork for {game}");
                        failCount = 0;
                    }
                    else failCount++;
                }
                if (failCount > 4)
                {
                    Console.WriteLine("Automatic Artwork Download has been skipped as it has failed 5 times.");
                    return;
                }
            }
        }

        static bool GetArtBG(string artUrl, string serialID)
        {
            try
            {
                using WebClient client = new();
                client.DownloadFile(new Uri(artUrl.Replace("SERIALID", serialID) + "_BG_00.png"), "temp_BG.png");
                if (File.Exists("temp_BG.png")) return true;
                Console.WriteLine($"Failed to download artwork for {serialID}.\nThe downloaded png image is missing.");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to download background artwork for {serialID}.\n{artUrl.Replace("SERIALID", serialID)}_BG.png\n{ex.Message}");
                return false;
            }
        }

        static bool GetArtICO(string artUrl, string serialID)
        {
            try
            {
                using WebClient client = new();
                client.DownloadFile(new Uri(artUrl.Replace("SERIALID", serialID) + "_ICO.png"), "temp_ICO.png");
                if (File.Exists("temp_ICO.png")) return true;
                Console.WriteLine($"Failed to download artwork for {serialID}.\nThe downloaded png image is missing.");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to download disc artwork for {serialID}.\n{artUrl.Replace("SERIALID", serialID)}_ICO.png\n{ex.Message}");
                return false;
            }
        }

        static bool SyncVMC(string gamePath, List<string> gameList)
        {
            List<string> gameListVMC = [];
            if (!Path.Exists($"{gamePath}/VMC"))
            {
                Directory.CreateDirectory($"{gamePath}/VMC");
            }
            string[] neededFiles = ["vmc_groups.list", "BlankVMC8.bin", "BlankVMC32.bin"];
            foreach (string file in neededFiles)
            {
                if (!File.Exists(file))
                {
                    Console.WriteLine($"Missing file {file}");
                    return false;
                }
            }
            string[] groupsVMC = File.ReadAllLines("vmc_groups.list");
            string crossSaveIDs = String.Join("", groupsVMC);
            foreach (var game in gameList)
            {
                string serialID = GetSerialID(gamePath + game);
                if (string.IsNullOrEmpty(serialID))
                {
                    Console.WriteLine($"Failed to get serial ID for {gamePath + game}");
                    continue;
                }
                string vmcRelativePath;
                string vmcFullPath;
                int currentVmcSize = 8;
                if (crossSaveIDs.Contains(serialID))
                {
                    string vmcFile = "";
                    bool checkSize = false;
                    string currentGroup = "";
                    
                    foreach (string line in groupsVMC)
                    {
                        if (checkSize)
                        {
                            checkSize = false;
                            if (line == "32") currentVmcSize = 32;
                            else currentVmcSize = 8;
                        }
                        if (line.Contains("XEBP"))
                        {
                            currentGroup = line;
                            checkSize = true;
                        }
                        else if (line == serialID && !string.IsNullOrEmpty(currentGroup))
                        {
                            vmcFile = $"{currentGroup}_0.bin";
                            break;
                        }
                    }
                    if (string.IsNullOrEmpty(vmcFile))
                    {
                        Console.Write($"Failed to find a group for {serialID}");
                        return false;
                    }
                    vmcRelativePath = $"/VMC/{vmcFile}";
                    vmcFullPath = $"{gamePath}{vmcRelativePath}";
                }
                else
                {
                    vmcRelativePath = $"/VMC/{serialID}_0.bin";
                    vmcFullPath = $"{gamePath}{vmcRelativePath}";
                }
                gameListVMC.Add($"{serialID} {game} {vmcRelativePath}");
                if (!File.Exists(vmcFullPath))
                {
                    if (CheckSpace($"BlankVMC{currentVmcSize}.bin", vmcFullPath))
                    {
                        File.Copy($"BlankVMC{currentVmcSize}.bin", vmcFullPath);
                        Console.WriteLine($"Created {vmcRelativePath} for {game}");
                    }
                    else
                    {
                        Console.WriteLine("Not enough space to create a new VMC file.");
                        return false;
                    }
                }
            }
            string hash = ComputeMD5(string.Join("\n", gameListVMC));
            gameListVMC.Add(hash);
            File.WriteAllLines("tempNeutrinoUDPBDList.txt", gameListVMC);
            return true;
        }

        static bool CheckSpace(string source, string destination)
        {
            FileInfo fileInfo = new(source);
            long fileSize = fileInfo.Length;
            string? dest = Path.GetPathRoot(destination);
            if (dest == null) return false;
            DriveInfo driveInfo = new(dest);
            long availableSpace = driveInfo.AvailableFreeSpace;
            if (availableSpace > fileSize)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        static void UpdateUDPConfig(IPAddress ps2ip)
        {
            string udpConf = File.ReadAllText("bsd-udpbd.toml").Replace("192.168.1.10", $"{ps2ip}");
            File.WriteAllText("tempbsd-udpbd.toml", udpConf);
            FTP.UploadFile($"ftp://{ps2ip}/mass/0/XEBPLUS/APPS/neutrinoLauncher/config/bsd-udpbd.toml", "tempbsd-udpbd.toml");
            Console.WriteLine($"Updated bsd-udpbd.toml with the IP address {ps2ip}");
        }

        static string GetSerialID(string fullGamePath)
        {
            try
            {
                string content;
                using (FileStream isoStream = File.Open(fullGamePath, FileMode.Open))
                {
                    CDReader cd = new(isoStream, true);
                    if (!cd.FileExists(@"SYSTEM.CNF"))
                    {
                        Console.WriteLine($"{fullGamePath} Is not a valid PS2 game ISO. The SYSTEM.CNF file is missing.");
                        return "";
                    }
                    using Stream fileStream = cd.OpenFile(@"SYSTEM.CNF", FileMode.Open);
                    using StreamReader reader = new(fileStream);
                    content = reader.ReadToEnd();
                }
                if (!content.Contains("BOOT2"))
                {
                    Console.WriteLine($"{fullGamePath} Is not a valid PS2 game ISO.\nThe SYSTEM.CNF file does not contain BOOT2.");
                    return "";
                }
                string serialID = SerialMask().Replace(content.Split("\n")[0], "");
                return serialID;
            }
            catch (Exception)
            {
                Console.WriteLine($"{fullGamePath} was unable to be read. The ISO file may be corrupt.");
                return "";
            }
        }

        static string ComputeMD5(string input)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = MD5.HashData(inputBytes);

            // Convert the byte array to a hexadecimal string
            StringBuilder sb = new();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2"));
            }
            return sb.ToString().ToLowerInvariant();
        }

        static bool KillServer()
        {
            string[] serverNames = ["udpbd-server", "udpbd-vexfat"];
            foreach (var server in serverNames)
            {
                Process[] processes = Process.GetProcessesByName(server);
                if (!(processes.Length == 0))
                {
                    Console.Write("The server is currently running, do you want to stop the server and sync? (y/n): ");
                    char response = Console.ReadKey().KeyChar;
                    Console.WriteLine();
                    if (response == 'y' || response == 'Y')
                    {
                        foreach (var item in processes) item.Kill();
                        return true;
                    }
                    else return false;
                }
            }
            return true;
        }

        public static void PauseExit(int number)
        {
            Console.Write("Press any key to continue . . . ");
            Console.ReadKey();
            Console.WriteLine();
            Environment.Exit(number);
        }

        [GeneratedRegex(@".*\\|;.*")]
        private static partial Regex SerialMask();
    }
}
