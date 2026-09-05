# PixelRick

A tiny pixel scientist for your Windows desktop. He wanders, naps, gets annoyed,
and makes dramatic exits through a green portal. No chat, accounts, or network connection.

![PixelRick behavior preview](ArtReview/full_behavior_demo.gif)

## Download

**[Download PixelRick-Setup.exe](https://github.com/XuJP264/PixelRick/releases/latest/download/PixelRick-Setup.exe)**

Windows 10/11 · 64-bit · No .NET installation required.
Prefer no installer? Get **PixelRick-Portable.zip** from [Releases](https://github.com/XuJP264/PixelRick/releases).

See [validation status](VALIDATION.md) for test coverage and release checks.

## Features

- Seventeen character animations and eight separate pixel effects.
- Gentle wandering, cursor curiosity, naps, and escalating click irritation.
- Pick him up, throw him, or double-click for a portal trip.
- Transparent desktop window with crisp pixel rendering and taskbar avoidance.
- Tray controls, adjustable size and activity, optional sounds and Windows startup.
- Multi-monitor positioning, remembered settings, and one pet per session.
- Runs entirely offline; no telemetry or AI chat.

## Installation

1. Download **PixelRick-Setup.exe** from Releases.
2. Double-click it and choose **Install**.
3. Leave **Launch PixelRick** checked. Rick appears above your taskbar.

The installer uses your local application folder and does not need administrator
privileges. Uninstall from Windows Settings; preferences are preserved.
For the portable download, extract the ZIP and double-click **PixelRick.exe**.

This first build is unsigned. Windows may show a publisher/SmartScreen prompt.

## Controls

| Action | Result |
| --- | --- |
| Click | A reaction; repeated clicks build irritation |
| Drag and release | Pick up, drop, or throw |
| Double-click | Portal teleport |
| Right-click Rick | Stay, wander, sleep, wake, teleport, settings, exit |
| Right-click tray icon | Show/hide and quick controls |
| Launch again | Show the existing pet |

Settings are saved in `%APPDATA%\PixelRick\config.json`. Sound and startup are
off by default. **Stay Here** stops walking; portal travel remains available.
Walking stays within the current monitor's work area. Dragging and portals can
move between monitors; disabling all-monitor behavior confines Rick to the primary screen.

## Screenshots / Demo

The animation above is an automated behavior preview on a presentation backdrop.
[Canonical reference](ArtReview/character_reference.png) ·
[All animation frames](ArtReview/all_animations_contact_sheet.png) ·
[Individual GIF previews](ArtReview) · [Live WPF render](ArtReview/app_screenshot.png) ·
[Settings](ArtReview/settings_screenshot.png)

## Building from Source

Development requires Windows and the .NET 8 SDK. Checked-in assets are ready to use.

```powershell
dotnet build
dotnet test
dotnet run --project src/PixelRick
```

For a self-contained build:

```powershell
dotnet publish src/PixelRick/PixelRick.csproj -c Release -r win-x64 --self-contained true
```

For the complete installer, portable ZIP, and SHA256 hashes, install Python 3.14
and Inno Setup 6, then run:

```powershell
python -m pip install -r tools/art_pipeline/requirements.txt
./tools/release.ps1
```

`version.json` controls application, installer, and release version. A matching
`vX.Y.Z` tag triggers the Windows release workflow, which fails on test or build errors.
The first review version is **0.1.0**.

The deterministic art pipeline rebuilds PNGs, metadata, GIFs, contact sheets,
and `art_report.json` from the canonical generated masters. It does not need an API key.
See [art pipeline notes](tools/art_pipeline/README.md) and [known issues](KNOWN_ISSUES.md).

## License

Source code and tooling: [MIT](LICENSE). Character art, icons, and visual previews
are excluded from MIT; see [artwork notice](ARTWORK-NOTICE.md).
Unofficial *Rick and Morty* fan project, with no affiliation or endorsement.
