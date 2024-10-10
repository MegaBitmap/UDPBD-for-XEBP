using System.IO;
using System.Net;
using System.Windows;

namespace UDPBD_for_XEB_
{
    internal class FTP
    {

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
            }
            catch (WebException ex) { MessageBox.Show($"Failed to upload file {filePath} to the PS2 via FTP.\n\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        public static bool DirectoryExists(string directoryPath)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(directoryPath);
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                using FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                return true;
            }
            catch { return false; }
        }

        public static long GetSize(string ftpUrl)
        {
            long totalSize = 0;
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpUrl);
                request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                using FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                using StreamReader reader = new(response.GetResponseStream());

                string? line = reader.ReadLine();
                while (!string.IsNullOrEmpty(line))
                {
                    string[] tokens = line.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                    if (tokens.Length > 0 && long.TryParse(tokens[4], out long fileSize))
                    {
                        totalSize += fileSize;
                    }
                    line = reader.ReadLine();
                }
            }
            catch (WebException ex) { MessageBox.Show($"Failed to get the size of file {ftpUrl} on the PS2 via FTP.\n\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
            return totalSize;
        }

        public static void CreateDirectory(string directoryPath)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(directoryPath);
                request.Method = WebRequestMethods.Ftp.MakeDirectory;
                using FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            }
            catch (WebException ex) { MessageBox.Show($"Failed to create the directory {directoryPath} on the PS2 via FTP.\n\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        public static void DeleteFolderContents(string url)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url);
                request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                using FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                using System.IO.StreamReader reader = new(response.GetResponseStream());
                while (!reader.EndOfStream)
                {
                    string? line = reader.ReadLine();
                    if (line == null) return;
                    string[] tokens = line.Split(" ", 9, StringSplitOptions.RemoveEmptyEntries);
                    string name = tokens[8];
                    string permissions = tokens[0];
                    string fileUrl = url + "/" + name;

                    if (permissions[0] == 'd' && !fileUrl.EndsWith("/.") && !fileUrl.EndsWith("/..")) DeleteFolderContents(fileUrl + "/");
                    else DeleteFile(fileUrl);
                }
            }
            catch (WebException ex) { MessageBox.Show($"Failed to delete the directory {url} on the PS2 via FTP.\n\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        public static void DeleteFile(string url)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url);
                request.Method = WebRequestMethods.Ftp.DeleteFile;
                using FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                if (FileExists(url)) MessageBox.Show($"Failed to delete the file {url} on the PS2 via FTP.\n\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); // for some reason going from launchELF_isr(2023-10-23) to launchELF(2019-1-11) throws an error once per file then fixes itself ???
            }
        }

        public static bool FileExists(string url)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url);
                request.Method = WebRequestMethods.Ftp.GetFileSize;
                using FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                return true;
            }
            catch { return false; }
        }


    }
}
