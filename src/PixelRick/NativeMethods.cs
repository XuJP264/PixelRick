using System.Runtime.InteropServices;
namespace PixelRick;
internal static class NativeMethods
{
    [StructLayout(LayoutKind.Sequential)] internal struct Point { public int X,Y; }
    [DllImport("user32.dll")] internal static extern bool GetCursorPos(out Point point);
    [DllImport("user32.dll")] internal static extern bool SetWindowPos(IntPtr hwnd,IntPtr after,int x,int y,int cx,int cy,uint flags);
    [DllImport("user32.dll",CharSet=CharSet.Unicode)] private static extern IntPtr FindWindow(string? cls,string title);
    [DllImport("user32.dll")] private static extern bool PostMessage(IntPtr hwnd,int msg,IntPtr w,IntPtr l);
    internal const int ShowMessage=0x8001;
    internal static void NotifyExisting()=>PostMessage(FindWindow(null,"PixelRick"),ShowMessage,IntPtr.Zero,IntPtr.Zero);
}
