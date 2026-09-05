namespace PixelRick;
public enum PetState { Idle,Blink,Walk,LookAround,Sit,SittingIdle,Sleep,WakeUp,Grabbed,Falling,Landing,Angry,Dizzy,Poked,Smirk,PortalEnter,PortalExit }
public sealed class PetStateMachine
{
    public PetState State {get;private set;}=PetState.Idle;
    public double Elapsed {get;private set;}
    public event Action<PetState>? Changed;
    public void Enter(PetState state){State=state;Elapsed=0;Changed?.Invoke(state);Diagnostics.Log("state="+state);}
    public void Update(double dt)=>Elapsed+=dt;
    public static string Animation(PetState state)=>state switch {PetState.LookAround=>"look_around",PetState.SittingIdle=>"sitting_idle",PetState.WakeUp=>"wake_up",PetState.PortalEnter=>"portal_enter",PetState.PortalExit=>"portal_exit",_=>state.ToString().ToLowerInvariant()};
}
