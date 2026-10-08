using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ModManager.Models;

namespace ModManager.Services
{
    public class ModService : IModService
    {
        private readonly string _paksRoot;

        public ModService(string paksRoot)
        {
            _paksRoot = paksRoot ?? throw new ArgumentNullException(nameof(paksRoot));
        }

        public string GetModsDirectory() => _paksRoot;

        /// <summary>Absolute path of a given mod root under the Paks directory.</summary>
        public string GetRootPath(ModRootKind kind) => Path.Combine(_paksRoot, ModRoots.FolderName(kind));

        public List<ModInfo> GetMods()
        {
            var mods = new List<ModInfo>();
            if (string.IsNullOrEmpty(_paksRoot))
                return mods;

            foreach (ModRootKind kind in Enum.GetValues(typeof(ModRootKind)))
            {
                string rootPath = GetRootPath(kind);
                if (!Directory.Exists(rootPath))
                    continue;

                foreach (var dir in Directory.EnumerateDirectories(rootPath))
                {
                    string folderName = Path.GetFileName(dir);

                    if (ModRoots.IsInstallTempFolder(folderName))
                    {
                        continue;
                    }

                    if (kind == ModRootKind.Logic)
                    {
                        bool disabled = folderName.StartsWith(ModRoots.DisabledPrefix, StringComparison.OrdinalIgnoreCase);
                        string name = disabled ? folderName.Substring(ModRoots.DisabledPrefix.Length) : folderName;
                        mods.Add(new ModInfo(name, dir, !disabled, kind, rootPath));
                    }
                    else
                    {
                        mods.Add(new ModInfo(folderName, dir, IsPakModEnabled(dir), kind, rootPath));
                    }
                }
            }

            return mods;
        }

        private ModRootKind DetermineKind(string modFolderPath)
        {
            string parent = Path.GetFileName(
                Path.GetDirectoryName(modFolderPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)) ?? string.Empty);

            foreach (ModRootKind kind in Enum.GetValues(typeof(ModRootKind)))
            {
                if (string.Equals(parent, ModRoots.FolderName(kind), StringComparison.OrdinalIgnoreCase))
                    return kind;
            }

            return ModRootKind.Standard;
        }

