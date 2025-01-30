namespace UDPBD_for_XEB__CLI
{
    internal class CDBin
    {
        public static string ScanBin(FileStream fileIn)
        {
            byte[] CDROMHeaderReference = [0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00];
            int sector_raw_size = 2352;

            int numSectors = (int)(fileIn.Length / sector_raw_size);
            for (int sectorIndex = 0; sectorIndex < numSectors; sectorIndex++)
            {
                fileIn.Position = sectorIndex * sector_raw_size;
                byte[] header = new byte[16];
                fileIn.Read(header, 0, header.Length);
                if (header[0..12].SequenceEqual(CDROMHeaderReference)) return "data";
            }
            return "";
        }

        public static void GenerateISO(FileStream fileIn, string outputISO)
        {
            byte[] CDROMHeaderReference = [0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00];
            int sector_raw_size = 2352;
            int sector_target_size = 2048;

            using FileStream fileOutISO = new(outputISO, FileMode.Create, FileAccess.Write);

            int numSectors = (int)(fileIn.Length / sector_raw_size);
            int sector_offset = 0;
            for (int sectorIndex = 0; sectorIndex < numSectors; sectorIndex++)
            {
                fileIn.Position = sectorIndex * sector_raw_size;
                byte[] header = new byte[16];
                fileIn.Read(header, 0, header.Length);
                if (header[0..12].SequenceEqual(CDROMHeaderReference))
                {
                    int mode = header[15];
                    if (mode == 1) sector_offset = 16;
                    else if (mode == 2) sector_offset = 24;
                    else Console.WriteLine($"Unable to decode the file header for {fileIn.Name}");

                    fileIn.Position = sectorIndex * sector_raw_size + sector_offset;
                    byte[] dataOut = new byte[sector_target_size];
                    fileIn.Read(dataOut, 0, dataOut.Length);
                    fileOutISO.Write(dataOut, 0, dataOut.Length);
                }
            }
        }

        public static void ConvertBin(string inputBin)
        {
            using FileStream fileIn = new(inputBin, FileMode.Open, FileAccess.Read);

            string outputFile = $"{Path.GetDirectoryName(fileIn.Name)}/{Path.GetFileNameWithoutExtension(fileIn.Name)}.iso";

            if (ValidateBin(fileIn) != true || File.Exists(outputFile)) return;
            if (ScanBin(fileIn).Contains("data"))
            {
                GenerateISO(fileIn, outputFile);
                Console.WriteLine($"{Path.GetFileName(outputFile)} was created.");
            }
            return;
        }

        public static bool ValidateBin(FileStream fileIn)
        {
            if (fileIn.Length == 0 || fileIn.Length % 2352 != 0)
            {
                Console.WriteLine($"{Path.GetFileName(fileIn.Name)} is unreadable,\nthe length should be divisible by 2352 (0x930) bytes.");
                return false;
            }
            return true;
        }
    }
}
