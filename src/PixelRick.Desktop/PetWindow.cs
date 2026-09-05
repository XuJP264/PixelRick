using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using System.Diagnostics;
namespace PixelRick.Desktop;

internal sealed partial class PetWindow : Window
{
    private readonly IClassicDesktopStyleApplicationLifetime lifetime;
    private readonly ConfigurationStore settings;
    private readonly PetEngine engine;
    private readonly SpriteView sprite;
    private readonly DispatcherTimer timer = new() { Interval = TimeSpan.FromMilliseconds(1000d / 60) };
    private readonly Stopwatch clock = Stopwatch.StartNew();
    private readonly bool mac = OperatingSystem.IsMacOS();
    private readonly List<TrayIcon> trays = new();
    private Window? settingsWindow;
    private IntPtr native;
    private double last, screensAt;
    private bool menuOpen;
    private (double X, double Y) fallbackCursor;
    internal PetWindow(IClassicDesktopStyleApplicationLifetime lifetime, string configFolder)
    {
        this.lifetime = lifetime; settings = new(configFolder);
        engine = new(settings.Value, new CharacterDefinition()); sprite = new(engine, new());
        Title = "PixelRick"; Width = Height = 128 * engine.Config.Scale;
        WindowDecorations = WindowDecorations.None; CanResize = false; ShowInTaskbar = false; ShowActivated = false;
        TransparencyLevelHint = [WindowTransparencyLevel.Transparent]; Background = Brushes.Transparent;
        Topmost = engine.Config.AlwaysOnTop; Content = sprite;
        Icon = new WindowIcon(Path.Combine(AppContext.BaseDirectory, "Assets", "app.ico"));
        engine.SoundRequested += () => { if (mac && engine.Config.Sound) MacNative.pr_sound(); };
        SetupMenus();
        Opened += (_, _) => {
            native = TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
            if (mac) { if (native == IntPtr.Zero) throw new InvalidOperationException("Missing native window"); MacNative.pr_configure(native); }
            RefreshScreens(); engine.ResetPosition(); UpdateWindow(); timer.Start();
            if (lifetime.Args?.Contains("--self-test") == true) StartValidation();
        };
        PointerPressed += (_, e) => {
            if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
            var p = CursorPosition(); if (engine.PointerDown(p.X, p.Y, clock.Elapsed.TotalSeconds, e.ClickCount)) e.Pointer.Capture(sprite);
            e.Handled = true;
        };
        PointerMoved += (_, e) => { var local = e.GetPosition(this); fallbackCursor = (engine.Movement.X - 64 * engine.Config.Scale + local.X, engine.Movement.Y - 120 * engine.Config.Scale + local.Y); var p = CursorPosition(); engine.PointerMove(p.X, p.Y, clock.Elapsed.TotalSeconds); };
        PointerReleased += (_, e) => { if (e.InitialPressMouseButton == MouseButton.Left) { engine.PointerUp(clock.Elapsed.TotalSeconds, mac ? MacNative.pr_double_click() : .5); e.Pointer.Capture(null); } };
        PointerCaptureLost += (_, _) => engine.CancelCapture();
        timer.Tick += (_, _) => Tick();
        Closed += (_, _) => { timer.Stop(); engine.RememberPosition(); settings.Save(); sprite.Dispose(); foreach (var tray in trays) tray.Dispose(); settingsWindow?.Close(); lifetime.Shutdown(); };
    }
    private void RefreshScreens()
    {
        if (mac) engine.SetScreens(MacNative.Areas());
        else engine.SetScreens(Screens.All.Select(s => new WorkArea(s.WorkingArea.X, s.WorkingArea.Y, s.WorkingArea.Right, s.WorkingArea.Bottom)));
    }
    private (double X, double Y) CursorPosition() => mac ? MacNative.Cursor() : fallbackCursor;
    private void Tick()
    {
        if (mac && MacNative.pr_take_show_requested() != 0) ShowRick();
        double now = clock.Elapsed.TotalSeconds, dt = now - last; last = now;
        if (now > screensAt) { RefreshScreens(); screensAt = now + 1; }
        var p = CursorPosition(); engine.Tick(dt, now, p.X, p.Y, IsVisible);
        if (IsVisible) UpdateWindow();
        UpdateValidation(now);
    }
    private void UpdateWindow()
    {
        double scale = engine.Config.Scale; Width = Height = 128 * scale;
        double left = engine.Movement.X - 64 * scale, top = engine.Movement.Y - 120 * scale;
        if (mac) {
            MacNative.pr_frame(native, left, top, Width, Height, engine.Config.AlwaysOnTop ? 1 : 0);
            var p = CursorPosition(); bool interactive = engine.Pressed || menuOpen || sprite.OpaqueAt(p.X - left, p.Y - top);
            MacNative.pr_ignore(native, interactive ? 0 : 1);
        } else { Position = new PixelPoint((int)left, (int)top); Topmost = engine.Config.AlwaysOnTop; }
        sprite.InvalidateVisual();
    }
    private void ShowRick() { Show(); engine.KeepVisible(); if (mac) MacNative.pr_show(native); }
    private void SetWander(bool enabled) { engine.SetWander(enabled); settings.Save(); }
    private void SetupMenus()
    {
        var context = new ContextMenu();
        var commands = new (string Label, Action Action)[] { ("Show Rick", ShowRick), ("Hide Rick", Hide), ("Wander", () => SetWander(true)), ("Stay", () => SetWander(false)), ("Sleep", engine.Sleep), ("Wake Up", engine.Wake), ("Teleport", engine.Teleport), ("Reset Position", () => engine.ResetPosition(false)), ("Settings", OpenSettings), ("Quit", Close) };
        foreach (var (label, action) in commands) { var item = new MenuItem { Header = label }; item.Click += (_, _) => action(); context.Items.Add(item); }
        context.Opened += (_, _) => menuOpen = true; context.Closed += (_, _) => menuOpen = false; sprite.ContextMenu = context;
        var nativeMenu = new NativeMenu();
        foreach (var (label, action) in commands) { var item = new NativeMenuItem(label); item.Click += (_, _) => action(); nativeMenu.Items.Add(item); }
        var tray = new TrayIcon { Icon = new WindowIcon(Path.Combine(AppContext.BaseDirectory, "Assets", "app.ico")), ToolTipText = "PixelRick", Menu = nativeMenu, IsVisible = true };
        tray.Clicked += (_, _) => ShowRick(); trays.Add(tray);
        var icons = new TrayIcons(); icons.Add(tray); TrayIcon.SetIcons(Application.Current!, icons);
    }
    private void OpenSettings()
    {
        if (settingsWindow is not null) { settingsWindow.Activate(); return; }
        settingsWindow = new PreferencesWindow(settings, mac ? new MacStartupService() : null, () => { RefreshScreens(); engine.KeepVisible(); UpdateWindow(); });
        settingsWindow.Closed += (_, _) => settingsWindow = null; settingsWindow.Show(); settingsWindow.Activate();
    }
}
