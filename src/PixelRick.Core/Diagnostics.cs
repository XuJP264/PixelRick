using System.IO;
namespace PixelRick;
public static class Diagnostics
{
    public static bool Enabled {get;set;}
    public static void Log(string text){if(!Enabled)return;try{Directory.CreateDirectory(ConfigurationStore.DefaultFolder);var file=Path.Combine(ConfigurationStore.DefaultFolder,"diagnostics.log");if(File.Exists(file)&&new FileInfo(file).Length>1_000_000)File.Move(file,file+".old",true);File.AppendAllText(file,$"{DateTimeOffset.Now:O} {text}\n");}catch(IOException){}}
}
