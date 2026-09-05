# macOS implementation and validation

PixelRick uses the open-source Avalonia 12.1.2 frontend on macOS and retains WPF
on Windows. Both frontends execute `PixelRick.Core.PetEngine`; state transitions,
randomness, irritation, cursor reactions, dragging, throws, sleep, portals, and
sprite/effect clocks are shared. `PixelRick.Assets.props` packages the original
sprite sheets without art generation or separate platform copies.

## Geometry and pixels

The native bridge reads `NSScreen.visibleFrame`, which excludes the Dock and menu
bar. It converts Cocoa's bottom-left coordinates into top-left coordinates in
**logical points**, using the primary display's top as the reference. A monitor
above or left of the primary display therefore has negative coordinates.
`NSWindow.setFrame` receives the inverse conversion; backing scale is not used
to multiply screen positions. This avoids mixing screen pixels and Cocoa points.

At the default scale, the canvas is 256×256 points and the character is about
184 points tall. Retina backing scale affects raster resolution, not logical size.
Avalonia uses `BitmapInterpolationMode.None`. Review captures render the live
sprite control at both 1x and 2x; the report separately records the real display's
backing scale. Synthetic 2x rendering is not a claim of physical Retina coverage.

## Transparent hit testing

Avalonia does not supply WPF-style per-pixel transparent-window hit testing.
The macOS host reads the current frame's alpha mask, inverse-mirrors it when
necessary, and updates `NSWindow.ignoresMouseEvents` from the global cursor at
60 Hz. Effects remain noninteractive. Drag capture and an open context menu
temporarily retain input. The transparent canvas passes input through except
where the visible character accepts input.

This is a sampled mask, not an OS-supplied atomic per-pixel region. A pointer move
immediately followed by a click within one timer interval can race the update.
macOS scheduling delays can lengthen that interval. Hosted validation checks
the native ignore/accept properties and exercises AppKit mouse events through
Avalonia's normal input path. Physical cross-application click-through, Spaces,
and mixed-DPI multi-monitor arrangements remain hardware review items.

## Platform services

- `LSUIElement` makes this an accessory app without a normal Dock application icon.
- Avalonia `TrayIcon` / `NativeMenu` creates the menu-bar status item.
- The pet shows without activation; Settings activates only when requested.
- The AppKit bridge sets floating level and join-all-Spaces / full-screen auxiliary
  behavior. Full-screen apps and Stage Manager still follow macOS policy.
- Preferences: `~/Library/Application Support/PixelRick/config.json`.
- Launch at Login: `SMAppService.mainAppService` (macOS 13+), behind `IStartupService`.
  macOS may require approval in System Settings → General → Login Items. Install
  in Applications before enabling it. Reboot/login behavior requires user hardware.
- The Windows host retains `%APPDATA%/PixelRick/config.json` and its existing
  current-user registry startup entry behind the same interface.

## Build and inspect

Develop with the .NET 10 SDK plus .NET 8 runtime for tests. Runtime targets remain
.NET 8; end-user packages include that runtime.

```sh
dotnet build PixelRick.Mac.slnf
dotnet test tests/PixelRick.Core.Tests/PixelRick.Core.Tests.csproj
python3 -m pip install -r tools/art_pipeline/requirements.txt
python3 tools/macos/package.py --arch arm64
python3 tools/macos/validate.py --arch arm64
```

Use `--arch x64` on an Intel runner. Native validation is required before advertising
an architecture. `dist/macos-ARCH/review` contains the JSON results, native-window
snapshot, 1x/2x captures, Settings capture, and animated behavior demo. The test
mounts the DMG, copies the bundle as a user would, launches it, checks normal input
and configuration, quits, and repeats the launch. Test settings use an isolated
temporary folder and never modify a real user's login-item registration.

## References

- [Avalonia window transparency limitations](https://docs.avaloniaui.net/docs/how-to/window-how-to)
- [Avalonia macOS deployment](https://docs.avaloniaui.net/docs/deployment/macos)
- [Apple NSWindow behavior](https://developer.apple.com/documentation/appkit/nswindow)
- [Apple SMAppService](https://developer.apple.com/documentation/servicemanagement/smappservice)
