using System.IO;
using System.Text.Json;
namespace PixelRick;
public sealed record AnimationDefinition
{
    public string File { get; init; } = "";
    public int FrameWidth { get; init; }
    public int FrameHeight { get; init; }
    public int Frames { get; init; }
    public double Fps { get; init; }
    public bool Loop { get; init; }
    public int AnchorX { get; init; }
    public int AnchorY { get; init; }
}
public sealed class CharacterDefinition
{
    public string Root { get; } = Path.Combine(AppContext.BaseDirectory, "Assets");
    public Dictionary<string, AnimationDefinition> Animations { get; }
    public CharacterDefinition()
    {
        Animations = JsonSerializer.Deserialize<Dictionary<string, AnimationDefinition>>(System.IO.File.ReadAllText(Path.Combine(Root,"Character/Rick/animations.json")), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        foreach (var (key, a) in Animations)
            if (a.Frames <= 0 || a.Fps <= 0 || !System.IO.File.Exists(Path.Combine(Root, a.File))) throw new InvalidDataException($"Invalid animation: {key}");
    }
}
