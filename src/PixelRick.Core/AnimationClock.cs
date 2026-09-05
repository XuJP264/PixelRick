namespace PixelRick;

/// <summary>Renderer-independent timing; both frontends display this exact frame index.</summary>
public sealed class AnimationClock(CharacterDefinition character)
{
    public string Current { get; private set; } = "idle";
    public double Elapsed { get; private set; }
    public AnimationDefinition Definition => character.Animations[Current];
    public bool Finished => !Definition.Loop && Elapsed >= Definition.Frames / Definition.Fps;
    public int FrameIndex => Definition.Loop ? (int)(Elapsed * Definition.Fps) % Definition.Frames
        : Math.Min(Definition.Frames - 1, (int)(Elapsed * Definition.Fps));
    public void Play(string name)
    {
        if (!character.Animations.ContainsKey(name)) throw new ArgumentException($"Unknown animation: {name}", nameof(name));
        Current = name;
        Elapsed = 0;
    }
    public void Update(double dt) => Elapsed += Math.Max(0, dt);
}

public sealed class EffectTimeline(CharacterDefinition character)
{
    public AnimationClock Animation { get; } = new(character);
    public string? Current { get; private set; }
    private double remaining;
    public void Play(string name, double seconds) { Current = name; remaining = seconds; Animation.Play(name); }
    public void Clear() => Current = null;
    public void Update(double dt) { Animation.Update(dt); remaining -= dt; if (remaining <= 0) Clear(); }
}
