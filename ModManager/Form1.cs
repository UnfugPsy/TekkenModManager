using ModManager.Properties;
using ModManager.Models;
using ModManager.Services;
using ModManager.Views;
using ModManager.Utils;
using ModManager.Presenters;
using ModManager.Configuration;
using ModManager.Controls;
using System.Diagnostics;
using System.IO;

namespace ModManager
{
  public partial class Form1 : Form, IMainView
  {
    public event EventHandler ViewLoaded;
    public event EventHandler SetGameLocationClicked;
    public event EventHandler RefreshClicked;
    public event EventHandler OpenModFolderClicked;
    public event EventHandler AddModZipClicked;
    public event EventHandler<ModFileDroppedEventArgs> ModFileDropped;
    public event EventHandler StartGameClicked;
    public event EventHandler HelpRequested;
    public event EventHandler<ModToggleEventArgs> ModToggled;
    public event EventHandler<ModDeleteEventArgs> ModDeleteRequested;
    public event EventHandler<ModEditEventArgs> ModEditRequested;
    
    public event EventHandler<ViewProfileEventArgs> ProfileSelected;
    public event EventHandler CreateProfileClicked;
    public event EventHandler ManageProfilesClicked;
    public event EventHandler<ViewProfileEventArgs> SaveCurrentAsProfileClicked;

    public string GameLocation { get; set; }

    private MainPresenter _presenter;
    private Model _model;
    private List<ModInfo> _mods = new();

    private ComboBox _profilesComboBox;
    private ModProfile _currentProfile;
    private ContextMenuStrip _listViewContextMenu;
    
    private ModViewManager _modViewManager;
    private ToolTip _toolTip;

    public Form1()
    {
      InitializeComponent();
      _model = new Model();

      TrySetWindowIcon();

      this.DoubleBuffered = true;
      this.KeyPreview = true;
      this.AllowDrop = true;

      _toolTip = new ToolTip();
      components.Add(_toolTip);

      ConfigureButtons();
      ConfigureDragDrop();
      ConfigureContextMenu();

      _profilesComboBox = cmbProfiles;
      
      InitializeModViewManager();
      
      _presenter = new MainPresenter(this);
    }

    private void TrySetWindowIcon()
    {
      try
      {
        using var stream = System.Reflection.Assembly.GetExecutingAssembly()
          .GetManifestResourceStream("ModManager.AppIcon.ico");
        if (stream != null)
        {
          this.Icon = new Icon(stream);
        }
      }
      catch
      {
      }
    }

    private void InitializeModViewManager()
    {
      try
      {
        _modViewManager = new ModViewManager
        {
          Name = "modViewManager",
          Visible = false
        };
        
        _modViewManager.ModToggled += (sender, e) => ModToggled?.Invoke(this, e);
        _modViewManager.ModDeleteRequested += (sender, e) => ModDeleteRequested?.Invoke(this, e);
        _modViewManager.ModEditRequested += (sender, e) => ModEditRequested?.Invoke(this, e);
        
        this.Controls.Add(_modViewManager);
        
        var oldListView = this.Controls.Find("listView1", true).FirstOrDefault();
        if (oldListView != null)
        {
          oldListView.Visible = false;
          
          _modViewManager.Dock = oldListView.Dock;
          _modViewManager.Location = oldListView.Location;
          _modViewManager.Size = oldListView.Size;
          _modViewManager.Anchor = oldListView.Anchor;
        }
        else
        {
          _modViewManager.Dock = DockStyle.Fill;
        }
        
        _modViewManager.Visible = true;
        _modViewManager.BringToFront();
        _modViewManager.ContextMenuStrip = _listViewContextMenu;

        System.Diagnostics.Debug.WriteLine("ModViewManager initialized successfully");
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine($"Error initializing ModViewManager: {ex.Message}");
        var oldListView = this.Controls.Find("listView1", true).FirstOrDefault();
        if (oldListView != null)
        {
          oldListView.Visible = true;
        }
      }
    }
    
    protected override void OnKeyDown(KeyEventArgs e)
    {
      if (e.KeyCode == Keys.F1)
      {
        HelpRequested?.Invoke(this, EventArgs.Empty);
        ShowHelp();
        e.Handled = true;
        return;
      }
      
      if (e.KeyCode == Keys.F5)
      {
        RefreshClicked?.Invoke(this, EventArgs.Empty);
        e.Handled = true;
        return;
      }
      
      if (e.Control && e.KeyCode == Keys.G)
      {
        SetGameLocationClicked?.Invoke(this, EventArgs.Empty);
        e.Handled = true;
        return;
      }
      
      if (e.Control && e.KeyCode == Keys.O)
      {
        OpenModFolderClicked?.Invoke(this, EventArgs.Empty);
        e.Handled = true;
        return;
      }
      
      if (e.Control && e.KeyCode == Keys.N)
      {
        AddModZipClicked?.Invoke(this, EventArgs.Empty);
        e.Handled = true;
        return;
      }
      
      base.OnKeyDown(e);
    }

