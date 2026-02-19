using DiscUtils.Iso9660;
using System.Text.RegularExpressions;

namespace UDPBDG;

internal partial class ISO
{
    public static string GetSerialID(string fullGamePath, Label logLabel, Panel logPanel)
    {
        try
        {
            string content;
            using (FileStream isoStream = File.Open(fullGamePath, FileMode.Open))
            {
                CDReader cd = new(isoStream, true);
                if (!cd.FileExists("SYSTEM.CNF"))
                {
                    SyncSNL.WriteLine($"{fullGamePath} Is not a valid PS2 game ISO. " +
                        "The SYSTEM.CNF file is missing.", logLabel, logPanel);
                    return "";
                }
                using Stream fileStream = cd.OpenFile("SYSTEM.CNF", FileMode.Open);
                using StreamReader reader = new(fileStream);
                content = reader.ReadToEnd();
            }
            if (!content.Contains("BOOT2"))
            {
                SyncSNL.WriteLine($"{fullGamePath} Is not a valid PS2 game ISO.\n" +
                    $"The SYSTEM.CNF file does not contain BOOT2.", logLabel, logPanel);
                return "";
            }
            string serialID = SerialMask().Replace(content.Split("\n")[0], "");
            return serialID;
        }
        catch (Exception)
        {
            SyncSNL.WriteLine($"{fullGamePath} was unable to be read. " +
                $"The ISO file may be corrupt.", logLabel, logPanel);
            return "";
        }
    }

    [GeneratedRegex(@".*\\|;.*")]
    private static partial Regex SerialMask();
}
