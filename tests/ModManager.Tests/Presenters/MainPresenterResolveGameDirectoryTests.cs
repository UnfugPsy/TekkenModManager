using ModManager.Presenters;
using Xunit;

namespace ModManager.Tests.Presenters;

public class MainPresenterResolveGameDirectoryTests
{
    private const string Game = @"D:\SteamLibrary\steamapps\common\TEKKEN 8";

    [Fact]
    public void ResolveGameDirectory_ReturnsGameFolder_ForThePaksFolder()
    {
        Assert.Equal(Game, MainPresenter.ResolveGameDirectory(Game + @"\Polaris\Content\Paks"));
    }

    [Fact]
    public void ResolveGameDirectory_ReturnsGameFolder_ForAModFolderUnderPaks()
    {
        Assert.Equal(Game, MainPresenter.ResolveGameDirectory(Game + @"\Polaris\Content\Paks\Mods"));
    }

    [Fact]
    public void ResolveGameDirectory_IgnoresATrailingSeparatorAndCase()
    {
        Assert.Equal(Game, MainPresenter.ResolveGameDirectory(Game + @"\polaris\Content\Paks\"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(@"D:\Somewhere\Else\Paks")]
    public void ResolveGameDirectory_ReturnsNull_WhenThereIsNoPolarisFolder(string? location)
    {
        Assert.Null(MainPresenter.ResolveGameDirectory(location));
    }
}
