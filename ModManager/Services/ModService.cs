using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ModManager.Models;

namespace ModManager.Services
{
    public class ModService : IModService
    {
        private readonly string _modsDirectory;

        public ModService(string modsDirectory)
        {
            _modsDirectory = modsDirectory ?? throw new ArgumentNullException(nameof(modsDirectory));
        }

        public string GetModsDirectory() => _modsDirectory;

        public List<ModInfo> GetMods()
        {
            var mods = new List<ModInfo>();
            if (!Directory.Exists(_modsDirectory))
                return mods;
                
            foreach (var dir in Directory.GetDirectories(_modsDirectory))
            {
                string name = Path.GetFileName(dir);
                bool enabled = IsModEnabled(dir);
                mods.Add(new ModInfo(name, dir, enabled));
            }
            return mods;
        }

        public bool IsModEnabled(string modFolderPath)
        {
            if (Directory.Exists(modFolderPath))
            {
                return Directory.GetFiles(modFolderPath, "*.pak", SearchOption.TopDirectoryOnly)
                    .Any(f => !f.EndsWith(".pak-x", StringComparison.OrdinalIgnoreCase));
            }
            return false;
        }

        public void ActivateMod(string modFolderPath)
        {
            if (!Directory.Exists(modFolderPath))
                return;
            try
            {
                foreach (var file in Directory.GetFiles(modFolderPath, "*.pak-x", SearchOption.TopDirectoryOnly))
                    File.Move(file, file.Replace(".pak-x", ".pak"));
                foreach (var file in Directory.GetFiles(modFolderPath, "*.ucas-x", SearchOption.TopDirectoryOnly))
                    File.Move(file, file.Replace(".ucas-x", ".ucas"));
                foreach (var file in Directory.GetFiles(modFolderPath, "*.utoc-x", SearchOption.TopDirectoryOnly))
                    File.Move(file, file.Replace(".utoc-x", ".utoc"));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to activate mod at '{modFolderPath}': {ex.Message}", ex);
            }
        }

        public void DeactivateMod(string modFolderPath)
        {
            if (!Directory.Exists(modFolderPath))
                return;
            try
            {
                foreach (var file in Directory.GetFiles(modFolderPath, "*.pak", SearchOption.TopDirectoryOnly)
                    .Where(f => !f.EndsWith(".pak-x", StringComparison.OrdinalIgnoreCase)))
                    File.Move(file, file + "-x");
                foreach (var file in Directory.GetFiles(modFolderPath, "*.ucas", SearchOption.TopDirectoryOnly)
                    .Where(f => !f.EndsWith(".ucas-x", StringComparison.OrdinalIgnoreCase)))
                    File.Move(file, file + "-x");
                foreach (var file in Directory.GetFiles(modFolderPath, "*.utoc", SearchOption.TopDirectoryOnly)
                    .Where(f => !f.EndsWith(".utoc-x", StringComparison.OrdinalIgnoreCase)))
                    File.Move(file, file + "-x");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to deactivate mod at '{modFolderPath}': {ex.Message}", ex);
            }
        }

        public void DeleteMod(string modFolderPath)
        {
            if (Directory.Exists(modFolderPath))
            {
                try
                {
                    Directory.Delete(modFolderPath, true);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to delete mod at {modFolderPath}: {ex.Message}", ex);
                }
            }
        }
    }
}
