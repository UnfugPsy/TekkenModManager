using ModManager.Models;
using Xunit;

namespace ModManager.Tests.Models;

public class ModProfileTests
{
    [Fact]
    public void Constructor_SetsName_AndGeneratesUniqueId()
    {
        var p1 = new ModProfile("Alpha");
        var p2 = new ModProfile("Beta");

        Assert.Equal("Alpha", p1.Name);
        Assert.NotEqual(p1.Id, p2.Id);
    }

    [Fact]
    public void Constructor_WithEnabledMods_StoresDefensiveCopy()
    {
        var mods = new List<string> { "mod_a", "mod_b" };
        var profile = new ModProfile("Test", "desc", mods);

        mods.Add("mod_c");

        Assert.Equal(2, profile.EnabledMods.Count);
    }

    [Fact]
    public void Clone_CreatesIndependentCopy_WithNewId()
    {
        var original = new ModProfile("Original", "desc", new List<string> { "mod_a" });
        var clone = original.Clone();

        Assert.NotEqual(original.Id, clone.Id);
        Assert.False(clone.IsDefault);

        clone.EnabledMods.Add("mod_b");
        Assert.Single(original.EnabledMods);
    }

    [Fact]
    public void Clone_AppendsCopySuffix_ToName()
    {
        var original = new ModProfile("MyProfile");
        var clone = original.Clone();

        Assert.Contains("Copy", clone.Name);
    }

    [Fact]
    public void ToString_ReturnsName()
    {
        var profile = new ModProfile("ProfileName");
        Assert.Equal("ProfileName", profile.ToString());
    }

    [Fact]
    public void NewProfile_IsNotDefault_ByDefault()
    {
        var profile = new ModProfile("Test");
        Assert.False(profile.IsDefault);
    }
}
