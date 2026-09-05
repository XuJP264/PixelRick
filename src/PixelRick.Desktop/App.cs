using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Themes.Fluent;
namespace PixelRick.Desktop;
public sealed class App : Application
{
    private FileStream? instance;
    public override void Initialize() { Styles.Add(new FluentTheme()); RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Dark; }
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            string folder = ConfigurationStore.DefaultFolder;
            if (desktop.Args?.Contains("--self-test") == true) folder = Path.Combine(Path.GetTempPath(), "PixelRick-mac-test-" + Environment.ProcessId);
            Directory.CreateDirectory(folder);
            try { instance = new FileStream(Path.Combine(folder, "instance.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None); }
            catch (IOException) { if (OperatingSystem.IsMacOS()) MacNative.pr_show_existing(); desktop.Shutdown(); return; }
            var pet = new PetWindow(desktop, folder); desktop.MainWindow = pet;
            desktop.Exit += (_, _) => instance.Dispose();
        }
        base.OnFrameworkInitializationCompleted();
    }
}
