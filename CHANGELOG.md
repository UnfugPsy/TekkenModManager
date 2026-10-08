# Changelog

One entry per release, newest first. The release script reads the section whose heading matches the version in `Directory.Build.props` and uses it as the GitHub release notes; `docs/forum-page.md` carries the same list for the TekkenMods page.

## [Unreleased]

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
