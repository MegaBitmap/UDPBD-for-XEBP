namespace UDPBDG;

internal class XEBPConf
{
    public static string Set(string pathPrefix)
    {
        return "" +
            "NEUTRINO_Bsd = \"\"\n" +
            "NEUTRINO_Fs = \"\"\n" +
            $"NEUTRINO_PathPrefix = \"{pathPrefix}\"\n" +
            "NEUTRINO_ListFile = \"mass:XEBPLUS/CFG/neutrinoLauncher/neutrinoUDPBD.list\"\n" +
            "NEUTRINO_DataFolder = \"mass:XEBPLUS/CFG/neutrinoLauncher/UDPBD/\"\n\n" +
            "dofile(xebLua_AppWorkingPath..\"neutrinoLauncher.lua\")\n";
    }
}
