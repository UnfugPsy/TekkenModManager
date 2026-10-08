# Releasing

1. **Fix and test.** `dotnet test ModManager.sln` is green, and the change was tried in the running app.
2. **Pick the version** (`MAJOR.MINOR.PATCH`): a bug fix is a patch, a backward-compatible feature a minor, a breaking change a major.
3. **Bump it once** in `Directory.Build.props` (`VersionPrefix`, and `VersionSuffix` while experimental). The window's ABOUT text and the exe's file properties read it from there.
4. **Write the changelog.** Add a `## [X.Y.Z] - date` section to `CHANGELOG.md`; the release script uses it as the release notes and fails if it is missing.
5. **Update the text that names the files.** The two zip names in `README.md`, the zip names and the Versions list in `docs/forum-page.md`; if a shortcut or menu changed, the in-app help (`src/ModManager/Views/HelpForm.cs`, `Utils/HelpFormUtils.cs`, the bottom label in `Form1.Designer.cs`).
6. **Build the zips.** `pwsh scripts/release.ps1` runs the tests, publishes both builds and writes `artifacts/release/` (the two zips, `SHA256SUMS.txt`, `notes.md`). Extract the self-contained zip somewhere clean and start `ModManager.exe`.
7. **Commit**, then tag `vX.Y.Z` (for example `v1.1.0-experimental`) and push the tag. On GitHub the Release workflow rebuilds the zips and creates the release; a tag with a suffix is marked pre-release.
8. **TekkenMods.** Upload the two zips to the mod page, paste the changelog lines into the Versions list and a comment.
9. **Reply.** Answer the comments that asked for it, name the version, and ask for a short yes or no (`docs/feedback.md`).
