using ModManager.Models;

namespace ModManager.Services
{
    public interface IModService
    {
        List<ModInfo> GetMods();
        bool IsModEnabled(string modFolderPath);
        string ActivateMod(string modFolderPath);
        string DeactivateMod(string modFolderPath);
        void DeleteMod(string modFolderPath);
        string RenameMod(string modFolderPath, string newName);
        string GetModsDirectory();
        string GetRootPath(ModRootKind kind);
    }
}