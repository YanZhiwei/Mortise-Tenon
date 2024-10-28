using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Dwm;
using Windows.Win32.UI.WindowsAndMessaging;
using AutoMapper;
using Tenon.Infra.Windows.Win32.Extensions;
using Tenon.Infra.Windows.Win32.Models;
using CsWin32 = Windows.Win32;

namespace Tenon.Infra.Windows.Win32;

public sealed class Window
{
    private static readonly IMapper Mapper;

    static Window()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<RECT, Rect>()
                .ForMember(dest => dest.Height, opt => opt.MapFrom(src => src.Height))
                .ForMember(dest => dest.Width, opt => opt.MapFrom(src => src.Width))
                .ForMember(dest => dest.IsEmpty, opt => opt.MapFrom(src => src.IsEmpty))
                .ForMember(dest => dest.X, opt => opt.MapFrom(src => src.X))
                .ForMember(dest => dest.Y, opt => opt.MapFrom(src => src.Y));
        });
        Mapper = config.CreateMapper();
    }

    /// <summary>
    ///     获取指定窗口的矩形区域。
    /// </summary>
    /// <param name="intPtrHandle">窗口句柄的IntPtr。</param>
    /// <returns>返回窗口的矩形区域，如果获取失败则返回null。</returns>
    public static Rect? GetRect(IntPtr intPtrHandle)
    {
        var hWnd = intPtrHandle.ToHWnd();
        if (hWnd == IntPtr.Zero || hWnd.IsNull()) return null;
        if (CsWin32.PInvoke.GetWindowRect(hWnd, out var rect)) return Mapper.Map<RECT, Rect>(rect);
        return null;
    }

    /// <summary>
    ///     获取指定窗口的矩形区域。
    /// </summary>
    /// <param name="intPtrHandle">窗口句柄的IntPtr。</param>
    /// <returns>返回窗口的矩形区域，如果获取失败则返回null。</returns>
    public static Rectangle? GetRectangle(IntPtr intPtrHandle)
    {
        var hWnd = intPtrHandle.ToHWnd();
        if (hWnd == IntPtr.Zero || hWnd.IsNull()) return null;
        if (CsWin32.PInvoke.GetWindowRect(hWnd, out var rect))
            return new Rectangle(rect.left, rect.top, rect.Width, rect.Height);
        return null;
    }

    /// <summary>
    ///     显示指定窗口的操作。
    /// </summary>
    /// <param name="intPtrHandle">窗口句柄的IntPtr。</param>
    /// <param name="showWindowCommand">要执行的窗口显示命令。</param>
    /// <returns>如果成功显示窗口，则返回true；否则返回false。</returns>
    public static bool Show(IntPtr intPtrHandle, ShowWindowCommand showWindowCommand)
    {
        var hWnd = intPtrHandle.ToHWnd();
        if (hWnd == IntPtr.Zero || hWnd.IsNull()) return false;
        var showCmdIndex = (int)showWindowCommand;
        return CsWin32.PInvoke.ShowWindow(hWnd, (SHOW_WINDOW_CMD)showCmdIndex);
    }

    /// <summary>
    ///     设置窗口的位置和大小
    /// </summary>
    /// <param name="intPtrHandle">要设置位置和大小的窗口的句柄</param>
    /// <param name="x">窗口左上角相对于其父窗口或屏幕的坐标</param>
    /// <param name="y">窗口左上角相对于其父窗口或屏幕的坐标</param>
    /// <param name="cx">窗口的宽度和高度</param>
    /// <param name="cy">窗口的宽度和高度</param>
    /// <param name="windowHandles">指定相对于其位置将设置窗口位置的窗口的句柄，可以是特殊句柄，如HWND_TOPMOST、HWND_NOTOPMOST、HWND_TOP或HWND_BOTTOM。</param>
    /// <param name="setWindowPosFlags">控制窗口大小和位置的标志，可以是SWP_NOSIZE、SWP_NOMOVE、SWP_NOZORDER、SWP_FRAMECHANGED等标志的组合。</param>
    /// <returns></returns>
    public static bool SetPos(IntPtr intPtrHandle, int x, int y, int cx, int cy, SpecialWindowHandles windowHandles,
        SetWindowPosFlags setWindowPosFlags)
    {
        var hWnd = intPtrHandle.ToHWnd();
        if (hWnd == IntPtr.Zero || hWnd.IsNull()) return false;
        var windowHandlesHwnd = (HWND)(IntPtr)windowHandles;
        return CsWin32.PInvoke.SetWindowPos(
            hWnd,
            windowHandlesHwnd,
            x,
            y,
            cx,
            cy,
            (SET_WINDOW_POS_FLAGS)setWindowPosFlags);
    }

    /// <summary>
    ///     获取指定窗口的长整型值。
    /// </summary>
    /// <param name="intPtrHandle">窗口句柄的IntPtr。</param>
    /// <param name="windowLongPtr">要获取的长整型值的索引。</param>
    /// <returns>返回窗口的长整型值。</returns>
    public static IntPtr GetLong(IntPtr intPtrHandle, WindowLongPtrIndex windowLongPtr)
    {
        var hWnd = intPtrHandle.ToHWnd();
        if (hWnd == IntPtr.Zero || hWnd.IsNull()) return IntPtr.Zero;
        var longPtrIndex = (int)windowLongPtr;
        return CsWin32.PInvoke.GetWindowLong(hWnd, (WINDOW_LONG_PTR_INDEX)longPtrIndex);
    }

    /// <summary>
    ///     设置指定窗口的长整型值。
    /// </summary>
    /// <param name="intPtrHandle">窗口句柄的IntPtr。</param>
    /// <param name="windowLongPtr">要设置的长整型值的索引。</param>
    /// <param name="dwNewLong">要设置的新的长整型值。</param>
    /// <returns>返回设置后的窗口长整型值。</returns>
    public static IntPtr SetLong(IntPtr intPtrHandle, WindowLongPtrIndex windowLongPtr, int dwNewLong)
    {
        var hWnd = intPtrHandle.ToHWnd();
        if (hWnd == IntPtr.Zero || hWnd.IsNull()) return IntPtr.Zero;
        var longPtrIndex = (int)windowLongPtr;
        return CsWin32.PInvoke.SetWindowLong(hWnd, (WINDOW_LONG_PTR_INDEX)longPtrIndex, dwNewLong);
    }

    /// <summary>
    ///     搜索与指定窗口类名和/或窗口名称匹配的窗口
    /// </summary>
    /// <param name="hWndParent">要搜索其子窗口的父窗口的句柄。要搜索所有窗口，请将此参数指定为0。</param>
    /// <param name="lpszClass">要查找的窗口的窗口类的名称。指定空字符串以忽略类。</param>
    /// <param name="hWndChildAfter">指定开始搜索的子窗口的句柄。搜索将从Z顺序中紧随此窗口后的子窗口开始。如果这是0，则搜索将从hwndParent的第一个子窗口开始。</param>
    /// <param name="lpszWindow"></param>
    /// <returns>要查找的窗口的标题栏文本的名称。指定空字符串以忽略窗口的标题。</returns>
    public static IntPtr FindEx(IntPtr hWndParent, string lpszClass, IntPtr? hWndChildAfter = null,
        string? lpszWindow = null)
    {
        if (hWndChildAfter.HasValue)
            CsWin32.PInvoke.FindWindowEx(hWndParent.ToHWnd(), hWndChildAfter.Value.ToHWnd(), lpszClass, lpszWindow);
        return CsWin32.PInvoke.FindWindowEx(hWndParent.ToHWnd(), default(HWND), lpszClass, lpszWindow);
    }

    /// <summary>
    ///     获取一个窗口的边界矩形,该矩形包括了窗口本身的边界以及可能的装饰（例如标题栏、边框）和 DWM 添加的额外区域（例如阴影效果）。
    /// </summary>
    /// <param name="intPtrHandle">窗口句柄的IntPtr。</param>
    /// <returns>窗口的边界矩形</returns>
    public static Rectangle? GetExtendedFrameBounds(IntPtr intPtrHandle)
    {
        var hWnd = intPtrHandle.ToHWnd();
        if (hWnd == IntPtr.Zero || hWnd.IsNull()) return null;
        var extendedFrameBounds = new RECT();
        unsafe
        {
            var sizeOfRect = Marshal.SizeOf(typeof(RECT));
            CsWin32.PInvoke.DwmGetWindowAttribute(hWnd, DWMWINDOWATTRIBUTE.DWMWA_EXTENDED_FRAME_BOUNDS,
                &extendedFrameBounds, (uint)sizeOfRect);

            if (!extendedFrameBounds.IsEmpty)
                return new Rectangle(extendedFrameBounds.left, extendedFrameBounds.top, extendedFrameBounds.Width,
                    extendedFrameBounds.Height);
        }

        return null;
    }

    public static IntPtr GetTop(Point point = default)
    {
        var posPoint = point != default ? point : CsWin32.PInvoke.GetCursorPos(out var p) ? p : default;
        return GetTopWindowHandle(CsWin32.PInvoke.WindowFromPoint(posPoint));
    }

    public static IntPtr Get(Point point = default)
    {
        var posPoint = point != default ? point : CsWin32.PInvoke.GetCursorPos(out var p) ? p : default;
        return CsWin32.PInvoke.WindowFromPoint(posPoint);
    }

    public static IntPtr GetProcessId(IntPtr intPtrHandle)
    {
        var hWnd = intPtrHandle.ToHWnd();
        if (hWnd == IntPtr.Zero || hWnd.IsNull()) return IntPtr.Zero;
        uint processId = 0;
        unsafe
        {
            var processIdPtr = &processId;
            var threadId = CsWin32.PInvoke.GetWindowThreadProcessId(hWnd, processIdPtr);
            return (IntPtr)(*processIdPtr);
        }
    }

    /// <summary>
    ///     用于获取指定窗口的类名
    /// </summary>
    /// <param name="intPtrHandle">窗口的句柄</param>
    /// <returns>窗口的类名</returns>
    public static string GetClassName(IntPtr intPtrHandle)
    {
        var hWnd = intPtrHandle.ToHWnd();
        if (hWnd == IntPtr.Zero || hWnd.IsNull()) return string.Empty;
        var nMaxCount = 256;
        unsafe
        {
            fixed (char* lpClassNameChars = new char[nMaxCount])
            {
                var lpClassName = new PWSTR(lpClassNameChars);
                var length = CsWin32.PInvoke.GetClassName(hWnd, lpClassName, nMaxCount);
                return length > 0 ? lpClassName.ToString() : string.Empty;
            }
        }
    }

    private static IntPtr GetTopWindowHandle(IntPtr hWnd)
    {
        var parentHandle = CsWin32.PInvoke.GetParent(hWnd.ToHWnd());
        return parentHandle == default ? hWnd : GetTopWindowHandle(parentHandle);
    }
}