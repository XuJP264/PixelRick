using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
namespace PixelRick;
public partial class MainWindow : Window
{
    private readonly SettingsService settings = new();
    private readonly ScreenService screens = new();
    private readonly PetEngine engine;
    private readonly SpriteAnimator animator;
    private readonly SpriteAnimator effectAnimator;
    private readonly DispatcherTimer timer = new() { Interval = TimeSpan.FromMilliseconds(1000d / 30) };
    private readonly Stopwatch clock = Stopwatch.StartNew();
    private double last;
    private bool captured;
    private IntPtr hwnd;
    private readonly string[] args;
    private PetSettings Config => settings.Value;
    private double Scale => Config.Scale;
    public MainWindow(string[] args)
    {
        this.args = args; InitializeComponent(); Diagnostics.Enabled = args.Contains("--diagnostics") || args.Contains("--smoke-test");
        var definition = new CharacterDefinition(); engine = new(Config, definition);
        animator = new(definition, engine.Animation); effectAnimator = new(definition, engine.Effects.Animation);
        engine.SoundRequested += PlaySound; SetupControls();
        SourceInitialized += (_, _) => { hwnd = new WindowInteropHelper(this).Handle; HwndSource.FromHwnd(hwnd).AddHook(Hook); ResetPosition(); };
        MouseLeftButtonDown += OnDown; MouseMove += OnMove; MouseLeftButtonUp += OnUp;
        LostMouseCapture += (_, _) => engine.CancelCapture();
        timer.Tick += (_, _) => Tick(); Loaded += (_, _) => timer.Start();
        Closed += (_, _) => { timer.Stop(); engine.RememberPosition(); settings.Save(); System.Windows.Application.Current.Shutdown(); };
    }
    private IntPtr Hook(IntPtr h, int msg, IntPtr w, IntPtr l, ref bool handled)
    {
        if (msg == NativeMethods.ShowMessage) { Show(); handled = true; }
        if (msg == 0x84) {
            var p = PointFromScreen(new System.Windows.Point((short)(l.ToInt64() & 0xffff), (short)((l.ToInt64() >> 16) & 0xffff)));
            int x = (int)(p.X / ActualWidth * 128), y = (int)(p.Y / ActualHeight * 128);
            if (x >= 0 && x < 128 && y >= 0 && y < 128) {
                if (engine.Flip) x = 127 - x;
                byte[] pixel = new byte[4]; animator.Frame.CopyPixels(new Int32Rect(x, y, 1, 1), pixel, 4, 0);
                if (pixel[3] == 0) { handled = true; return new IntPtr(-1); }
            }
        }
        return IntPtr.Zero;
    }
    private void ResetPosition() { engine.SetScreens(screens.Areas(Config.MultiMonitor)); engine.ResetPosition(); Draw(); }
    private void KeepVisible() { engine.SetScreens(screens.Areas(Config.MultiMonitor)); engine.KeepVisible(); }
    private void Draw()
    {
        Topmost = Config.AlwaysOnTop;
        NativeMethods.SetWindowPos(hwnd, IntPtr.Zero, (int)Math.Round(engine.Movement.X - 64 * Scale), (int)Math.Round(engine.Movement.Y - 120 * Scale), (int)(128 * Scale), (int)(128 * Scale), 0x0014);
        Character.Source = animator.Frame; Character.RenderTransform = new ScaleTransform(engine.Flip ? -1 : 1, 1); Character.Opacity = engine.Opacity;
        EffectBehind.Source = engine.Effects.Current == "portal" ? effectAnimator.Frame : null;
        EffectFront.Source = engine.Effects.Current is not null and not "portal" ? effectAnimator.Frame : null;
    }
    private void Tick()
    {
        double now = clock.Elapsed.TotalSeconds, dt = Math.Clamp(now - last, 0, .05); last = now;
        engine.SetScreens(screens.Areas(Config.MultiMonitor)); var p = ScreenService.Cursor(); engine.Tick(dt, now, p.X, p.Y, IsVisible);
        if (!IsVisible) return; UpdateDemo(now); Draw();
        if (!captured && now > 2 && args.Contains("--capture")) {
            int i = Array.IndexOf(args, "--capture");
            if (i + 1 < args.Length) {
                var bitmap = new System.Windows.Media.Imaging.RenderTargetBitmap((int)ActualWidth, (int)ActualHeight, 96, 96, PixelFormats.Pbgra32); bitmap.Render(this);
                var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder(); encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmap));
                using var stream = System.IO.File.Create(args[i + 1]); encoder.Save(stream);
            }
            captured = true;
        }
        if (args.Contains("--smoke-test") && now > 8) { Diagnostics.Log("SMOKE PASS"); Close(); }
    }
    private void OnDown(object sender, MouseButtonEventArgs e) { var p = ScreenService.Cursor(); if (engine.PointerDown(p.X, p.Y, clock.Elapsed.TotalSeconds, e.ClickCount)) Character.CaptureMouse(); e.Handled = true; }
    private void OnMove(object sender, System.Windows.Input.MouseEventArgs e) { var p = ScreenService.Cursor(); engine.PointerMove(p.X, p.Y, clock.Elapsed.TotalSeconds); if (engine.Dragging) Draw(); }
    private void OnUp(object sender, MouseButtonEventArgs e) { engine.PointerUp(clock.Elapsed.TotalSeconds, System.Windows.Forms.SystemInformation.DoubleClickTime / 1000d); Character.ReleaseMouseCapture(); }
}
