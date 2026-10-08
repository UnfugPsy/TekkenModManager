using ModManager.Models;

namespace ModManager.Services
{
    public static class ConflictDetector
    {
        public static HashSet<string> FindConflictingModNames(IEnumerable<ModInfo> mods)
        {
            var enabledMods = mods.Where(m => m.IsEnabled).ToList();

            var fileToMods = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            foreach (var mod in enabledMods)
            {
                if (!Directory.Exists(mod.Path))
                    continue;

                var pakNames = Directory.GetFiles(mod.Path, "*.pak", SearchOption.AllDirectories)
                    .Where(f => !f.EndsWith(".pak-x", StringComparison.OrdinalIgnoreCase))
                    .Select(f => Path.GetFileName(f))
                    .Distinct(StringComparer.OrdinalIgnoreCase);

                foreach (var fileName in pakNames)
                {
                    if (!fileToMods.TryGetValue(fileName, out var owners))
                    {
                        owners = new List<string>();
                        fileToMods[fileName] = owners;
                    }
                    owners.Add(mod.Name);
                }
            }

            var conflicting = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in fileToMods.Values.Where(v => v.Count > 1))
                foreach (var name in entry)
                    conflicting.Add(name);

            return conflicting;
        }
    }
}
