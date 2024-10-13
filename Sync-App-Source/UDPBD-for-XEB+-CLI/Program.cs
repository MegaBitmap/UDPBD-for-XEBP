using DiscUtils.Iso9660;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace UDPBD_for_XEB__CLI
{
    internal partial class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"UDPBD for XEB+ CLI Sync App {Assembly.GetExecutingAssembly().GetName().Version} by MegaBitmap");

            const string artUrl = "https://archive.org/download/OPLM_ART_2024_09/OPLM_ART_2024_09.zip/PS2/SERIALID/SERIALID";
            string gamePath = "";
            IPAddress ps2ip = IPAddress.Parse("192.168.0.10");
            bool enableArt = false;
            bool enableBin2ISO = false;
            
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
                else if (arg.Contains("-download-art"))
                {
                    enableArt = true;
                }
                else if (arg.Contains("-bin2iso"))
                {
                    enableBin2ISO = true;
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
            FTP.UploadFile($"ftp://{ps2ip}/mass/0/XEBPLUS/CFG/neutrinoLauncher/neutrinoUDPBD.list", "tempNeutrinoUDPBDList.txt");
            if (enableArt)
            {
                DownloadArtList(gameList, ps2ip, artUrl);
            }
            Console.WriteLine("Synchronization with the PS2 is now complete!");
            PauseExit(9);
        }

        static void PrintHelp()
        {
            Console.WriteLine("Usage Example:\n");
            Console.WriteLine(@"dotnet UDPBD-for-XEB+-CLI.dll -path 'C:\PS2\' -ps2ip 192.168.0.10" + "\n");
            Console.WriteLine("-path is the file path to the CD and DVD folder that contain game ISOs.\n");
            Console.WriteLine("-ps2ip is the ip address for connecting to the PS2 with PS2Net.\n");
            Console.WriteLine("-download-art enables automatic game artwork downloading.\n");
            Console.WriteLine("-bin2iso enables automatic CD-ROM Bin to ISO conversion.\n");
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
                        //Console.WriteLine(file.Replace(scanPath, "").Replace(@"\", "/"));
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
                }
                else
                {
                    Console.WriteLine($"Unable to find a serial Game ID for {game}");
                }
            }
            string hash = ComputeMD5(string.Join("\n", gameListWithID));
            //Console.WriteLine(hash);
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
                    foreach (string binFile in binFiles) CDBin.ConvertBin(binFile);
                }
            }
            return;
        }

        static void DownloadArtList(List<string> gameList, IPAddress ps2ip, string artUrl)
        {
            foreach (var game in gameList)
            {
                string serialID = GetSerialID(game);
                if (!string.IsNullOrEmpty(serialID) && GetArtwork(artUrl, serialID) == true)
                {
                    FTP.UploadFile($"ftp://{ps2ip}/mass/0/XEBPLUS/GME/ART/{serialID}_BG.png", "temp_BG.png");
                    FTP.UploadFile($"ftp://{ps2ip}/mass/0/XEBPLUS/GME/ART/{serialID}_ICO.png", "temp_ICO.png");
                }
            }
        }

        static bool GetArtwork(string artUrl, string serialID)
        {
            try
            {
                using WebClient client = new();
                client.DownloadFile(new Uri(artUrl.Replace("SERIALID", serialID) + "_BG_00.png"), "temp_BG.png");
                client.DownloadFile(new Uri(artUrl.Replace("SERIALID", serialID) + "_ICO.png"), "temp_ICO.png");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to download artwork for {serialID}.\n{ex.Message}");
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
            Environment.Exit(number);
        }

        [GeneratedRegex(@".*\\|;.*")]
        private static partial Regex SerialMask();
    }
}
