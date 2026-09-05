namespace PixelRick;

/// <summary>The accepted Windows orchestration, shared by WPF and Avalonia.
/// Coordinates use one consistent top-left-origin space supplied by the host:
/// Windows physical pixels; macOS Cocoa points converted by the native bridge.</summary>
public sealed class PetEngine
{
    public PetSettings Config { get; }
    public MovementController Movement { get; } = new();
    public PhysicsController Physics { get; } = new();
    public PetStateMachine State { get; } = new();
    public BehaviorScheduler Behavior { get; }
    public InteractionController Interaction { get; } = new();
    public AnimationClock Animation { get; }
    public EffectTimeline Effects { get; }
    public bool Pressed { get; private set; }
    public bool Dragging { get; private set; }
    public bool IsPortal => State.State is PetState.PortalEnter or PetState.PortalExit;
    public bool Flip => Movement.FacingLeft && State.State is PetState.Walk or PetState.LookAround;
    public double Opacity => State.State switch {
        PetState.PortalEnter => State.Elapsed < .35 ? 1 : Math.Clamp(1 - (State.Elapsed - .35) / .6, 0, 1),
        PetState.PortalExit => Math.Clamp((State.Elapsed - .2) / .7, 0, 1), _ => 1 };
    public event Action? SoundRequested;
    public WorkArea Area => Nearest(Movement.X, Movement.Y - 1);
    private WorkArea[] areas = [new(0, 0, 1920, 1080)];
    private bool sleepRequested, hardLanding;
    private double nextBehavior = 5, cursorCooldown, pendingClick = -1;
    private double pressX, pressY, lastX, lastY, offsetX, offsetY, vx, vy, lastMoveTime;
    private double Scale => Config.Scale;

