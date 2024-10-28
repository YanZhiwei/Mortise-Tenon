using System.ComponentModel;
using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using CsWin32 = Windows.Win32;
using SysProcess = System.Diagnostics.Process;

namespace Tenon.Infra.Windows.Win32.Hooks;

public sealed class MouseHook
{
    private static CsWin32.UnhookWindowsHookExSafeHandle _hookExSafeHandle = new(IntPtr.Zero);

    public static void Install()
    {
        if (_hookExSafeHandle.IsInvalid)
            _hookExSafeHandle = SetHook(HookCallback);
        if (!_hookExSafeHandle.IsInvalid) return;
        throw new Win32Exception(Marshal.GetLastWin32Error(), "MouseHook install failed.");
    }

    public static void Uninstall()
    {
        if (_hookExSafeHandle.IsClosed)
            return;

        unsafe
        {
            // 使用转换将 IntPtr 转为 void*
            var hookHandlePtr = _hookExSafeHandle.DangerousGetHandle().ToPointer();

            if (!CsWin32.PInvoke.UnhookWindowsHookEx(new HHOOK(hookHandlePtr)))
                throw new Win32Exception(Marshal.GetLastWin32Error(), "MouseHook uninstall failed.");
        }

        _hookExSafeHandle = new CsWin32.UnhookWindowsHookExSafeHandle(IntPtr.Zero);
    }

    private static CsWin32.UnhookWindowsHookExSafeHandle SetHook(HOOKPROC proc)
    {
        using (var module = SysProcess.GetCurrentProcess().MainModule)
        {
            var moduleHandle = CsWin32.PInvoke.GetModuleHandle(module.ModuleName);
            return CsWin32.PInvoke.SetWindowsHookEx(WINDOWS_HOOK_ID.WH_MOUSE_LL, proc, moduleHandle, 0);
        }
    }

    private static LRESULT HookCallback(int code, WPARAM wParam, LPARAM lParam)
    {
        if (code < 0) return CsWin32.PInvoke.CallNextHookEx(_hookExSafeHandle, code, wParam, lParam);
        var hookStruct = (MOUSEHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MOUSEHOOKSTRUCT))!;
        switch (wParam.Value)
        {
            case CsWin32.PInvoke.WM_LBUTTONDOWN:
                LeftButtonDown?.Invoke(null,
                    new MouseEventArgs(MouseButtons.Left, 1, hookStruct.pt.X, hookStruct.pt.Y, 0));
                break;
            case CsWin32.PInvoke.WM_LBUTTONUP:
                LeftButtonUp?.Invoke(null,
                    new MouseEventArgs(MouseButtons.Left, 1, hookStruct.pt.X, hookStruct.pt.Y, 0));
                break;
            case CsWin32.PInvoke.WM_RBUTTONDOWN:
                RightButtonDown?.Invoke(null,
                    new MouseEventArgs(MouseButtons.Right, 1, hookStruct.pt.X, hookStruct.pt.Y, 0));
                break;
            case CsWin32.PInvoke.WM_RBUTTONUP:
                RightButtonUp?.Invoke(null,
                    new MouseEventArgs(MouseButtons.Right, 1, hookStruct.pt.X, hookStruct.pt.Y, 0));
                break;
            case CsWin32.PInvoke.WM_MOUSEMOVE:
                MouseMove?.Invoke(null,
                    new MouseEventArgs(MouseButtons.None, 1, hookStruct.pt.X, hookStruct.pt.Y, 0));
                break;
            case CsWin32.PInvoke.WM_MOUSEWHEEL:
                MouseWheel?.Invoke(null,
                    new MouseEventArgs(MouseButtons.None, 1, hookStruct.pt.X, hookStruct.pt.Y, 0));
                break;
            case CsWin32.PInvoke.WM_LBUTTONDBLCLK:
                DoubleClick?.Invoke(null,
                    new MouseEventArgs(MouseButtons.Left, 2, hookStruct.pt.X, hookStruct.pt.Y, 0));
                break;
            case CsWin32.PInvoke.WM_MBUTTONDOWN:
                MiddleButtonDown?.Invoke(null,
                    new MouseEventArgs(MouseButtons.Middle, 1, hookStruct.pt.X, hookStruct.pt.Y, 0));
                break;
            case CsWin32.PInvoke.WM_MBUTTONUP:
                MiddleButtonUp?.Invoke(null,
                    new MouseEventArgs(MouseButtons.Middle, 1, hookStruct.pt.X, hookStruct.pt.Y, 0));
                break;
        }

        return CsWin32.PInvoke.CallNextHookEx(_hookExSafeHandle, code, wParam, lParam);
    }

    public static event MouseEventHandler LeftButtonDown;
    public static event MouseEventHandler LeftButtonUp;
    public static event MouseEventHandler RightButtonDown;
    public static event MouseEventHandler RightButtonUp;
    public static event MouseEventHandler MouseMove;
    public static event MouseEventHandler MouseWheel;
    public static event MouseEventHandler DoubleClick;
    public static event MouseEventHandler MiddleButtonDown;
    public static event MouseEventHandler MiddleButtonUp;
}