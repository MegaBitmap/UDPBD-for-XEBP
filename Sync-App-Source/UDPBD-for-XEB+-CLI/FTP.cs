using System.Net;
using System.Net.NetworkInformation;

namespace UDPBD_for_XEB__CLI
{
    internal class FTP
    {
        public static bool TestConnection(IPAddress ps2ip)
        {
            Ping pingSender = new();
            PingReply reply = pingSender.Send(ps2ip, 6000);
            if (!(reply.Status == IPStatus.Success))
            {
                Console.WriteLine($"Connection to {ps2ip} failed: {reply.Status}");
                return false;
            }
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"ftp://{ps2ip}");
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                using FtpWebResponse response = (FtpWebResponse)request.GetResponse();

                Console.WriteLine("Connected to the PS2's FTP server Successfully!");
                Thread.Sleep(100);
                return true;
            }
            catch (WebException ex)
            {
                Console.WriteLine($"Failed to connect to the PS2's FTP server.\n{ex.Message}");
                return false;
            }
        }

        public static bool DirectoryExists(string directoryPath)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(directoryPath);
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                using FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                Thread.Sleep(100);
                return true;
            }
            catch { return false; }
        }

        public static void CreateDirectory(string directoryPath)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(directoryPath);
                request.Method = WebRequestMethods.Ftp.MakeDirectory;
                using FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                Thread.Sleep(200);
            }
            catch (WebException ex)
            {
                Console.WriteLine($"Failed to create the directory {directoryPath} on the PS2 via FTP.\n{ex.Message}");
                Program.PauseExit(10);
            }
        }

        public static void UploadFile(string ftpUrl, string filePath)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpUrl);
                request.Method = WebRequestMethods.Ftp.UploadFile;

                using (FileStream fileStream = File.OpenRead(filePath))
                using (Stream requestStream = request.GetRequestStream())
                {
                    fileStream.CopyTo(requestStream);
                }
                using FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                Thread.Sleep(200);
            }
            catch (WebException ex)
            {
                Console.WriteLine($"Failed to upload file {filePath} to the PS2 via FTP.\n{ex.Message}");
                Program.PauseExit(11);
            }
        }

        public static bool FileExists(string ftpUrl)
        {
            try
            {
                var request = (FtpWebRequest)WebRequest.Create(ftpUrl);
                request.Method = WebRequestMethods.Ftp.GetFileSize;
                using var response = (FtpWebResponse)request.GetResponse();
                Thread.Sleep(100);
                return true;
            }
            catch { return false; }
        }
    }
}
