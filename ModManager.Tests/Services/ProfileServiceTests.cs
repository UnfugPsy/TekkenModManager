using ModManager.Models;
using ModManager.Services;
using Xunit;

namespace ModManager.Tests.Services;

public class ProfileServiceTests : IDisposable
{
    private readonly string _tempFile;
    private ProfileService NewSut() => new ProfileService(_tempFile);

    public ProfileServiceTests()
    {
        _tempFile = Path.Combine(Path.GetTempPath(), $"profiles_{Guid.NewGuid():N}.json");
    }

    public void Dispose()
    {
        if (File.Exists(_tempFile)) File.Delete(_tempFile);
        string tmp = _tempFile + ".tmp";
        if (File.Exists(tmp)) File.Delete(tmp);
    }

    // --- Initialization ---

    [Fact]
    public void NewService_CreatesDefaultProfile_WhenFileAbsent()
    {
        var sut = NewSut();
        var profiles = sut.GetProfiles();

        Assert.NotEmpty(profiles);
        Assert.True(profiles.Any(p => p.IsDefault));
    }

    [Fact]
    public void NewService_PersistsToFile_OnFirstInit()
    {
        _ = NewSut();
        Assert.True(File.Exists(_tempFile));
    }

    [Fact]
    public void NewService_LoadsPersistedProfiles_OnSecondInit()
    {
        var sut1 = NewSut();
        sut1.CreateProfile("Persisted", "desc");

        var sut2 = NewSut();
        Assert.Contains(sut2.GetProfiles(), p => p.Name == "Persisted");
    }

    // --- CreateProfile ---

    [Fact]
    public void CreateProfile_AddsProfileToList()
    {
        var sut = NewSut();
        int before = sut.GetProfiles().Count;

        sut.CreateProfile("New", "desc");

        Assert.Equal(before + 1, sut.GetProfiles().Count);
    }

    [Fact]
    public void CreateProfile_RaisesProfilesChanged()
    {
        var sut = NewSut();
        ProfileEventArgs? received = null;
        sut.ProfilesChanged += (_, e) => received = e;

        sut.CreateProfile("Evt", "desc");

        Assert.NotNull(received);
        Assert.Equal(ProfileEventType.Created, received!.EventType);
    }

    // --- GetProfile / GetDefaultProfile ---

    [Fact]
    public void GetProfile_ReturnsNull_ForUnknownId()
    {
        var sut = NewSut();
        Assert.Null(sut.GetProfile("does-not-exist"));
    }

    [Fact]
    public void GetDefaultProfile_ReturnsDefaultFlaggedProfile()
    {
        var sut = NewSut();
        var def = sut.GetDefaultProfile();

        Assert.NotNull(def);
        Assert.True(def!.IsDefault);
    }

    // --- DeleteProfile ---

    [Fact]
    public void DeleteProfile_RemovesNonDefaultProfile()
    {
        var sut = NewSut();
        sut.CreateProfile("ToDelete");
        var toDelete = sut.GetProfiles().First(p => p.Name == "ToDelete");

        sut.DeleteProfile(toDelete.Id);

        Assert.DoesNotContain(sut.GetProfiles(), p => p.Id == toDelete.Id);
    }

    [Fact]
    public void DeleteProfile_DoesNotRemoveDefaultProfile()
    {
        var sut = NewSut();
        var def = sut.GetDefaultProfile()!;

        sut.DeleteProfile(def.Id);

        Assert.Contains(sut.GetProfiles(), p => p.Id == def.Id);
    }

    // --- DuplicateProfile ---

    [Fact]
    public void DuplicateProfile_CreatesNewProfileWithNewId()
    {
        var sut = NewSut();
        var original = sut.GetDefaultProfile()!;

        sut.DuplicateProfile(original.Id, "Duplicate");
        var dup = sut.GetProfiles().First(p => p.Name == "Duplicate");

        Assert.NotEqual(original.Id, dup.Id);
    }

    // --- SetDefaultProfile ---

    [Fact]
    public void SetDefaultProfile_ChangesDefaultFlag()
    {
        var sut = NewSut();
        sut.CreateProfile("Second");
        var second = sut.GetProfiles().First(p => p.Name == "Second");

        sut.SetDefaultProfile(second.Id);
        var profiles = sut.GetProfiles();

        Assert.True(profiles.Single(p => p.Id == second.Id).IsDefault);
        Assert.False(profiles.Where(p => p.Id != second.Id).Any(p => p.IsDefault));
    }

    // --- UpdateProfile ---

    [Fact]
    public void UpdateProfile_PersistsNameChange()
    {
        var sut = NewSut();
        sut.CreateProfile("OldName");
        var profile = sut.GetProfiles().First(p => p.Name == "OldName");
        profile.Name = "NewName";

        sut.UpdateProfile(profile);

        var reloaded = NewSut().GetProfile(profile.Id);
        Assert.Equal("NewName", reloaded!.Name);
    }

    // --- UpdateProfileWithCurrentMods ---

