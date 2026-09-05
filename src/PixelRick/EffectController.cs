using System.Windows.Media.Imaging;
namespace PixelRick;
public sealed class EffectController
{
    private readonly SpriteAnimator animator;
    public string? Current {get;private set;}
    private double elapsed,duration;
    public EffectController(CharacterDefinition character)=>animator=new(character);
    public void Play(string name,double seconds){Current=name;elapsed=0;duration=seconds;animator.Play(name);}
    public void Clear()=>Current=null;
    public void Update(double dt){elapsed+=dt;animator.Update(dt);if(elapsed>=duration)Current=null;}
    public BitmapSource? Frame=>Current is null?null:animator.Frame;
}
