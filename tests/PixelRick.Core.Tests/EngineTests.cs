using Xunit;
namespace PixelRick.Tests;
public class EngineTests
{
    private static PetEngine Create() { var e = new PetEngine(new() { Wander = false, InteractionFrequency = 0 }, new(), 42); e.SetScreens([new(0, 24, 1440, 850), new(-1920, -300, 0, 1080)]); e.ResetPosition(false); return e; }
    private static void Tick(PetEngine e, double from, double seconds) { for (double t = from; t < from + seconds; t += .02) e.Tick(.02, t, 99999, 99999); }
    [Fact] public void SleepWakeTransitions() { var e = Create(); e.Sleep(); Tick(e, 0, 2); Assert.Equal(PetState.Sleep, e.State.State); Assert.Equal("zzz", e.Effects.Current); e.Wake(); Tick(e, 2, 2); Assert.Equal(PetState.Idle, e.State.State); Assert.Null(e.Effects.Current); }
    [Fact] public void PortalCompletesOnValidWorkArea() { var e = Create(); e.Teleport(); Tick(e, 0, 1.2); Assert.Equal(0, e.Opacity); Tick(e, 1.2, 2); Assert.Equal(PetState.Idle, e.State.State); Assert.Equal(e.Area.Bottom, e.Movement.Y); Assert.InRange(e.Movement.X, e.Area.Left + 96, e.Area.Right - 96); }
    [Fact] public void DragThrowLandingReaction() { var e = Create(); var states = new List<PetState>(); e.State.Changed += states.Add; e.PointerDown(1200, 700, 0, 1); e.PointerMove(1100, 400, .05); Assert.Equal(PetState.Grabbed, e.State.State); e.PointerUp(.06, .5); Tick(e, .06, 5); Assert.Contains(PetState.Falling, states); Assert.Contains(PetState.Landing, states); Assert.Contains(PetState.Dizzy, states); Assert.Equal(e.Area.Bottom, e.Movement.Y); }
    [Fact] public void DoubleClickCancelsDelayedSingleClick() { var e = Create(); e.PointerDown(1200, 700, 0, 1); e.PointerUp(.01, .5); e.PointerDown(1200, 700, .1, 2); Tick(e, .1, .7); Assert.Equal(PetState.PortalEnter, e.State.State); Assert.Equal(0, e.Interaction.Irritation); }
    [Fact] public void LostCaptureFalls() { var e = Create(); e.PointerDown(1200, 700, 0, 1); e.PointerMove(1100, 400, .05); e.CancelCapture(); Assert.False(e.Pressed); Assert.Equal(PetState.Falling, e.State.State); }
    [Theory][InlineData(20, PetState.Poked)][InlineData(150, PetState.LookAround)]
    public void CursorAwarenessReactsWithoutFollowingOrRepeating(double distance, PetState expected)
    {
        var e = Create(); e.Config.InteractionFrequency = 1;
        double x = e.Movement.X, y = e.Movement.Y - 55 * e.Config.Scale;
        e.Tick(.02, .02, x + distance, y);
        Assert.Equal(expected, e.State.State); Assert.Equal(x, e.Movement.X);
        e.State.Enter(PetState.Idle); e.Tick(.02, .04, x + distance, y);
        Assert.Equal(PetState.Idle, e.State.State); Assert.Equal(x, e.Movement.X);
    }
    [Theory][InlineData(1)][InlineData(2)][InlineData(1.5)] public void PixelHitCoordinatesIndependentOfBackingScale(double backingScale) { var p = DesktopCoordinates.SpritePixel(164, 240, 100, 100, 2, false); Assert.Equal((32, 70), p); Assert.Equal(64 * backingScale, (164 - 100) * backingScale); }
    [Fact] public void CocoaVisibleFrameRespectsDockAndMenuBar() { Assert.Equal(new WorkArea(0, 24, 1440, 850), DesktopCoordinates.FromCocoa(0, 50, 1440, 826, 900)); }
    [Fact] public void DisplayAboveAndLeftPreservesNegativeCoordinates() { Assert.Equal(new WorkArea(-1920, -1080, 0, 0), DesktopCoordinates.FromCocoa(-1920, 900, 1920, 1080, 900)); }
    [Fact] public void MonitorRemovalRecoversPosition() { var e = Create(); e.Movement.X = -1800; e.Movement.Y = -200; e.SetScreens([new(0, 24, 1440, 850)]); e.KeepVisible(); Assert.InRange(e.Movement.X, 96, 1344); Assert.InRange(e.Movement.Y, 216, 850); }
    [Fact] public void NonLoopTimingClampsAtLastFrame() { var c = new AnimationClock(new()); c.Play("landing"); c.Update(100); Assert.True(c.Finished); Assert.Equal(5, c.FrameIndex); }
    [Fact] public void SettingsRoundTripWithoutWindowsDependencies() { var folder = Path.Combine(Path.GetTempPath(), "PixelRick-test-" + Guid.NewGuid()); try { var c = new ConfigurationStore(folder); c.Value.Scale = 3; c.Value.X = -500; c.Save(); var read = new ConfigurationStore(folder); Assert.Equal(3, read.Value.Scale); Assert.Equal(-500, read.Value.X); } finally { Directory.Delete(folder, true); } }
}
