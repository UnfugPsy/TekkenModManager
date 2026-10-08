# TEKKEN 8 Mod Manager

A lightweight Windows desktop app for installing, enabling, and organizing **TEKKEN 8** mods. Toggle mods on/off, group them into profiles, edit metadata etc

## Features

- **One-click enable/disable** — toggle mods via the ACTIVE checkbox; pak mods rename files in place (`.pak`/`.ucas`/`.utoc` ⇄ `-x` disabled), logic mods rename their folder (`.disabled_` prefix).
- **All three mod folders** — manage `Mods`, `~mods`, and `LogicMods` (UE4SS script mods) in one list, with a TYPE column showing each mod's folder.
- **Install from archives** — add mods from ZIP/RAR/7Z via button or drag & drop; the destination folder is auto-detected and confirmable.
- **Profiles** — save and apply sets of enabled mods, with a default profile.
- **Editable metadata** — version, category, author, and description per mod (right-click → Edit Metadata, or double-click).
- **Rename mods** — rename a mod's folder from the list (right-click → Rename Mod, or press `F2`); profile references update automatically.
- **Conflict detection** — mods sharing a `.pak` filename are flagged so overrides are obvious.
- **Search** — filter the list by name, category, author, or type.
- **Keyboard shortcuts** — fast access to common actions.

## Keyboard Shortcuts

| Shortcut | Action |
|----------|------------------------|
| `F1`     | Show help dialog       |
| `F2`     | Rename selected mod    |
| `F5`     | Refresh mod list       |
| `Ctrl+G` | Set game location      |
| `Ctrl+O` | Open mod folder        |
| `Ctrl+N` | Add new mod from ZIP   |
| `Escape` | Close dialogs          |

## Installation

Download the latest release from the [Releases](../../releases) page. Two builds are available — pick one:

> ⚠️ **Experimental build.** This 1.1.0-experimental release introduces multi-root support (Mods / ~mods / LogicMods) and real-time folder watching. It is provided for testing — please back up your mods before use and report any issues.

| Build | Download | .NET required? | Size |
|-------|----------|----------------|------|
| **Self-contained** (recommended) | `TekkenModManager-1.1.0-experimental-win-x64-selfcontained.zip` | No — everything is bundled | ~63 MB |
| **Framework-dependent** | `TekkenModManager-1.1.0-experimental-win-x64-framework-dependent.zip` | Yes — [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) | ~1 MB |

**To install:** extract the ZIP to any folder and run `ModManager.exe`. No setup or admin rights needed.

> If you use the framework-dependent build and the app won't start, install the [.NET 8 Desktop Runtime (x64)](https://dotnet.microsoft.com/download/dotnet/8.0) and try again.

## Getting Started

1. **Set Game Location** — click *Set Game Location* (`Ctrl+G`) and browse to `TEKKEN8\Polaris\Content\Paks`. The `Mods`, `~mods`, and `LogicMods` folders are used automatically when present.
2. **Add a mod** — click *Add Zipped Mod* (`Ctrl+N`) or drag a ZIP/RAR/7Z onto the window; confirm the detected destination folder.
3. **Enable/Disable** — tick the ACTIVE checkbox; the STATUS column shows ACTIVE/INACTIVE.


## Requirements

- Windows
- TEKKEN 8 (PC)

## Building from source

Needs the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (`global.json` pins 8.0.425).

```
dotnet test ModManager.sln
dotnet run --project src/ModManager/ModManager.csproj
pwsh scripts/release.ps1
```

The last command builds the two release zips into `artifacts/release`; the steps around it are in [docs/releasing.md](docs/releasing.md). Changes per version: [CHANGELOG.md](CHANGELOG.md).

## Feedback

Bug reports and ideas are welcome as GitHub issues, or as comments on the [TekkenMods page](https://tekkenmods.com/mod/7283/tekken-8-mod-manager). Please include the manager version (F1 shows it) and the game location you set.

## Disclaimer

This is a community tool and is not affiliated with Bandai Namco. Use mods at your own risk.
