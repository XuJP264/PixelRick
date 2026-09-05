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
