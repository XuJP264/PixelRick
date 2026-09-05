using System.Text.Json;
namespace PixelRick;

public interface IStartupService
{
    bool Enabled { get; }
    void SetEnabled(bool enabled);
}

public sealed class ConfigurationStore
{
    public static string DefaultFolder => OperatingSystem.IsMacOS()
        ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library", "Application Support", "PixelRick")
        : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PixelRick");
    public string Folder { get; }
    public PetSettings Value { get; }
    public ConfigurationStore(string? folder = null)
    {
        Folder = folder ?? DefaultFolder;
        try { Value = JsonSerializer.Deserialize<PetSettings>(File.ReadAllText(Path.Combine(Folder, "config.json"))) ?? new(); }
        catch (Exception e) when (e is IOException or JsonException or UnauthorizedAccessException) { Value = new(); }
        Value.Normalize();
    }
    public void Save()
    {
        Value.Normalize();
        Directory.CreateDirectory(Folder);
        var path = Path.Combine(Folder, "config.json");
        File.WriteAllText(path + ".tmp", JsonSerializer.Serialize(Value, new JsonSerializerOptions { WriteIndented = true }));
        File.Move(path + ".tmp", path, true);
    }
}
