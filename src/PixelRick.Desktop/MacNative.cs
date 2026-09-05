using System.Runtime.InteropServices;
namespace PixelRick.Desktop;
internal static class MacNative
{
    private const string Lib = "PixelRickMac";
    [StructLayout(LayoutKind.Sequential)] internal struct Screen { public double Left, Top, Right, Bottom, Scale; }
    [DllImport(Lib)] internal static extern int pr_screens([Out] Screen[] screens, int capacity);
    [DllImport(Lib)] internal static extern void pr_cursor(out double x, out double y);
    [DllImport(Lib)] internal static extern double pr_double_click();
    [DllImport(Lib)] internal static extern void pr_configure(IntPtr handle);
    [DllImport(Lib)] internal static extern void pr_frame(IntPtr handle, double left, double top, double width, double height, int topmost);
    [DllImport(Lib)] internal static extern void pr_ignore(IntPtr handle, int ignore);
    [DllImport(Lib)] internal static extern int pr_ignoring(IntPtr handle);
    [DllImport(Lib)] internal static extern int pr_is_opaque(IntPtr handle);
    [DllImport(Lib)] internal static extern int pr_is_accessory();
    [DllImport(Lib)] internal static extern double pr_backing_scale(IntPtr handle);
    [DllImport(Lib)] internal static extern void pr_show_existing();
    [DllImport(Lib)] internal static extern int pr_take_show_requested();
    [DllImport(Lib)] internal static extern void pr_show(IntPtr handle);
    [DllImport(Lib)] internal static extern int pr_login_status();
    [DllImport(Lib)] internal static extern int pr_login_set(int enabled);
    [DllImport(Lib)] internal static extern void pr_sound();
    [DllImport(Lib)] internal static extern void pr_test_mouse(IntPtr handle, int kind, double x, double y, int clicks);
    internal static WorkArea[] Areas() { var result = new Screen[32]; int count = pr_screens(result, result.Length); return result.Take(count).Select(s => new WorkArea(s.Left, s.Top, s.Right, s.Bottom)).ToArray(); }
    internal static (double X, double Y) Cursor() { pr_cursor(out double x, out double y); return (x, y); }
}

internal sealed class MacStartupService : IStartupService
{
    public bool Enabled => MacNative.pr_login_status() is 1 or 2;
    public void SetEnabled(bool enabled)
    {
        if (MacNative.pr_login_set(enabled ? 1 : 0) == 0) throw new InvalidOperationException("macOS could not update Launch at Login. Install PixelRick in Applications and check System Settings → General → Login Items.");
    }
}
