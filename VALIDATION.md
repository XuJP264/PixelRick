# PixelRick 0.1.0 validation

Validated on the available Windows desktop, 2026-09-05.

| Check | Result |
| --- | --- |
| Clean .NET 8 Release build | Passed; zero warnings/errors |
| .NET behavior/physics tests | 9 passed |
| Python art tests | 5 passed |
| Final sprite sheets | 25 passed; 138 RGBA frames |
| Visual reference and contact-sheet review | Completed; side-pose specks, eyelids, and trouser color repaired |
| Native click, drag, fall, and landing | Passed |
| Double-click portal entry and exit | Passed |
| Single-instance activation | Passed |
| Right-click menu and Settings Save | Passed using Windows UI Automation |
| Self-contained executable launch | Passed |
| Installer installation and upgrade | Passed |
| Installed files against published files | SHA256 matches |
| Start Menu shortcut | Verified |
| Uninstall and preference preservation | Passed |
| Portable ZIP integrity and file hashes | Passed |
| Extracted portable executable launch | Passed |
| Source secret-pattern scan | Passed |

`ArtReview/full_behavior_demo.gif` is a deterministic sprite-based behavior preview,
not a screen recording. `app_screenshot.png` is rendered by the live WPF window;
`settings_screenshot.png` captures the application's Settings content only.
No private desktop screenshot is included.

The source is prepared for a v0.1.0 release. GitHub publishing requires the local
GitHub CLI authentication requested during handoff; connected app tools do not
expose repository creation or release uploads. No remote publication is claimed
until authentication and the release workflow complete.

See `KNOWN_ISSUES.md` for hardware coverage and unsigned-executable limitations.
