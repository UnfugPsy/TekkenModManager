namespace ModManager.Models
{
    public class ModInfo
    {
        public string Name { get; }
        public string Path { get; }
        public bool IsEnabled { get; }
        public string Version { get; set; } = "1.0";
        public string Category { get; set; } = "Misc";
        public string Description { get; set; } = "";
        public long Size { get; set; }
        public DateTime DateAdded { get; set; } = DateTime.Now;
        public string Author { get; set; } = "Unknown";
        public string ThumbnailPath { get; set; } = "";

        public string FormattedSize => Size > 0 ? FormatFileSize(Size) : "Unknown";

        public ModInfo(string name, string path, bool enabled)
        {
            Name = name;
            Path = path;
            IsEnabled = enabled;
            
            if (Directory.Exists(path))
            {
                try
                {
                    Size = CalculateDirectorySize(path);
                }
                catch
                {
                    Size = 0;
                }
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
