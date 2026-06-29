using ModManager.Models;

namespace ModManager.Services
{
    public interface IProfileService
    {
        event EventHandler<ProfileEventArgs> ProfilesChanged;

        List<ModProfile> GetProfiles();
        ModProfile? GetProfile(string id);
        ModProfile? GetDefaultProfile();
        void CreateProfile(string name, string description = "");
        void DeleteProfile(string id);
        void DuplicateProfile(string sourceId, string newName);
        void SetDefaultProfile(string id);
        void SaveProfile(ModProfile profile);
        void UpdateProfile(ModProfile profile);
        void ApplyProfile(string id, IModService modService);
        ModProfile CreateFromCurrentState(string name, string description, List<ModInfo> currentMods);
        void UpdateProfileWithCurrentMods(string profileId, List<ModInfo> currentMods);
    }
}