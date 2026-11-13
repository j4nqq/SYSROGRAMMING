using System;
using System.Runtime.InteropServices;

namespace WinFormsApp1;

public readonly record struct KeyboardHookEvent(uint VirtualKey, bool IsKeyDown, bool IsSystemKey);

internal sealed class LowLevelKeyboardHook : IDisposable
{
    private readonly NativeMethods.HookProc _callback;

    private IntPtr _hookHandle = IntPtr.Zero;

    public event Action<KeyboardHookEvent>? KeyEvent;

    public bool IsInstalled => _hookHandle != IntPtr.Zero;

    public LowLevelKeyboardHook() => _callback = HookCallback;

    public void Install()
    {
        if (IsInstalled) return;

        _hookHandle = NativeMethods.SetWindowsHookEx(
            NativeMethods.WH_KEYBOARD_LL,
            _callback,
            IntPtr.Zero,
            0);

        if (_hookHandle == IntPtr.Zero)
            throw new InvalidOperationException(
                $"Hook не установлен. Win32 error = {Marshal.GetLastWin32Error()}");
    }

    public void Uninstall()
    {
        if (!IsInstalled) return;
        NativeMethods.UnhookWindowsHookEx(_hookHandle);
        _hookHandle = IntPtr.Zero;
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            int message = wParam.ToInt32();

            bool isDown = message is NativeMethods.WM_KEYDOWN or NativeMethods.WM_SYSKEYDOWN;
            bool isUp = message is NativeMethods.WM_KEYUP or NativeMethods.WM_SYSKEYUP;

            if (isDown || isUp)
            {
                var data = Marshal.PtrToStructure<NativeMethods.KBDLLHOOKSTRUCT>(lParam);

                KeyEvent?.Invoke(new KeyboardHookEvent(
                    data.vkCode,
                    isDown,
                    message is NativeMethods.WM_SYSKEYDOWN or NativeMethods.WM_SYSKEYUP));
            }
        }

        return NativeMethods.CallNextHookEx(_hookHandle, nCode, wParam, lParam);
    }

    public void Dispose() => Uninstall();
}