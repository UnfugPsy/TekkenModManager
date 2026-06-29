using ModManager.Models;

namespace ModManager.Services
{
    public interface IModService
    {
        List<ModInfo> GetMods();
        bool IsModEnabled(string modFolderPath);
        void ActivateMod(string modFolderPath);
        void DeactivateMod(string modFolderPath);
        void DeleteMod(string modFolderPath);
        string GetModsDirectory();
    }
}