using System;
using System.Collections.Generic;

namespace ModManager.Models
{
    /// <summary>
    /// The UE5 / UE4SS mod folders that live side by side under the game's Paks directory.
    /// </summary>
    public enum ModRootKind
    {
        /// <summary>Standard pak-based mods (...\Paks\Mods).</summary>
        Standard,

        /// <summary>Legacy cosmetic pak mods (...\Paks\~mods).</summary>
        Legacy,

        /// <summary>Blueprint / scripting mods, typically UE4SS Lua/DLL (...\Paks\LogicMods).</summary>
        Logic
    }

    /// <summary>
    /// Central mapping between <see cref="ModRootKind"/> values and the folder names on disk.
    /// </summary>
    public static class ModRoots
    {
        /// <summary>Prefix used to mark a Logic mod's folder as disabled.</summary>
        public const string DisabledPrefix = ".disabled_";

        public static readonly IReadOnlyDictionary<ModRootKind, string> FolderNames =
            new Dictionary<ModRootKind, string>
            {
                [ModRootKind.Standard] = "Mods",
                [ModRootKind.Legacy]   = "~mods",
                [ModRootKind.Logic]    = "LogicMods",
            };

        /// <summary>Short, user-facing label for a root kind (shown in the grid's Type column).</summary>
        public static string DisplayName(ModRootKind kind) => kind switch
        {
            ModRootKind.Standard => "Mods",
            ModRootKind.Legacy   => "~mods",
            ModRootKind.Logic    => "LogicMods",
            _ => kind.ToString()
        };

        /// <summary>Resolves the on-disk folder name for a root kind.</summary>
        public static string FolderName(ModRootKind kind) =>
            FolderNames.TryGetValue(kind, out var name) ? name : kind.ToString();
    }
}
