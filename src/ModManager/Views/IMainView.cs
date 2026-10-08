using ModManager.Models;
using ModManager.Services;

namespace ModManager.Views
{
    public interface IMainView
    {
        string GameLocation { get; set; }
        
        // Events that the presenter will handle
        event EventHandler ViewLoaded;
        event EventHandler SetGameLocationClicked;
        event EventHandler RefreshClicked;
        event EventHandler OpenModFolderClicked;
        event EventHandler AddModZipClicked;
        event EventHandler<ModFileDroppedEventArgs> ModFileDropped;
        event EventHandler StartGameClicked;
        event EventHandler<ModToggleEventArgs> ModToggled;
        event EventHandler<ModDeleteEventArgs> ModDeleteRequested;
        event EventHandler<ModEditEventArgs> ModEditRequested;
        event EventHandler<ModRenameEventArgs> ModRenameRequested;

        event EventHandler<ViewProfileEventArgs> ProfileSelected;
        event EventHandler CreateProfileClicked;
        event EventHandler ManageProfilesClicked;
        event EventHandler<ViewProfileEventArgs> SaveCurrentAsProfileClicked;

        void ShowMods(List<ModInfo> mods);
        void ShowMessage(string message, string title = null, MessageType messageType = MessageType.Information);
        void ShowProgress(string message);
        void HideProgress();
        void RefreshDisplay();
        void SetGameLocationDisplay(string location);
        void ShowConflicts(HashSet<string> conflictingModNames);

        void UpdateProfilesList(List<ModProfile> profiles);
        void SetCurrentProfile(ModProfile profile);
        void ShowProfileManager(IProfileService profileService);

        string ShowAddModDialog();
        bool ShowConfirmDialog(string message, string title);
        string ShowCreateProfileDialog(string defaultName = "");
        ModMetadataResult? ShowEditModDialog(ModInfo mod);
        string ShowRenameModDialog(string currentName);
        ModRootKind? ShowSelectModRootDialog(string modName, ModRootKind detectedKind);
    }
    
    public class ModToggleEventArgs : EventArgs
    {
        public string ModName { get; set; }
        public string ModKey { get; set; }
        public bool IsEnabled { get; set; }

        public ModToggleEventArgs(string modName, bool isEnabled, string modKey = null)
        {
            ModName = modName;
            IsEnabled = isEnabled;
            ModKey = modKey ?? modName;
        }
    }

    public class ViewProfileEventArgs : EventArgs
    {
        public ModProfile Profile { get; set; }
        public string Action { get; set; }
        
        public ViewProfileEventArgs(ModProfile profile, string action = "")
        {
            Profile = profile;
            Action = action;
        }
    }
    
    public class ModFileDroppedEventArgs : EventArgs
    {
        public string FilePath { get; set; }
        
        public ModFileDroppedEventArgs(string filePath)
        {
            FilePath = filePath;
        }
    }
    
    public class ModDeleteEventArgs : EventArgs
    {
        public string ModName { get; set; }
        public string ModKey { get; set; }

        public ModDeleteEventArgs(string modName, string modKey = null)
        {
            ModName = modName;
            ModKey = modKey ?? modName;
        }
    }
    
    public enum MessageType
    {
        Information,
        Warning,
        Error
    }

    public class ModEditEventArgs : EventArgs
    {
        public ModInfo Mod { get; }
        public ModEditEventArgs(ModInfo mod) => Mod = mod;
    }

    public class ModRenameEventArgs : EventArgs
    {
        public string ModName { get; }
        public string ModKey { get; }
        public ModRenameEventArgs(string modName, string modKey = null)
        {
            ModName = modName;
            ModKey = modKey ?? modName;
        }
    }

    public sealed record ModMetadataResult(
        string Version,
        string Category,
        string Description,
        string Author);
}