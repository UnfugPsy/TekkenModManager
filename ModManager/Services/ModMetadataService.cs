using ModManager.Models;
using System.Text.Json;

namespace ModManager.Services
{
    public class ModMetadataService : IModMetadataService
    {
        private const string FileName = "modinfo.json";

        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        public void EnrichMod(ModInfo mod)
        {
            string path = SidecarPath(mod.Path);
            if (!File.Exists(path))
                return;

            try
            {
                string json = File.ReadAllText(path);
                var dto = JsonSerializer.Deserialize<ModMetadataDto>(json, _jsonOptions);
                if (dto == null)
                    return;

                if (!string.IsNullOrWhiteSpace(dto.Version))
                    mod.Version = dto.Version;
                if (!string.IsNullOrWhiteSpace(dto.Category))
                    mod.Category = dto.Category;
                if (!string.IsNullOrWhiteSpace(dto.Description))
                    mod.Description = dto.Description;
                if (!string.IsNullOrWhiteSpace(dto.Author))
                    mod.Author = dto.Author;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ModMetadataService] Failed to read sidecar for '{mod.Name}': {ex.Message}");
            }
        }

        public void SaveMetadata(ModInfo mod)
        {
            try
            {
                var dto = new ModMetadataDto(mod.Version, mod.Category, mod.Description, mod.Author);
                string json = JsonSerializer.Serialize(dto, _jsonOptions);
                File.WriteAllText(SidecarPath(mod.Path), json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ModMetadataService] Failed to save sidecar for '{mod.Name}': {ex.Message}");
            }
        }

        private static string SidecarPath(string modFolderPath) =>
            Path.Combine(modFolderPath, FileName);
    }
}
