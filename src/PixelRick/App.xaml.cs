using System.Threading;
using System.Windows;
namespace PixelRick;
public partial class App : System.Windows.Application
{
    private Mutex? instance;
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        instance = new Mutex(true, "Local\\PixelRick.DesktopPet", out bool first);
        if (!first) { NativeMethods.NotifyExisting(); Shutdown(); return; }
        DispatcherUnhandledException += (_, args) => { Diagnostics.Log(args.Exception.ToString()); Shutdown(1); args.Handled = true; };
        var window = new MainWindow(e.Args); MainWindow = window; window.Show();
    }
    protected override void OnExit(ExitEventArgs e) { instance?.Dispose(); base.OnExit(e); }
}
