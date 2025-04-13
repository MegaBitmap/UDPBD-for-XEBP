using System.Net;
using System.Net.NetworkInformation;
using FluentFTP;

namespace UDPBD_for_XEB__CLI
{
    internal class FTP
    {
        public static bool TestConnection(FtpClient client, IPAddress ps2ip)
        {
            try
            {
                Ping pingSender = new();
                PingReply reply = pingSender.Send(ps2ip, 6000);
                if (!(reply.Status == IPStatus.Success))
                {
                    Console.WriteLine($"\nCONNECTION FAILED\n\nFailed to receive a ping reply:\nPlease verify that your network settings are configured properly and all cables are connected.\nTry adjusting the IP address settings in launchELF.\n{reply.Status}");
                    return false;
                }
            }
            catch (PingException ex)
            {
                Console.WriteLine($"\nCONNECTION FAILED\n\nThe network location cannot be reached:\nPlease verify that your network settings are configured properly and all cables are connected.\nTry manually assigning an IPv4 address and subnet mask to this PC.\n{ex.Message}");
                return false;
            }
            try
            {
                client.Connect();
                Thread.Sleep(200);
                client.GetListing();
                Thread.Sleep(200);
                Console.WriteLine("Connected to the PS2's FTP server Successfully!");
                return true;
            }
            catch (Exception ex)
            {
                client.Disconnect();
                Console.WriteLine($"\nCONNECTION FAILED\n\nFailed to connect to the PS2's FTP server.\n{ex.Message}");
                return false;
            }
        }

        public static void CreateDirectory(FtpClient client, string directoryPath)
        {
            try
            {
                client.CreateDirectory(directoryPath);
                Thread.Sleep(200);
                if (!DirectoryExists(client, directoryPath))
                {
                    Console.WriteLine($"Failed to create the directory {directoryPath} on the PS2 via FTP.\nNo exceptions raised.");
                    client.Disconnect();
                    Program.PauseExit(11);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create the directory {directoryPath} on the PS2 via FTP.\n{ex.Message}");
                client.Disconnect();
                Program.PauseExit(12);
            }
        }

        public static bool DirectoryExists(FtpClient client, string directoryPath)
        {
            try
            {
                client.GetListing(directoryPath);
                Thread.Sleep(200);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void UploadFile(FtpClient client, string filePath, string ftpPath, string targetFile)
        {
            try
            {
                client.UploadFile(filePath, ftpPath + targetFile);
                Thread.Sleep(200);
                if (!FileExists(client, ftpPath, targetFile))
                {
                    Console.WriteLine($"Failed to upload file {filePath} to the PS2 via FTP.\nNo exceptions raised.");
                    client.Disconnect();
                    Program.PauseExit(13);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to upload file {filePath} to the PS2 via FTP.\n{ex.Message}\n{ex.InnerException}");
                client.Disconnect();
                Program.PauseExit(14);
            }
        }

        public static string GetDir(FtpClient client, string ftpPath)
        {
            try
            {
                string returnList = "";
                var ftpList = client.GetListing(ftpPath);
                Thread.Sleep(200);
                foreach (var item in ftpList)
                {
                    returnList += $" {item.Name} ";
                }
                return returnList;
            }
            catch
            {
                return "";
            }
        }

        public static bool FileExists(FtpClient client, string ftpPath, string ftpFile)
        {
            try
            {
                var files = client.GetListing(ftpPath);
                Thread.Sleep(200);
                foreach (var file in files)
                {
                    if (file.Name.Contains(ftpFile))
                    {
                        return true;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
