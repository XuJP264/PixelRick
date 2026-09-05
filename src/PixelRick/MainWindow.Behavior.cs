namespace PixelRick;
public partial class MainWindow
{
    private double nextDemo;
    private int demoStep;
    private void UpdateDemo(double now)
    {
        if (!args.Contains("--demo") || now < nextDemo) return;
        var sequence = new[] { PetState.Idle, PetState.Walk, PetState.Blink, PetState.LookAround, PetState.Angry, PetState.Smirk, PetState.Sit, PetState.Sleep, PetState.WakeUp, PetState.Grabbed, PetState.Falling, PetState.PortalEnter };
        var s = sequence[demoStep++ % sequence.Length]; nextDemo = now + (s == PetState.Sleep ? 4 : 2.5);
        if (s == PetState.PortalEnter) engine.Teleport();
        else { engine.State.Enter(s); if (s == PetState.Walk) engine.Movement.Destination = engine.Movement.X - 150; if (s == PetState.Grabbed) engine.Movement.Y -= 100; if (s == PetState.Falling) engine.Physics.Release(130, -100, true); if (s == PetState.Angry) engine.Effects.Play("anger", 1); }
    }
}
