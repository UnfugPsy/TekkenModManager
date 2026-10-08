# 0001 — Multi-Root Support (Mods / ~mods / LogicMods)

| Field        | Value                    |
|--------------|--------------------------|
| **Spec ID**  | 0001                     |
| **Status**   | Shipped                  |
| **Author**   | Unfug                    |
| **Created**  | 2025-01-01               |
| **Target**   | 1.1.0                    |
| **Shipped**  | 1.1.0-experimental       |

> This spec is back-filled from the shipped 1.1.0 work to serve as a worked
> example of the spec format. Future specs should be written *before* coding.

## Summary

Manage all three TEKKEN 8 mod folders — `Mods` (standard), `~mods` (legacy),
and `LogicMods` (UE4SS scripts) — from a single unified list, with automatic
detection of the correct destination when installing a mod.

## Motivation

Previously only the standard `Mods` folder was managed. Users with legacy
`~mods` packs or UE4SS `LogicMods` scripts had to manage those by hand. A single
list with a TYPE column removes that friction and prevents mistakes.

## Goals

- Show mods from all three roots in one list, tagged by type.
- Auto-detect the target root when adding a mod archive.
- Enable/disable correctly per root (file suffix vs. folder rename).
- Migrate existing setups automatically.

## Non-Goals

- Managing arbitrary user-defined roots beyond the three known folders.
- Editing mod file contents.

## Requirements

1. R1 — A `ModRootKind` enum models `Standard`, `Legacy`, `Logic`; `ModRoots` maps each to its folder name (`Mods`, `~mods`, `LogicMods`).
2. R2 — `GetMods` scans all three roots, skipping any that don't exist, and tags each `ModInfo` with its `RootKind` and `RootPath`.
3. R3 — Standard/Legacy mods toggle by suffixing payload files (`.pak`/`.ucas`/`.utoc`/`.sig`/`.txt`) with `-x` as one atomic set.
4. R4 — Logic mods toggle by renaming the folder with a `.disabled_` prefix; internal files stay pristine.
5. R5 — Profiles key mods by the compound identifier `"{RootKind}:{Name}"` so same-named mods in different roots never collide.
6. R6 — Install auto-detects the destination root from archive contents; the user can override via a dialog.
7. R7 — Legacy config pointing at `...\Paks\Mods` migrates up to `...\Paks` on startup.

## Design / Approach

- `ModManager/Models/ModRootKind.cs` — enum + `ModRoots` lookup table + `DisabledPrefix`.
- `ModManager/Models/ModInfo.cs` — adds `RootKind`, `RootPath`, and computed `Key`.
- `ModManager/Services/ModService.cs` — multi-root scan; per-root toggle logic; companion-file atomicity helpers.
- `ModManager/Presenters/MainPresenter.cs` — `DetectRootKind` heuristic; root-selection dialog wiring.
- `ModManager/Services/ProfileService.cs` — compound-key storage, rename, and migration.
- `ModManager/Controls/ModViewManager.cs` — TYPE column + search.

## Edge Cases & Risks

- Cross-root name collisions — handled by the compound `Key` (R5).
- Case/whitespace in archive paths — `DetectRootKind` trims and compares `OrdinalIgnoreCase`.
- Missing root folders on a fresh setup — scan guards each root with `Directory.Exists`.
- Logic-toggle folder rename invalidates the in-memory path — handlers re-resolve by `Key` after each op.

## Acceptance Criteria

- [x] AC1 — All three roots appear in the list with a correct TYPE tag.
- [x] AC2 — Toggling a standard mod moves all companion files atomically.
- [x] AC3 — Toggling a logic mod renames the folder and leaves inner files untouched.
- [x] AC4 — Two mods named the same in different roots never overwrite each other in a profile.
- [x] AC5 — Adding a mod auto-selects the right root, overridable by the user.
- [x] AC6 — Legacy `...\Paks\Mods` config migrates to `...\Paks`.
- [x] Build succeeds and all tests pass.
- [x] Docs updated (README / FORUM_DESCRIPTION / in-app help).

## Test Plan

- Unit/integration: `ModServiceTests`, `MainPresenterDetectRootKindTests`,
  `MainPresenterExtractArchiveTests`, `ProfileServiceTests`, `ModFileWatcherTests`.
- Manual: install a mod of each type, toggle each, create a profile with
  same-named mods across roots, restart to confirm migration.

## Open Questions

- None.
