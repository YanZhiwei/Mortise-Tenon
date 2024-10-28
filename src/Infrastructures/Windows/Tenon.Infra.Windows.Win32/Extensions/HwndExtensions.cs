using Windows.Win32.Foundation;

namespace Tenon.Infra.Windows.Win32.Extensions;

internal static unsafe class HwndExtensions
{
    public static bool IsNull(this HWND hWnd)
    {
        return hWnd.Value == null;
    }
}