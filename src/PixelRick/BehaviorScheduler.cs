namespace PixelRick;
public sealed class BehaviorScheduler
{
    private readonly Random random;
    public BehaviorScheduler(int? seed=null)=>random=seed.HasValue?new Random(seed.Value):new Random();
    public double Delay(double frequency)=> (4+random.NextDouble()*10)/Math.Clamp(frequency,.25,2);
    public PetState Choose(bool wander,double irritation)
    {
        int pick=random.Next(100);
        if(pick<35)return PetState.Idle;
        if(pick<52)return PetState.Blink;
        if(pick<67)return PetState.LookAround;
        if(pick<86 && wander)return PetState.Walk;
        if(pick<93)return PetState.Sit;
        return irritation>60?PetState.Angry:PetState.Smirk;
    }
    public double Between(double min,double max)=>min+random.NextDouble()*(max-min);
    public int Index(int count)=>random.Next(count);
}
