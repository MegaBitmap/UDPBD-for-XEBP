using System.Text;

namespace UDPBD_for_XEB__CLI;

internal class GameID
{
    const int ISO_SECTOR_SIZE = 2048;
    static readonly byte[] VOLUME_DESCRIPTOR_HEADER = [1, 0x43, 0x44, 0x30, 0x30, 0x31, 1];
    static readonly byte[] SYSTEM_CNF = Encoding.UTF8.GetBytes("SYSTEM.CNF");

    public static string Get(string iso_path)
    {
        long iso_size = new FileInfo(iso_path).Length;
        if (iso_size % ISO_SECTOR_SIZE != 0 || iso_size == 0)
        {
            Console.WriteLine("Sector size mismatch");
            return "";
        }
        using FileStream iso_stream = File.OpenRead(iso_path);
        using BinaryReader iso_reader = new(iso_stream);
        iso_reader.BaseStream.Seek(0x8000, SeekOrigin.Begin);
        if (!iso_reader.ReadBytes(7).SequenceEqual(VOLUME_DESCRIPTOR_HEADER))
        {
            Console.WriteLine("Failed to find the primary volume descriptor");
            return "";
        }
        iso_reader.BaseStream.Seek(0x809E, SeekOrigin.Begin);
        long lba_location = (long)iso_reader.ReadUInt32() * ISO_SECTOR_SIZE;
        iso_reader.BaseStream.Seek(0x80A6, SeekOrigin.Begin);
        long lba_end = iso_reader.ReadUInt32() + lba_location;
        iso_reader.BaseStream.Seek(lba_location, SeekOrigin.Begin);
        long systemcnf_location = 0;
        int systemcnf_size = 0;
        while (iso_reader.BaseStream.Position < lba_end)
        {
            int entry_length = iso_reader.ReadByte();
            byte[] entry = iso_reader.ReadBytes(entry_length - 1);
            if (entry.AsSpan()[32..42].SequenceEqual(SYSTEM_CNF))
            {
                systemcnf_location = (long)BitConverter.ToUInt32(entry.AsSpan()[1..5]) * ISO_SECTOR_SIZE;
                systemcnf_size = (int)BitConverter.ToUInt32(entry.AsSpan()[9..13]);
                break;
            }
        }
        if (systemcnf_location == 0)
        {
            Console.WriteLine("Failed to find SYSTEM.CNF");
            return "";
        }
        iso_reader.BaseStream.Seek(systemcnf_location, SeekOrigin.Begin);
        string systemcnf_content = Encoding.UTF8.GetString(iso_reader.ReadBytes(systemcnf_size));
        if (!systemcnf_content.Contains("BOOT2"))
        {
            Console.WriteLine("BOOT2 is missing in the ISO's SYSTEM.CNF");
            return "";
        }
        string game_id = systemcnf_content.Split("cdrom0")[1].Split(';')[0].Replace(":", "").Replace("\\", "");
        if (game_id.Length != 11 || game_id[4] != '_' || game_id[8] != '.')
        {
            Console.WriteLine($"{game_id} is not a valid PS2 game ID");
            return "";
        }
        return game_id;
    }
}
