using System.IO;
using System.Text.Json;
namespace PixelRick;
public sealed class PetSettings
{
    public double Scale {get;set;}=2;
    public double MovementSpeed {get;set;}=65;
    public double ActivityFrequency {get;set;}=1;
    public double InteractionFrequency {get;set;}=.65;
    public bool AlwaysOnTop {get;set;}=true;
    public bool Sound {get;set;}
    public bool StartupWithWindows {get;set;}
    public bool RememberPosition {get;set;}=true;
    public bool ThrowingPhysics {get;set;}=true;
    public bool MultiMonitor {get;set;}=true;
    public bool Wander {get;set;}=true;
    public double? X {get;set;}
    public double? Y {get;set;}
    public void Normalize(){Scale=double.IsFinite(Scale)?Math.Clamp(Scale,1,4):2;MovementSpeed=double.IsFinite(MovementSpeed)?Math.Clamp(MovementSpeed,20,180):65;ActivityFrequency=double.IsFinite(ActivityFrequency)?Math.Clamp(ActivityFrequency,.25,2):1;InteractionFrequency=double.IsFinite(InteractionFrequency)?Math.Clamp(InteractionFrequency,0,1):.65;if(X.HasValue&&!double.IsFinite(X.Value))X=null;if(Y.HasValue&&!double.IsFinite(Y.Value))Y=null;}
}
public sealed class SettingsService
{
    public static string Folder=>Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"PixelRick");
    public PetSettings Value {get;}
    public SettingsService(){try{Value=JsonSerializer.Deserialize<PetSettings>(File.ReadAllText(Path.Combine(Folder,"config.json")))??new();}catch(Exception e) when(e is IOException or JsonException or UnauthorizedAccessException){Value=new();}Value.Normalize();}
    public void Save(){Directory.CreateDirectory(Folder);string path=Path.Combine(Folder,"config.json");File.WriteAllText(path+".tmp",JsonSerializer.Serialize(Value,new JsonSerializerOptions{WriteIndented=true}));File.Move(path+".tmp",path,true);}
    public void SetStartup(bool enabled){using var key=Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");if(enabled)key.SetValue("PixelRick",$"\"{Environment.ProcessPath}\"");else key.DeleteValue("PixelRick",false);Value.StartupWithWindows=enabled;}
}
public static class Diagnostics
{
    public static bool Enabled {get;set;}
    public static void Log(string text){if(!Enabled)return;try{Directory.CreateDirectory(SettingsService.Folder);var file=Path.Combine(SettingsService.Folder,"diagnostics.log");if(File.Exists(file)&&new FileInfo(file).Length>1_000_000)File.Move(file,file+".old",true);File.AppendAllText(file,$"{DateTimeOffset.Now:O} {text}\n");}catch(IOException){}}
}
