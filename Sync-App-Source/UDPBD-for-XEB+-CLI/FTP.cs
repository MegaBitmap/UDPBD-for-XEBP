using System.Net;
using System.Net.NetworkInformation;
using FluentFTP;

namespace UDPBD_for_XEB__CLI
{
    internal class FTP
    {
        public static bool TestConnection(IPAddress ps2ip)
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
                FtpClient client = new(ps2ip.ToString());
                client.Connect();
                client.GetListing();
                client.Disconnect();
                Console.WriteLine("Connected to the PS2's FTP server Successfully!");
                Thread.Sleep(100);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nCONNECTION FAILED\n\nFailed to connect to the PS2's FTP server.\n{ex.Message}");
                return false;
            }
        }

        public static bool DirectoryExists(IPAddress ps2ip, string directoryPath)
        {
            try
            {
                FtpClient client = new(ps2ip.ToString());
                client.Connect();
                client.GetListing(directoryPath);
                client.Disconnect();
                Thread.Sleep(100);
                return true;
            }
            catch { return false; }
        }

        public static void CreateDirectory(IPAddress ps2ip, string directoryPath)
        {
            try
            {
                FtpClient client = new(ps2ip.ToString());
                client.Connect();
                client.CreateDirectory(directoryPath);
                client.Disconnect();
                Thread.Sleep(200);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create the directory {directoryPath} on the PS2 via FTP.\n{ex.Message}");
                Program.PauseExit(10);
            }
        }

        public static void UploadFile(IPAddress ps2ip, string filePath, string ftpPath)
        {
            try
            {
                FtpClient client = new(ps2ip.ToString());
                client.Connect();
                client.UploadFile(filePath, ftpPath);
                Thread.Sleep(200);
                var fileExists = client.FileExists(ftpPath);
                client.Disconnect();
                Thread.Sleep(100);
                if (!fileExists)
                {
                    Console.WriteLine($"Failed to upload file {filePath} to the PS2 via FTP.\nNo exceptions raised.");
                    Program.PauseExit(11);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to upload file {filePath} to the PS2 via FTP.\n{ex.Message}");
                Program.PauseExit(11);
            }
        }

        public static bool FileExists(IPAddress ps2ip, string ftpPath)
        {
            try
            {
                FtpClient client = new(ps2ip.ToString());
                client.Connect();
                var fileExists = client.FileExists(ftpPath);
                client.Disconnect();
                Thread.Sleep(100);
                if (fileExists)
                {
                    return true;
                }
                return false;
            }
            catch { return false; }
        }
    }
}
