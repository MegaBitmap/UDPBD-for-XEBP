using DiscUtils.Iso9660;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;

namespace UDPBD_for_XEB_
{
    internal partial class Sync
    {
        const string artUrl = "https://archive.org/download/OPLM_ART_2024_09/OPLM_ART_2024_09.zip/PS2/SERIALID/SERIALID";
        const string udpbdConfigXeb = "/mass/0/XEBPLUS/APPS/neutrinoUDPBD/config/bsd-udpbd.toml";
        const string tempUdpbdConfig = "tempbsd-udpbd.toml";
        const string defaultUdpbdConfig = "bsd-udpbd.toml";

        public static void Games(List<string> games, string gamePath, IPAddress address, bool? artEnable)
        {
            foreach (string game in games)
            {
                string serialID = GetSerialID(gamePath, game);
                TextWriter tempIso = new StreamWriter("tempIso.txt");
                tempIso.Write(serialID);
                tempIso.Close();
                FTP.UploadFile($"ftp://{address}/mass/0/UDPBD-XEBP-Sync/{game.Replace(@"\", "/")}", "tempIso.txt");
                if (artEnable == true && !string.IsNullOrEmpty(serialID))
                {
                    if (GetArtwork(serialID) == true)
                    {
                        FTP.UploadFile($"ftp://{address}/mass/0/XEBPLUS/GME/ART/{serialID}_BG.png", "temp_BG.png");
                        FTP.UploadFile($"ftp://{address}/mass/0/XEBPLUS/GME/ART/{serialID}_ICO.png", "temp_ICO.png");
                    }
                }
            }
        }

        private static string GetSerialID(string gamePath, string game)
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
                        MessageBox.Show(game + " Is not a valid PS2 game ISO.\nThe SYSTEM.CNF file is missing.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return "";
                    }
                    using Stream fileStream = cd.OpenFile(@"SYSTEM.CNF", FileMode.Open);
                    using StreamReader reader = new(fileStream);
                    content = reader.ReadToEnd();
                }
                if (!content.Contains("BOOT2"))
                {
                    MessageBox.Show(game + " Is not a valid PS2 game ISO.\nThe SYSTEM.CNF file does not contain BOOT2.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return "";
                }
                string serialID = SerialMask().Replace(content.Split("\n")[0], "");
                return serialID;
            }
            catch (Exception)
            {
                MessageBox.Show(game + " was unable to be read.\nThe ISO file may be corrupt.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return "";
            }
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
                MessageBox.Show($"Failed to download artwork for {serialID}.", "Image Download Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
        }

        public static void ResetSyncFolder(IPAddress address)
        {
            string vmcFolder = $"ftp://{address}/mass/0/UDPBD-XEBP-Sync/VMC";
            if (FTP.DirectoryExists(vmcFolder)) FTP.DeleteFolderContents(vmcFolder);
            string syncFolder = $"ftp://{address}/mass/0/UDPBD-XEBP-Sync";
            if (!FTP.DirectoryExists(syncFolder)) FTP.CreateDirectory(syncFolder);
            string[] folders = [$"ftp://{address}/mass/0/UDPBD-XEBP-Sync/DVD", $"ftp://{address}/mass/0/UDPBD-XEBP-Sync/CD"];
            foreach (string folder in folders)
            {
                if (!FTP.DirectoryExists(folder)) FTP.CreateDirectory(folder);
                else if (FTP.GetSize(folder) < 262144) FTP.DeleteFolderContents(folder); // only delete the folder contents if less than 0.25MB. The dummy ISO files should only be 11 bytes each.
            }
        }

        public static void UpdateUDPConfig(IPAddress address)
        {
            TextReader UDPCFG = new StreamReader(defaultUdpbdConfig);
            string stringUDPCFG = UDPCFG.ReadToEnd().Replace("192.168.1.10", address.ToString());
            UDPCFG.Close();
            TextWriter tempUDPCFG = new StreamWriter(tempUdpbdConfig);
            tempUDPCFG.Write(stringUDPCFG);
            tempUDPCFG.Close();
            FTP.UploadFile($"ftp://{address}{udpbdConfigXeb}", tempUdpbdConfig);
        }

        [GeneratedRegex(@".*\\|;.*")]
        private static partial Regex SerialMask();
    }
}
