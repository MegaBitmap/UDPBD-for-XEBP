using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using static UDPBDTray.TrayNotifyIcon;

namespace UDPBDTray
{
    public partial class CustomConsole : Form
    {
        const int STD_OUTPUT_HANDLE = -11;

        struct COORD
        {
            public short X;
            public short Y;
        }
        struct SMALL_RECT
        {
            public short Left;
            public short Top;
            public short Right;
            public short Bottom;
        }
        struct CONSOLE_SCREEN_BUFFER_INFO
        {
            public COORD dwSize;
            public COORD dwCursorPosition;
            public ushort wAttributes;
            public SMALL_RECT srWindow;
            public COORD dwMaximumWindowSize;
        }
        [LibraryImport("kernel32.dll")]
        private static partial nint GetStdHandle(int nStdHandle);
        [LibraryImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool GetConsoleScreenBufferInfo(nint hConsoleOutput, out CONSOLE_SCREEN_BUFFER_INFO lpConsoleScreenBufferInfo);
        [LibraryImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool SetConsoleCursorPosition(nint hConsoleOutput, COORD dwModedwCursorPosition);
        [LibraryImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool ReadConsoleOutputCharacterA(nint hConsoleOutput, [Out] byte[] lpCharacter,
            uint nLength, COORD dwReadCoord, out uint lpNumberOfCharsRead);

        static COORD origin;
        static COORD rowIndex;
        static readonly byte newLine = (byte)'\n';

        public CustomConsole()
        {
            InitializeComponent();
            Text = Assembly.GetExecutingAssembly().Location + ' '
                + Assembly.GetExecutingAssembly().GetName().Version + ' '
                + ServerName;
            origin.X = 0;
            origin.Y = 0;
            rowIndex.X = 0;
        }

        private async Task ConsoleUpdateAsync()
        {
            nint outHandle = GetStdHandle(STD_OUTPUT_HANDLE);
            GetConsoleScreenBufferInfo(outHandle, out CONSOLE_SCREEN_BUFFER_INFO screenBufferInfo);
            short conWidth = screenBufferInfo.dwMaximumWindowSize.X;

            while (true)
            {
                await Task.Delay(80);
                GetConsoleScreenBufferInfo(outHandle, out screenBufferInfo);
                short conRow = screenBufferInfo.dwCursorPosition.Y;
                short conHeight = (short)(MainPanel.Height >> 4); // divide by 16
                uint nLength;
                if (conRow > conHeight)
                {
                    rowIndex.Y = (short)(conRow - conHeight);
                    nLength = (uint)((conHeight + 1) * conWidth);
                }
                else
                {
                    rowIndex.Y = 0;
                    nLength = (uint)((conRow + 1) * conWidth);
                }
                byte[] conBuffer = new byte[nLength];
                bool read = ReadConsoleOutputCharacterA(outHandle, conBuffer, nLength, rowIndex, out uint lpNumberOfCharsRead);
                if (!read) MessageBox.Show("Error: Failed to read console output");
                if (conRow > 1000 || conRow + 50 > screenBufferInfo.dwSize.Y)
                {
                    ClearSaveConsole();
                }
                string conOut = InjectLF(conBuffer[..(int)lpNumberOfCharsRead], conWidth);
                if (conOut != MainLabel.Text)
                {
                    UpdateText(conOut);
                }
            }
        }

        private static string InjectLF(byte[] input, short charPerLine)
        {
            byte[] temp = new byte[input.Length + (input.Length / charPerLine)];
            int offsetLF = 0;
            for (int i = 0; i < input.Length; i += charPerLine)
            {
                if (i + charPerLine < input.Length)
                {
                    Buffer.BlockCopy(input, i, temp, i + offsetLF, charPerLine);
                    temp[i + offsetLF + charPerLine] = newLine;
                }
                else
                {
                    input[i..].CopyTo(temp, i + offsetLF);
                }
                offsetLF++;
            }
            return Encoding.UTF8.GetString(temp).TrimEnd('\0');
        }

        private static void ClearSaveConsole()
        {
            nint outHandle = GetStdHandle(STD_OUTPUT_HANDLE);
            GetConsoleScreenBufferInfo(outHandle, out CONSOLE_SCREEN_BUFFER_INFO screenBufferInfo);
            short conWidth = screenBufferInfo.dwMaximumWindowSize.X;
            short conRow = screenBufferInfo.dwCursorPosition.Y;
            uint nLengthFull = (uint)((conRow + 1) * conWidth);
            byte[] conBuffer = new byte[nLengthFull];
            bool read = ReadConsoleOutputCharacterA(outHandle, conBuffer, nLengthFull, origin, out uint lpNumberOfCharsRead);
            if (!read) MessageBox.Show("Error: Failed to read console output2");
            Console.Clear();
            SetConsoleCursorPosition(outHandle, origin);
            conHistory.Append(InjectLF(conBuffer[..(int)lpNumberOfCharsRead], conWidth).TrimEnd(' '));
        }

        public static void CopyConsoleHistory()
        {
            nint outHandle = GetStdHandle(STD_OUTPUT_HANDLE);
            GetConsoleScreenBufferInfo(outHandle, out CONSOLE_SCREEN_BUFFER_INFO screenBufferInfo);
            short conWidth = screenBufferInfo.dwMaximumWindowSize.X;
            short conRow = screenBufferInfo.dwCursorPosition.Y;
            uint nLengthFull = (uint)((conRow + 1) * conWidth);
            byte[] conBuffer = new byte[nLengthFull];
            bool read = ReadConsoleOutputCharacterA(outHandle, conBuffer, nLengthFull, origin, out uint lpNumberOfCharsRead);
            if (!read) MessageBox.Show("Error: Failed to read console output3");
            Clipboard.SetText(conHistory.ToString() + InjectLF(conBuffer[..(int)lpNumberOfCharsRead], conWidth));
        }

        private void CustomConsole_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (flagAskShutdown)
            {
                DialogResult response = MessageBox.Show($"Would you like to shut down {ServerName}?",
                    "Shut Down Server", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (response == DialogResult.Yes)
                {
                    Environment.Exit(0);
                }
            }
            flagAskShutdown = true;
            showConsole = false;
            menuItemConsoleToggle.Checked = false;
        }

        private void UpdateText(string conOut)
        {
            MainLabel.Text = conOut;
            MainPanel.VerticalScroll.Value = MainPanel.VerticalScroll.Maximum;
            MainPanel.HorizontalScroll.Value = MainPanel.HorizontalScroll.Minimum;
            MainPanel.PerformLayout();
        }

        private void CopyButton_Click(object sender, EventArgs e)
        {
            CopyConsoleHistory();
        }

        private void CustomConsole_Load(object sender, EventArgs e)
        {
            _ = ConsoleUpdateAsync();
        }
    }
}
