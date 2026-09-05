# Known issues and review limits

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
