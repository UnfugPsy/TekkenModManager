using ModManager.Models;
using ModManager.Presenters;
using Xunit;

namespace ModManager.Tests.Presenters;

public class MainPresenterDetectRootKindTests
{
    [Fact]
    public void DetectRootKind_ReturnsLogic_WhenLogicModsSegmentPresent()
    {
        var entries = new[] { "LogicMods/FrameDataTool/Scripts/main.lua" };

        Assert.Equal(ModRootKind.Logic, MainPresenter.DetectRootKind(entries));
    }

    [Fact]
    public void DetectRootKind_ReturnsLegacy_WhenLegacySegmentPresent()
    {
        var entries = new[] { "~mods/CoolSkin/skin.pak" };

        Assert.Equal(ModRootKind.Legacy, MainPresenter.DetectRootKind(entries));
    }

    [Fact]
    public void DetectRootKind_ReturnsStandard_WhenModsSegmentPresent()
    {
        var entries = new[] { "Mods/CoolSkin/skin.pak" };

        Assert.Equal(ModRootKind.Standard, MainPresenter.DetectRootKind(entries));
    }

    [Fact]
    public void DetectRootKind_ReturnsLogic_ForLuaHeuristic_WhenNoRootSegment()
    {
        var entries = new[] { "FrameDataTool/Scripts/main.lua" };

        Assert.Equal(ModRootKind.Logic, MainPresenter.DetectRootKind(entries));
    }

    [Fact]
    public void DetectRootKind_ReturnsLogic_ForDllHeuristic_WhenNoRootSegment()
    {
        var entries = new[] { "SomeTool/tool.dll" };

        Assert.Equal(ModRootKind.Logic, MainPresenter.DetectRootKind(entries));
    }

    [Fact]
    public void DetectRootKind_DefaultsToStandard_ForPlainPak()
    {
        var entries = new[] { "CoolSkin/skin.pak" };

        Assert.Equal(ModRootKind.Standard, MainPresenter.DetectRootKind(entries));
    }

    [Fact]
    public void DetectRootKind_HandlesBackslashSeparators()
    {
        var entries = new[] { "LogicMods\\FrameDataTool\\main.lua" };

        Assert.Equal(ModRootKind.Logic, MainPresenter.DetectRootKind(entries));
    }

    [Fact]
    public void DetectRootKind_DefaultsToStandard_ForEmptyInput()
    {
        Assert.Equal(ModRootKind.Standard, MainPresenter.DetectRootKind(new string[0]));
    }

    // --- Case-insensitivity & whitespace tolerance ---

    [Theory]
    [InlineData("logicmods/tool/main.lua")]
    [InlineData("LOGICMODS/tool/main.lua")]
    [InlineData(" LogicMods /tool/main.lua")]
    [InlineData("LogicMods /tool/data.bin")]
    public void DetectRootKind_MapsLogic_RegardlessOfCaseOrWhitespace(string entry)
    {
        Assert.Equal(ModRootKind.Logic, MainPresenter.DetectRootKind(new[] { entry }));
    }

    [Theory]
    [InlineData(" ~mods /skin/skin.pak")]
    [InlineData("~MODS/skin/skin.pak")]
    public void DetectRootKind_MapsLegacy_RegardlessOfCaseOrWhitespace(string entry)
    {
        Assert.Equal(ModRootKind.Legacy, MainPresenter.DetectRootKind(new[] { entry }));
    }

    [Fact]
    public void DetectRootKind_ExplicitSegment_TakesPrecedenceOverLuaHeuristic()
    {
        // A .lua present but the archive is rooted under ~mods -> honor the explicit root.
        var entries = new[] { "~mods/Pack/readme.lua", "~mods/Pack/skin.pak" };

        Assert.Equal(ModRootKind.Legacy, MainPresenter.DetectRootKind(entries));
    }
}
