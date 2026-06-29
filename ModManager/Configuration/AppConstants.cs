namespace ModManager.Configuration
{
    public static class AppConstants
    {
        public static class UI
        {
            public const int MaxFormWidth = 1024;
            public const int MaxFormHeight = 720;
            public const int MinFormWidth = 600;
            public const int MinFormHeight = 300;
            public const int ListViewVerticalPadding = 2;
            public const int StatusColumnWidth = 120;
            
            // Profile Manager
            public static readonly Size ProfileManagerSize = new Size(600, 450);
            public const int ProfileListWidth = 40; // percentage
            public const int ProfileDetailsWidth = 60; // percentage
            
            public const int CheckboxColumnWidth = 40;
            public const int NameColumnWidth = 200;
            public const int VersionColumnWidth = 80;
            public const int CategoryColumnWidth = 100;
            public const int SizeColumnWidth = 80;
        }
        
        public static class FileExtensions
        {
            public static readonly string[] ModFiles = { ".pak", ".ucas", ".utoc" };
            public static readonly string[] DisabledModFiles = { ".pak-x", ".ucas-x", ".utoc-x" };
            public static readonly string[] ArchiveFiles = { ".zip", ".rar", ".7z" };
        }
        
        public static class Messages
        {
            public const string SetGameLocationFirst = "Please set the game location for the first time.";
            public const string GameLocationNotFound = "The previously saved game location could not be found.";
            public const string ModFolderNotSet = "Mod folder not set or does not exist.";
            public const string SetValidGameLocation = "Please set a valid game location first.";
            public const string SetValidModFolder = "Please set a valid mod folder location first.";
            public const string GameExeNotFound = "TEKKEN 8 executable not found. Please make sure the game is installed correctly.";
        }
        
        public static class Dialogs
        {
            public const string ModArchiveTitle = "Select a Mod Archive File";
            public const string ModArchiveFilter = "Archive files (*.zip;*.rar;*.7z)|*.zip;*.rar;*.7z|ZIP files (*.zip)|*.zip|RAR files (*.rar)|*.rar|7Z files (*.7z)|*.7z|All files (*.*)|*.*";
            public const string GameLocationTitle = "Select TEKKEN 8's Paks directory (or a file inside it)";
            public const string GameLocationFilter = "TEKKEN 8 Pak Files|*.pak|All Files (*.*)|*.*";
            public const string FolderExistsTitle = "Folder Exists";
        }
    }
}