    private void ShowHelp()
    {
      using (HelpForm helpForm = new HelpForm())
      {
        helpForm.ShowDialog(this);
      }
    }

    private void ConfigureButtons()
    {
      ConfigureButton(btnStartGame, "Start TEKKEN 8", 110, "Launch the game with selected mods enabled");
      ConfigureButton(btnAddModZip, "Add Zipped Mod", 110, "Install a mod from a ZIP file (Ctrl+N)");
      ConfigureButton(btnRefresh, "Refresh", 80, "Refresh the mod list (F5)");
      ConfigureButton(btnOpenModFolder, "Open Mod Folder", 120, "Open the mods directory in Windows Explorer (Ctrl+O)");
      ConfigureButton(btnSetGameLocation, "Set Game Location", 130, "Browse for the game installation directory (Ctrl+G)");
      ConfigureHelpButton();
    }

    private void ConfigureButton(Button btn, string text, int width, string tooltip)
    {
        btn.Size = new Size(width, Theme.ButtonHeight);
        btn.Text = text;
        btn.BackColor = Theme.ButtonBackground;
        btn.ForeColor = Theme.AccentCyan;
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderColor = Theme.AccentCyan;
        btn.FlatAppearance.BorderSize = Theme.BorderSize;
        btn.FlatAppearance.MouseOverBackColor = Theme.ButtonHover;
        btn.Font = Theme.FontBold;
        btn.Margin = new Padding(0, 0, Theme.ButtonMargin, 0);
        btn.Cursor = Cursors.Hand;
        btn.UseVisualStyleBackColor = false;

        _toolTip.SetToolTip(btn, tooltip);
    }

    private void ConfigureHelpButton()
    {
        btnHelp.BackColor = Theme.ButtonBackground;
        btnHelp.ForeColor = Theme.AccentCyan;
        btnHelp.FlatStyle = FlatStyle.Flat;
        btnHelp.FlatAppearance.BorderColor = Theme.AccentCyan;
        btnHelp.FlatAppearance.BorderSize = Theme.BorderSize;
        btnHelp.FlatAppearance.MouseOverBackColor = Theme.ButtonHover;
        btnHelp.Font = Theme.FontSmallBold;
        btnHelp.Cursor = Cursors.Hand;

        _toolTip.SetToolTip(btnHelp, "Show help and keyboard shortcuts (F1)");
    }

    private void ConfigureDragDrop()
    {
      this.DragEnter += Form1_DragEnter;
      this.DragDrop += Form1_DragDrop;
      
      listView1.AllowDrop = true;
      listView1.DragEnter += Form1_DragEnter;
      listView1.DragDrop += Form1_DragDrop;
    }

    private void Form1_DragEnter(object sender, DragEventArgs e)
    {
      if (e.Data.GetDataPresent(DataFormats.FileDrop))
      {
        string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
        
        foreach (string file in files)
        {
          string extension = Path.GetExtension(file).ToLowerInvariant();
          if (extension == ".zip" || extension == ".rar" || extension == ".7z")
          {
            e.Effect = DragDropEffects.Copy;
            return;
          }
        }
      }
      
      e.Effect = DragDropEffects.None;
    }

    private void Form1_DragDrop(object sender, DragEventArgs e)
    {
      if (e.Data.GetDataPresent(DataFormats.FileDrop))
      {
        string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
        
        foreach (string file in files)
        {
          string extension = Path.GetExtension(file).ToLowerInvariant();
          if (extension == ".zip" || extension == ".rar" || extension == ".7z")
          {
            ModFileDropped?.Invoke(this, new ModFileDroppedEventArgs(file));
            
            break;
          }
        }
      }
    }

