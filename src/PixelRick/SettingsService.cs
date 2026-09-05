using System.IO;
using System.Text.Json;
namespace PixelRick;
public sealed class SettingsService
{
    public static string Folder=>ConfigurationStore.DefaultFolder;
    private readonly ConfigurationStore store = new();
    private readonly IStartupService startup = new WindowsStartupService();
    public PetSettings Value=>store.Value;
    public void Save()=>store.Save();
    public void SetStartup(bool enabled){startup.SetEnabled(enabled);Value.StartupWithWindows=enabled;}
}
internal sealed class WindowsStartupService : IStartupService
{
    public bool Enabled { get { using var key=Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");return key?.GetValue("PixelRick") is string; } }
    public void SetEnabled(bool enabled){using var key=Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");if(enabled)key.SetValue("PixelRick",$"\"{Environment.ProcessPath}\"");else key.DeleteValue("PixelRick",false);}
}
