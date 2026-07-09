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
    public void ResolveModsDirectory_ReturnsModsSubfolder_WhenPaksSelected()
    {
        string paks = Path.Combine(_root, "Paks");
        Directory.CreateDirectory(paks);

        string result = Model.ResolveModsDirectory(paks);

        Assert.Equal(Path.Combine(paks, "Mods"), result);
    }

    [Fact]
    public void ResolveModsDirectory_ReturnsSameFolder_WhenModsSelected()
    {
        string mods = Path.Combine(_root, "Paks", "Mods");
        Directory.CreateDirectory(mods);

        string result = Model.ResolveModsDirectory(mods);

        Assert.Equal(mods, result);
    }

    [Fact]
    public void ResolveModsDirectory_IsCaseInsensitive_ForFolderName()
    {
        string paks = Path.Combine(_root, "paks");
        Directory.CreateDirectory(paks);

        string result = Model.ResolveModsDirectory(paks);

        Assert.Equal(Path.Combine(paks, "Mods"), result);
    }

    [Fact]
    public void ResolveModsDirectory_FindsPaks_WhenParentSelected()
    {
        string paks = Path.Combine(_root, "Content", "Paks");
        Directory.CreateDirectory(paks);
        string parent = Path.Combine(_root, "Content");

        string result = Model.ResolveModsDirectory(parent);

        Assert.Equal(Path.Combine(paks, "Mods"), result);
    }

    [Fact]
    public void ResolveModsDirectory_FindsMods_WhenParentContainsMods()
    {
        string mods = Path.Combine(_root, "Paks", "Mods");
        Directory.CreateDirectory(mods);
        string parent = Path.Combine(_root, "Paks");

        string result = Model.ResolveModsDirectory(parent);

        Assert.Equal(mods, result);
    }

    [Fact]
    public void ResolveModsDirectory_TrimsTrailingSeparator()
    {
        string paks = Path.Combine(_root, "Paks");
        Directory.CreateDirectory(paks);

        string result = Model.ResolveModsDirectory(paks + Path.DirectorySeparatorChar);

        Assert.Equal(Path.Combine(paks, "Mods"), result);
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
}
