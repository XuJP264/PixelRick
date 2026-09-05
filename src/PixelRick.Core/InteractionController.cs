namespace PixelRick;
public sealed class InteractionController
{
    public double Irritation {get;private set;}
    public double InactiveSeconds {get;private set;}
    public void Update(double dt){Irritation=Math.Max(0,Irritation-dt*1.4);InactiveSeconds+=dt;}
    public void Touch()=>InactiveSeconds=0;
    public PetState Click(BehaviorScheduler random){Touch();Irritation=Math.Min(100,Irritation+16);return Irritation>60?PetState.Angry:new[]{PetState.Poked,PetState.LookAround,PetState.Smirk,PetState.Angry}[random.Index(4)];}
}
