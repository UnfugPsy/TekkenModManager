using ModManager.Properties;
using System;
using System.IO;
using System.Windows.Forms;

namespace ModManager.Models
{
  public class Model
  {
    private string _gameLocation;

    public string GameLocation { get => _gameLocation; set => _gameLocation = value; }

    public Model()
    {
      if (!String.IsNullOrEmpty(Settings.Default.GameLocationPath))
      {
        GameLocation = Settings.Default.GameLocationPath;
      }
    }

    public void SetGameLocation()
    {
      FolderBrowserDialog folderBrowserDialog = new()
      {
        Description = "Select TEKKEN 8's 'Paks' (or 'Mods') directory",
        UseDescriptionForTitle = true,
        ShowNewFolderButton = false
      };

      if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
      {
        return;
      }

      string? modsDirectory = ResolveModsDirectory(folderBrowserDialog.SelectedPath);

      if (modsDirectory == null)
      {
        MessageBox.Show("Please select TEKKEN 8's 'Paks' or 'Mods' directory.");
        return;
      }

      Directory.CreateDirectory(modsDirectory);
      GameLocation = modsDirectory;
      Settings.Default.GameLocationPath = modsDirectory;
      Settings.Default.Save();

      MessageBox.Show($"Mods directory set to: {GameLocation}");
    }

    public static string? ResolveModsDirectory(string? selectedPath)
    {
      if (string.IsNullOrWhiteSpace(selectedPath))
      {
        return null;
      }

      string trimmed = selectedPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
      string folderName = Path.GetFileName(trimmed);

      if (string.Equals(folderName, "Mods", StringComparison.OrdinalIgnoreCase))
      {
        return trimmed;
      }

      if (string.Equals(folderName, "Paks", StringComparison.OrdinalIgnoreCase))
      {
        return Path.Combine(trimmed, "Mods");
      }

      if (Directory.Exists(Path.Combine(trimmed, "Paks")))
      {
        return Path.Combine(trimmed, "Paks", "Mods");
      }

      if (Directory.Exists(Path.Combine(trimmed, "Mods")))
      {
        return Path.Combine(trimmed, "Mods");
      }

      return null;
    }
  }
}
