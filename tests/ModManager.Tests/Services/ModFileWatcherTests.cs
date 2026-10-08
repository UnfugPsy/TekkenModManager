using ModManager.Services;
using Xunit;

namespace ModManager.Tests.Services;

public class ModFileWatcherTests : IDisposable
{
    private readonly string _paksRoot;

    public ModFileWatcherTests()
    {
        _paksRoot = Path.Combine(Path.GetTempPath(), $"ModFileWatcherTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_paksRoot);
    }

    public void Dispose()
    {
        if (Directory.Exists(_paksRoot))
            Directory.Delete(_paksRoot, true);
    }

    [Fact]
    public void Constructor_Throws_WhenPaksRootMissing()
    {
        string missing = Path.Combine(_paksRoot, "nope");
        Assert.Throws<DirectoryNotFoundException>(() => new ModFileWatcher(missing));
    }

    [Fact]
    public void Constructor_Throws_WhenPathEmpty()
    {
        Assert.Throws<ArgumentException>(() => new ModFileWatcher(" "));
    }

    [Fact]
    public async Task Changed_FiresOnce_ForBurstOfExternalChanges()
    {
        Directory.CreateDirectory(Path.Combine(_paksRoot, "Mods"));
        using var watcher = new ModFileWatcher(_paksRoot, debounceMilliseconds: 150);

        int fireCount = 0;
        watcher.Changed += (_, _) => Interlocked.Increment(ref fireCount);
        watcher.Start();

        // Simulate a burst of Explorer activity across the roots.
        for (int i = 0; i < 10; i++)
        {
            File.WriteAllText(Path.Combine(_paksRoot, "Mods", $"file_{i}.pak"), "x");
        }

        // Wait past the debounce quiet window plus margin.
        await Task.Delay(600);

        Assert.Equal(1, fireCount);
    }

    [Fact]
    public async Task Changed_DoesNotFire_AfterDispose()
    {
        using var mods = new DirectoryScope(Path.Combine(_paksRoot, "LogicMods"));
        var watcher = new ModFileWatcher(_paksRoot, debounceMilliseconds: 100);

        int fireCount = 0;
        watcher.Changed += (_, _) => Interlocked.Increment(ref fireCount);
        watcher.Start();
        watcher.Dispose();

        File.WriteAllText(Path.Combine(mods.Path, "late.txt"), "x");
        await Task.Delay(400);

        Assert.Equal(0, fireCount);
    }

    [Fact]
    public async Task Changed_DoesNotFire_ForChangesInsideSuppressionScope()
    {
        Directory.CreateDirectory(Path.Combine(_paksRoot, "Mods"));
        using var watcher = new ModFileWatcher(_paksRoot, debounceMilliseconds: 100);

        int fireCount = 0;
        watcher.Changed += (_, _) => Interlocked.Increment(ref fireCount);
        watcher.Start();

        using (watcher.SuppressNotifications())
        {
            for (int i = 0; i < 5; i++)
            {
                File.WriteAllText(Path.Combine(_paksRoot, "Mods", $"self_{i}.pak"), "x");
            }

            await Task.Delay(300);
        }

        Assert.Equal(0, fireCount);
    }

    [Fact]
    public async Task Changed_ResumesFiring_AfterSuppressionScopeDisposed()
    {
        Directory.CreateDirectory(Path.Combine(_paksRoot, "Mods"));
        using var watcher = new ModFileWatcher(_paksRoot, debounceMilliseconds: 100);

        int fireCount = 0;
        watcher.Changed += (_, _) => Interlocked.Increment(ref fireCount);
        watcher.Start();

        using (watcher.SuppressNotifications())
        {
            File.WriteAllText(Path.Combine(_paksRoot, "Mods", "internal.pak"), "x");
        }

        // External change after suppression ends must still be observed.
        File.WriteAllText(Path.Combine(_paksRoot, "Mods", "external.pak"), "x");
        await Task.Delay(400);

        Assert.Equal(1, fireCount);
    }

    [Fact]
    public void SuppressNotifications_IsReentrant_AndBalances()
    {
        Directory.CreateDirectory(Path.Combine(_paksRoot, "Mods"));
        using var watcher = new ModFileWatcher(_paksRoot, debounceMilliseconds: 50);

        // Nested scopes must not throw and must dispose cleanly in any order.
        var outer = watcher.SuppressNotifications();
        var inner = watcher.SuppressNotifications();
        inner.Dispose();
        inner.Dispose(); // double-dispose is a no-op
        outer.Dispose();
    }

    private sealed class DirectoryScope : IDisposable
    {
        public string Path { get; }

        public DirectoryScope(string path)
        {
            Path = path;
            Directory.CreateDirectory(path);
        }

        public void Dispose()
        {
        }
    }
}
