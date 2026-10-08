using ModManager.Models;
using ModManager.Services;
using Xunit;

namespace ModManager.Tests.Services;

public class ConflictDetectorTests : IDisposable
{
    private readonly string _modsDir;

    public ConflictDetectorTests()
    {
        _modsDir = Path.Combine(Path.GetTempPath(), $"ConflictTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_modsDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_modsDir))
            Directory.Delete(_modsDir, true);
    }

    private ModInfo MakeEnabledMod(string name, params string[] pakNames)
    {
        string folder = Path.Combine(_modsDir, name);
        Directory.CreateDirectory(folder);
        foreach (var pak in pakNames)
            File.WriteAllText(Path.Combine(folder, pak), "data");
        return new ModInfo(name, folder, enabled: true);
    }

    private ModInfo MakeDisabledMod(string name, params string[] pakNames)
    {
        string folder = Path.Combine(_modsDir, name);
        Directory.CreateDirectory(folder);
        foreach (var pak in pakNames)
            File.WriteAllText(Path.Combine(folder, pak + "-x"), "data");
        return new ModInfo(name, folder, enabled: false);
    }

    [Fact]
    public void NoConflicts_WhenModsHaveDistinctPakNames()
    {
        var mods = new List<ModInfo>
        {
            MakeEnabledMod("mod_a", "a.pak"),
            MakeEnabledMod("mod_b", "b.pak")
        };

        var result = ConflictDetector.FindConflictingModNames(mods);

        Assert.Empty(result);
    }

    [Fact]
    public void DetectsConflict_WhenTwoEnabledModsSharePakName()
    {
        var mods = new List<ModInfo>
        {
            MakeEnabledMod("mod_a", "shared.pak"),
            MakeEnabledMod("mod_b", "shared.pak")
        };

        var result = ConflictDetector.FindConflictingModNames(mods);

        Assert.Contains("mod_a", result);
        Assert.Contains("mod_b", result);
    }

    [Fact]
    public void DisabledMods_AreIgnored_InConflictDetection()
    {
        var mods = new List<ModInfo>
        {
            MakeEnabledMod ("mod_active",   "shared.pak"),
            MakeDisabledMod("mod_disabled", "shared.pak")
        };

        var result = ConflictDetector.FindConflictingModNames(mods);

        Assert.Empty(result);
    }

    [Fact]
    public void EmptyList_ReturnsEmptySet()
    {
        var result = ConflictDetector.FindConflictingModNames(new List<ModInfo>());
        Assert.Empty(result);
    }

    [Fact]
    public void OnlyOneConflictingPak_StillFlagsAllOwners()
    {
        var mods = new List<ModInfo>
        {
            MakeEnabledMod("mod_a", "unique_a.pak", "common.pak"),
            MakeEnabledMod("mod_b", "unique_b.pak", "common.pak"),
            MakeEnabledMod("mod_c", "unique_c.pak")
        };

        var result = ConflictDetector.FindConflictingModNames(mods);

        Assert.Contains("mod_a", result);
        Assert.Contains("mod_b", result);
        Assert.DoesNotContain("mod_c", result);
    }

    [Fact]
    public void NestedPaks_AreComparedToo()
    {
        var nested = MakeEnabledMod("mod_nested");
        string deep = Path.Combine(nested.Path, "Content", "Paks");
        Directory.CreateDirectory(deep);
        File.WriteAllText(Path.Combine(deep, "common.pak"), "data");

        var mods = new List<ModInfo> { nested, MakeEnabledMod("mod_flat", "common.pak") };

        var result = ConflictDetector.FindConflictingModNames(mods);

        Assert.Contains("mod_nested", result);
        Assert.Contains("mod_flat", result);
    }

    [Fact]
    public void ASamePakNameTwiceInsideOneMod_IsNotAConflictWithItself()
    {
        var mod = MakeEnabledMod("mod_a", "common.pak");
        string sub = Path.Combine(mod.Path, "alt");
        Directory.CreateDirectory(sub);
        File.WriteAllText(Path.Combine(sub, "common.pak"), "data");

        var result = ConflictDetector.FindConflictingModNames(new List<ModInfo> { mod });

        Assert.Empty(result);
    }
}
