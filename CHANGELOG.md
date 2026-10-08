# Changelog

One entry per release, newest first. The release script reads the section whose heading matches the version in `Directory.Build.props` and uses it as the GitHub release notes; `docs/forum-page.md` carries the same list for the TekkenMods page.

## [Unreleased]

## [1.1.1] - 2026-10-08

- No longer marked experimental: the multi-root support from 1.1.0 has run for twelve weeks without a bug report against it
- Fix "TEKKEN 8 executable not found" when pressing Start TEKKEN 8 (reported on the TekkenMods page): the game folder is now found by climbing to the `Polaris` folder
- Ask for confirmation before every delete, including the right-click "Delete Mod" (a delete is permanent, not to the Recycle Bin)
- Enabling or disabling a mod is all-or-none: if one file cannot be renamed, the others are put back instead of leaving the mod half-switched
- Conflict warnings now also compare `.pak` files in nested subfolders
- The installer's scratch folder no longer shows up as a mod while an archive is extracting
- Faster refresh: folder sizes are measured when shown, not on every scan

## [1.1.0-experimental] - 2026-07-12

- Support all three mod folders (Mods, ~mods, LogicMods) in one list with a new TYPE column
- Auto-detect the target folder when adding a mod, confirmable in a dialog
- Logic (UE4SS script) mods now enable and disable correctly via folder renaming
- Real-time folder watching: changes made in Explorer show up without a refresh
- Existing setups migrate automatically (stored game location and profile keys)
- Experimental - testing feedback welcome

## [1.0.3]

- Rename a mod's folder from the list (right-click > Rename, or F2); profile references update automatically

## [1.0.2] - 2026-07-01

- Fix mods that stay inactive and cannot be toggled when their .pak/.ucas/.utoc files live in a nested subfolder (for example Content/Paks)

## [1.0.1] - 2026-06-30

- Fix "Directory does not exist to extract to" when adding .rar/.7z mods (the destination directory is now created before extraction)

## [1.0.0] - 2026-06-29

- Initial release