    public PetEngine(PetSettings config, CharacterDefinition definition, int? seed = null)
    {
        Config = config; Behavior = new(seed); Animation = new(definition); Effects = new(definition);
        State.Changed += s => Animation.Play(PetStateMachine.Animation(s));
    }
    public void SetScreens(IEnumerable<WorkArea> screens)
    {
        var valid = screens.Where(a => a.Width > 0 && a.Height > 0).ToArray();
        if (valid.Length > 0) areas = Config.MultiMonitor ? valid : [valid[0]];
    }
    private WorkArea Nearest(double x, double y) => areas.MinBy(a =>
        Math.Pow(x - Math.Clamp(x, a.Left, a.Right), 2) + Math.Pow(y - Math.Clamp(y, a.Top, a.Bottom), 2));
    public void KeepVisible() => Movement.Clamp(Area, Math.Min(48 * Scale, Area.Width / 2), Math.Min(96 * Scale, Area.Height));
    public void ResetPosition(bool remember = true)
    {
        Movement.X = remember && Config.RememberPosition && Config.X.HasValue ? Config.X.Value : areas[0].Right - 180;
        Movement.Y = remember && Config.RememberPosition && Config.Y.HasValue ? Config.Y.Value : areas[0].Bottom;
        KeepVisible(); State.Enter(Movement.Y < Area.Bottom ? PetState.Falling : PetState.Idle);
    }
    public void Tick(double dt, double now, double cursorX, double cursorY, bool visible = true)
    {
        dt = Math.Clamp(dt, 0, .05);
        Animation.Update(dt); State.Update(dt); Interaction.Update(dt);
        if (!visible) return;
        if (pendingClick >= 0 && now >= pendingClick && !Pressed)
        {
            pendingClick = -1;
            if (!IsPortal) { State.Enter(Interaction.Click(Behavior)); Effects.Play(State.State == PetState.Angry ? "anger" : "impact", .6); SoundRequested?.Invoke(); }
        }
        Effects.Update(dt); cursorCooldown -= dt;
        switch (State.State)
        {
            case PetState.Walk:
                if (Movement.Walk(dt, Config.MovementSpeed, Area, 48 * Scale)) State.Enter(PetState.Idle);
                break;
            case PetState.Falling:
                if (Physics.Update(Movement, dt, Area, 48 * Scale, 96 * Scale)) { State.Enter(PetState.Landing); Effects.Play("dust", .6); }
                break;
            case PetState.Sit:
                if (Animation.Finished) State.Enter(sleepRequested ? PetState.Sleep : PetState.SittingIdle);
                break;
            case PetState.Landing:
                if (Animation.Finished) { State.Enter(hardLanding ? PetState.Dizzy : PetState.Poked); hardLanding = false; }
                break;
            case PetState.SittingIdle:
                if (State.Elapsed > 10) State.Enter(sleepRequested || Interaction.InactiveSeconds > 150 ? PetState.Sleep : PetState.WakeUp);
                break;
            case PetState.Sleep:
                if (Effects.Current != "zzz") Effects.Play("zzz", 3600);
                break;
            case PetState.PortalEnter:
                if (State.Elapsed > 1.4) {
                    var a = areas[Behavior.Index(areas.Length)];
                    var margin = Math.Min(100 * Scale, a.Width / 2);
                    Movement.X = Behavior.Between(a.Left + margin, a.Right - margin); Movement.Y = a.Bottom;
                    State.Enter(PetState.PortalExit); Effects.Play("portal", 1.4);
                }
                break;
            case PetState.PortalExit:
                if (State.Elapsed > 1.4) { Effects.Clear(); State.Enter(PetState.Idle); }
                break;
            case PetState.Idle:
                if (Interaction.InactiveSeconds > 180 && State.Elapsed > 10 && Behavior.Between(0, 1) < dt * .07) { sleepRequested = true; State.Enter(PetState.Sit); }
                else if (State.Elapsed > nextBehavior) {
                    nextBehavior = Behavior.Delay(Config.ActivityFrequency); var choice = Behavior.Choose(Config.Wander, Interaction.Irritation);
                    if (choice == PetState.Walk) Movement.Destination = Behavior.Between(Area.Left + 48 * Scale, Area.Right - 48 * Scale);
                    State.Enter(choice);
                }
                break;
            case PetState.Grabbed: break;
            default: if (Animation.Finished) { Effects.Clear(); State.Enter(PetState.Idle); } break;
        }
        if (!Pressed && !IsPortal && State.State == PetState.Idle && cursorCooldown <= 0) {
            double distance = Math.Sqrt(Math.Pow(cursorX - Movement.X, 2) + Math.Pow(cursorY - (Movement.Y - 55 * Scale), 2));
            if (distance < 140 * Scale) {
                cursorCooldown = Behavior.Between(8, 16);
                if (Behavior.Between(0, 1) < Config.InteractionFrequency) { Movement.FaceToward(cursorX); State.Enter(distance < 35 * Scale ? PetState.Poked : PetState.LookAround); Effects.Play(distance < 35 * Scale ? "exclamation" : "question", 1); }
            }
        }
        if (!Dragging) KeepVisible();
    }
    public bool PointerDown(double x, double y, double now, int clicks)
    {
        if (IsPortal) return false;
        if (clicks == 2) { pendingClick = -1; Teleport(); return false; }
        Wake(); Pressed = true; pressX = lastX = x; pressY = lastY = y;
        offsetX = Movement.X - x; offsetY = Movement.Y - y; vx = vy = 0; lastMoveTime = now; Interaction.Touch(); return true;
    }
    public void PointerMove(double x, double y, double now)
    {
        if (!Pressed) return;
        double dt = Math.Max(.008, now - lastMoveTime);
        if (!Dragging && Math.Abs(x - pressX) + Math.Abs(y - pressY) > 6) { pendingClick = -1; Dragging = true; State.Enter(PetState.Grabbed); }
        if (Dragging) { vx = .4 * vx + .6 * (x - lastX) / dt; vy = .4 * vy + .6 * (y - lastY) / dt; Movement.X = x + offsetX; Movement.Y = y + offsetY; }
        lastX = x; lastY = y; lastMoveTime = now;
    }
    public void PointerUp(double now, double doubleClickSeconds)
    {
        if (!Pressed) return; Pressed = false;
        if (Dragging) { Dragging = false; if (now - lastMoveTime > .12) vx = vy = 0;
            hardLanding = Config.ThrowingPhysics && (Math.Abs(vx) > 500 || Math.Abs(vy) > 500);
            Physics.Release(vx, vy, Config.ThrowingPhysics); State.Enter(PetState.Falling);
        } else pendingClick = now + doubleClickSeconds;
    }
    public void CancelCapture() { if (!Pressed) return; Pressed = false; if (Dragging) { Dragging = false; Physics.Release(0, 0, false); State.Enter(PetState.Falling); } }
    public void SetWander(bool wander) { Config.Wander = wander; if (State.State == PetState.Walk) State.Enter(PetState.Idle); }
    public void Sleep() { if (IsPortal) return; Interaction.Touch(); State.Enter(PetState.Sit); sleepRequested = true; }
    public void Wake() { sleepRequested = false; Interaction.Touch(); Effects.Clear(); if (State.State is PetState.Sleep or PetState.SittingIdle or PetState.Sit) State.Enter(PetState.WakeUp); }
    public void Teleport() { if (IsPortal || Dragging) return; Wake(); Interaction.Touch(); State.Enter(PetState.PortalEnter); Effects.Play("portal", 1.5); SoundRequested?.Invoke(); }
    public void RememberPosition() { Config.X = Movement.X; Config.Y = Movement.Y; }
}
