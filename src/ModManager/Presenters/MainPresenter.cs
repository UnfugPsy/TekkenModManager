using ModManager.Models;
using ModManager.Services;
using ModManager.Views;
using ModManager.Configuration;
using System.Diagnostics;
using System.IO.Compression;
using SharpCompress.Archives;
using SharpCompress.Common;

namespace ModManager.Presenters
{
    public class MainPresenter
    {
        private readonly IMainView _view;
        private readonly Model _model;
        private IModService? _modService;
        private readonly IProfileService _profileService;
        private readonly IModMetadataService _metadataService;
        private ModProfile _currentProfile;
        private ModFileWatcher? _fileWatcher;

        public MainPresenter(IMainView view)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _model = new Model();
            _profileService = new ProfileService();
            _metadataService = new ModMetadataService();

            InitializeModService();
            SubscribeToViewEvents();
            SubscribeToProfileEvents();

            LoadProfiles();
        }
        
        private void InitializeModService()
        {
            if (!string.IsNullOrEmpty(_model.GameLocation))
            {
                _modService = new ModService(_model.GameLocation);
                _view.SetGameLocationDisplay(_model.GameLocation);
                StartFileWatcher(_model.GameLocation);
            }
        }

        /// <summary>
        /// (Re)starts the external-change watcher for the given Paks root. Any previous
        /// watcher is disposed first so switching game locations never leaks handles.
        /// </summary>
        private void StartFileWatcher(string paksRoot)
        {
            _fileWatcher?.Dispose();
            _fileWatcher = null;

            if (string.IsNullOrEmpty(paksRoot) || !Directory.Exists(paksRoot))
                return;

            try
            {
                _fileWatcher = new ModFileWatcher(paksRoot);
                _fileWatcher.Changed += OnExternalModsChanged;
                _fileWatcher.Start();
            }
            catch
            {
                // Watching is a convenience; the manual F5 refresh still works if it fails.
                _fileWatcher = null;
            }
        }

        /// <summary>
        /// Fired on a background thread when Windows Explorer (or any external tool) mutates
        /// the mod folders. Triggers the same refresh the F5 key does, asynchronously.
        /// </summary>
        private void OnExternalModsChanged(object sender, EventArgs e)
        {
            RefreshModsList();
        }

        /// <summary>
        /// Runs one of the app's OWN file-system mutations with the watcher muted, so our
        /// writes under the Paks root don't bounce back as a phantom "external change" (which
        /// could fire a refresh mid-rename and read a half-toggled mod). If no watcher is
        /// active the action simply runs. We refresh explicitly after every such operation.
        /// </summary>
        private void RunSuppressingWatcher(Action action)
        {
            if (_fileWatcher == null)
            {
                action();
                return;
            }

            using (_fileWatcher.SuppressNotifications())
            {
                action();
            }
        }
        
        private void SubscribeToViewEvents()
        {
            _view.ViewLoaded += OnViewLoaded;
            _view.SetGameLocationClicked += OnSetGameLocationClicked;
            _view.RefreshClicked += OnRefreshClicked;
            _view.OpenModFolderClicked += OnOpenModFolderClicked;
            _view.AddModZipClicked += OnAddModZipClicked;
            _view.ModFileDropped += OnModFileDropped;
            _view.StartGameClicked += OnStartGameClicked;
            _view.HelpRequested += OnHelpRequested;
            _view.ModToggled += OnModToggled;
            _view.ModDeleteRequested += OnModDeleteRequested;
            _view.ModEditRequested += OnModEditRequested;
            _view.ModRenameRequested += OnModRenameRequested;
            
            _view.ProfileSelected += OnProfileSelected;
            _view.CreateProfileClicked += OnCreateProfileClicked;
            _view.ManageProfilesClicked += OnManageProfilesClicked;
            _view.SaveCurrentAsProfileClicked += OnSaveCurrentAsProfileClicked;
        }
        
        private void SubscribeToProfileEvents()
        {
            _profileService.ProfilesChanged += OnProfilesChanged;
        }
        
        private void LoadProfiles()
        {
            var profiles = _profileService.GetProfiles();
            _view.UpdateProfilesList(profiles);
            
            _currentProfile = _profileService.GetDefaultProfile();
            if (_currentProfile != null)
            {
                _view.SetCurrentProfile(_currentProfile);
            }
        }
        
