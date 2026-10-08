using ModManager.Models;
using Xunit;

namespace ModManager.Tests.Models;

public class ModelResolveModsDirectoryTests : IDisposable
{
    private readonly string _root;

    public ModelResolveModsDirectoryTests()
    {
        _root = Path.Combine(Path.GetTempPath(), $"ResolveModsDirTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_root);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
            Directory.Delete(_root, true);
    }

    [Fact]
    public void ResolveModsDirectory_ReturnsPaks_WhenPaksSelected()
    {
        string paks = Path.Combine(_root, "Paks");
        Directory.CreateDirectory(paks);

        string result = Model.ResolveModsDirectory(paks);

        Assert.Equal(paks, result);
    }

    [Fact]
    public void ResolveModsDirectory_ReturnsPaks_WhenModsSelected()
    {
        string mods = Path.Combine(_root, "Paks", "Mods");
        Directory.CreateDirectory(mods);

        string result = Model.ResolveModsDirectory(mods);

        Assert.Equal(Path.Combine(_root, "Paks"), result);
    }

    [Fact]
    public void ResolveModsDirectory_ReturnsPaks_WhenLogicModsSelected()
    {
        string logic = Path.Combine(_root, "Paks", "LogicMods");
        Directory.CreateDirectory(logic);

        string result = Model.ResolveModsDirectory(logic);

        Assert.Equal(Path.Combine(_root, "Paks"), result);
    }

    [Fact]
    public void ResolveModsDirectory_IsCaseInsensitive_ForFolderName()
    {
        string paks = Path.Combine(_root, "paks");
        Directory.CreateDirectory(paks);

        string result = Model.ResolveModsDirectory(paks);

        Assert.Equal(paks, result);
    }

    [Fact]
    public void ResolveModsDirectory_FindsPaks_WhenParentSelected()
    {
        string paks = Path.Combine(_root, "Content", "Paks");
        Directory.CreateDirectory(paks);
        string parent = Path.Combine(_root, "Content");

        string result = Model.ResolveModsDirectory(parent);

        Assert.Equal(paks, result);
    }

    [Fact]
    public void ResolveModsDirectory_ReturnsRoot_WhenParentContainsModFolder()
    {
        string mods = Path.Combine(_root, "Paks", "Mods");
        Directory.CreateDirectory(mods);
        string parent = Path.Combine(_root, "Paks");

        string result = Model.ResolveModsDirectory(parent);

        Assert.Equal(parent, result);
    }

    [Fact]
    public void ResolveModsDirectory_TrimsTrailingSeparator()
    {
        string paks = Path.Combine(_root, "Paks");
        Directory.CreateDirectory(paks);

        string result = Model.ResolveModsDirectory(paks + Path.DirectorySeparatorChar);

        Assert.Equal(paks, result);
    }

    [Fact]
    public void ResolveModsDirectory_ReturnsNull_ForUnrelatedFolder()
    {
        string other = Path.Combine(_root, "Desktop");
        Directory.CreateDirectory(other);

        string result = Model.ResolveModsDirectory(other);

        Assert.Null(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ResolveModsDirectory_ReturnsNull_ForEmptyInput(string input)
    {
        string result = Model.ResolveModsDirectory(input);

        Assert.Null(result);
    }

    // --- MigrateLegacyModsPath ---

    [Fact]
    public void MigrateLegacyModsPath_WalksUp_FromModsToPaks()
    {
        string legacy = Path.Combine(_root, "Paks", "Mods");

        string result = Model.MigrateLegacyModsPath(legacy);

        Assert.Equal(Path.Combine(_root, "Paks"), result);
    }

    [Fact]
    public void MigrateLegacyModsPath_LeavesPaksRootUnchanged()
    {
        string paks = Path.Combine(_root, "Paks");

        string result = Model.MigrateLegacyModsPath(paks);

        Assert.Equal(paks, result);
    }

    [Fact]
    public void MigrateLegacyModsPath_TrimsTrailingSeparator()
    {
        string legacy = Path.Combine(_root, "Paks", "Mods") + Path.DirectorySeparatorChar;

        string result = Model.MigrateLegacyModsPath(legacy);

        Assert.Equal(Path.Combine(_root, "Paks"), result);
    }
}
