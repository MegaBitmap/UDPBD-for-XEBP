namespace UDPBDG;

internal class CDBin
{
    static readonly byte[] CDROMHeaderReference = [0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00];
    const int sector_raw_size = 2352;
    const int sector_target_size = 2048;

    public static void ConvertFolder(string scanPath)
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
                        ConvertBin(binFile);
                    else
                        MessageBox.Show($"There is not enough space to convert {binFile} to ISO.",
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }

    private static string ScanBin(FileStream fileIn)
    {
        int numSectors = (int)(fileIn.Length / sector_raw_size);
        for (int sectorIndex = 0; sectorIndex < numSectors; sectorIndex++)
        {
            fileIn.Position = sectorIndex * sector_raw_size;
            byte[] header = new byte[16];
            fileIn.ReadExactly(header);
            if (header.AsSpan()[0..12].SequenceEqual(CDROMHeaderReference)) return "data";
        }
        return "";
    }

    private static void GenerateISO(FileStream fileIn, string outputISO)
    {
        using FileStream fileOutISO = new(outputISO, FileMode.Create, FileAccess.Write);

        int numSectors = (int)(fileIn.Length / sector_raw_size);
        int sector_offset = 0;
        for (int sectorIndex = 0; sectorIndex < numSectors; sectorIndex++)
        {
            fileIn.Position = sectorIndex * sector_raw_size;
            byte[] header = new byte[16];
            fileIn.ReadExactly(header);
            if (header.AsSpan()[0..12].SequenceEqual(CDROMHeaderReference))
            {
                int mode = header[15];
                if (mode == 1)
                    sector_offset = 16;
                else if (mode == 2)
                    sector_offset = 24;
                else
                    MessageBox.Show($"Unable to decode the file header for {fileIn.Name}",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                fileIn.Position = sectorIndex * sector_raw_size + sector_offset;
                byte[] dataOut = new byte[sector_target_size];
                fileIn.ReadExactly(dataOut);
                fileOutISO.Write(dataOut, 0, dataOut.Length);
            }
        }
    }

    private static void ConvertBin(string inputBin)
    {
        using FileStream fileIn = new(inputBin, FileMode.Open, FileAccess.Read);
        string outputFile = $"{Path.GetDirectoryName(fileIn.Name)}/{Path.GetFileNameWithoutExtension(fileIn.Name)}.iso";

        if (ValidateBin(fileIn) != true || File.Exists(outputFile)) return;
        if (ScanBin(fileIn).Contains("data"))
            GenerateISO(fileIn, outputFile);

        return;
    }

    private static bool ValidateBin(FileStream fileIn)
    {
        if (fileIn.Length == 0 || fileIn.Length % sector_raw_size != 0)
        {
            MessageBox.Show($"{Path.GetFileName(fileIn.Name)} is unreadable,\n" +
                $"the length should be divisible by 2352 (0x930) bytes.",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
        return true;
    }

    private static bool CheckSpace(string source, string destination)
    {
        FileInfo fileInfo = new(source);
        long fileSize = fileInfo.Length;
        string? dest = Path.GetPathRoot(destination);
        if (dest == null) return false;
        DriveInfo driveInfo = new(dest);
        long availableSpace = driveInfo.AvailableFreeSpace;
        if (availableSpace > fileSize) return true;
        else return false;
    }

    public static bool CheckSpace(int source, string destination)
    {
        long fileSize = source;
        string? dest = Path.GetPathRoot(destination);
        if (dest == null) return false;
        DriveInfo driveInfo = new(dest);
        long availableSpace = driveInfo.AvailableFreeSpace;
        if (availableSpace > fileSize) return true;
        else return false;
    }
}
