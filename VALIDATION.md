# Cross-platform preview validation

The accepted [v0.1.0 Windows baseline](docs/WINDOWS_BASELINE_VALIDATION.md) remains
published and unchanged. The Mac port retains WPF on Windows, extracts the common
behavior engine, and reuses the exact original sprite sheets and metadata.

The [final pre-release validation run](https://github.com/XuJP264/PixelRick/actions/runs/33991063635)
passed on 2026-09-05 (source `52223f3`). Review captures were visually inspected on
both architectures; sprite outlines, transparency, effects and Settings controls
render correctly. [Capture provenance](ArtReview/macOS/review-source.json).

| Platform | Result |
| --- | --- |
| Windows x64 | 24 .NET tests, 5 art tests, native interactions, Settings UI and installer/portable lifecycle passed |
| macOS Apple Silicon | 15 core tests; 23 live app checks on both first launch and restart; DMG validation passed |
| macOS Intel | 15 core tests; 23 live app checks on both first launch and restart; DMG validation passed |

The stable-publication guard was also tested and rejected unsigned Mac reports.
The tag workflow repeats all platform checks before publishing fresh artifacts.

The [v0.2.0-preview.1 release run](https://github.com/XuJP264/PixelRick/actions/runs/33991468750)
also passed. All five published downloads were fetched again and matched
`SHA256SUMS.txt`; the portable runtime/assets and the release's four Mac launch
reports were verified. [Download verification](ArtReview/macOS/release-verification.json).
GitHub still identifies v0.1.0 as the latest stable release.

## Windows regression

- Clean Release build with .NET 10 SDK targeting .NET 8; zero warnings/errors.
- 9 original Windows tests plus 15 platform-independent core tests.
- Native mouse click, drag, fall/landing, double-click portals, single instance.
- Right-click context menu, Settings window and Save via Windows UI Automation.
- Live WPF rendering and clean process exit.
- Self-contained installer, upgrade, installed-file hashes, shortcuts, uninstall
  with preference preservation, portable ZIP integrity and extracted-app launch.
- Art pipeline: 5 tests, 25 RGBA sprite sheets, 138 frames; no Mac-specific art changes.

## macOS validation

The native `macos-15` Apple Silicon runner and `macos-15-intel` runner each build,
test, package, mount the DMG, copy the app, and run it twice. Application tests use
real AppKit events through Avalonia's input handlers. They cover:

- Transparent native window, accessory activation policy/no Dock icon, menu-bar configuration.
- Native screen work areas excluding menu bar and Dock; walking displacement.
- Sprite-alpha click-through selection and native ignore/accept mouse-event properties.
- Click irritation, dragging, throw velocity, gravity, landing, sleep/wake, portal entry/exit.
- Hide/show, a second process revealing the hidden pet, clean second-process exit.
- Settings presentation, JSON persistence, application quit and restart.
- Live-rendered 1x/2x images with exact nearest-neighbor color and alpha comparison.
- Native executable architecture, bundled .NET runtime, all asset SHA256 hashes,
  DMG integrity, Applications link, and ad-hoc bundle signature after installation.

`PixelRick-Validation.zip` on the preview release contains JSON reports, Mac
screenshots, live-rendered behavior GIFs, Windows TRX results and installer evidence.
The real desktop screenshot is captured only on disposable GitHub runners.
Local review runs never capture a user's entire desktop.

## Coverage boundaries

The hosted Mac displays report **1x backing scale**. Explicit 2x renderer captures
verify raster quality; they do not constitute physical Retina or mixed-DPI monitor
validation. The behavior GIF is an animation preview, not a desktop screen recording.

Core tests cover negative monitor coordinates, Dock/menu-bar conversion and display
removal. Physical multi-monitor arrangements, Spaces/Stage Manager/full-screen
combinations, cross-application mouse delivery, and login after reboot need broader
hardware coverage. Windows 10 and macOS 13/14 have not been run on separate machines.

The sampled click-through mask has a possible one-update race after rapid pointer
movement. No claim of atomic OS per-pixel hit testing is made.

Mac DMGs are **development artifacts**, without Developer ID signing/notarization.
The signing/notarization path is configured and fails closed on partial credentials,
but its Apple-service steps cannot be verified without credentials. Stable release
publication explicitly rejects unsigned Mac reports. See [known issues](KNOWN_ISSUES.md)
and [signing setup](docs/MACOS_SIGNING.md).
