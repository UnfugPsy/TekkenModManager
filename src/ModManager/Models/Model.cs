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
        GameLocation = MigrateLegacyModsPath(Settings.Default.GameLocationPath);

        if (!string.Equals(GameLocation, Settings.Default.GameLocationPath, StringComparison.Ordinal))
        {
          Settings.Default.GameLocationPath = GameLocation;
          Settings.Default.Save();
        }
      }
    }

    public void SetGameLocation()
    {
      FolderBrowserDialog folderBrowserDialog = new()
      {
        Description = "Select TEKKEN 8's 'Paks' directory",
        UseDescriptionForTitle = true,
        ShowNewFolderButton = false
      };

      if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
      {
        return;
      }

      string? paksRoot = ResolveModsDirectory(folderBrowserDialog.SelectedPath);

      if (paksRoot == null)
      {
        MessageBox.Show("Please select TEKKEN 8's 'Paks' directory (or one of its mod folders).");
        return;
      }

      Directory.CreateDirectory(paksRoot);
      GameLocation = paksRoot;
      Settings.Default.GameLocationPath = paksRoot;
      Settings.Default.Save();

      MessageBox.Show($"Paks directory set to: {GameLocation}");
    }

    /// <summary>
    /// Resolves the selected folder to the game's Paks root, from which the three mod
    /// folders (Mods, ~mods, LogicMods) are derived. Returns null if it can't be determined.
    /// </summary>
    public static string? ResolveModsDirectory(string? selectedPath)
    {
      if (string.IsNullOrWhiteSpace(selectedPath))
      {
        return null;
      }

      string trimmed = selectedPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
      string folderName = Path.GetFileName(trimmed);

      // Selected the Paks folder directly.
      if (string.Equals(folderName, "Paks", StringComparison.OrdinalIgnoreCase))
      {
        return trimmed;
      }

      // Selected one of the mod folders -> walk up to Paks.
      if (IsKnownModFolderName(folderName))
      {
        string? parent = Path.GetDirectoryName(trimmed);
        if (!string.IsNullOrEmpty(parent))
        {
          return parent;
        }
      }

      // Selected a folder that contains Paks (e.g. the Content folder).
      if (Directory.Exists(Path.Combine(trimmed, "Paks")))
      {
        return Path.Combine(trimmed, "Paks");
      }

      // Selected a folder that already contains one of the mod folders -> treat it as the Paks root.
      if (ContainsAnyModFolder(trimmed))
      {
        return trimmed;
      }

      return null;
    }

    /// <summary>
    /// Migrates a previously stored path that pointed at ...\Paks\Mods up to the Paks root,
    /// so existing configurations keep working after the multi-root change.
    /// </summary>
    public static string MigrateLegacyModsPath(string storedPath)
    {
      if (string.IsNullOrWhiteSpace(storedPath))
      {
        return storedPath;
      }

      string trimmed = storedPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
      string folderName = Path.GetFileName(trimmed);

      if (IsKnownModFolderName(folderName))
      {
        string? parent = Path.GetDirectoryName(trimmed);
        if (!string.IsNullOrEmpty(parent))
        {
          return parent;
        }
      }

      return trimmed;
    }

    private static bool IsKnownModFolderName(string folderName)
    {
      foreach (var name in ModRoots.FolderNames.Values)
      {
        if (string.Equals(folderName, name, StringComparison.OrdinalIgnoreCase))
        {
          return true;
        }
      }
      return false;
    }

    private static bool ContainsAnyModFolder(string root)
    {
      foreach (var name in ModRoots.FolderNames.Values)
      {
        if (Directory.Exists(Path.Combine(root, name)))
        {
          return true;
        }
      }
      return false;
    }
  }
}
