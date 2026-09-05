using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
namespace PixelRick.Desktop;
internal sealed class PreferencesWindow : Window
{
    public PreferencesWindow(ConfigurationStore store, IStartupService? startup, Action applied)
    {
        Title = "PixelRick Settings"; Width = 430; Height = 650; CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        var panel = new StackPanel { Margin = new Thickness(24), Spacing = 10 };
        Content = new ScrollViewer { Content = panel };
        panel.Children.Add(new TextBlock { Text = "A tiny genius. Your rules.", FontSize = 24, FontWeight = FontWeight.SemiBold });
        var version = typeof(PreferencesWindow).Assembly.GetName().Version!;
        panel.Children.Add(new TextBlock { Text = $"PixelRick {version.Major}.{version.Minor}.{version.Build} · macOS preview", Foreground = Brushes.LightSkyBlue });
        var c = store.Value;
        Slider Slider(string name, double min, double max, double value, double tick) { var label = new TextBlock(); var s = new Slider { Minimum = min, Maximum = max, Value = value, TickFrequency = tick, IsSnapToTickEnabled = true }; void Update() => label.Text = $"{name}: {s.Value:0.##}"; s.PropertyChanged += (_, e) => { if (e.Property == Avalonia.Controls.Slider.ValueProperty) Update(); }; Update(); panel.Children.Add(label); panel.Children.Add(s); return s; }
        CheckBox Check(string name, bool value) { var item = new CheckBox { Content = name, IsChecked = value }; panel.Children.Add(item); return item; }
        var size = Slider("Size", 1, 4, c.Scale, .5); var speed = Slider("Walking speed", 20, 180, c.MovementSpeed, 5);
        var activity = Slider("Activity", .25, 2, c.ActivityFrequency, .25); var curiosity = Slider("Cursor curiosity", 0, 1, c.InteractionFrequency, .1);
        var top = Check("Always on top", c.AlwaysOnTop); var sounds = Check("Soft sounds", c.Sound);
        var login = Check("Launch at Login", startup?.Enabled == true); login.IsEnabled = startup is not null;
        var remember = Check("Remember position", c.RememberPosition); var throwing = Check("Playful throws", c.ThrowingPhysics); var monitors = Check("Use all monitors", c.MultiMonitor);
        var status = new TextBlock { TextWrapping = TextWrapping.Wrap, Foreground = Brushes.Salmon }; panel.Children.Add(status);
        var save = new Button { Content = "Save settings", HorizontalAlignment = HorizontalAlignment.Stretch }; panel.Children.Add(save);
        save.Click += (_, _) => { try {
            if (startup is not null && startup.Enabled != (login.IsChecked == true)) startup.SetEnabled(login.IsChecked == true);
            c.Scale = size.Value; c.MovementSpeed = speed.Value; c.ActivityFrequency = activity.Value; c.InteractionFrequency = curiosity.Value;
            c.AlwaysOnTop = top.IsChecked == true; c.Sound = sounds.IsChecked == true; c.RememberPosition = remember.IsChecked == true;
            c.ThrowingPhysics = throwing.IsChecked == true; c.MultiMonitor = monitors.IsChecked == true;
            store.Save(); applied(); Close();
        } catch (Exception e) when (e is IOException or UnauthorizedAccessException or InvalidOperationException) { status.Text = e.Message; } };
    }
}
