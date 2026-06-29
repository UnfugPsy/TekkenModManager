using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ModManager.Utils;

namespace ModManager.Views
{
  public partial class HelpForm : Form
  {
    private Color accentColor = Theme.AccentCyan;
    private Color highlightColor = Theme.AccentPink;
    private Color backgroundColor = Theme.Background;
    private Color textColor = Theme.TextPrimary;
    private Color headerColor = Color.FromArgb(220, 220, 255);
    private Panel contentPanel;

    public HelpForm()
    {
      InitializeComponent();
      ThemeUtils.ApplyCyberpunkTheme(this);
    }

    protected override void OnShown(EventArgs e)
    {
      base.OnShown(e);

      if (contentPanel != null)
      {
        contentPanel.AutoScrollPosition = new Point(0, 0);
        contentPanel.Invalidate();
      }
    }

    private void InitializeComponent()
    {
      SuspendLayout();

      AutoScaleDimensions = new SizeF(7F, 15F);
      AutoScaleMode = AutoScaleMode.Font;
      ClientSize = new Size(750, 650);
      FormBorderStyle = FormBorderStyle.FixedDialog;
      MaximizeBox = false;
      MinimizeBox = false;
      StartPosition = FormStartPosition.CenterParent;
      Text = "Tekken Mod Manager - Help";
      ShowIcon = false;
      KeyPreview = true;
      KeyDown += HelpForm_KeyDown;

      CreateStyledHelpContent();

      ResumeLayout(false);
    }

