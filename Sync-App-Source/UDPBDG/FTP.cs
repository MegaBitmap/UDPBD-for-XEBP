using FluentFTP;

namespace UDPBDG;

internal class FTP
{
    public static async Task CreateDirectory(AsyncFtpClient client, string directoryPath, Label logLabel, Panel logPanel)
    {
        for (int i = 0; i < 5; i++)
        {
            try
            {
                await client.Connect();
                await client.CreateDirectory(directoryPath);
                await client.Disconnect();
                if (!await DirectoryExists(client, directoryPath))
                    SyncSNL.WriteLine($"Failed to create the directory {directoryPath} on the PS2 via FTP.\nNo exceptions raised.", logLabel, logPanel);
                else return;
            }
            catch (Exception ex)
            {
                SyncSNL.WriteLine($"Failed to create the directory {directoryPath} on the PS2 via FTP.\n{ex.Message}", logLabel, logPanel);
                await client.Disconnect();
            }
        }
    }

    public static async Task<bool> DirectoryExists(AsyncFtpClient client, string directoryPath)
    {
        try
        {
            await client.Connect();
            await client.GetListing(directoryPath); // for compatibility with ps2ftpd, reconnect every time FtpDataStream is used
            await client.Disconnect();
            await client.Connect();
            await client.GetListing(directoryPath);
            await client.Disconnect();
            return true;
        }
        catch
        {
            await client.Disconnect();
            return false;
        }
    }

    public static async Task UploadFile(AsyncFtpClient client, string filePath, string ftpPath, string targetFile, Label logLabel, Panel logPanel)
    {
        for (int i = 0; i < 5; i++)
        {
            try
            {
                if (await FileExists(client, ftpPath, targetFile))
                {
                    await client.Connect();
                    await client.DeleteFile(ftpPath + targetFile);
                    await client.Disconnect();
                }
                await client.Connect();
                await client.UploadFile(Path.GetFullPath(filePath), ftpPath + targetFile, FtpRemoteExists.NoCheck); // for compatibility with ps2ftpd, reconnect every time FtpDataStream is used
                                                                                                                    // FtpRemoteExists.NoCheck is needed otherwise internal fluentFTP methods will use FtpDataStream more than once
                await client.Disconnect();
                if (!await FileExists(client, ftpPath, targetFile))
                    SyncSNL.WriteLine($"Failed to upload file {filePath} to the PS2 via FTP.\nNo exceptions raised.\nRetrying . . .", logLabel, logPanel);

                FileInfo fileInfo = new(filePath);
                long ftpSize = await GetSize(client, ftpPath, targetFile, logLabel, logPanel);
                if (ftpSize == fileInfo.Length) return;
                SyncSNL.WriteLine($"Target file size: {fileInfo.Length}", logLabel, logPanel);
                SyncSNL.WriteLine($"File size reported by launchELF: {ftpSize}", logLabel, logPanel);
                SyncSNL.WriteLine($"Failed to upload file {filePath} to the PS2 via FTP.\nWrong file size.\nRetrying . . .", logLabel, logPanel);

            }
            catch (Exception ex)
            {
                SyncSNL.WriteLine($"Failed to upload file {filePath} to the PS2 via FTP.\n{ex.Message}\n{ex.InnerException}\nRetrying . . .", logLabel, logPanel);
                await client.Disconnect();
            }
        }
        SyncSNL.WriteLine("FTP.UploadFile Failed too many times.", logLabel, logPanel);
    }

    public static async Task<string> GetDir(AsyncFtpClient client, string ftpPath)
    {
        try
        {
            string returnList = "";
            for (int i = 0; i < 2; i++)
            {
                await client.Connect();
                var ftpList = await client.GetListing(ftpPath); // for compatibility with ps2ftpd, reconnect every time FtpDataStream is used
                await client.Disconnect();
                foreach (var item in ftpList)
                    if (!returnList.Contains(item.ToString()))
                        returnList += $" {item.Name} ";
            }
            return returnList;
        }
        catch
        {
            await client.Disconnect();
            return "";
        }
    }

    public static async Task<long> GetSize(AsyncFtpClient client, string ftpPath, string file, Label logLabel, Panel logPanel)
    {
        try
        {
            await client.Connect();
            var ftpList = await client.GetListing(ftpPath); // for compatibility with ps2ftpd, reconnect every time FtpDataStream is used
            await client.Disconnect();
            foreach (var item in ftpList)
                if (item.Name == file)
                    return item.Size;
        }
        catch (Exception ex)
        {
            SyncSNL.WriteLine($"GetSize() failed to list {ftpPath} via FTP.\n{ex.Message}\n{ex.InnerException}", logLabel, logPanel);
            await client.Disconnect();
        }
        return 0;
    }

    public static async Task<bool> FileExists(AsyncFtpClient client, string ftpPath, string ftpFile)
    {
        try
        {
            await client.Connect();
            var files = await client.GetListing(ftpPath); // for compatibility with ps2ftpd, reconnect every time FtpDataStream is used
            await client.Disconnect();
            foreach (var file in files)
                if (file.Name.Contains(ftpFile)) return true;

            return false;
        }
        catch
        {
            await client.Disconnect();
            return false;
        }
    }
}
