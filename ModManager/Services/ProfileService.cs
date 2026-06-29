using ModManager.Models;
using System.Text.Json;

namespace ModManager.Services
{
    public class ProfileService : IProfileService
    {
        private readonly string _profilesFilePath;
        private List<ModProfile> _profiles;
        private bool _isInitialized = false;
        
        public event EventHandler<ProfileEventArgs>? ProfilesChanged;

        public ProfileService()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appFolder = Path.Combine(appDataPath, "TekkenModManager");
            Directory.CreateDirectory(appFolder);
            _profilesFilePath = Path.Combine(appFolder, "profiles.json");

            _profiles = new List<ModProfile>();
            Initialize();
        }

        internal ProfileService(string profilesFilePath)
        {
            _profilesFilePath = profilesFilePath ?? throw new ArgumentNullException(nameof(profilesFilePath));
            _profiles = new List<ModProfile>();
            Initialize();
        }

        private void Initialize()
        {
            try
            {
                LoadProfiles();
                EnsureDefaultProfile();
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ProfileService] Initialization failed, falling back to defaults: {ex}");
                _profiles = new List<ModProfile>();
                CreateDefaultProfileSilently();
                _isInitialized = true;
            }
        }

        public List<ModProfile> GetProfiles()
        {
            EnsureInitialized();
            return _profiles.ToList();
        }

        public ModProfile? GetProfile(string id)
        {
            EnsureInitialized();
            return _profiles.FirstOrDefault(p => p.Id == id);
        }

        public ModProfile? GetDefaultProfile()
        {
            EnsureInitialized();
            return _profiles.FirstOrDefault(p => p.IsDefault) ?? _profiles.FirstOrDefault();
        }

        private void EnsureInitialized()
        {
            if (!_isInitialized)
            {
                Initialize();
            }
        }

        public void CreateProfile(string name, string description = "")
        {
            EnsureInitialized();
            var profile = new ModProfile(name, description);
            _profiles.Add(profile);
            SaveProfiles();
            
            FireProfilesChanged(ProfileEventType.Created, profile);
        }

        public ModProfile CreateFromCurrentState(string name, string description, List<ModInfo> currentMods)
        {
            EnsureInitialized();
            var enabledModNames = currentMods.Where(m => m.IsEnabled).Select(m => m.Name).ToList();
            var profile = new ModProfile(name, description, enabledModNames);
            _profiles.Add(profile);
            SaveProfiles();
            
            FireProfilesChanged(ProfileEventType.Created, profile);
            return profile;
        }

        public void UpdateProfile(ModProfile profile)
        {
            EnsureInitialized();
            var existingProfile = _profiles.FirstOrDefault(p => p.Id == profile.Id);
            if (existingProfile == null) return;

            existingProfile.Name = profile.Name;
            existingProfile.Description = profile.Description;
            existingProfile.EnabledMods = profile.EnabledMods.ToList();
            existingProfile.LastUsed = DateTime.Now;
            
            SaveProfiles();
            FireProfilesChanged(ProfileEventType.Updated, existingProfile);
        }

        public void DeleteProfile(string id)
        {
            EnsureInitialized();
            var profile = _profiles.FirstOrDefault(p => p.Id == id);
            if (profile == null || profile.IsDefault) return;

            _profiles.Remove(profile);
            SaveProfiles();
            
            FireProfilesChanged(ProfileEventType.Deleted, profile);
        }

        public void DuplicateProfile(string sourceId, string newName)
        {
            EnsureInitialized();
            var originalProfile = _profiles.FirstOrDefault(p => p.Id == sourceId);
            if (originalProfile == null) throw new ArgumentException("Profile not found");

            var duplicatedProfile = originalProfile.Clone();
            duplicatedProfile.Name = newName;
            _profiles.Add(duplicatedProfile);
            SaveProfiles();
            
            FireProfilesChanged(ProfileEventType.Created, duplicatedProfile);
        }

        public void SetDefaultProfile(string id)
        {
            EnsureInitialized();
            var profile = _profiles.FirstOrDefault(p => p.Id == id);
            if (profile == null) return;

            foreach (var p in _profiles)
                p.IsDefault = false;

            profile.IsDefault = true;
            SaveProfiles();
            
            FireProfilesChanged(ProfileEventType.DefaultChanged, profile);
        }

        public void SaveProfile(ModProfile profile)
        {
            UpdateProfile(profile);
        }

        public void ApplyProfile(string id, IModService modService)
        {
            if (modService == null) return;
            EnsureInitialized();

            var profile = _profiles.FirstOrDefault(p => p.Id == id);
            if (profile == null) return;

            var allMods = modService.GetMods();
            var toDeactivate = allMods.Where(m => m.IsEnabled && !profile.EnabledMods.Contains(m.Name)).ToList();
            var toActivate = allMods.Where(m => !m.IsEnabled && profile.EnabledMods.Contains(m.Name)).ToList();

            foreach (var mod in toDeactivate)
                modService.DeactivateMod(mod.Path);

            foreach (var mod in toActivate)
                modService.ActivateMod(mod.Path);

            profile.LastUsed = DateTime.Now;
            SaveProfiles();

            FireProfilesChanged(ProfileEventType.Applied, profile);
        }

        public void UpdateProfileWithCurrentMods(string profileId, List<ModInfo> currentMods)
        {
            EnsureInitialized();
            var profile = _profiles.FirstOrDefault(p => p.Id == profileId);
            if (profile == null) return;

            profile.EnabledMods = currentMods.Where(m => m.IsEnabled).Select(m => m.Name).ToList();
            profile.LastUsed = DateTime.Now;
            SaveProfiles();
            
            FireProfilesChanged(ProfileEventType.Updated, profile);
        }

        private void EnsureDefaultProfile()
        {
            if (_profiles.Count == 0 || !_profiles.Any(p => p.IsDefault))
            {
                CreateDefaultProfileSilently();
            }
        }

        private void CreateDefaultProfileSilently()
        {
            var defaultProfile = new ModProfile("Default", "Default mod configuration")
            {
                IsDefault = true
            };
            _profiles.Insert(0, defaultProfile);
            SaveProfiles();
        }

        private void FireProfilesChanged(ProfileEventType eventType, ModProfile profile)
        {
            if (_isInitialized)
            {
                try
                {
                    ProfilesChanged?.Invoke(this, new ProfileEventArgs(eventType, profile));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ProfileService] A ProfilesChanged subscriber threw an exception: {ex}");
                }
            }
        }

        private void LoadProfiles()
        {
            try
            {
                if (File.Exists(_profilesFilePath))
                {
                    string json = File.ReadAllText(_profilesFilePath);
                    
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        var profiles = JsonSerializer.Deserialize<List<ModProfile>>(json);
                        _profiles = profiles ?? new List<ModProfile>();
                    }
                    else
                    {
                        _profiles = new List<ModProfile>();
                    }
                }
                else
                {
                    _profiles = new List<ModProfile>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ProfileService] Failed to load profiles from '{_profilesFilePath}': {ex}");
                _profiles = new List<ModProfile>();

                if (File.Exists(_profilesFilePath))
                {
                    try
                    {
                        string backupPath = _profilesFilePath + ".backup." + DateTime.Now.ToString("yyyyMMdd_HHmmss");
                        File.Copy(_profilesFilePath, backupPath);
                        System.Diagnostics.Debug.WriteLine($"[ProfileService] Corrupt profiles backed up to '{backupPath}'");
                    }
                    catch (Exception backupEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"[ProfileService] Could not back up corrupt profiles file: {backupEx.Message}");
                    }
                }
            }
        }

        private void SaveProfiles()
        {
            try
            {
                if (_profiles == null)
                {
                    _profiles = new List<ModProfile>();
                    return;
                }

                if (!_profiles.Any(p => p.IsDefault) && _profiles.Count > 0)
                {
                    _profiles[0].IsDefault = true;
                }

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                string json = JsonSerializer.Serialize(_profiles, options);
                
                string tempFile = _profilesFilePath + ".tmp";
                File.WriteAllText(tempFile, json);
                
                if (File.Exists(_profilesFilePath))
                {
                    File.Replace(tempFile, _profilesFilePath, null);
                }
                else
                {
                    File.Move(tempFile, _profilesFilePath);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to save profiles", ex);
            }
        }
    }

    public class ProfileEventArgs : EventArgs
    {
        public ProfileEventType EventType { get; }
        public ModProfile Profile { get; }

        public ProfileEventArgs(ProfileEventType eventType, ModProfile profile)
        {
            EventType = eventType;
            Profile = profile;
        }
    }

    public enum ProfileEventType
    {
        Created,
        Updated,
        Deleted,
        Applied,
        DefaultChanged
    }
}