        private void OnViewLoaded(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_model.GameLocation) && Directory.Exists(_model.GameLocation))
            {
                RefreshModsList();
            }
            else if (string.IsNullOrEmpty(_model.GameLocation))
            {
                _view.ShowMessage(AppConstants.Messages.SetGameLocationFirst);
                SetGameLocation();
            }
            else
            {
                _view.ShowMessage(AppConstants.Messages.GameLocationNotFound);
            }
        }
        
        private void OnSetGameLocationClicked(object sender, EventArgs e)
        {
            SetGameLocation();
        }
        
        private void SetGameLocation()
        {
            try
            {
                _model.SetGameLocation();
                _view.SetGameLocationDisplay(_model.GameLocation);
                
                if (!string.IsNullOrEmpty(_model.GameLocation))
                {
                    _modService = new ModService(_model.GameLocation);
                    StartFileWatcher(_model.GameLocation);
                    RefreshModsList();
                }
            }
            catch (Exception ex)
            {
                _view.ShowMessage($"Error setting game location: {ex.Message}", "Error", MessageType.Error);
            }
        }
        
        private void OnRefreshClicked(object sender, EventArgs e)
        {
            RefreshModsList();
        }
        
        private void RefreshModsList()
        {
            try
            {
                if (_modService == null)
                {
                    _view.ShowMods(new List<ModInfo>());
                    return;
                }

                var mods = _modService.GetMods();
                foreach (var mod in mods)
                    _metadataService.EnrichMod(mod);

                var conflicts = ConflictDetector.FindConflictingModNames(mods);
                _view.ShowConflicts(conflicts);
                _view.ShowMods(mods);
            }
            catch (Exception ex)
            {
                _view.ShowMessage($"Error refreshing mods list: {ex.Message}", "Error", MessageType.Error);
            }
        }
        
        private void OnOpenModFolderClicked(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(_model.GameLocation) && Directory.Exists(_model.GameLocation))
                {
                    Process.Start("explorer.exe", _model.GameLocation);
                }
                else
                {
                    _view.ShowMessage("Mod folder not set or does not exist.", "Warning", MessageType.Warning);
                }
            }
            catch (Exception ex)
            {
                _view.ShowMessage($"Error opening mod folder: {ex.Message}", "Error", MessageType.Error);
            }
        }
        
        private async void OnAddModZipClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_model.GameLocation) || !Directory.Exists(_model.GameLocation))
            {
                _view.ShowMessage("Please set a valid mod folder location first.", "Warning", MessageType.Warning);
                return;
            }
            
            try
            {
                string archivePath = _view.ShowAddModDialog();
                if (string.IsNullOrEmpty(archivePath))
                    return;
                
                await InstallModAsync(archivePath);
            }
            catch (Exception ex)
            {
                _view.HideProgress();
                _view.ShowMessage($"Error adding mod: {ex.Message}", "Error", MessageType.Error);
            }
        }
        
        private async void OnModFileDropped(object sender, ModFileDroppedEventArgs e)
        {
            if (string.IsNullOrEmpty(_model.GameLocation) || !Directory.Exists(_model.GameLocation))
            {
                _view.ShowMessage("Please set a valid mod folder location first.", "Warning", MessageType.Warning);
                return;
            }
            
            try
            {
                await InstallModAsync(e.FilePath);
            }
            catch (Exception ex)
            {
                _view.HideProgress();
                _view.ShowMessage($"Error installing dropped mod: {ex.Message}", "Error", MessageType.Error);
            }
        }
        
        private async Task InstallModAsync(string archivePath)
        {
            string archiveName = Path.GetFileNameWithoutExtension(archivePath);

            ModRootKind detectedKind = DetectRootKind(ReadArchiveEntryPaths(archivePath));
            ModRootKind? chosenKind = _view.ShowSelectModRootDialog(archiveName, detectedKind);
            if (chosenKind == null)
                return;

            string rootPath = _modService != null
                ? _modService.GetRootPath(chosenKind.Value)
                : Path.Combine(_model.GameLocation, ModRoots.FolderName(chosenKind.Value));
            Directory.CreateDirectory(rootPath);

            string finalPath = Path.Combine(rootPath, archiveName);
            string tempPath = Path.Combine(rootPath, ModRoots.InstallTempName(archiveName));

            if (Directory.Exists(finalPath))
            {
                bool overwrite = _view.ShowConfirmDialog(
                    $"A mod folder named '{archiveName}' already exists. Overwrite?",
                    "Folder Exists");

                if (!overwrite)
                    return;
            }

            _view.ShowProgress("Installing mod...");

            try
            {
                await Task.Run(() => ExtractArchive(archivePath, tempPath));

                RunSuppressingWatcher(() =>
                {
                    if (Directory.Exists(finalPath))
                        Directory.Delete(finalPath, true);

                    Directory.Move(tempPath, finalPath);
                });

                _view.ShowMessage($"Mod '{archiveName}' installed successfully.");
                RefreshModsList();
            }
            catch
            {
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
                throw;
            }
            finally
            {
                _view.HideProgress();
            }
        }
        
        public static void ExtractArchive(string archivePath, string extractPath)
        {
            Directory.CreateDirectory(extractPath);

            string fileExtension = Path.GetExtension(archivePath).ToLower();

            if (fileExtension == ".zip")
            {
                ZipFile.ExtractToDirectory(archivePath, extractPath);
            }
            else
            {
                using (var archive = ArchiveFactory.OpenArchive(archivePath))
                {
                    foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
                    {
                        entry.WriteToDirectory(extractPath, new ExtractionOptions()
                        {
                            ExtractFullPath = true,
                            Overwrite = true
                        });
                    }
                }
            }
        }

        /// <summary>Reads the relative entry paths inside an archive without extracting.</summary>
        private static List<string> ReadArchiveEntryPaths(string archivePath)
        {
            var paths = new List<string>();
            try
            {
                string fileExtension = Path.GetExtension(archivePath).ToLower();
                if (fileExtension == ".zip")
                {
                    using var zip = ZipFile.OpenRead(archivePath);
                    foreach (var entry in zip.Entries)
                    {
                        paths.Add(entry.FullName);
                    }
                }
                else
                {
                    using var archive = ArchiveFactory.OpenArchive(archivePath);
                    foreach (var entry in archive.Entries.Where(e => !e.IsDirectory))
                    {
                        if (entry.Key != null)
                        {
                            paths.Add(entry.Key);
                        }
                    }
                }
            }
            catch
            {
                // Detection is best-effort; fall back to Standard when the archive can't be inspected.
            }
            return paths;
        }

        /// <summary>
        /// Infers the destination mod root from an archive's entry paths.
        /// Matches an explicit mod-folder segment first, then falls back to a script heuristic.
        /// </summary>
        public static ModRootKind DetectRootKind(IEnumerable<string> entryPaths)
        {
            bool hasScript = false;

            foreach (var raw in entryPaths)
            {
                if (string.IsNullOrEmpty(raw))
                    continue;

                string normalized = raw.Replace('\\', '/');
                var segments = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries);

                foreach (var segment in segments)
                {
                    string trimmedSegment = segment.Trim();
                    if (trimmedSegment.Length == 0)
                        continue;

                    if (string.Equals(trimmedSegment, ModRoots.FolderName(ModRootKind.Logic), StringComparison.OrdinalIgnoreCase))
                        return ModRootKind.Logic;
                    if (string.Equals(trimmedSegment, ModRoots.FolderName(ModRootKind.Legacy), StringComparison.OrdinalIgnoreCase))
                        return ModRootKind.Legacy;
                    if (string.Equals(trimmedSegment, ModRoots.FolderName(ModRootKind.Standard), StringComparison.OrdinalIgnoreCase))
                        return ModRootKind.Standard;
                }

                string ext = Path.GetExtension(normalized).ToLowerInvariant();
                if (ext == ".lua" || ext == ".dll")
                {
                    hasScript = true;
                }
            }

            return hasScript ? ModRootKind.Logic : ModRootKind.Standard;
        }

        private void OnStartGameClicked(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(_model.GameLocation) || !Directory.Exists(_model.GameLocation))
                {
                    _view.ShowMessage("Please set a valid game location first.", "Warning", MessageType.Warning);
                    return;
                }

                if (_modService != null)
                {
                    var mods = _modService.GetMods();
                    if (mods.Count > 0 && !mods.Any(m => m.IsEnabled))
                    {
                        bool proceed = _view.ShowConfirmDialog(
                            "No mods are currently active. Launch TEKKEN 8 without any mods?",
                            "No Active Mods");
                        if (!proceed)
                            return;
                    }

                    var conflicts = ConflictDetector.FindConflictingModNames(mods);
                    if (conflicts.Count > 0)
                    {
                        bool proceed = _view.ShowConfirmDialog(
                            $"{conflicts.Count} mod(s) have conflicting .pak filenames and may override each other.\n\nLaunch anyway?",
                            "Mod Conflicts Detected");
                        if (!proceed)
                            return;
                    }
                }

                StartGame();
            }
            catch (Exception ex)
            {
                _view.ShowMessage($"Failed to start TEKKEN 8: {ex.Message}", "Error", MessageType.Error);
            }
        }
        
        private void StartGame()
        {
            string? gameDirectory = ResolveGameDirectory(_model.GameLocation);

            if (gameDirectory is null)
            {
                _view.ShowMessage(AppConstants.Messages.GameExeNotFound, "Error", MessageType.Error);
                return;
            }

            string exePath = Path.Combine(gameDirectory, "TEKKEN 8.exe");

            if (!File.Exists(exePath))
            {
                exePath = Path.Combine(gameDirectory, "Polaris-Win64-Shipping.exe");
            }

            if (File.Exists(exePath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = exePath,
                    UseShellExecute = true,
                    WorkingDirectory = gameDirectory
                });
            }
            else
            {
                _view.ShowMessage("TEKKEN 8 executable not found. Please make sure the game is installed correctly.", "Error", MessageType.Error);
            }
        }
        
        /// <summary>
        /// Finds the game folder (the parent of "Polaris") by climbing from the stored location,
        /// which is the Paks folder or, for older settings, one of its mod folders.
        /// </summary>
        public static string? ResolveGameDirectory(string? gameLocation)
        {
            if (string.IsNullOrWhiteSpace(gameLocation))
            {
                return null;
            }

            string? current = gameLocation.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            while (!string.IsNullOrEmpty(current))
            {
                if (string.Equals(Path.GetFileName(current), "Polaris", StringComparison.OrdinalIgnoreCase))
                {
                    return Path.GetDirectoryName(current);
                }

                current = Path.GetDirectoryName(current);
            }

            return null;
        }

        private void OnHelpRequested(object sender, EventArgs e)
        {
        }

        private void OnModToggled(object sender, ModToggleEventArgs e)
        {
            try
            {
                if (_modService == null)
                    return;
                
                var mods = _modService.GetMods();
                var mod = mods.FirstOrDefault(m => m.Key == e.ModKey);

                if (mod == null)
                    return;

                if (e.IsEnabled)
                {
                    // Logic toggles rename the folder; each subsequent handler re-resolves the
                    // mod via a fresh GetMods() by Key, so the stale ModInfo.Path is never reused.
                    RunSuppressingWatcher(() => _modService.ActivateMod(mod.Path));
                }
                else
                {
                    RunSuppressingWatcher(() => _modService.DeactivateMod(mod.Path));
                }
                
                RefreshModsList();

                if (_currentProfile != null)
                {
                    UpdateCurrentProfileFromMods();
                }
            }
            catch (Exception ex)
            {
                _view.ShowMessage($"Error toggling mod: {ex.Message}", "Error", MessageType.Error);
            }
        }

        private void OnModDeleteRequested(object sender, ModDeleteEventArgs e)
        {
            try
            {
                if (_modService == null)
                {
                    _view.ShowMessage("Mod service not initialized.", "Error", MessageType.Error);
                    return;
                }
                
                var mods = _modService.GetMods();
                var mod = mods.FirstOrDefault(m => m.Key == e.ModKey);

                if (mod == null)
                {
                    _view.ShowMessage($"Mod '{e.ModName}' not found.", "Error", MessageType.Error);
                    return;
                }

                bool confirmed = _view.ShowConfirmDialog(
                    $"Delete '{mod.Name}' permanently?\n\nThe folder is removed from disk, not moved to the Recycle Bin.",
                    "Delete Mod");

                if (!confirmed)
                {
                    return;
                }

                _view.ShowProgress("Deleting mod...");
                
                try
                {
                    if (Directory.Exists(mod.Path))
                    {
                        RunSuppressingWatcher(() => Directory.Delete(mod.Path, true));
                    }

                    _view.ShowMessage($"Mod '{e.ModName}' has been successfully deleted.", "Mod Deleted", MessageType.Information);
                    RefreshModsList();

                    if (_currentProfile != null)
                    {
                        UpdateCurrentProfileFromMods();
                    }
                }
                catch (Exception deleteEx)
                {
                    _view.ShowMessage($"Error deleting mod files: {deleteEx.Message}", "Delete Failed", MessageType.Error);
                }
                finally
                {
                    _view.HideProgress();
                }
            }
            catch (Exception ex)
            {
                _view.HideProgress();
                _view.ShowMessage($"Error deleting mod: {ex.Message}", "Error", MessageType.Error);
            }
        }
        

        private void OnProfileSelected(object sender, ViewProfileEventArgs e)
        {
            try
            {
                if (_modService == null || e.Profile == null)
                    return;
                
                _currentProfile = e.Profile;
                _profileService.ApplyProfile(e.Profile.Id, _modService);
                _view.SetCurrentProfile(e.Profile);
                RefreshModsList();
                
                _view.ShowMessage($"Applied profile: {e.Profile.Name}", "Profile Applied");
            }
            catch (Exception ex)
            {
                _view.ShowMessage($"Error applying profile: {ex.Message}", "Error", MessageType.Error);
            }
        }
        
        private void OnCreateProfileClicked(object sender, EventArgs e)
        {
            try
            {
                string profileName = _view.ShowCreateProfileDialog();
                if (string.IsNullOrWhiteSpace(profileName))
                    return;
                
                if (_modService == null)
                {
                    _view.ShowMessage("Please set game location first.", "Warning", MessageType.Warning);
                    return;
                }
                
                var currentMods = _modService.GetMods();
                var newProfile = _profileService.CreateFromCurrentState(profileName, 
                    "Profile created from current mod configuration", currentMods);
                
                _currentProfile = newProfile;
                _view.SetCurrentProfile(newProfile);
                
                _view.ShowMessage($"Profile '{profileName}' created successfully!", "Profile Created");
            }
            catch (Exception ex)
            {
                _view.ShowMessage($"Error creating profile: {ex.Message}", "Error", MessageType.Error);
            }
        }
        
        private void OnManageProfilesClicked(object sender, EventArgs e)
        {
            try
            {
                _view.ShowProfileManager(_profileService);
            }
            catch (Exception ex)
            {
                _view.ShowMessage($"Error opening profile manager: {ex.Message}", "Error", MessageType.Error);
            }
        }
        
        private void OnSaveCurrentAsProfileClicked(object sender, ViewProfileEventArgs e)
        {
            try
            {
                if (_modService == null)
                {
                    _view.ShowMessage("Please set game location first.", "Warning", MessageType.Warning);
                    return;
                }
                
                if (_currentProfile == null)
                {
                    OnCreateProfileClicked(sender, EventArgs.Empty);
                    return;
                }

                var currentMods = _modService.GetMods();
                _profileService.UpdateProfileWithCurrentMods(_currentProfile.Id, currentMods);
                
                _view.ShowMessage($"Profile '{_currentProfile.Name}' updated successfully!", "Profile Updated");
            }
            catch (Exception ex)
            {
                _view.ShowMessage($"Error saving profile: {ex.Message}", "Error", MessageType.Error);
            }
        }
        
        private void OnProfilesChanged(object sender, Services.ProfileEventArgs e)
        {
            var profiles = _profileService.GetProfiles();
            _view.UpdateProfilesList(profiles);
        }
        
        private void UpdateCurrentProfileFromMods()
        {
            if (_currentProfile == null || _modService == null)
                return;

            var currentMods = _modService.GetMods();
            _profileService.UpdateProfileWithCurrentMods(_currentProfile.Id, currentMods);
        }

        private void OnModEditRequested(object sender, ModEditEventArgs e)
        {
            try
            {
                var result = _view.ShowEditModDialog(e.Mod);
                if (result == null)
                    return;

                e.Mod.Version = result.Version;
                e.Mod.Category = result.Category;
                e.Mod.Description = result.Description;
                e.Mod.Author = result.Author;

                _metadataService.SaveMetadata(e.Mod);
                RefreshModsList();
            }
            catch (Exception ex)
            {
                _view.ShowMessage($"Error saving mod metadata: {ex.Message}", "Error", MessageType.Error);
            }
        }

        private void OnModRenameRequested(object sender, ModRenameEventArgs e)
        {
            try
            {
                if (_modService == null)
                {
                    _view.ShowMessage("Mod service not initialized.", "Error", MessageType.Error);
                    return;
                }

                var mods = _modService.GetMods();
                var mod = mods.FirstOrDefault(m => m.Key == e.ModKey);

                if (mod == null)
                {
                    _view.ShowMessage($"Mod '{e.ModName}' not found.", "Error", MessageType.Error);
                    return;
                }

                string newName = _view.ShowRenameModDialog(mod.Name);
                if (string.IsNullOrWhiteSpace(newName))
                    return;

                newName = newName.Trim();
                if (string.Equals(newName, mod.Name, StringComparison.Ordinal))
                    return;

                RunSuppressingWatcher(() => _modService.RenameMod(mod.Path, newName));
                _profileService.RenameModInProfiles(mod.RootKind, mod.Name, newName);

                RefreshModsList();
            }
            catch (Exception ex)
            {
                _view.ShowMessage($"Error renaming mod: {ex.Message}", "Error", MessageType.Error);
            }
        }
    }
}