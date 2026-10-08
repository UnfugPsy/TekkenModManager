namespace ModManager.Models
{
    public class ModProfile
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> EnabledMods { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastUsed { get; set; }
        public bool IsDefault { get; set; }

        public ModProfile()
        {
            Id = Guid.NewGuid().ToString();
            Name = string.Empty;
            Description = string.Empty;
            EnabledMods = new List<string>();
            Created = DateTime.Now;
            LastUsed = DateTime.Now;
            IsDefault = false;
        }

        public ModProfile(string name, string description = "") : this()
        {
            Name = name;
            Description = description;
        }

        public ModProfile(string name, string description, List<string> enabledMods) : this(name, description)
        {
            EnabledMods = enabledMods?.ToList() ?? new List<string>();
        }

        public ModProfile Clone()
        {
            return new ModProfile
            {
                Id = Guid.NewGuid().ToString(),
                Name = $"{Name} (Copy)",
                Description = Description,
                EnabledMods = EnabledMods.ToList(),
                Created = DateTime.Now,
                LastUsed = DateTime.Now,
                IsDefault = false
            };
        }

        public override string ToString()
        {
            return Name;
        }
    }
}