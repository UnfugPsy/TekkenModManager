using ModManager.Models;
using Xunit;

namespace ModManager.Tests.Models;

public class ModInfoSizeTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "mm_size_" + Guid.NewGuid().ToString("N"));

    public ModInfoSizeTests()
    {
        Directory.CreateDirectory(_dir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_dir))
        {
            Directory.Delete(_dir, true);
        }
    }

    [Fact]
    public void Size_IsMeasuredOnFirstRead_NotWhenTheModInfoIsBuilt()
    {
        File.WriteAllBytes(Path.Combine(_dir, "a.pak"), new byte[100]);
        var mod = new ModInfo("m", _dir, true);

        File.WriteAllBytes(Path.Combine(_dir, "b.pak"), new byte[50]);

        Assert.Equal(150, mod.Size);
    }

    [Fact]
    public void Size_IsMeasuredOnlyOnce()
    {
        File.WriteAllBytes(Path.Combine(_dir, "a.pak"), new byte[100]);
        var mod = new ModInfo("m", _dir, true);
        Assert.Equal(100, mod.Size);

        File.WriteAllBytes(Path.Combine(_dir, "b.pak"), new byte[50]);

        Assert.Equal(100, mod.Size);
    }

    [Fact]
    public void Size_IsZeroAndFormatsAsUnknown_WhenTheFolderIsGone()
    {
        var mod = new ModInfo("m", Path.Combine(_dir, "missing"), true);

        Assert.Equal(0, mod.Size);
        Assert.Equal("Unknown", mod.FormattedSize);
    }

    [Fact]
    public void Size_CanBeSetExplicitly()
    {
        var mod = new ModInfo("m", _dir, true) { Size = 2048 };

        Assert.Equal("2 KB", mod.FormattedSize);
    }
}
