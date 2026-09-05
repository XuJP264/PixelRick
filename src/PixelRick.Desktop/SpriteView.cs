using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using System.Runtime.InteropServices;
namespace PixelRick.Desktop;

internal sealed class SpriteView : Control, IDisposable
{
    private readonly PetEngine engine;
    private readonly Dictionary<string, Bitmap> images = new();
    private readonly Dictionary<string, byte[]> pixels = new();
    private readonly CharacterDefinition definition;
    public SpriteView(PetEngine engine, CharacterDefinition definition)
    {
        this.engine = engine; this.definition = definition;
        RenderOptions.SetBitmapInterpolationMode(this, BitmapInterpolationMode.None);
        foreach (var (name, animation) in definition.Animations) {
            var bmp = new Bitmap(Path.Combine(definition.Root, animation.File)); images[name] = bmp;
            var data = new byte[bmp.PixelSize.Width * bmp.PixelSize.Height * 4];
            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try { bmp.CopyPixels(new PixelRect(bmp.PixelSize), handle.AddrOfPinnedObject(), data.Length, bmp.PixelSize.Width * 4); } finally { handle.Free(); }
            pixels[name] = data;
        }
    }
    public bool OpaqueAt(double x, double y)
    {
        var p = DesktopCoordinates.SpritePixel(x, y, 0, 0, engine.Config.Scale, engine.Flip);
        if (p.X < 0 || p.X >= 128 || p.Y < 0 || p.Y >= 128 || engine.Opacity < .1) return false;
        var a = engine.Animation; int width = definition.Animations[a.Current].Frames * 128;
        return pixels[a.Current][(p.Y * width + a.FrameIndex * 128 + p.X) * 4 + 3] > 16;
    }
    public override void Render(DrawingContext context)
    {
        base.Render(context);
        if (engine.Effects.Current == "portal") Draw(context, engine.Effects.Animation, false, 1);
        Draw(context, engine.Animation, engine.Flip, engine.Opacity);
        if (engine.Effects.Current is not null and not "portal") Draw(context, engine.Effects.Animation, false, 1);
    }
    private void Draw(DrawingContext context, AnimationClock animation, bool flip, double opacity)
    {
        using var alpha = context.PushOpacity(opacity);
        using var transform = context.PushTransform(flip ? new Matrix(-1, 0, 0, 1, Bounds.Width, 0) : Matrix.Identity);
        context.DrawImage(images[animation.Current], new Rect(animation.FrameIndex * 128, 0, 128, 128), new Rect(0, 0, 128 * engine.Config.Scale, 128 * engine.Config.Scale));
    }
    public void Capture(string path, double backingScale)
    {
        using var target = new RenderTargetBitmap(new PixelSize((int)(Bounds.Width * backingScale), (int)(Bounds.Height * backingScale)), new Vector(96 * backingScale, 96 * backingScale));
        target.Render(this); target.Save(path, PngBitmapEncoderOptions.Default);
    }
    public void Dispose() { foreach (var image in images.Values) image.Dispose(); }
}
