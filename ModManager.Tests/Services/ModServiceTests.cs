using ModManager.Models;
using ModManager.Services;
using Xunit;

namespace ModManager.Tests.Services;

public class ModServiceTests : IDisposable
{
    private readonly string _paksRoot;
    private readonly string _modsDir;
    private readonly ModService _sut;

    public ModServiceTests()
    {
        _paksRoot = Path.Combine(Path.GetTempPath(), $"ModServiceTests_{Guid.NewGuid():N}");
        _modsDir = Path.Combine(_paksRoot, "Mods");
        Directory.CreateDirectory(_modsDir);
        _sut = new ModService(_paksRoot);
    }

    public void Dispose()
    {
        if (Directory.Exists(_paksRoot))
            Directory.Delete(_paksRoot, true);
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

    // --- Nested subfolder layouts (e.g. Content/Paks) ---

    private string CreateNestedModFolder(string name, bool enabled)
    {
        string folder = Path.Combine(_modsDir, name);
        string nested = Path.Combine(folder, "Content", "Paks");
        Directory.CreateDirectory(nested);
        string ext = enabled ? ".pak" : ".pak-x";
        File.WriteAllText(Path.Combine(nested, $"test{ext}"), "data");
        return folder;
    }

    [Fact]
    public void IsModEnabled_ReturnsTrue_ForEnabledFileInNestedSubfolder()
    {
        string folder = CreateNestedModFolder("nested_enabled", enabled: true);
        Assert.True(_sut.IsModEnabled(folder));
    }

    [Fact]
    public void IsModEnabled_ReturnsFalse_ForDisabledFileInNestedSubfolder()
    {
        string folder = CreateNestedModFolder("nested_disabled", enabled: false);
        Assert.False(_sut.IsModEnabled(folder));
    }

    [Fact]
    public void ActivateMod_RenamesFilesInNestedSubfolder()
    {
        string folder = CreateNestedModFolder("nested_mod", enabled: false);
        Assert.False(_sut.IsModEnabled(folder));

        _sut.ActivateMod(folder);

        Assert.True(_sut.IsModEnabled(folder));
        Assert.Empty(Directory.GetFiles(folder, "*.pak-x", SearchOption.AllDirectories));
    }

    [Fact]
    public void DeactivateMod_RenamesFilesInNestedSubfolder()
    {
        string folder = CreateNestedModFolder("nested_mod", enabled: true);
        Assert.True(_sut.IsModEnabled(folder));

        _sut.DeactivateMod(folder);

        Assert.False(_sut.IsModEnabled(folder));
        Assert.NotEmpty(Directory.GetFiles(folder, "*.pak-x", SearchOption.AllDirectories));
    }

    [Fact]
    public void ActivateThenDeactivate_TogglesNestedMod_BothWays()
    {
        string folder = CreateNestedModFolder("nested_toggle", enabled: false);

        _sut.ActivateMod(folder);
        Assert.True(_sut.IsModEnabled(folder));

        _sut.DeactivateMod(folder);
        Assert.False(_sut.IsModEnabled(folder));
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

    // --- RenameMod ---

    [Fact]
    public void RenameMod_MovesFolderToNewName()
    {
        string folder = CreateModFolder("old_name", enabled: true);

        string newPath = _sut.RenameMod(folder, "new_name");

        Assert.False(Directory.Exists(folder));
        Assert.True(Directory.Exists(newPath));
        Assert.Equal("new_name", Path.GetFileName(newPath));
    }

    [Fact]
    public void RenameMod_PreservesFilesAndEnabledState()
    {
        string folder = CreateModFolder("mod", enabled: true);

        string newPath = _sut.RenameMod(folder, "renamed");

        Assert.True(_sut.IsModEnabled(newPath));
        Assert.NotEmpty(Directory.GetFiles(newPath, "*.pak"));
    }

    [Fact]
    public void RenameMod_KeepsMetadataSidecar()
    {
        string folder = CreateModFolder("mod", enabled: false);
        File.WriteAllText(Path.Combine(folder, "modinfo.json"), "{}");

        string newPath = _sut.RenameMod(folder, "renamed");

        Assert.True(File.Exists(Path.Combine(newPath, "modinfo.json")));
    }

    [Fact]
    public void RenameMod_ReturnsSamePath_WhenNameUnchanged()
    {
        string folder = CreateModFolder("same", enabled: true);

        string result = _sut.RenameMod(folder, "same");

        Assert.Equal(folder, result);
        Assert.True(Directory.Exists(folder));
    }

    [Fact]
    public void RenameMod_TrimsWhitespace()
    {
        string folder = CreateModFolder("trim_me", enabled: false);

        string newPath = _sut.RenameMod(folder, "  clean_name  ");

        Assert.Equal("clean_name", Path.GetFileName(newPath));
        Assert.True(Directory.Exists(newPath));
    }

    [Fact]
    public void RenameMod_Throws_WhenTargetNameAlreadyExists()
    {
        string folder = CreateModFolder("mod_a", enabled: true);
        CreateModFolder("mod_b", enabled: true);

        Assert.Throws<InvalidOperationException>(() => _sut.RenameMod(folder, "mod_b"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void RenameMod_Throws_ForBlankName(string newName)
    {
        string folder = CreateModFolder("mod", enabled: true);

        Assert.Throws<ArgumentException>(() => _sut.RenameMod(folder, newName));
    }

    [Fact]
    public void RenameMod_Throws_ForInvalidCharacters()
    {
        string folder = CreateModFolder("mod", enabled: true);

        Assert.Throws<ArgumentException>(() => _sut.RenameMod(folder, "bad/name"));
    }

    [Fact]
    public void RenameMod_Throws_ForNonExistentFolder()
    {
        Assert.Throws<InvalidOperationException>(
            () => _sut.RenameMod(Path.Combine(_modsDir, "ghost"), "whatever"));
    }

    [Fact]
    public void RenameMod_AllowsCaseOnlyChange()
    {
        string folder = CreateModFolder("casemod", enabled: true);

        string newPath = _sut.RenameMod(folder, "CaseMod");

        Assert.Equal("CaseMod", Path.GetFileName(newPath));
        Assert.True(Directory.Exists(newPath));
        Assert.True(_sut.IsModEnabled(newPath));
    }

    // --- Multi-root scanning ---

    private string CreateRootModFolder(ModRootKind kind, string name, bool enabled)
    {
        string root = Path.Combine(_paksRoot, ModRoots.FolderName(kind));
        Directory.CreateDirectory(root);

        if (kind == ModRootKind.Logic)
        {
            string logicFolder = Path.Combine(root, enabled ? name : ModRoots.DisabledPrefix + name);
            Directory.CreateDirectory(logicFolder);
            File.WriteAllText(Path.Combine(logicFolder, "main.lua"), "print('x')");
            return logicFolder;
        }

        string folder = Path.Combine(root, name);
        Directory.CreateDirectory(folder);
        string ext = enabled ? ".pak" : ".pak-x";
        File.WriteAllText(Path.Combine(folder, $"test{ext}"), "data");
        return folder;
    }

    [Fact]
    public void GetMods_AggregatesAllThreeRoots()
    {
        CreateRootModFolder(ModRootKind.Standard, "std_mod", enabled: true);
        CreateRootModFolder(ModRootKind.Legacy, "legacy_mod", enabled: true);
        CreateRootModFolder(ModRootKind.Logic, "logic_mod", enabled: true);

        var mods = _sut.GetMods();

        Assert.Contains(mods, m => m.Name == "std_mod" && m.RootKind == ModRootKind.Standard);
        Assert.Contains(mods, m => m.Name == "legacy_mod" && m.RootKind == ModRootKind.Legacy);
        Assert.Contains(mods, m => m.Name == "logic_mod" && m.RootKind == ModRootKind.Logic);
    }

    [Fact]
    public void GetMods_TagsRootPath()
    {
        CreateRootModFolder(ModRootKind.Legacy, "legacy_mod", enabled: true);

        var mod = _sut.GetMods().Single(m => m.Name == "legacy_mod");

        Assert.Equal(Path.Combine(_paksRoot, "~mods"), mod.RootPath);
    }

    // --- Logic mod folder-level toggling ---

    [Fact]
    public void GetMods_ReadsLogicMod_DisabledState_FromFolderPrefix()
    {
        CreateRootModFolder(ModRootKind.Logic, "enabled_logic", enabled: true);
        CreateRootModFolder(ModRootKind.Logic, "disabled_logic", enabled: false);

        var mods = _sut.GetMods();

        Assert.True(mods.Single(m => m.Name == "enabled_logic").IsEnabled);
        Assert.False(mods.Single(m => m.Name == "disabled_logic").IsEnabled);
    }

    [Fact]
    public void DeactivateMod_RenamesLogicFolder_WithDisabledPrefix()
    {
        string folder = CreateRootModFolder(ModRootKind.Logic, "logic_mod", enabled: true);

        _sut.DeactivateMod(folder);

        string disabledPath = Path.Combine(Path.GetDirectoryName(folder)!, ModRoots.DisabledPrefix + "logic_mod");
        Assert.False(Directory.Exists(folder));
        Assert.True(Directory.Exists(disabledPath));
        Assert.False(_sut.IsModEnabled(disabledPath));
    }

    [Fact]
    public void ActivateMod_StripsDisabledPrefix_FromLogicFolder()
    {
        string folder = CreateRootModFolder(ModRootKind.Logic, "logic_mod", enabled: false);

        _sut.ActivateMod(folder);

        string enabledPath = Path.Combine(Path.GetDirectoryName(folder)!, "logic_mod");
        Assert.True(Directory.Exists(enabledPath));
        Assert.True(_sut.IsModEnabled(enabledPath));
    }

    [Fact]
    public void LogicMod_DoesNotUsePakRename()
    {
        string folder = CreateRootModFolder(ModRootKind.Logic, "logic_mod", enabled: true);

        _sut.DeactivateMod(folder);

        string disabledPath = Path.Combine(Path.GetDirectoryName(folder)!, ModRoots.DisabledPrefix + "logic_mod");
        Assert.True(File.Exists(Path.Combine(disabledPath, "main.lua")));
    }

    // --- Companion-file atomicity (.sig/.txt travel with .pak/.ucas/.utoc) ---

    private static string CreatePayloadMod(string folder)
    {
        Directory.CreateDirectory(folder);
        File.WriteAllText(Path.Combine(folder, "Mod.pak"), "pak");
        File.WriteAllText(Path.Combine(folder, "Mod.ucas"), "ucas");
        File.WriteAllText(Path.Combine(folder, "Mod.utoc"), "utoc");
        File.WriteAllText(Path.Combine(folder, "Mod.sig"), "sig");
        File.WriteAllText(Path.Combine(folder, "Mod.txt"), "readme");
        return folder;
    }

    [Fact]
    public void DeactivateMod_SuffixesAllCompanions_AsAtomicBlock()
    {
        string folder = CreatePayloadMod(Path.Combine(_modsDir, "companions"));

        _sut.DeactivateMod(folder);

        Assert.True(File.Exists(Path.Combine(folder, "Mod.pak-x")));
        Assert.True(File.Exists(Path.Combine(folder, "Mod.ucas-x")));
        Assert.True(File.Exists(Path.Combine(folder, "Mod.utoc-x")));
        Assert.True(File.Exists(Path.Combine(folder, "Mod.sig-x")));
        Assert.True(File.Exists(Path.Combine(folder, "Mod.txt-x")));
        // No enabled remnants left behind.
        Assert.Empty(Directory.GetFiles(folder).Where(f => !f.EndsWith("-x", StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public void ActivateMod_RestoresAllCompanions_FromDisabledBlock()
    {
        string folder = CreatePayloadMod(Path.Combine(_modsDir, "companions"));
        _sut.DeactivateMod(folder);

        _sut.ActivateMod(folder);

        Assert.True(File.Exists(Path.Combine(folder, "Mod.pak")));
        Assert.True(File.Exists(Path.Combine(folder, "Mod.ucas")));
        Assert.True(File.Exists(Path.Combine(folder, "Mod.utoc")));
        Assert.True(File.Exists(Path.Combine(folder, "Mod.sig")));
        Assert.True(File.Exists(Path.Combine(folder, "Mod.txt")));
        Assert.Empty(Directory.GetFiles(folder, "*-x"));
    }

    // --- Toggle lifecycle: return path reflects on-disk mutation ---

    [Fact]
    public void DeactivateMod_Logic_ReturnsRenamedDisabledPath()
    {
        string folder = CreateRootModFolder(ModRootKind.Logic, "logic_mod", enabled: true);

        string resulting = _sut.DeactivateMod(folder);

        string expected = Path.Combine(Path.GetDirectoryName(folder)!, ModRoots.DisabledPrefix + "logic_mod");
        Assert.Equal(expected, resulting);
        Assert.True(Directory.Exists(resulting));
    }

    [Fact]
    public void ActivateMod_Logic_ReturnsEnabledPath()
    {
        string folder = CreateRootModFolder(ModRootKind.Logic, "logic_mod", enabled: false);

        string resulting = _sut.ActivateMod(folder);

        string expected = Path.Combine(Path.GetDirectoryName(folder)!, "logic_mod");
        Assert.Equal(expected, resulting);
        Assert.True(Directory.Exists(resulting));
    }

    [Fact]
    public void ToggleLifecycle_ReturnedPath_RemainsUsable_ForFollowUpOps()
    {
        string folder = CreateRootModFolder(ModRootKind.Logic, "logic_mod", enabled: true);

        // Simulate the presenter capturing the new path after a toggle, then deleting it.
        string disabledPath = _sut.DeactivateMod(folder);
        Assert.False(Directory.Exists(folder));

        _sut.DeleteMod(disabledPath);
        Assert.False(Directory.Exists(disabledPath));
    }

    [Fact]
    public void PakMod_ActivateDeactivate_ReturnsSameFolderPath()
    {
        string folder = CreateModFolder("pakmod", enabled: true);

        Assert.Equal(folder, _sut.DeactivateMod(folder));
        Assert.Equal(folder, _sut.ActivateMod(folder));
    }

    // --- Missing directory graceful degradation ---

    [Fact]
    public void GetMods_DoesNotThrow_WhenRootFoldersMissing()
    {
        // Fresh Paks root with no ~mods or LogicMods folders at all.
        string freshRoot = Path.Combine(_paksRoot, "fresh");
        Directory.CreateDirectory(freshRoot);
        var service = new ModService(freshRoot);

        var mods = service.GetMods();

        Assert.Empty(mods);
    }

    [Fact]
    public void ActivateMod_ReturnsPath_WhenFolderMissing()
    {
        string missing = Path.Combine(_modsDir, "does_not_exist");

        Assert.Equal(missing, _sut.ActivateMod(missing));
    }
}
