namespace ModManager.Models
{
    public class ModInfo
    {
        public string Name { get; }
        public string Path { get; }
        public bool IsEnabled { get; }
        public ModRootKind RootKind { get; }
        public string RootPath { get; }
        public string Version { get; set; } = "1.0";
        public string Category { get; set; } = "Misc";
        public string Description { get; set; } = "";
        private long? _size;

        /// <summary>The folder's total size, measured on first read so scans that never show it do not walk the tree.</summary>
        public long Size
        {
            get => _size ??= MeasureSize();
            set => _size = value;
        }
        public DateTime DateAdded { get; set; } = DateTime.Now;
        public string Author { get; set; } = "Unknown";
        public string ThumbnailPath { get; set; } = "";

        public string FormattedSize => Size > 0 ? FormatFileSize(Size) : "Unknown";

        /// <summary>
        /// Stable identity that is unique across the three mod roots (e.g. "Logic:FrameDataTool").
        /// Used by profiles, toggling, delete and rename so same-named mods in different roots don't clash.
        /// </summary>
        public string Key => $"{RootKind}:{Name}";

        public ModInfo(string name, string path, bool enabled)
            : this(name, path, enabled, ModRootKind.Standard, System.IO.Path.GetDirectoryName(path) ?? string.Empty)
        {
        }

        public ModInfo(string name, string path, bool enabled, ModRootKind rootKind, string rootPath)
        {
            Name = name;
            Path = path;
            IsEnabled = enabled;
            RootKind = rootKind;
            RootPath = rootPath;
        }

        private long MeasureSize()
        {
            if (!Directory.Exists(Path))
            {
                return 0;
            }

            try
            {
                return CalculateDirectorySize(Path);
            }
            catch (IOException)
            {
                return 0;
            }
            catch (UnauthorizedAccessException)
            {
                return 0;
            }
        }

        private static long CalculateDirectorySize(string directoryPath)
        {
            var dirInfo = new DirectoryInfo(directoryPath);
            return dirInfo.GetFiles("*", SearchOption.AllDirectories).Sum(file => file.Length);
        }
        
        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}
