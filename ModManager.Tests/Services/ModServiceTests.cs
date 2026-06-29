using ModManager.Services;
using Xunit;

namespace ModManager.Tests.Services;

public class ModServiceTests : IDisposable
{
    private readonly string _modsDir;
    private readonly ModService _sut;

    public ModServiceTests()
    {
        _modsDir = Path.Combine(Path.GetTempPath(), $"ModServiceTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_modsDir);
        _sut = new ModService(_modsDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_modsDir))
            Directory.Delete(_modsDir, true);
    }

    private string CreateModFolder(string name, bool enabled)
    {
        string folder = Path.Combine(_modsDir, name);
        Directory.CreateDirectory(folder);
        string ext = enabled ? ".pak" : ".pak-x";
        File.WriteAllText(Path.Combine(folder, $"test{ext}"), "data");
        return folder;
    }

    // --- GetMods ---

    [Fact]
    public void GetMods_ReturnsEmptyList_WhenDirectoryIsEmpty()
    {
        var mods = _sut.GetMods();
        Assert.Empty(mods);
    }

    [Fact]
    public void GetMods_ReturnsOneMod_PerSubdirectory()
    {
        CreateModFolder("mod_a", enabled: true);
        CreateModFolder("mod_b", enabled: false);

        var mods = _sut.GetMods();

        Assert.Equal(2, mods.Count);
    }

    [Fact]
    public void GetMods_ReportsEnabled_Correctly()
    {
        CreateModFolder("enabled_mod", enabled: true);
        CreateModFolder("disabled_mod", enabled: false);

        var mods = _sut.GetMods();
        var enabled = mods.Single(m => m.Name == "enabled_mod");
        var disabled = mods.Single(m => m.Name == "disabled_mod");

        Assert.True(enabled.IsEnabled);
        Assert.False(disabled.IsEnabled);
    }

    // --- IsModEnabled ---

    [Fact]
    public void IsModEnabled_ReturnsFalse_ForNonExistentPath()
    {
        Assert.False(_sut.IsModEnabled(Path.Combine(_modsDir, "nonexistent")));
    }

    [Fact]
    public void IsModEnabled_ReturnsFalse_WhenOnlyDisabledFilesPresent()
    {
        string folder = CreateModFolder("mod", enabled: false);
        Assert.False(_sut.IsModEnabled(folder));
    }

    // --- ActivateMod / DeactivateMod round-trip ---

    [Fact]
    public void ActivateMod_RenamesDisabledFiles_ToEnabled()
    {
        string folder = CreateModFolder("mod", enabled: false);
        Assert.False(_sut.IsModEnabled(folder));

        _sut.ActivateMod(folder);

        Assert.True(_sut.IsModEnabled(folder));
        Assert.Empty(Directory.GetFiles(folder, "*.pak-x"));
    }

    [Fact]
    public void DeactivateMod_RenamesEnabledFiles_ToDisabled()
    {
        string folder = CreateModFolder("mod", enabled: true);
        Assert.True(_sut.IsModEnabled(folder));

        _sut.DeactivateMod(folder);

        Assert.False(_sut.IsModEnabled(folder));
        Assert.NotEmpty(Directory.GetFiles(folder, "*.pak-x"));
    }

    [Fact]
    public void ActivateMod_ThenDeactivateMod_TogglesBothWays()
    {
        string folder = CreateModFolder("mod", enabled: false);

        _sut.ActivateMod(folder);
        Assert.True(_sut.IsModEnabled(folder));

        _sut.DeactivateMod(folder);
        Assert.False(_sut.IsModEnabled(folder));
    }

    [Fact]
    public void ActivateMod_HandlesUcasAndUtoc()
    {
        string folder = Path.Combine(_modsDir, "multi_ext");
        Directory.CreateDirectory(folder);
        File.WriteAllText(Path.Combine(folder, "test.pak-x"), "data");
        File.WriteAllText(Path.Combine(folder, "test.ucas-x"), "data");
        File.WriteAllText(Path.Combine(folder, "test.utoc-x"), "data");

        _sut.ActivateMod(folder);

        Assert.NotEmpty(Directory.GetFiles(folder, "*.pak"));
        Assert.NotEmpty(Directory.GetFiles(folder, "*.ucas"));
        Assert.NotEmpty(Directory.GetFiles(folder, "*.utoc"));
        Assert.Empty(Directory.GetFiles(folder, "*.pak-x"));
        Assert.Empty(Directory.GetFiles(folder, "*.ucas-x"));
        Assert.Empty(Directory.GetFiles(folder, "*.utoc-x"));
    }

    // --- DeleteMod ---

    [Fact]
    public void DeleteMod_RemovesFolderFromDisk()
    {
        string folder = CreateModFolder("to_delete", enabled: true);
        Assert.True(Directory.Exists(folder));

        _sut.DeleteMod(folder);

        Assert.False(Directory.Exists(folder));
    }

    [Fact]
    public void DeleteMod_IsNoOp_ForNonExistentPath()
    {
        var ex = Record.Exception(() => _sut.DeleteMod(Path.Combine(_modsDir, "ghost")));
        Assert.Null(ex);
    }
}
