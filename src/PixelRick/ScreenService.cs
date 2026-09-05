using Forms=System.Windows.Forms;
namespace PixelRick;
public sealed class ScreenService
{
    public WorkArea[] Areas(bool multi=true)=> (multi?Forms.Screen.AllScreens:[Forms.Screen.PrimaryScreen!]).Select(s=>new WorkArea(s.WorkingArea.Left,s.WorkingArea.Top,s.WorkingArea.Right,s.WorkingArea.Bottom)).ToArray();
    public WorkArea Nearest(double x,double y,bool multi=true)=>Areas(multi).MinBy(a=>Math.Pow(x-Math.Clamp(x,a.Left,a.Right),2)+Math.Pow(y-Math.Clamp(y,a.Top,a.Bottom),2));
    public static (double X,double Y) Cursor(){NativeMethods.GetCursorPos(out var p);return(p.X,p.Y);}
}