    private void ConfigureContextMenu()
    {
      _listViewContextMenu = new ContextMenuStrip();
      _listViewContextMenu.BackColor = Theme.PanelDark;
      _listViewContextMenu.ForeColor = Theme.TextPrimary;
      _listViewContextMenu.Font = Theme.FontRegular;
      
      var toggleItem = new ToolStripMenuItem("Toggle Enable/Disable");
      toggleItem.Click += ContextMenu_ToggleMod;
      toggleItem.Image = null;
      _listViewContextMenu.Items.Add(toggleItem);

      var editItem = new ToolStripMenuItem("Edit Metadata");
      editItem.Click += ContextMenu_EditMod;
      _listViewContextMenu.Items.Add(editItem);

      _listViewContextMenu.Items.Add(new ToolStripSeparator());
      
      var deleteItem = new ToolStripMenuItem("Delete Mod");
      deleteItem.Click += ContextMenu_DeleteMod;
      deleteItem.ForeColor = Theme.AccentPink; 
      _listViewContextMenu.Items.Add(deleteItem);
      
      _listViewContextMenu.Items.Add(new ToolStripSeparator());
      
      var openFolderItem = new ToolStripMenuItem("Open Mod Folder");
      openFolderItem.Click += ContextMenu_OpenModFolder;
      _listViewContextMenu.Items.Add(openFolderItem);
      
      var refreshItem = new ToolStripMenuItem("Refresh List");
      refreshItem.Click += ContextMenu_Refresh;
      _listViewContextMenu.Items.Add(refreshItem);
      
      }

    private void ContextMenu_ToggleMod(object sender, EventArgs e)
    {
      var mod = _modViewManager?.SelectedMod;
      if (mod == null) return;
      ModToggled?.Invoke(this, new ModToggleEventArgs(mod.Name, !mod.IsEnabled));
    }

    private void ContextMenu_EditMod(object sender, EventArgs e)
    {
      var mod = _modViewManager?.SelectedMod;
      if (mod == null) return;
      ModEditRequested?.Invoke(this, new ModEditEventArgs(mod));
    }

    private void ContextMenu_DeleteMod(object sender, EventArgs e)
    {
      var mod = _modViewManager?.SelectedMod;
      if (mod == null) return;
      ModDeleteRequested?.Invoke(this, new ModDeleteEventArgs(mod.Name));
    }

    private void ContextMenu_OpenModFolder(object sender, EventArgs e)
    {
      OpenModFolderClicked?.Invoke(this, EventArgs.Empty);
    }

    private void ContextMenu_Refresh(object sender, EventArgs e)
    {
      RefreshClicked?.Invoke(this, EventArgs.Empty);
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        _listViewContextMenu?.Dispose();
        components?.Dispose();
      }
      base.Dispose(disposing);
    }

    private void MainForm_Load(object sender, EventArgs e)
    {
      ViewLoaded?.Invoke(this, EventArgs.Empty);
    }

    private void btnSetGameLocation_Click(object sender, EventArgs e)
    {
      SetGameLocationClicked?.Invoke(this, EventArgs.Empty);
    }

    private void btnRefresh_Click(object sender, EventArgs e)
    {
      RefreshClicked?.Invoke(this, EventArgs.Empty);
    }

    private void btnOpenModFolder_Click(object sender, EventArgs e)
    {
      OpenModFolderClicked?.Invoke(this, EventArgs.Empty);
    }

    private void btnAddModZip_Click(object sender, EventArgs e)
    {
      AddModZipClicked?.Invoke(this, EventArgs.Empty);
    }

    private void btnStartGame_Click(object sender, EventArgs e)
    {
      StartGameClicked?.Invoke(this, EventArgs.Empty);
    }

    private void btnHelp_Click(object sender, EventArgs e)
    {
      HelpRequested?.Invoke(this, EventArgs.Empty);
      ShowHelp();
    }

