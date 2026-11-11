using System.Runtime.InteropServices;

namespace ConsoleApp13.Interop;

public static class WindowsApi
{
    [DllImport(
        "kernel32.dll",
        EntryPoint = "GetCurrentProcessId",
        CallingConvention = CallingConvention.Winapi)]
    public static extern uint GetCurrentProcessId();
}
