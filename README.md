# TEKKEN 8 Mod Manager

A lightweight Windows desktop app for installing, enabling, and organizing **TEKKEN 8** mods. Toggle mods on/off, group them into profiles, edit metadata etc

## Features

- **One-click enable/disable** — toggle mods via the ACTIVE checkbox; files are renamed in place (`.pak`/`.ucas`/`.utoc` ⇄ `-x` disabled).
- **Install from archives** — add mods from ZIP/RAR/7Z via button or drag & drop.
- **Profiles** — save and apply sets of enabled mods, with a default profile.
- **Editable metadata** — version, category, author, and description per mod (right-click → Edit Metadata, or double-click).
- **Conflict detection** — mods sharing a `.pak` filename are flagged so overrides are obvious.
- **Search** — filter the list by name, category, or author.
- **Keyboard shortcuts** — fast access to common actions.

## Keyboard Shortcuts

| Shortcut | Action |
|----------|------------------------|
| `F1`     | Show help dialog       |
| `F5`     | Refresh mod list       |
| `Ctrl+G` | Set game location      |
| `Ctrl+O` | Open mod folder        |
| `Ctrl+N` | Add new mod from ZIP   |
| `Escape` | Close dialogs          |

## Installation

Download the latest release from the [Releases](../../releases) page. Two builds are available — pick one:

| Build | Download | .NET required? | Size |
|-------|----------|----------------|------|
| **Self-contained** (recommended) | `TekkenModManager-1.0.2-win-x64-selfcontained.zip` | No — everything is bundled | ~63 MB |
| **Framework-dependent** | `TekkenModManager-1.0.2-win-x64-framework-dependent.zip` | Yes — [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) | ~1 MB |

**To install:** extract the ZIP to any folder and run `ModManager.exe`. No setup or admin rights needed.

> If you use the framework-dependent build and the app won't start, install the [.NET 8 Desktop Runtime (x64)](https://dotnet.microsoft.com/download/dotnet/8.0) and try again.

## Getting Started

1. **Set Game Location** — click *Set Game Location* (`Ctrl+G`) and browse to `TEKKEN8\Polaris\Content\Paks`. A `Mods` folder is created automatically if needed.
2. **Add a mod** — click *Add Zipped Mod* (`Ctrl+N`) or drag a ZIP/RAR/7Z onto the window.
3. **Enable/Disable** — tick the ACTIVE checkbox; the STATUS column shows ACTIVE/INACTIVE.


## Requirements

- Windows
- TEKKEN 8 (PC)

## Disclaimer

This is a community tool and is not affiliated with Bandai Namco. Use mods at your own risk.
