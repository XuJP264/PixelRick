namespace PixelRick;
public readonly record struct WorkArea(double Left,double Top,double Right,double Bottom)
{
    public double Width=>Right-Left;
    public double Height=>Bottom-Top;
    public bool Contains(double x,double y)=>x>=Left&&x<Right&&y>=Top&&y<Bottom;
}