        public bool IsModEnabled(string modFolderPath)
        {
            if (!Directory.Exists(modFolderPath))
                return false;

            if (DetermineKind(modFolderPath) == ModRootKind.Logic)
            {
                string folderName = Path.GetFileName(modFolderPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                return !folderName.StartsWith(ModRoots.DisabledPrefix, StringComparison.OrdinalIgnoreCase);
            }

            return IsPakModEnabled(modFolderPath);
        }

        private static bool IsPakModEnabled(string modFolderPath)
        {
            return Directory.GetFiles(modFolderPath, "*.pak", SearchOption.AllDirectories)
                .Any(f => !f.EndsWith(".pak-x", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Payload extensions that make up a single mod package. When a mod is toggled
        /// every file sharing a base name across these extensions must move together so
        /// no signature (.sig) or configuration (.txt) is ever orphaned from its .pak set.
        /// </summary>
        private static readonly string[] PayloadExtensions = { ".pak", ".ucas", ".utoc", ".sig", ".txt" };

        private const string DisabledFileSuffix = "-x";

        public string ActivateMod(string modFolderPath)
        {
            if (!Directory.Exists(modFolderPath))
                return modFolderPath;

            if (DetermineKind(modFolderPath) == ModRootKind.Logic)
            {
                return EnableLogicMod(modFolderPath);
            }

            try
            {
                foreach (var file in GetDisabledPayloadFiles(modFolderPath))
                {
                    File.Move(file, file.Substring(0, file.Length - DisabledFileSuffix.Length));
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to activate mod at '{modFolderPath}': {ex.Message}", ex);
            }

            return modFolderPath;
        }

        public string DeactivateMod(string modFolderPath)
        {
            if (!Directory.Exists(modFolderPath))
                return modFolderPath;

            if (DetermineKind(modFolderPath) == ModRootKind.Logic)
            {
                return DisableLogicMod(modFolderPath);
            }

            try
            {
                foreach (var file in GetEnabledPayloadFiles(modFolderPath))
                {
                    File.Move(file, file + DisabledFileSuffix);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to deactivate mod at '{modFolderPath}': {ex.Message}", ex);
            }

            return modFolderPath;
        }

        /// <summary>
        /// Enabled payload files (.pak/.ucas/.utoc/.sig/.txt) that do not yet carry the
        /// disabled "-x" suffix. Treated as one atomic set so companions move together.
        /// </summary>
        private static IEnumerable<string> GetEnabledPayloadFiles(string modFolderPath)
        {
            return Directory.EnumerateFiles(modFolderPath, "*", SearchOption.AllDirectories)
                .Where(f => PayloadExtensions.Any(ext =>
                    f.EndsWith(ext, StringComparison.OrdinalIgnoreCase)));
        }

        /// <summary>
        /// Disabled payload files carrying the "-x" suffix (e.g. Mod.pak-x, Mod.sig-x).
        /// </summary>
        private static IEnumerable<string> GetDisabledPayloadFiles(string modFolderPath)
        {
            return Directory.EnumerateFiles(modFolderPath, "*" + DisabledFileSuffix, SearchOption.AllDirectories)
                .Where(f => PayloadExtensions.Any(ext =>
                    f.EndsWith(ext + DisabledFileSuffix, StringComparison.OrdinalIgnoreCase)));
        }

        /// <summary>
        /// Enables a Logic mod by stripping the ".disabled_" prefix from its folder name.
        /// Returns the resulting folder path.
        /// </summary>
        private static string EnableLogicMod(string modFolderPath)
        {
            string trimmed = modFolderPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string parent = Path.GetDirectoryName(trimmed) ?? string.Empty;
            string folderName = Path.GetFileName(trimmed);

            if (!folderName.StartsWith(ModRoots.DisabledPrefix, StringComparison.OrdinalIgnoreCase))
                return trimmed;

            string target = Path.Combine(parent, folderName.Substring(ModRoots.DisabledPrefix.Length));

            try
            {
                if (!Directory.Exists(target))
                    Directory.Move(trimmed, target);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to activate logic mod at '{modFolderPath}': {ex.Message}", ex);
            }

            return target;
        }

        /// <summary>
        /// Disables a Logic mod by prefixing its folder name with ".disabled_".
        /// Returns the resulting folder path.
        /// </summary>
        private static string DisableLogicMod(string modFolderPath)
        {
            string trimmed = modFolderPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string parent = Path.GetDirectoryName(trimmed) ?? string.Empty;
            string folderName = Path.GetFileName(trimmed);

            if (folderName.StartsWith(ModRoots.DisabledPrefix, StringComparison.OrdinalIgnoreCase))
                return trimmed;

            string target = Path.Combine(parent, ModRoots.DisabledPrefix + folderName);

            try
            {
                if (!Directory.Exists(target))
                    Directory.Move(trimmed, target);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to deactivate logic mod at '{modFolderPath}': {ex.Message}", ex);
            }

            return target;
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

        public string RenameMod(string modFolderPath, string newName)
        {
            if (!Directory.Exists(modFolderPath))
                throw new InvalidOperationException($"Mod folder does not exist: '{modFolderPath}'.");

            if (string.IsNullOrWhiteSpace(newName))
                throw new ArgumentException("New name cannot be empty.", nameof(newName));

            string trimmed = newName.Trim();

            if (trimmed.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                throw new ArgumentException("New name contains invalid characters.", nameof(newName));

            string parent = Path.GetDirectoryName(modFolderPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
                ?? throw new InvalidOperationException($"Could not determine parent directory of '{modFolderPath}'.");

            string currentName = Path.GetFileName(modFolderPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

            // Preserve the disabled-state prefix for Logic mods so renaming doesn't re-enable them.
            bool disabledLogic = DetermineKind(modFolderPath) == ModRootKind.Logic
                && currentName.StartsWith(ModRoots.DisabledPrefix, StringComparison.OrdinalIgnoreCase);
            string targetName = disabledLogic ? ModRoots.DisabledPrefix + trimmed : trimmed;

            if (string.Equals(currentName, targetName, StringComparison.Ordinal))
                return modFolderPath;

            string target = Path.Combine(parent, targetName);

            bool caseOnlyChange = string.Equals(currentName, targetName, StringComparison.OrdinalIgnoreCase);
            if (!caseOnlyChange && Directory.Exists(target))
                throw new InvalidOperationException($"A mod named '{trimmed}' already exists.");

            try
            {
                if (caseOnlyChange)
                {
                    string temp = Path.Combine(parent, targetName + "_" + Guid.NewGuid().ToString("N"));
                    Directory.Move(modFolderPath, temp);
                    Directory.Move(temp, target);
                }
                else
                {
                    Directory.Move(modFolderPath, target);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to rename mod to '{trimmed}': {ex.Message}", ex);
            }

            return target;
        }
    }
}
