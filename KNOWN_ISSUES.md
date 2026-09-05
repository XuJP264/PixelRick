# Known issues and review limits

## macOS preview

- Mac packages have an ad-hoc integrity signature, **not** an Apple Developer ID
  signature or notarization. They are development prereleases. Use macOS Privacy
  & Security's Open Anyway flow when reviewing a trusted download.
- Native CI exercises macOS 15 on Apple Silicon and Intel. The minimum deployment
  target is macOS 13; separate macOS 13/14 machines have not been exercised.
- Hosted displays run at 1x. Live-rendered 2x captures verify nearest-neighbor
  pixels, but physical Retina and mixed-DPI multi-monitor hardware remain untested.
- Click-through uses the current sprite alpha and `NSWindow.ignoresMouseEvents`,
  sampled at 60 Hz. A move followed immediately by a click can race one update;
  system scheduling delays can extend that interval. Cross-application click
  delivery on physical desktops needs broader validation.
- Spaces/full-screen apps/Stage Manager follow macOS window-management policy.
  Join-all-Spaces is requested; those combinations are not fully validated in CI.
- Launch at Login uses SMAppService and may need approval in System Settings.
  Install in Applications before enabling it. Reboot/login behavior and Developer
  ID signing/notarization need a Mac with the corresponding user approval/credentials.
- The behavior GIF is made from the running application's sprite renderer. It is
  an animation preview, not a recording of the entire desktop.

## Windows and shared behavior

- The installer and executable are unsigned; Windows may show a publisher warning.
- Live launch and interaction checks run on the available Windows desktop. Multiple
  monitor coordinates are unit tested; mixed-DPI physical monitor combinations and
  Windows 10 have not been tested on separate hardware.
- Walking uses the current monitor's work area. Use dragging or a portal to change
  monitors. Throws bounce at horizontal edges and settle directly on the floor.
- Art uses generated canonical masters plus deterministic cutout animation. Reactions
  are deliberately compact, and portal disappearance uses runtime opacity.
- Optional sound uses the Windows system notification sound; no show audio is included.
- Portable means no installation or runtime setup. Preferences still use AppData.
