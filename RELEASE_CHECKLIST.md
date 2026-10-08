# Release Checklist

Follow this after you've fixed or added something and you're ready to ship a new version.

## 1. Verify the code is good
- [ ] Build the solution — no errors.
- [ ] Run the full test suite — everything green.
- [ ] Manually smoke-test the change in the running app.

## 2. Pick the new version number
Use semantic versioning `MAJOR.MINOR.PATCH`:
- **PATCH** (e.g. 1.0.3 → 1.0.4) — bug fix, no new features.
- **MINOR** (e.g. 1.0.3 → 1.1.0) — new feature, backward compatible.
- **MAJOR** (e.g. 1.0.3 → 2.0.0) — breaking change.

## 3. Bump the version
In `ModManager\ModManager.csproj`:
- [ ] `<Version>` → new version (e.g. `1.0.4`)
- [ ] `<FileVersion>` → new version with `.0` suffix (e.g. `1.0.4.0`)

## 4. Update the docs
- [ ] `FORUM_DESCRIPTION.md`
  - Add a line to the **Versions** changelog.
  - Update the two zip filenames under **Download & install**.
  - If the change is user-facing, update **Features at a glance**, **Keyboard shortcuts**, and/or the **FAQ**.
- [ ] `README.md`
  - Update the two zip filenames under the download table.
  - Update **Features** / **Keyboard Shortcuts** if user-facing.
- [ ] In-app help (`ModManager\Views\HelpForm.cs`) — update shortcuts / right-click menu / steps if behavior changed.
  - If you added a keyboard shortcut, also update the highlight check in `ModManager\Utils\HelpFormUtils.cs` and the bottom label in `ModManager\Form1.Designer.cs`.

## 5. Publish both builds
Run from the repo root (`K:\AI_Workspace\Repositories\ModManager\ModManager`).

Self-contained (no runtime needed, ~63 MB):
```
dotnet publish ModManager\ModManager.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

Framework-dependent (needs .NET 8 Desktop Runtime, ~1 MB):
```
dotnet publish ModManager\ModManager.csproj -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o "publish\framework-dependent"
```

## 6. Package the zips
- Stage each publish output, excluding `.pdb` and `.xml` files.
- Zip with version in the name (replace `X.Y.Z`):
  - `TekkenModManager-X.Y.Z-win-x64-selfcontained.zip`
  - `TekkenModManager-X.Y.Z-win-x64-framework-dependent.zip`
- Delete the previous version's zips from `release\`.

Self-contained source: `ModManager\bin\Release\net8.0-windows\win-x64\publish`
Framework-dependent source: `publish\framework-dependent`

Each zip should contain only `ModManager.exe` and `ModManager.dll.config`.

## 7. Verify the packages
- [ ] Both zips exist in `release\` with the new version in the filename.
- [ ] Sizes look right (~63 MB self-contained, ~1 MB framework-dependent).
- [ ] Extract the self-contained zip somewhere clean and confirm `ModManager.exe` launches.
- [ ] Right-click the exe → Properties → Details shows the new version.

## 8. Commit
- [ ] Commit code + doc changes with a clear message (e.g. `Release vX.Y.Z: <summary>`).
- [ ] Attach the two zips to the release / forum post.
- [ ] Paste the changelog line into the forum entry.

## Quick reference: files that carry a version number
- `ModManager\ModManager.csproj` — `<Version>` and `<FileVersion>`
- `README.md` — zip filenames
- `FORUM_DESCRIPTION.md` — zip filenames + Versions changelog
- `release\` — the produced `.zip` filenames
