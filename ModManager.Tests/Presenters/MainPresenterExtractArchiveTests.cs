using System.IO.Compression;
using ModManager.Presenters;
using Xunit;

namespace ModManager.Tests.Presenters;

public class MainPresenterExtractArchiveTests : IDisposable
{
    private readonly string _root;

    public MainPresenterExtractArchiveTests()
    {
        _root = Path.Combine(Path.GetTempPath(), $"ExtractArchiveTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_root);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
            Directory.Delete(_root, true);
    }

    private string CreateZip(string fileName, string entryName)
    {
        string zipPath = Path.Combine(_root, fileName);
        using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
        {
            var entry = zip.CreateEntry(entryName);
            using var writer = new StreamWriter(entry.Open());
            writer.Write("data");
        }
        return zipPath;
    }

    [Fact]
    public void ExtractArchive_CreatesDestination_WhenItDoesNotExist()
    {
        string zipPath = CreateZip("mod.zip", "test.pak");
        string extractPath = Path.Combine(_root, "does_not_exist_yet");

        Assert.False(Directory.Exists(extractPath));

        MainPresenter.ExtractArchive(zipPath, extractPath);

        Assert.True(Directory.Exists(extractPath));
        Assert.True(File.Exists(Path.Combine(extractPath, "test.pak")));
    }

    [Fact]
    public void ExtractArchive_Succeeds_WhenDestinationAlreadyExists()
    {
        string zipPath = CreateZip("mod2.zip", "test.pak");
        string extractPath = Path.Combine(_root, "already_here");
        Directory.CreateDirectory(extractPath);

        MainPresenter.ExtractArchive(zipPath, extractPath);

        Assert.True(File.Exists(Path.Combine(extractPath, "test.pak")));
    }
}
