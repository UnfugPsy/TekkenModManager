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
            string finalPath = Path.Combine(_model.GameLocation, archiveName);
            string tempPath = Path.Combine(_model.GameLocation, $"_{archiveName}_tmp_{Guid.NewGuid():N}");

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

                if (Directory.Exists(finalPath))
                    Directory.Delete(finalPath, true);

                Directory.Move(tempPath, finalPath);

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
        
        private void ExtractArchive(string archivePath, string extractPath)
        {
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
            string modsDirectory = _model.GameLocation;
            string? paksDirectory = Path.GetDirectoryName(modsDirectory);
            string? contentDirectory = paksDirectory is null ? null : Path.GetDirectoryName(paksDirectory);
            string? polarisDirectory = contentDirectory is null ? null : Path.GetDirectoryName(contentDirectory);
            string? gameDirectory = polarisDirectory is null ? null : Path.GetDirectoryName(polarisDirectory);

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
                var mod = mods.FirstOrDefault(m => m.Name == e.ModName);
                
                if (mod == null)
                    return;
                
                if (e.IsEnabled)
                {
                    _modService.ActivateMod(mod.Path);
                }
                else
                {
                    _modService.DeactivateMod(mod.Path);
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
                var mod = mods.FirstOrDefault(m => m.Name == e.ModName);
                
                if (mod == null)
                {
                    _view.ShowMessage($"Mod '{e.ModName}' not found.", "Error", MessageType.Error);
                    return;
                }
                
                _view.ShowProgress("Deleting mod...");
                
                try
                {
                    if (Directory.Exists(mod.Path))
                    {
                        Directory.Delete(mod.Path, true);
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
    }
}