    [Fact]
    public void UpdateProfileWithCurrentMods_StoresEnabledModNames()
    {
        var sut = NewSut();
        var profile = sut.GetDefaultProfile()!;
        var mods = new List<ModInfo>
        {
            new ModInfo("mod_a", "fake/path", true),
            new ModInfo("mod_b", "fake/path", false)
        };

        sut.UpdateProfileWithCurrentMods(profile.Id, mods);

        var updated = sut.GetProfile(profile.Id)!;
        Assert.Single(updated.EnabledMods);
        Assert.Equal("Standard:mod_a", updated.EnabledMods[0]);
    }

    // --- CreateFromCurrentState ---

    [Fact]
    public void CreateFromCurrentState_OnlySavesEnabledMods()
    {
        var sut = NewSut();
        var mods = new List<ModInfo>
        {
            new ModInfo("active", "fake/path", true),
            new ModInfo("inactive", "fake/path", false)
        };

        var created = sut.CreateFromCurrentState("Snapshot", "desc", mods);

        Assert.Single(created.EnabledMods);
        Assert.Equal("Standard:active", created.EnabledMods[0]);
    }

    // --- RenameModInProfiles ---

    [Fact]
    public void RenameModInProfiles_UpdatesMatchingEntries()
    {
        var sut = NewSut();
        var profile = sut.GetDefaultProfile()!;
        sut.UpdateProfileWithCurrentMods(profile.Id, new List<ModInfo>
        {
            new ModInfo("old_name", "fake/path", true)
        });

        sut.RenameModInProfiles(ModRootKind.Standard, "old_name", "new_name");

        var updated = sut.GetProfile(profile.Id)!;
        Assert.Contains("Standard:new_name", updated.EnabledMods);
        Assert.DoesNotContain("Standard:old_name", updated.EnabledMods);
    }

    [Fact]
    public void RenameModInProfiles_UpdatesAcrossMultipleProfiles()
    {
        var sut = NewSut();
        var p1 = sut.GetDefaultProfile()!;
        sut.UpdateProfileWithCurrentMods(p1.Id, new List<ModInfo> { new ModInfo("shared", "p", true) });
        var p2 = sut.CreateFromCurrentState("Second", "desc", new List<ModInfo> { new ModInfo("shared", "p", true) });

        sut.RenameModInProfiles(ModRootKind.Standard, "shared", "renamed");

        Assert.Contains("Standard:renamed", sut.GetProfile(p1.Id)!.EnabledMods);
        Assert.Contains("Standard:renamed", sut.GetProfile(p2.Id)!.EnabledMods);
    }

    [Fact]
    public void RenameModInProfiles_LeavesNonMatchingEntriesUntouched()
    {
        var sut = NewSut();
        var profile = sut.GetDefaultProfile()!;
        sut.UpdateProfileWithCurrentMods(profile.Id, new List<ModInfo>
        {
            new ModInfo("keep_me", "fake/path", true)
        });

        sut.RenameModInProfiles(ModRootKind.Standard, "other_mod", "new_name");

        var updated = sut.GetProfile(profile.Id)!;
        Assert.Contains("Standard:keep_me", updated.EnabledMods);
        Assert.DoesNotContain("Standard:new_name", updated.EnabledMods);
    }

    [Fact]
    public void RenameModInProfiles_Persists()
    {
        var sut1 = NewSut();
        var profile = sut1.GetDefaultProfile()!;
        sut1.UpdateProfileWithCurrentMods(profile.Id, new List<ModInfo>
        {
            new ModInfo("old_name", "fake/path", true)
        });

        sut1.RenameModInProfiles(ModRootKind.Standard, "old_name", "new_name");

        var sut2 = NewSut();
        Assert.Contains("Standard:new_name", sut2.GetProfile(profile.Id)!.EnabledMods);
    }

    // --- Cross-root naming collisions (compound key {RootKind}:{Name}) ---

    [Fact]
    public void CreateFromCurrentState_SameName_DifferentRoots_StoredSeparately()
    {
        var sut = NewSut();
        var mods = new List<ModInfo>
        {
            new ModInfo("Interface", "p/legacy", true, ModRootKind.Legacy, "p"),
            new ModInfo("Interface", "p/logic", true, ModRootKind.Logic, "p")
        };

        var created = sut.CreateFromCurrentState("Snapshot", "desc", mods);

        Assert.Equal(2, created.EnabledMods.Count);
        Assert.Contains("Legacy:Interface", created.EnabledMods);
        Assert.Contains("Logic:Interface", created.EnabledMods);
    }

    [Fact]
    public void RenameModInProfiles_SameName_OnlyAffectsTargetedRoot()
    {
        var sut = NewSut();
        var profile = sut.GetDefaultProfile()!;
        sut.UpdateProfileWithCurrentMods(profile.Id, new List<ModInfo>
        {
            new ModInfo("Interface", "p/legacy", true, ModRootKind.Legacy, "p"),
            new ModInfo("Interface", "p/logic", true, ModRootKind.Logic, "p")
        });

        sut.RenameModInProfiles(ModRootKind.Logic, "Interface", "InterfaceRenamed");

        var updated = sut.GetProfile(profile.Id)!;
        Assert.Contains("Legacy:Interface", updated.EnabledMods);
        Assert.Contains("Logic:InterfaceRenamed", updated.EnabledMods);
        Assert.DoesNotContain("Logic:Interface", updated.EnabledMods);
    }
}
