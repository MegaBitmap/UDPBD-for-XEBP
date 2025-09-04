using System.Net;
using System.Net.NetworkInformation;
using FluentFTP;

namespace UDPBD_for_XEB__CLI
{
    internal class FTP
    {
        public static bool TestConnection(FtpClient client, IPAddress ps2ip)
        {
            if (!CheckPing(ps2ip)) {  return false; }
            try
            {
                client.Connect();
                client.GetListing(); // for compatibility with ps2ftpd, reconnect every time FtpDataStream is used
                client.Disconnect();
                client.Connect();
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

        public static bool CheckPing(IPAddress ps2ip)
        {
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    Ping pingSender = new();
                    PingReply reply = pingSender.Send(ps2ip, 3000);
                    if (reply.Status == IPStatus.Success)
                    {
                        return true;
                    }
                    else
                    {
                        Console.WriteLine($"\nCONNECTION FAILED\n\nFailed to receive a ping reply:\nPlease verify that your network settings are configured properly and all cables are connected.\nTry adjusting the IP address settings in launchELF.\n{reply.Status}");
                    }
                }
                catch (PingException ex)
                {
                    Console.WriteLine($"\nCONNECTION FAILED\n\nThe network location cannot be reached:\nPlease verify that your network settings are configured properly and all cables are connected.\nTry manually assigning an IPv4 address and subnet mask to this PC.\n{ex.Message}");
                }
            }
            return false;
        }

        public static void CreateDirectory(FtpClient client, string directoryPath)
        {
            for (int i = 0;i < 5; i++)
            {
                try
                {
                    client.CreateDirectory(directoryPath);
                    if (!DirectoryExists(client, directoryPath))
                    {
                        Console.WriteLine($"Failed to create the directory {directoryPath} on the PS2 via FTP.\nNo exceptions raised.");
                        client.Disconnect();
                        client.Connect();
                    }
                    else
                    {
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to create the directory {directoryPath} on the PS2 via FTP.\n{ex.Message}");
                    client.Disconnect();
                    client.Connect();
                }
            }
            client.Disconnect();
            Program.PauseExit(11);
        }

        public static bool DirectoryExists(FtpClient client, string directoryPath)
        {
            try
            {
                client.GetListing(directoryPath); // for compatibility with ps2ftpd, reconnect every time FtpDataStream is used
                client.Disconnect();
                client.Connect();
                client.GetListing(directoryPath);
                client.Disconnect();
                client.Connect();
                return true;
            }
            catch
            {
                client.Disconnect();
                client.Connect();
                return false;
            }
        }

        public static void UploadFile(FtpClient client, string filePath, string ftpPath, string targetFile)
        {
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    if (FileExists(client, ftpPath, targetFile))
                    {
                        client.DeleteFile(ftpPath + targetFile);
                    }
                    client.UploadFile(Path.GetFullPath(filePath), ftpPath + targetFile, FtpRemoteExists.NoCheck); // for compatibility with ps2ftpd, reconnect every time FtpDataStream is used
                    // FtpRemoteExists.NoCheck is needed otherwise internal fluentFTP methods will use FtpDataStream more than once
                    client.Disconnect();
                    client.Connect();
                    if (!FileExists(client, ftpPath, targetFile))
                    {
                        Console.WriteLine($"Failed to upload file {filePath} to the PS2 via FTP.\nNo exceptions raised.\nRetrying . . .");
                    }
                    FileInfo fileInfo = new(filePath);
                    long ftpSize = GetSize(client, ftpPath, targetFile);
                    if (ftpSize == fileInfo.Length) { return; }
                    Console.WriteLine($"Target file size: {fileInfo.Length}");
                    Console.WriteLine($"File size reported by launchELF: {ftpSize}");
                    Console.WriteLine($"Failed to upload file {filePath} to the PS2 via FTP.\nWrong file size.\nRetrying . . .");

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to upload file {filePath} to the PS2 via FTP.\n{ex.Message}\n{ex.InnerException}\nRetrying . . .");
                }
            }
            Console.WriteLine("FTP.UploadFile Failed too many times.");
            client.Disconnect();
            Program.PauseExit(13);
        }

        public static string GetDir(FtpClient client, string ftpPath)
        {
            try
            {
                string returnList = "";
                for (int i = 0; i < 2; i++)
                {
                    var ftpList = client.GetListing(ftpPath); // for compatibility with ps2ftpd, reconnect every time FtpDataStream is used
                    client.Disconnect();
                    client.Connect();
                    foreach (var item in ftpList)
                    {
                        if (!returnList.Contains(item.ToString()))
                        {
                            returnList += $" {item.Name} ";
                        }
                    }
                }
                return returnList;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to list {ftpPath} via FTP.\n{ex.Message}\n{ex.InnerException}");
                client.Disconnect();
                client.Connect();
                return "";
            }
        }

        public static long GetSize(FtpClient client, string ftpPath, string file)
        {
            try
            {
                var ftpList = client.GetListing(ftpPath); // for compatibility with ps2ftpd, reconnect every time FtpDataStream is used
                client.Disconnect();
                client.Connect();
                foreach (var item in ftpList)
                {
                    if (item.Name == file)
                    {
                        return item.Size;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to list {ftpPath} via FTP.\n{ex.Message}\n{ex.InnerException}");
                client.Disconnect();
                client.Connect();
            }
            return 0;
        }

        public static bool FileExists(FtpClient client, string ftpPath, string ftpFile)
        {
            try
            {
                var files = client.GetListing(ftpPath); // for compatibility with ps2ftpd, reconnect every time FtpDataStream is used
                client.Disconnect();
                client.Connect();
                foreach (var file in files)
                {
                    if (file.Name.Contains(ftpFile))
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to list {ftpPath} via FTP.\n{ex.Message}\n{ex.InnerException}");
                client.Disconnect();
                client.Connect();
                return false;
            }
        }
    }
}
