using System.Runtime.InteropServices;

namespace KaedePhi.Tool.App;

internal static class TerminalDetector
{
    public static readonly ITerminalDetector Instance = Create();

    private static ITerminalDetector Create()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return new WindowsTerminalDetector();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return new LinuxTerminalDetector();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return new MacTerminalDetector();

        return new WindowsTerminalDetector();
    }
}
