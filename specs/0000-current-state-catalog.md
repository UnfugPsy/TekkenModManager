# 0000 — Current State Catalog (Baseline)

| Field        | Value                          |
|--------------|--------------------------------|
| **Spec ID**  | 0000                           |
| **Status**   | Shipped                        |
| **Author**   | Unfug                          |
| **Created**  | 2025-01-01                     |
| **Target**   | 1.1.0                          |
| **Shipped**  | 1.1.0-experimental             |

> This is a **baseline catalog**, not a forward spec. It records what the
> application already does as of `1.1.0-experimental`, so future specs
> (`0001+`) have a documented starting point. Numbered `0000` because it
> predates the spec-driven workflow.

## Summary

TEKKEN 8 Mod Manager is a .NET 8 WinForms desktop app (MVP architecture) that
installs, organizes, toggles, and profiles game mods across the three UE5/UE4SS
mod roots, with conflict detection, metadata enrichment, and live filesystem
watching.

## Architecture

- **Pattern:** Model–View–Presenter.
  - View: `Form1` implements `Views/IMainView.cs`.
  - Presenter: `Presenters/MainPresenter.cs` (orchestration, no UI types).
  - Services: injected via interfaces (`IModService`, `IProfileService`, `IModMetadataService`).
- **Target:** `net8.0-windows`, WinExe, C# 12, nullable + implicit usings on.
- **Packaging:** SharpCompress 0.48.0 for archive extraction.

## Feature Inventory

| Area | Capability | Primary Code | Spec |
|------|-----------|--------------|------|
| Mod roots | Standard / Legacy / Logic detection & mapping | `Models/ModRootKind.cs`, `Services/ModService.cs` | 0001 |
| Listing | Unified multi-root mod list with Type column | `Controls/ModViewManager.cs`, `Services/ModService.GetMods` | 0001 |
| Enable/Disable | Companion-file `-x` suffix (Standard/Legacy); folder `.disabled_` rename (Logic) | `Services/ModService.cs` | 0001 |
| Install | Add from `.zip/.rar/.7z`, auto-detect root, override dialog | `Presenters/MainPresenter.cs`, `Views/SelectModRootDialog.cs` | 0001 |
| Profiles | Create / duplicate / delete / default / apply; compound `RootKind:Name` keys | `Services/ProfileService.cs`, `Models/ModProfile.cs`, `Views/ProfileManagerForm.cs` | — |
| Metadata | Version / category / description / author enrichment + edit dialog | `Services/ModMetadataService.cs`, `Views/ModMetadataEditDialog.cs`, `Models/ModMetadataDto.cs` | — |
| Conflicts | Detect mods that clash; surface in the UI | `Services/ConflictDetector.cs`, `IMainView.ShowConflicts` | — |
| Live watch | Debounced `FileSystemWatcher` w/ re-entrancy suppression | `Services/ModFileWatcher.cs` | 0001 |
| Config/paths | Game location resolve + legacy `\Paks\Mods` migration | `Models/Model.cs`, `Configuration/AppConstants.cs` | 0001 |
| Theming | Light/dark theme | `Theme.cs`, `Utils/ThemeUtils.cs` | — |
| Help | In-app help window | `Views/HelpForm.cs`, `Utils/HelpFormUtils.cs` | — |

## Service Contracts (as shipped)

- `IModService` — `GetMods`, `IsModEnabled`, `ActivateMod`, `DeactivateMod`, `DeleteMod`, `RenameMod`, `GetModsDirectory`, `GetRootPath(kind)`.
- `IProfileService` — CRUD + `SetDefaultProfile`, `ApplyProfile`, `CreateFromCurrentState`, `UpdateProfileWithCurrentMods`, `RenameModInProfiles`, `ProfilesChanged` event.
- `IModMetadataService` — `EnrichMod`, `SaveMetadata`.
- `IMainView` — event-driven surface (toggle/delete/edit/rename/profile events) + display methods and modal dialogs.

## Test Coverage (baseline)

120 xUnit tests across:

- `Services/ModServiceTests.cs`
- `Services/ProfileServiceTests.cs`
- `Services/ModMetadataServiceTests.cs`
- `Services/ConflictDetectorTests.cs`
- `Services/ModFileWatcherTests.cs`
- `Presenters/MainPresenterDetectRootKindTests.cs`
- `Presenters/MainPresenterExtractArchiveTests.cs`
- `Models/ModProfileTests.cs`
- `Models/ModelResolveModsDirectoryTests.cs`

## Known Gaps / Spec Debt

These shipped features have **no dedicated spec** yet. Candidates to back-fill
as `0002+` when next touched:

- [ ] Profiles system (create/apply/default semantics, compound keys).
- [ ] Metadata enrichment & persistence format (`ModMetadataDto`).
- [ ] Conflict detection rules.
- [ ] Theming.
- [ ] Config/game-location resolution & migration rules.

## Notes

- Only spec `0001` (multi-root) is back-filled in detail; this catalog covers the rest at inventory level.
- New work should follow `specs/README.md`: write a spec before code, back-filling the relevant gap above if you touch an un-spec'd area.
