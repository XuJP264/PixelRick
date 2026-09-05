using System.Text.Json;
using Avalonia;
using Avalonia.Media.Imaging;
namespace PixelRick.Desktop;
internal sealed partial class PetWindow
{
    private string? reviewFolder;
    private int validationStage = -1, captureIndex;
    private double nextCapture;
    private readonly HashSet<PetState> visited = new();
    private readonly Dictionary<string, bool> checks = new();
    private void StartValidation()
    {
        var args = lifetime.Args!; int index = Array.IndexOf(args, "--review-dir");
        reviewFolder = index >= 0 ? Path.GetFullPath(args[index + 1]) : Path.Combine(Path.GetTempPath(), "PixelRick-MacReview");
        Directory.CreateDirectory(reviewFolder); Directory.CreateDirectory(Path.Combine(reviewFolder, "frames"));
        engine.Config.Wander = false; engine.Config.InteractionFrequency = 0;
        engine.State.Changed += s => visited.Add(s); visited.Add(engine.State.State);
        checks["nativeTransparentWindow"] = mac && MacNative.pr_is_opaque(native) == 0;
        checks["menuBarConfigured"] = trays.Count == 1 && trays[0].IsVisible;
        checks["nativeScreensAvailable"] = mac && MacNative.Areas().Length > 0;
        checks["emptyPixelClickThrough"] = !sprite.OpaqueAt(0, 0);
        MacNative.pr_ignore(native, 1); checks["appKitIgnoresMouseEvents"] = MacNative.pr_ignoring(native) == 1;
        MacNative.pr_ignore(native, 0); checks["appKitAcceptsMouseEvents"] = MacNative.pr_ignoring(native) == 0;
        settings.Value.MovementSpeed = 70; settings.Save(); checks["settingsRoundTrip"] = new ConfigurationStore(settings.Folder).Value.MovementSpeed == 70;
    }
    private void UpdateValidation(double now)
    {
        if (reviewFolder is null) return;
        try {
            if (now > nextCapture && IsVisible) {
                nextCapture = now + .2;
                sprite.Capture(Path.Combine(reviewFolder, "frames", $"{captureIndex++:D4}.png"), 2);
            }
            int stage = (int)(now / 2);
            if (stage == validationStage) return; validationStage = stage;
            switch (stage) {
                case 1:
                    sprite.Capture(Path.Combine(reviewFolder, "macos-1x.png"), 1);
                    sprite.Capture(Path.Combine(reviewFolder, "macos-2x.png"), 2);
                    checks["render1xAnd2x"] = true;
                    checks["nativeSnapshot"] = MacNative.pr_snapshot(native, Path.Combine(reviewFolder, "native-window.png")) != 0;
                    engine.State.Enter(PetState.Walk); engine.Movement.Destination = engine.Movement.X - 90; break;
                case 2:
                    engine.State.Enter(PetState.Idle);
                    MacNative.pr_test_mouse(native, 0, 128, 140, 1); MacNative.pr_test_mouse(native, 2, 128, 140, 1); break;
                case 3:
                    checks["nativeClickReaction"] = engine.Interaction.Irritation > 0;
                    MacNative.pr_test_mouse(native, 0, 128, 140, 1); break;
                case 4:
                    MacNative.pr_test_mouse(native, 1, 100, -60, 1); break;
                case 5:
                    checks["nativeDrag"] = engine.Dragging;
                    MacNative.pr_test_mouse(native, 1, 200, -90, 1); MacNative.pr_test_mouse(native, 2, 200, -90, 1); break;
                case 7:
                    checks["fallAndLanding"] = visited.Contains(PetState.Falling) && visited.Contains(PetState.Landing);
                    engine.Sleep(); break;
                case 8: checks["sleep"] = engine.State.State == PetState.Sleep; engine.Wake(); break;
                case 9:
                    checks["wake"] = visited.Contains(PetState.WakeUp);
                    MacNative.pr_test_mouse(native, 0, 128, 140, 2); MacNative.pr_test_mouse(native, 2, 128, 140, 2); break;
                case 11:
                    checks["nativeDoubleClickPortal"] = visited.Contains(PetState.PortalEnter) && visited.Contains(PetState.PortalExit);
                    checks["validWorkArea"] = engine.Movement.Y <= engine.Area.Bottom && engine.Movement.Y >= engine.Area.Top + 96 * engine.Config.Scale && engine.Movement.X >= engine.Area.Left && engine.Movement.X <= engine.Area.Right;
                    Hide(); checks["hide"] = !IsVisible; break;
                case 12: ShowRick(); checks["show"] = IsVisible; OpenSettings(); break;
                case 13:
                    checks["settingsWindow"] = settingsWindow?.IsVisible == true;
                    if (settingsWindow is not null) { using var bitmap = new RenderTargetBitmap(new PixelSize(860, 1300), new Vector(192, 192)); bitmap.Render(settingsWindow); bitmap.Save(Path.Combine(reviewFolder, "settings.png"), PngBitmapEncoderOptions.Default); settingsWindow.Close(); }
                    break;
                case 14:
                    var report = new { architecture = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString(), os = System.Runtime.InteropServices.RuntimeInformation.OSDescription, backingScale = MacNative.pr_backing_scale(native), screens = MacNative.Areas(), checks, visited = visited.Select(s => s.ToString()).Order().ToArray(), passed = checks.Values.All(x => x), limitations = new[] { "Hosted runner display only; physical mixed-DPI displays, Spaces and login after reboot require hardware review.", "1x/2x images use the live renderer at explicit backing resolutions; only backingScale above describes the actual display.", "Click-through mask sampled at 60 Hz; rapid pointer movement immediately followed by click has a one-tick race." } };
                    File.WriteAllText(Path.Combine(reviewFolder, "macos-report.json"), JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
                    if (!report.passed) { lifetime.Shutdown(2); } else Close();
                    break;
            }
        } catch (Exception e) { File.WriteAllText(Path.Combine(reviewFolder, "failure.txt"), e.ToString()); lifetime.Shutdown(3); }
    }
}
