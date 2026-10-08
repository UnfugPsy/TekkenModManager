using ModManager.Services;
using Xunit;

namespace ModManager.Tests.Services;

public class ModServiceRollbackTests : IDisposable
{
    private readonly string _paks = Path.Combine(Path.GetTempPath(), "mm_rollback_" + Guid.NewGuid().ToString("N"));
    private readonly string _mod;
    private readonly ModService _sut;

    public ModServiceRollbackTests()
    {
        _mod = Path.Combine(_paks, "Mods", "m");
        Directory.CreateDirectory(_mod);
        _sut = new ModService(_paks);
    }

    public void Dispose()
    {
        if (Directory.Exists(_paks))
        {
            Directory.Delete(_paks, true);
        }
    }

    private void Write(string name)
    {
        File.WriteAllText(Path.Combine(_mod, name), "data");
    }

    private bool Has(string name) => File.Exists(Path.Combine(_mod, name));

    [Fact]
    public void Deactivate_LeavesTheModUntouched_WhenOneFileIsLocked()
    {
        Write("a.pak");
        Write("b.ucas");
        Write("z.utoc");

        using (new FileStream(Path.Combine(_mod, "z.utoc"), FileMode.Open, FileAccess.Read, FileShare.None))
        {
            Assert.Throws<InvalidOperationException>(() => _sut.DeactivateMod(_mod));
        }

        Assert.True(Has("a.pak") && Has("b.ucas") && Has("z.utoc"));
        Assert.False(Has("a.pak-x") || Has("b.ucas-x") || Has("z.utoc-x"));
    }

    [Fact]
    public void Activate_LeavesTheModUntouched_WhenOneFileIsLocked()
    {
        Write("a.pak-x");
        Write("b.ucas-x");
        Write("z.utoc-x");

        using (new FileStream(Path.Combine(_mod, "z.utoc-x"), FileMode.Open, FileAccess.Read, FileShare.None))
        {
            Assert.Throws<InvalidOperationException>(() => _sut.ActivateMod(_mod));
        }

        Assert.True(Has("a.pak-x") && Has("b.ucas-x") && Has("z.utoc-x"));
        Assert.False(Has("a.pak") || Has("b.ucas") || Has("z.utoc"));
    }

    [Fact]
    public void Deactivate_StillWorks_WhenNothingIsLocked()
    {
        Write("a.pak");
        Write("z.utoc");

        _sut.DeactivateMod(_mod);

        Assert.True(Has("a.pak-x") && Has("z.utoc-x"));
    }
}
