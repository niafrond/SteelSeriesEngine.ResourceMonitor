# SteelSeries.ResourceMonitor

SteelSeries.ResourceMonitor is a .NET 8 tray application that monitors your system's CPU, RAM and GPU usage and temperature, and displays this information on the OLED screen of a SteelSeries Arctis Nova Pro (Wireless) base station.

## Features

- **Real-time Monitoring:** Displays CPU, RAM, GPU and disk usage on the OLED screen.
- **Compact OLED Layout:** Shows the current time at the top and one left-aligned row per resource with a percentage and horizontal bar.
- **Cross-vendor Support:** Works with both NVIDIA and AMD GPUs.
- **Runs in the system tray:** No console window, no dock/taskbar clutter - just a tray icon.
- **Starts with Windows:** Enabled by default after install, can be toggled from the tray menu at any time.
- **Simple Setup:** Install with the setup wizard, no configuration required.

## How It Works

The Arctis Nova Pro base station's 128x64 OLED screen is **not** exposed by the SteelSeries GameSense API (only Apex keyboards, Rival mice, GameDAC/Arctis Pro Wireless are). This app instead shells out to [ggoled](https://github.com/JerwuQu/ggoled), a third-party tool that talks to the base station directly over USB, and periodically updates a compact frame with the current time plus one row per resource (CPU, RAM, GPU and disk).

## Requirements

- Windows 10/11 (x64)
- An Arctis Nova Pro / Nova Pro Wireless base station connected over USB.

The installer ships a self-contained build (no separate .NET runtime install required) and bundles [`ggoled.exe`](https://github.com/JerwuQu/ggoled), so there's nothing extra to download.

## Installation

1. Download `SteelSeriesResourceMonitor-Setup.exe` (see [Building the installer](#building-the-installer) below to produce it yourself) and run it. No admin rights are required - it installs for the current user only.
2. Launch the app (or let the installer launch it for you). It appears as an icon in the system tray.

### Tray menu

Right-click the tray icon to:
- **Démarrer avec Windows** - toggle whether the app launches automatically at Windows sign-in (on by default after install).
- **Quitter** - exit the app.

### Uninstalling

Use "Apps & Features" in Windows Settings, or the shortcut in the Start Menu group. This also removes the auto-start registry entry.

## Building the installer

Requires [Inno Setup 6](https://jrsoftware.org/isinfo.php) (`winget install JRSoftware.InnoSetup`) and the .NET 8 SDK.

```powershell
installer\build.ps1
```

This publishes a self-contained, single-file `win-x64` build and compiles `installer\Output\SteelSeriesResourceMonitor-Setup.exe`.

## Troubleshooting

- If the tray shows a "ggoled.exe introuvable" notification, `ggoled.exe` is missing from the install folder (e.g. a manual/dev build) - grab it from its [releases page](https://github.com/JerwuQu/ggoled/releases/latest) and place it next to `SteelSeries.SysMonitor.exe`, or add it to `PATH`, then relaunch the app.
- If stats are sent but nothing shows up, run `ggoled probe` manually to confirm it can see the base station over USB.

## Project Structure

- `Program.cs` - Entry point (single-instance check, starts the tray application).
- `TrayApplicationContext.cs` - System tray icon, context menu (autostart toggle, exit).
- `Display/OledController.cs` - Handles ggoled communication and periodic OLED updates.
- `Display/OledFrameRenderer.cs` - Renders the compact OLED frame with the time and per-resource rows.
- `Hardware/SystemResourceMonitor.cs` - Gathers system stats using LibreHardwareMonitor.
- `Helpers/Utilities.cs` - Utility functions for HTTP and file operations.
- `Helpers/StartupManager.cs` - Manages the "start with Windows" registry entry.
- `installer/setup.iss` - Inno Setup script for the installer.
- `installer/build.ps1` - Publishes the app and builds the installer.
- `installer/vendor/` - Bundled `ggoled.exe` plus its GPLv3 license and a third-party notice.

## Dependencies

- [LibreHardwareMonitorLib](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor) - For hardware monitoring.
- [ggoled](https://github.com/JerwuQu/ggoled) (GPLv3) - Bundled unmodified to talk to the base station's OLED screen. See `installer/vendor/THIRD-PARTY-NOTICES.txt`.

## License

MIT License. See [LICENSE](LICENSE) for details. Note: the bundled `ggoled.exe` is third-party software under GPLv3 - see [installer/vendor/THIRD-PARTY-NOTICES.txt](installer/vendor/THIRD-PARTY-NOTICES.txt).
