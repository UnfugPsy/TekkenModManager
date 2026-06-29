using ModManager.Models;

namespace ModManager.Services
{
    public interface IModMetadataService
    {
        void EnrichMod(ModInfo mod);
        void SaveMetadata(ModInfo mod);
    }
}
