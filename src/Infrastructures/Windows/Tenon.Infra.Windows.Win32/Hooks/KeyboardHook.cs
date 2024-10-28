using System.ComponentModel;
using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using CsWin32 = Windows.Win32;
using SysProcess = System.Diagnostics.Process;

namespace Tenon.Infra.Windows.Win32.Hooks;

public sealed class KeyboardHook
{
    private static CsWin32.UnhookWindowsHookExSafeHandle _hookExSafeHandle = new(IntPtr.Zero);

    public static void Install()
    {
        if (_hookExSafeHandle.IsInvalid)
            _hookExSafeHandle = SetHook(HookCallback);
        if (!_hookExSafeHandle.IsInvalid) return;
        throw new Win32Exception(Marshal.GetLastWin32Error(), "KeyboardHook install failed.");
    }

    public static void Uninstall()
    {
        if (_hookExSafeHandle.IsClosed)
            return;

        unsafe
        {
            // 使用 Unsafe.AsPointer 将 IntPtr 转换为 void*
            void* hookHandlePtr = (void*)_hookExSafeHandle.DangerousGetHandle();

            if (!CsWin32.PInvoke.UnhookWindowsHookEx(new HHOOK(hookHandlePtr)))
                throw new Win32Exception(Marshal.GetLastWin32Error(), "KeyboardHook uninstall failed.");
        }

        _hookExSafeHandle = new CsWin32.UnhookWindowsHookExSafeHandle(IntPtr.Zero);
    }

    private static LRESULT HookCallback(int code, WPARAM wParam, LPARAM lParam)
    {
        if (code < 0) return CsWin32.PInvoke.CallNextHookEx(_hookExSafeHandle, code, wParam, lParam);
        if (wParam == CsWin32.PInvoke.WM_KEYDOWN || wParam == CsWin32.PInvoke.WM_SYSKEYDOWN)
        {
            var keyCode = Marshal.ReadInt32(lParam);
            var key = Enum.IsDefined(typeof(Keys), keyCode) ? (Keys)keyCode : Keys.NoName;
            KeyDown?.Invoke(null, new KeyEventArgs(key));
        }

        if (lParam == CsWin32.PInvoke.WM_KEYUP || lParam == CsWin32.PInvoke.WM_SYSKEYUP)
        {
            var keyCode = Marshal.ReadInt32(lParam);
            var key = Enum.IsDefined(typeof(Keys), keyCode) ? (Keys)keyCode : Keys.NoName;
            KeyUp?.Invoke(null, new KeyEventArgs(key));
        }

        return CsWin32.PInvoke.CallNextHookEx(_hookExSafeHandle, code, wParam, lParam);
    }

    private static CsWin32.UnhookWindowsHookExSafeHandle SetHook(HOOKPROC proc)
    {
        using (var module = SysProcess.GetCurrentProcess().MainModule)
        {
            var hookExSafeHandle = CsWin32.PInvoke.SetWindowsHookEx(WINDOWS_HOOK_ID.WH_KEYBOARD_LL, proc,
                CsWin32.PInvoke.GetModuleHandle(module.ModuleName), 0);
            return hookExSafeHandle;
        }
    }

    public static event KeyEventHandler KeyDown;
    public static event KeyEventHandler KeyUp;
}