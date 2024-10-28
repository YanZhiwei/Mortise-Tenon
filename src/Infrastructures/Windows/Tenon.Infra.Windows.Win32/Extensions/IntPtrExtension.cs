using Windows.Win32.Foundation;

namespace Tenon.Infra.Windows.Win32.Extensions;

internal static class IntPtrExtension
{
    public static HWND ToHWnd(this IntPtr intPtrHandle)
    {
        return intPtrHandle == IntPtr.Zero ? default : (HWND)intPtrHandle;
    }
}