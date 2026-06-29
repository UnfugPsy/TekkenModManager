using ModManager.Models;
using ModManager.Services;
using Xunit;

namespace ModManager.Tests.Services;

public class ModMetadataServiceTests : IDisposable
{
    private readonly string _modsDir;
    private readonly ModMetadataService _sut;

    public ModMetadataServiceTests()
    {
        _modsDir = Path.Combine(Path.GetTempPath(), $"ModMetaTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_modsDir);
        _sut = new ModMetadataService();
    }

    public void Dispose()
    {
        if (Directory.Exists(_modsDir))
            Directory.Delete(_modsDir, true);
    }

    private ModInfo MakeMod(string name) =>
        new ModInfo(name, Path.Combine(_modsDir, name), enabled: false);

    private void EnsureFolder(ModInfo mod) =>
        Directory.CreateDirectory(mod.Path);

    [Fact]
    public void EnrichMod_DoesNotThrow_WhenNoSidecarExists()
    {
        var mod = MakeMod("no_sidecar");
        EnsureFolder(mod);
        _sut.EnrichMod(mod);
        Assert.Equal("1.0", mod.Version);
    }

    [Fact]
    public void SaveThenEnrich_RoundTripsAllFields()
    {
        var mod = MakeMod("roundtrip");
        EnsureFolder(mod);
        mod.Version     = "2.5";
        mod.Category    = "Character";
        mod.Author      = "TestDev";
        mod.Description = "A test mod";

        _sut.SaveMetadata(mod);

        var mod2 = MakeMod("roundtrip");
        _sut.EnrichMod(mod2);

        Assert.Equal("2.5",        mod2.Version);
        Assert.Equal("Character",  mod2.Category);
        Assert.Equal("TestDev",    mod2.Author);
        Assert.Equal("A test mod", mod2.Description);
    }

    [Fact]
    public void EnrichMod_DoesNotOverwrite_WhenSidecarFieldIsEmpty()
    {
        var mod = MakeMod("partial");
        EnsureFolder(mod);
        mod.Category = "Skin";
        mod.Version  = "";
        _sut.SaveMetadata(mod);

        var mod2 = MakeMod("partial");
        mod2.Version = "1.0";
        _sut.EnrichMod(mod2);

        Assert.Equal("1.0",  mod2.Version);
        Assert.Equal("Skin", mod2.Category);
    }

    [Fact]
    public void SaveMetadata_WritesSidecarFile()
    {
        var mod = MakeMod("sidecar_written");
        EnsureFolder(mod);
        _sut.SaveMetadata(mod);

        string sidecar = Path.Combine(mod.Path, "modinfo.json");
        Assert.True(File.Exists(sidecar));
    }

    [Fact]
    public void EnrichMod_DoesNotThrow_WhenSidecarIsCorrupt()
    {
        var mod = MakeMod("corrupt");
        EnsureFolder(mod);
        File.WriteAllText(Path.Combine(mod.Path, "modinfo.json"), "not valid json {{");

        var ex = Record.Exception(() => _sut.EnrichMod(mod));
        Assert.Null(ex);
    }
}
