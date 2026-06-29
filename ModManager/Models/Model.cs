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
      OpenFileDialog openFileDialog = new()
      {
        Title = "Select TEKKEN 8's Paks directory (or a file inside it)",
        Filter = "TEKKEN 8 Pak Files|*.pak|All Files (*.*)|*.*"
      };

      if (openFileDialog.ShowDialog() == DialogResult.OK)
      {
        string paksDirectory = Path.GetDirectoryName(openFileDialog.FileName);

        if (paksDirectory != null && paksDirectory.EndsWith("Paks", StringComparison.OrdinalIgnoreCase))
        {
          string modsDirectory = Path.Combine(paksDirectory, "Mods");
          Directory.CreateDirectory(modsDirectory);
          GameLocation = modsDirectory;
          Settings.Default.GameLocationPath = modsDirectory;
          Settings.Default.Save();

          MessageBox.Show($"Mods directory set to: {GameLocation}");
        }
        else
        {
          MessageBox.Show("Please select a file from within the 'Paks' directory.");
        }
      }
    }
  }
}
