namespace UDPBDG;

sealed class PyConsoleWrite
{
    public static void flush()
    {
        // ignore flush calls
    }
    public static void write(string s)
    {
        Console.Write(s);
    }
}
