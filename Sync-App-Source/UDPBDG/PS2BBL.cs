namespace UDPBDG;

internal class PS2BBL
{
    public static string Config(string installDevice)
    {
        return "" +
            "# PlayStation2 Basic BootLoader config file\n" +
            "SKIP_PS2LOGO = 0\n" +
            "KEY_READ_WAIT_TIME = 400\n" +
            "OSDHISTORY_READ = 1\n" +
            "TRAY_EJECT = 1\n\n" +
            $"LK_AUTO_E1 = {installDevice}\n" +
            "LK_AUTO_E2 = mc?:/APPS/OPNPS2LD.ELF\n" +
            "LK_AUTO_E3 = mc?:/OPL/OPNPS2LD.ELF\n\n" +
            "LK_R1_E1 = mc?:/BOOT/ULE.ELF\n" +
            "LK_R1_E2 = mc?:/APPS/ULE.ELF\n" +
            "LK_R1_E3 = mc?:/BOOT/BOOT.ELF\n\n" +
            "LK_SELECT_E1 = rom0:TESTMODE\n" +
            "LK_START_E1 = $OSDSYS\n\n" +
            "LK_DOWN_E3 = $OSDSYS\n\n" +
            "LK_CROSS_E1 = $CDVD\n" +
            "LK_TRIANGLE_E1 = $CDVD_NO_PS2LOGO\n" +
            "LK_SQUARE_E1 = $CREDITS\n";
    }
}