    private void cmbProfiles_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (_profilesComboBox.SelectedItem is ModProfile selectedProfile)
      {
        ProfileSelected?.Invoke(this, new ViewProfileEventArgs(selectedProfile));
      }
    }

    private void btnCreateProfile_Click(object sender, EventArgs e)
    {
      CreateProfileClicked?.Invoke(this, EventArgs.Empty);
    }

    private void btnManageProfiles_Click(object sender, EventArgs e)
    {
      ManageProfilesClicked?.Invoke(this, EventArgs.Empty);
    }

    private void btnSaveProfile_Click(object sender, EventArgs e)
    {
      SaveCurrentAsProfileClicked?.Invoke(this, new ViewProfileEventArgs(_currentProfile));
    }

    #region IMainView Implementation

    public void ShowMods(List<ModInfo> mods)
    {
      if (InvokeRequired)
      {
        Invoke(new Action(() => ShowMods(mods)));
        return;
      }

      _mods = mods;

      _modViewManager?.LoadMods(_mods);

      int activeCount = _mods.Count(m => m.IsEnabled);
      this.Text = $"Tekken Mod Manager — {_mods.Count} mods, {activeCount} active";
    }

    public void ShowMessage(string message, string title = null, MessageType messageType = MessageType.Information)
    {
      if (InvokeRequired)
      {
        Invoke(new Action(() => ShowMessage(message, title, messageType)));
        return;
      }

      MessageBoxIcon icon = messageType switch
      {
        MessageType.Warning => MessageBoxIcon.Warning,
        MessageType.Error => MessageBoxIcon.Error,
        _ => MessageBoxIcon.Information
      };

      MessageBox.Show(this, message, title ?? "Tekken Mod Manager", MessageBoxButtons.OK, icon);
    }

    public void ShowProgress(string message)
    {
      if (InvokeRequired)
      {
        Invoke(new Action(() => ShowProgress(message)));
        return;
      }

      this.Cursor = Cursors.WaitCursor;
      this.Text = $"Tekken Mod Manager - {message}";
    }

    public void HideProgress()
    {
      if (InvokeRequired)
      {
        Invoke(new Action(() => HideProgress()));
        return;
      }

      this.Cursor = Cursors.Default;
      int activeCount = _mods.Count(m => m.IsEnabled);
      this.Text = $"Tekken Mod Manager — {_mods.Count} mods, {activeCount} active";
    }

    public void RefreshDisplay()
    {
      if (InvokeRequired)
      {
        Invoke(new Action(() => RefreshDisplay()));
        return;
      }

      _modViewManager?.RefreshView();
    }

    public void SetGameLocationDisplay(string location)
    {
      if (InvokeRequired)
      {
        Invoke(new Action(() => SetGameLocationDisplay(location)));
        return;
      }

      GameLocation = location;
      lblGameLocation.Text = $"Game Location: {location}";
    }

    public string ShowAddModDialog()
    {
      using (OpenFileDialog openFileDialog = new OpenFileDialog())
      {
        openFileDialog.Title = AppConstants.Dialogs.ModArchiveTitle;
        openFileDialog.Filter = AppConstants.Dialogs.ModArchiveFilter;
        
        if (openFileDialog.ShowDialog(this) == DialogResult.OK)
        {
          return openFileDialog.FileName;
        }
      }
      return string.Empty;
    }

    public bool ShowConfirmDialog(string message, string title)
    {
      var result = MessageBox.Show(this, message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
      return result == DialogResult.Yes;
    }

    public string ShowCreateProfileDialog(string defaultName = "")
    {
      using var dialog = new CreateProfileDialog(defaultName);
      if (dialog.ShowDialog(this) == DialogResult.OK)
      {
        return dialog.ProfileName;
      }
      return string.Empty;
    }

    public void UpdateProfilesList(List<ModProfile> profiles)
    {
      if (InvokeRequired)
      {
        Invoke(new Action(() => UpdateProfilesList(profiles)));
        return;
      }

      cmbProfiles.SelectedIndexChanged -= cmbProfiles_SelectedIndexChanged;
      
      _profilesComboBox.Items.Clear();
      foreach (var profile in profiles)
      {
        _profilesComboBox.Items.Add(profile);
      }

      if (_profilesComboBox.Items.Count > 0 && _currentProfile != null)
      {
        var currentIndex = profiles.FindIndex(p => p.Id == _currentProfile.Id);
        if (currentIndex >= 0)
        {
          _profilesComboBox.SelectedIndex = currentIndex;
        }
      }
      
      cmbProfiles.SelectedIndexChanged += cmbProfiles_SelectedIndexChanged;
    }

    public void SetCurrentProfile(ModProfile profile)
    {
      if (InvokeRequired)
      {
        Invoke(new Action(() => SetCurrentProfile(profile)));
        return;
      }

      cmbProfiles.SelectedIndexChanged -= cmbProfiles_SelectedIndexChanged;

      _currentProfile = profile;
      if (_profilesComboBox.Items.Cast<ModProfile>().Any(p => p.Id == profile?.Id))
      {
        _profilesComboBox.SelectedItem = profile;
      }
      
      cmbProfiles.SelectedIndexChanged += cmbProfiles_SelectedIndexChanged;
    }

    public void ShowProfileManager(IProfileService profileService)
    {
      using var profileManager = new ProfileManagerForm(profileService);
      profileManager.ShowDialog(this);
    }

    public void ShowConflicts(HashSet<string> conflictingModNames)
    {
      if (InvokeRequired)
      {
        Invoke(new Action(() => ShowConflicts(conflictingModNames)));
        return;
      }

      _modViewManager?.SetConflicts(conflictingModNames);
    }

    public ModMetadataResult? ShowEditModDialog(ModInfo mod)
    {
      using var dlg = new ModMetadataEditDialog(mod);
      if (dlg.ShowDialog(this) != DialogResult.OK)
        return null;
      return new ModMetadataResult(dlg.ResultVersion, dlg.ResultCategory, dlg.ResultDescription, dlg.ResultAuthor);
    }

    #endregion
  }
}