    private void CreateStyledHelpContent()
    {
      contentPanel = new Panel();
      contentPanel.Dock = DockStyle.Fill;
      contentPanel.AutoScroll = true;
      contentPanel.AutoScrollPosition = new Point(0, 0);
      contentPanel.BackColor = backgroundColor;
      this.Controls.Add(contentPanel);

      int yPos = 20;

      yPos = HelpFormUtils.CreateHeaderSection(contentPanel, yPos, headerColor);
      yPos += 20;

      yPos = HelpFormUtils.CreateSection(contentPanel, yPos, "GETTING STARTED",
          new string[] {
                    "STEP 1: Set Game Location",
                    "   • Click 'Set Game Location' button or press Ctrl+G",
                    "   • Browse to your TEKKEN 8 installation folder",
                    "   • Navigate to: TEKKEN8\\Polaris\\Content\\Paks",
                    "   • Click 'Select Folder' to confirm",
                    "   • A 'Mods' folder will be created automatically if needed",
                    "",
                    "STEP 2: Add Your First Mod",
                    "   • Method 1: Click 'Add Zipped Mod' button or press Ctrl+N",
                    "     - Select a ZIP, RAR, or 7Z file containing the mod",
                    "   • Method 2: Drag & Drop",
                    "     - Simply drag ZIP, RAR, or 7Z files into the app window",
                    "     - Drop them anywhere on the mod manager interface",
                    "   • The mod will be extracted to your Mods directory",
                    "   • You'll see the mod appear in the list below",
                    "",
                    "STEP 3: Enable/Disable Mods",
                    "   • Tick the ACTIVE checkbox in a mod row to enable/disable it",
                    "   • Right-click any mod for more options (edit, delete, toggle)",
                    "   • Watch the 'STATUS' column change to ACTIVE or INACTIVE",
                    "   • Use F5 or 'Refresh' button to reload the mod list",
                    "   • Use Ctrl+O or 'Open Mod Folder' to browse mod files"
          }, accentColor, textColor);
      yPos += 30;

      yPos = HelpFormUtils.CreateSection(contentPanel, yPos, "KEYBOARD SHORTCUTS",
          new string[] {
                    "F1          Show this help dialog",
                    "F5          Refresh mod list",
                    "Ctrl+G      Set game location",
                    "Ctrl+O      Open mod folder",
                    "Ctrl+N      Add new mod from ZIP file",
                    "Escape      Close dialogs"
          }, highlightColor, textColor);
      yPos += 30;

      yPos = HelpFormUtils.CreateSection(contentPanel, yPos, "RIGHT-CLICK MENU",
          new string[] {
                    "Right-click any mod in the list to access:",
                    "",
                    "• Toggle Enable/Disable   Enable or disable the selected mod",
                    "• Edit Metadata           Edit version, category, author, description",
                    "• Delete Mod              Permanently remove the mod (with confirmation)",
                    "• Open Mod Folder         Browse to the mods directory",
                    "• Refresh List            Update the mod list",
                    "",
                    "Warning: Deleting a mod permanently removes all its files!"
          }, highlightColor, textColor);
      yPos += 30;

      yPos = HelpFormUtils.CreateSection(contentPanel, yPos, "MOD METADATA",
          new string[] {
                    "Each mod stores editable details, shown when you hover its name:",
                    "",
                    "• Version       The mod version number",
                    "• Category      Group mods by type (e.g. Costume, Stage, UI)",
                    "• Author        Who created the mod",
                    "• Description   Free-text notes about the mod",
                    "",
                    "To edit: right-click a mod > 'Edit Metadata', or double-click",
                    "the row. Hover a mod name to preview these details."
          }, accentColor, textColor);
      yPos += 30;

      yPos = HelpFormUtils.CreateSection(contentPanel, yPos, "MOD MANAGEMENT",
          new string[] {
                    "• Enable / Disable  Tick the ACTIVE checkbox in the mod row",
                    "• Add Mods          Drag & drop ZIP/RAR/7Z files or use Ctrl+N",
                    "• Right-Click       Access context menu for more options",
                    "• Edit Metadata     Right-click mod > Edit Metadata (or double-click)",
                    "• Delete Mod        Right-click mod > Delete (with confirmation)",
                    "• Search            Filter by name, category, or author",
                    "• Conflicts         Orange names share a .pak and may override",
                    "• Browse            Opens Windows Explorer to your mods folder",
                    "",
                    "Note: Changes take effect immediately when you toggle a mod"
          }, accentColor, textColor);
      yPos += 30;

      yPos = HelpFormUtils.CreateSection(contentPanel, yPos, "TROUBLESHOOTING",
          new string[] {
                    "Game won't start:",
                    "   • Make sure game location points to the 'Paks' folder",
                    "   • Try disabling all mods to test if one is corrupted",
                    "   • Verify TEKKEN 8 is properly installed",
                    "",
                    "Mod not working:",
                    "   • Check if the mod is enabled (STATUS shows ACTIVE)",
                    "   • Tick the ACTIVE checkbox to enable it if inactive",
                    "   • Refresh the mod list after adding new mods",
                    "   • Orange mod names conflict and may override each other",
                    "",
                    "Performance issues:",
                    "   • Too many large mods can affect game performance",
                    "   • Try enabling fewer mods at once",
                    "   • Check mod compatibility with your game version"
          }, accentColor, textColor);
      yPos += 30;

      yPos = HelpFormUtils.CreateSection(contentPanel, yPos, "ABOUT",
          new string[] {
                    "TEKKEN 8 Mod Manager v1.0",
                    "A simple tool for managing TEKKEN 8 game modifications",
                    "",
                    "made by Unfug",
                    "Features:",
                    "   • Easy mod installation from ZIP/RAR/7Z files",
                    "   • Drag & drop support for mod archives",
                    "   • One-click enable/disable for mods",
                    "   • Editable metadata and profiles",
                    "   • Conflict detection and search",
                    "   • Dark theme UI with keyboard shortcuts",
                    "",
                    "Press Escape or click outside to close this help window"
          }, headerColor, textColor);
    }

    private void HelpForm_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.KeyCode == Keys.Escape)
      {
        this.Close();
        e.Handled = true;
      }
    }
  }
}

