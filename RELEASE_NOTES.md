PixelRick now has a native macOS desktop frontend using open-source Avalonia,
with the same sprites and shared behavior engine as the preserved WPF Windows app.

![Mac behavior preview](https://raw.githubusercontent.com/XuJP264/PixelRick/v0.2.0-preview.1/ArtReview/macOS/arm64/macos-behavior-demo.gif)

**This is the v0.2.0 development preview.** The accepted **v0.1.0 Windows release
remains the latest stable release** and has not been replaced.

| Your computer | Download |
| --- | --- |
| Windows 10/11, 64-bit | **PixelRick-Setup.exe** |
| Mac with Apple Silicon | **PixelRick-macOS-arm64.dmg** |
| Intel Mac | **PixelRick-macOS-x64.dmg** |

Windows: double-click the installer. Mac: open the DMG, drag PixelRick.app to
Applications, and launch it. No .NET, Homebrew, terminal, or Mac App Store required.
The Windows portable ZIP is also available.

**Mac preview signing:** ad-hoc integrity signature only; not Developer ID signed
or notarized. If blocked, use System Settings → Privacy & Security → Open Anyway
after checking the source. Production signing/notarization is configured for
GitHub Actions secrets but cannot be verified until Apple credentials are supplied.

Includes transparent rendering, crisp pixel scaling, walking, click irritation,
drag/throw/gravity, sleep/wake, portals, menu-bar controls, settings, Dock/menu-bar
work areas, and a macOS Launch at Login implementation.

**PixelRick-Validation.zip** contains Mac screenshots, live-rendered animation
demos, architecture/signature/package reports, and Windows regression evidence.
**SHA256SUMS.txt** covers all release downloads.

Pre-release validation passed: Windows 24 .NET tests, 5 art tests, native interactions,
Settings UI and installer/portable checks; each Mac architecture passed 15 core tests,
23 live application checks on first launch and restart, and installed-DMG validation.

Known coverage limits: physical Retina/mixed-DPI monitors, cross-application
click-through, Spaces/Stage Manager, and login after reboot need broader hardware
coverage. Transparent hit testing samples the sprite alpha at 60 Hz and has a
possible one-update race after a rapid cursor move.

See the repository's VALIDATION.md, KNOWN_ISSUES.md, and docs/MACOS_SIGNING.md.
Source/tooling are MIT; existing character artwork remains covered by the separate
artwork notice. No character art was regenerated for the Mac port.
