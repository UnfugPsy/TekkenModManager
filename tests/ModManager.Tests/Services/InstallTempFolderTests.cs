using ModManager.Models;
using ModManager.Services;
using Xunit;

namespace ModManager.Tests.Services;

public class InstallTempFolderTests : IDisposable
{
    private readonly string _paks = Path.Combine(Path.GetTempPath(), "mm_tmpfolder_" + Guid.NewGuid().ToString("N"));

    public InstallTempFolderTests()
    {
        Directory.CreateDirectory(Path.Combine(_paks, "Mods"));
        Directory.CreateDirectory(Path.Combine(_paks, "LogicMods"));
    }

    public void Dispose()
    {
        if (Directory.Exists(_paks))
        {
            Directory.Delete(_paks, true);
        }
    }

    [Fact]
    public void ANameFromInstallTempName_IsRecognised()
    {
        Assert.True(ModRoots.IsInstallTempFolder(ModRoots.InstallTempName("Cool Skin")));
    }

    [Theory]
    [InlineData("Cool Skin")]
    [InlineData("_hidden_mod")]
    [InlineData("my_tmp_mod")]
    [InlineData("_mod_tmp_notahexstring")]
    [InlineData("mod_tmp_0123456789abcdef0123456789abcdef")]
    [InlineData("")]
    public void OrdinaryFolderNames_AreNotInstallTempFolders(string name)
    {
        Assert.False(ModRoots.IsInstallTempFolder(name));
    }

    [Fact]
    public void GetMods_SkipsTheScratchFolderOfARunningInstall()
    {
        string real = Path.Combine(_paks, "Mods", "RealMod");
        Directory.CreateDirectory(real);
        File.WriteAllText(Path.Combine(real, "a.pak"), "data");
        Directory.CreateDirectory(Path.Combine(_paks, "Mods", ModRoots.InstallTempName("Half")));
        Directory.CreateDirectory(Path.Combine(_paks, "LogicMods", ModRoots.InstallTempName("HalfLogic")));

        var mods = new ModService(_paks).GetMods();

        Assert.Single(mods);
        Assert.Equal("RealMod", mods[0].Name);
    }
}
