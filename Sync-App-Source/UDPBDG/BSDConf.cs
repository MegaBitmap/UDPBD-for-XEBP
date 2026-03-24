namespace UDPBDG;

internal class BSDConf
{
    public static string Udpbd(string ps2ip, string mode)
    {
        return "" +
            "# Name of loaded config, to show to user\n" +
            "name = \"UDPBD BDM driver\"\n\n" +
            "# Drivers this driver depends on (config file must exist)\n" +
            "depends = [\"i_bdm\", \"i_dev9_hidden\"]\n\n" +
            "# Modules to load\n" +
            "[[module]]\n" +
            "file = \"smap.irx\"\n" +
            "env = [\"LE\", \"EE\"]\n" +
            "[[module]]\n" +
            "file = \"ministack.irx\"\n" +
            $"args = [\"ip={ps2ip}\"]\n" +
            "env = [\"LE\", \"EE\"]\n" +
            "[[module]]\n" +
            $"file = \"{mode}.irx\"\n" +
            "env = [\"LE\", \"EE\"]\n";
    }

    public static string Udpfs(string ps2ip)
    {
        return "" +
            "# Name of loaded config, to show to user\n" +
            "name = \"UDPFS driver\"\n\n" +
            "# Drivers this driver depends on (config file must exist)\n" +
            "depends = [\"i_dev9_hidden\"]\n\n" +
            "# Modules to load\n" +
            "[[module]]\n" +
            "file = \"iomanX.irx\"\n" +
            "env = [\"LE\"]\n" +
            "[[module]]\n" +
            "file = \"fileXio.irx\"\n" +
            "env = [\"LE\"]\n" +
            "[[module]]\n" +
            "file = \"smap.irx\"\n" +
            "env = [\"LE\", \"EE\"]\n" +
            "[[module]]\n" +
            "file = \"ministack.irx\"\n" +
            $"args = [\"ip={ps2ip}\"]\n" +
            "env = [\"LE\", \"EE\"]\n" +
            "[[module]]\n" +
            "file = \"udpfs_ioman.irx\"\n" +
            "env = [\"LE\"]\n" +
            "[[module]]\n" +
            "file = \"udpfs_fhi.irx\"\n" +
            "env = [\"EE\"]\n";
    }
}
