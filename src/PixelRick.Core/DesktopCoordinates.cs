namespace PixelRick;
public static class DesktopCoordinates
{
    // Cocoa points have a bottom-left origin; the engine uses top-left-origin points.
    public static WorkArea FromCocoa(double x, double y, double width, double height, double primaryTop)
        => new(x, primaryTop - y - height, x + width, primaryTop - y);
    public static (double X, double Y) CocoaPoint(double x, double y, double primaryTop) => (x, primaryTop - y);
    public static (int X, int Y) SpritePixel(double x, double y, double left, double top, double scale, bool flip)
    {
        int px = (int)Math.Floor((x - left) / scale), py = (int)Math.Floor((y - top) / scale);
        return (flip ? 127 - px : px, py);
    }
}
