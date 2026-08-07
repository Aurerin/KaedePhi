using System.Runtime.InteropServices;

namespace KaedePhi.Tool.App;

internal sealed class LinuxTerminalDetector : ITerminalDetector
{
    public bool IsInteractiveTerminal()
    {
        return isatty(1) == 1;
    }

    [DllImport("libc", SetLastError = true)]
    private static extern int isatty(int fd);
}
