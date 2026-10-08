# TEKKEN 8 Mod Manager

**A lightweight Windows app for installing, toggling, and organizing your TEKKEN 8 mods.**

![MainWindowWithMods](https://dist.tekkenmods.com/dist-cache/1920/80945/media/1a73613752e70248394a8a64b3e8577d-802x584.png)

---

## Why you'll like it

**One-click on/off — no manual file renaming**
Every mod has an ACTIVE checkbox. Tick it to enable, untick to disable. The app renames the mod files in place for you (pak / ucas / utoc), so you never have to dig through folders or rename anything by hand.

**Install straight from ZIP, RAR, or 7Z**
Add a mod with the button or just **drag and drop the archive onto the window**. It extracts and installs in the right place automatically.

**Profiles — swap whole mod loadouts in seconds**
Save a set of enabled mods as a profile, then switch between them instantly. Great for keeping a "tournament-legal" set, a "fun/chaos" set, and a "screenshot/photo-mode" set side by side.

**Conflict detection that actually warns you**
If two mods ship the same pak file, they'll fight over the same slot. The manager flags these conflicts up front so you know exactly which mods override each other — no more silent breakage.

**Editable metadata for every mod**
Add a version, category, author, and description to each mod (right-click a mod, or double-click it). Keeps a big collection tidy and searchable.

![EditMetaData](https://dist.tekkenmods.com/dist-cache/1920/80945/media/70e75e3762eb29fc187f6a07bbb4e3b6-770x572.png)

**Instant search**
Filter your whole list by name, category, or author as you type.

**Pick your game folder the easy way**
Setting your game location opens a simple folder browser — just select your Paks (or Mods) folder and click OK.

---

## Features at a glance

- One-click enable/disable via the ACTIVE checkbox (auto file renaming)
- Supports all three mod folders — Mods, ~mods, and LogicMods (UE4SS script mods)
- Install from ZIP / RAR / 7Z — button or drag and drop, with automatic folder detection
- Profiles — save and apply sets of enabled mods, with a default profile
- Conflict detection for mods sharing the same pak name
- Editable metadata — version, category, author, description per mod
- Rename a mod's folder right from the list (F2)
- Search by name, category, author, or type
- Handy keyboard shortcuts
- Clean, modern Windows interface

---

## Keyboard shortcuts

- **F1** — Show help dialog
- **F2** — Rename the selected mod
- **F5** — Refresh mod list
- **Ctrl + G** — Set game location
- **Ctrl + O** — Open mod folder
- **Ctrl + N** — Add new mod from archive
- **Esc** — Close dialogs

---

## Download & install

Grab the latest release, pick **one** of the two builds, extract it anywhere, and run **ModManager.exe**. No setup and no admin rights needed.

> ⚠️ **Experimental build** — 1.1.0-experimental adds multi-root support (Mods / ~mods / LogicMods) and real-time folder watching. It's provided for testing; please back up your mods first and report anything that misbehaves.

**Recommended — Self-contained** (about 63 MB)
Everything is bundled. Nothing extra to install. Just unzip and run.
File: **TekkenModManager-1.1.0-experimental-win-x64-selfcontained.zip**

**Smaller — Framework-dependent** (about 1 MB)
Tiny download, but it needs the **.NET 8 Desktop Runtime (x64)** installed on your PC.
File: **TekkenModManager-1.1.0-experimental-win-x64-framework-dependent.zip**
Get the runtime here: https://dotnet.microsoft.com/download/dotnet/8.0

> If you use the smaller build and the app won't open, install the **.NET 8 Desktop Runtime (x64)** from the link above and try again.

---

## Getting started

1. **Set your game location** — click Set Game Location (Ctrl + G) and pick your TEKKEN 8 Paks folder (typically TEKKEN8 \ Polaris \ Content \ Paks). The Mods, ~mods, and LogicMods folders are used automatically if they exist.
2. **Add a mod** — click Add Zipped Mod (Ctrl + N), or drag a ZIP / RAR / 7Z onto the window. The manager detects whether it's a standard, legacy, or logic mod and lets you confirm the destination folder.
3. **Enable or disable** — tick the ACTIVE checkbox; the STATUS column shows ACTIVE or INACTIVE.

![](https://dist.tekkenmods.com/dist-cache/1920/80945/media/3ef5b6328d01a4d75c4356ccff37a246-628x430.png)

---

## FAQ

**I already have mods installed but don't have the ZIPs anymore. Do I have to re-download them all?**
Nope. Just open your Paks / Mods folder and create one folder per mod, then copy the **full mod** into its own folder. The manager picks them up automatically — it works on a folder basis, one folder per mod. Heads up: if you mix files from different mods into the same folder, the manager treats it as a single mod and won't separate them, so keep each mod in its own folder.

**Will it delete or break my existing mods?**
No. Enabling and disabling just renames the mod files in place (it adds a marker to disable, removes it to enable). Nothing is deleted when you toggle, and it's fully reversible.

**How do I turn a mod off without removing it?**
Untick its ACTIVE checkbox. The mod stays on your drive and in the list — it's just inactive until you tick it again.

**Where does it keep my mods?**
In your game's Paks / Mods folder (it creates the Mods folder for you if it isn't there). Each mod lives in its own subfolder.

**Can I still install mods manually while using this?**
Yes. Manual folders and archive installs live happily side by side — drop a folder in yourself or use Add Zipped Mod, whichever you prefer.

**The app won't open.**
If you grabbed the smaller framework-dependent build, install the **.NET 8 Desktop Runtime (x64)** and try again. The self-contained build needs nothing extra.

---

## Requirements

- Windows
- TEKKEN 8 (PC)

---

## Versions

- 1.0.0: Initial release
- 1.0.1: Fix "Directory does not exist to extract to" when adding .rar/.7z mods (create destination directory before extraction in ExtractArchive)
- 1.0.2: Fix mods that stay inactive and can't be toggled when their .pak/.ucas/.utoc files live in a nested subfolder (e.g. Content/Paks)
- 1.0.3: Add the ability to rename a mod's folder from the list (right-click > Rename, or press F2); profile references update automatically
- 1.1.0-experimental: Support all three mod folders (Mods, ~mods, LogicMods) in one list with a new TYPE column; auto-detect the target folder when adding a mod; logic (UE4SS script) mods now enable/disable correctly via folder renaming; real-time folder watching; existing setups migrate automatically. Experimental — testing feedback welcome

---

## Disclaimer

This is a free community tool and is **not affiliated with Bandai Namco**. Use mods at your own risk.

I might continue but this is it for now.