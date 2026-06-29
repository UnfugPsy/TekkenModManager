using ModManager.Models;
using ModManager.Services;
using ModManager.Utils;
using ModManager.Configuration;

namespace ModManager.Views
{
    public partial class ProfileManagerForm : Form
    {
        private readonly IProfileService _profileService;
        private ListBox _profilesListBox;
        private Button _createButton;
        private Button _deleteButton;
        private Button _duplicateButton;
        private Button _setDefaultButton;
        private Button _saveButton;
        private Button _closeButton;
        private TextBox _nameTextBox;
        private TextBox _descriptionTextBox;
        private Label _modCountLabel;
        private Label _lastUsedLabel;
        private Panel _detailsPanel;
        private ModProfile _selectedProfile;

        public ProfileManagerForm(IProfileService profileService)
        {
            _profileService = profileService ?? throw new ArgumentNullException(nameof(profileService));
            InitializeComponent();
            LoadProfiles();
            
            _profileService.ProfilesChanged += OnProfilesChanged;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Text = "Profile Manager";
            this.Size = AppConstants.UI.ProfileManagerSize;
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowIcon = false;
            this.KeyPreview = true;
            this.KeyDown += ProfileManagerForm_KeyDown;

            ThemeUtils.ApplyCyberpunkTheme(this);

            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(10)
            };
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, AppConstants.UI.ProfileListWidth));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, AppConstants.UI.ProfileDetailsWidth));

            var leftPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(5)
            };

            var profilesLabel = new Label
            {
                Text = "PROFILES",
                Dock = DockStyle.Top,
                Height = 25,
                Font = Theme.FontBold,
                ForeColor = Theme.AccentCyan,
                TextAlign = ContentAlignment.MiddleLeft
            };

            _profilesListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                BackColor = Theme.ListViewDark,
                ForeColor = Theme.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle,
                Font = Theme.FontRegular,
                ItemHeight = 20
            };
            _profilesListBox.SelectedIndexChanged += ProfilesListBox_SelectedIndexChanged;
            _profilesListBox.DrawMode = DrawMode.OwnerDrawFixed;
            _profilesListBox.DrawItem += ProfilesListBox_DrawItem;

            var buttonsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0, 5, 0, 0)
            };

            _createButton = CreateButton("Create", 60);
            _deleteButton = CreateButton("Delete", 60);
            _duplicateButton = CreateButton("Duplicate", 70);
            _setDefaultButton = CreateButton("Set Default", 80);

            _createButton.Click += CreateButton_Click;
            _deleteButton.Click += DeleteButton_Click;
            _duplicateButton.Click += DuplicateButton_Click;
            _setDefaultButton.Click += SetDefaultButton_Click;

            buttonsPanel.Controls.AddRange(new Control[] { _createButton, _deleteButton, _duplicateButton, _setDefaultButton });

            leftPanel.Controls.Add(_profilesListBox);
            leftPanel.Controls.Add(profilesLabel);
            leftPanel.Controls.Add(buttonsPanel);

            _detailsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10, 5, 5, 5)
            };

            var detailsLabel = new Label
            {
                Text = "PROFILE DETAILS",
                Dock = DockStyle.Top,
                Height = 25,
                Font = Theme.FontBold,
                ForeColor = Theme.AccentCyan,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var detailsContent = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 5, 0, 0)
            };

            var nameLabel = new Label
            {
                Text = "Name:",
                Location = new Point(0, 10),
                Size = new Size(100, 20),
                Font = Theme.FontRegular,
                ForeColor = Theme.TextSecondary
            };

            _nameTextBox = new TextBox
            {
                Location = new Point(0, 35),
                Size = new Size(250, 25),
                Font = Theme.FontRegular,
                BackColor = Theme.InputBackground,
                ForeColor = Theme.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle
            };

            var descLabel = new Label
            {
                Text = "Description:",
                Location = new Point(0, 70),
                Size = new Size(100, 20),
                Font = Theme.FontRegular,
                ForeColor = Theme.TextSecondary
            };

            _descriptionTextBox = new TextBox
            {
                Location = new Point(0, 95),
                Size = new Size(250, 60),
                Font = Theme.FontRegular,
                BackColor = Theme.InputBackground,
                ForeColor = Theme.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle,
                Multiline = true
            };

            _modCountLabel = new Label
            {
                Location = new Point(0, 165),
                Size = new Size(250, 20),
                Font = Theme.FontRegular,
                ForeColor = Theme.TextSecondary
            };

            _lastUsedLabel = new Label
            {
                Location = new Point(0, 185),
                Size = new Size(250, 20),
                Font = Theme.FontRegular,
                ForeColor = Theme.TextSecondary
            };

            _saveButton = new Button
            {
                Text = "Save Changes",
                Location = new Point(0, 220),
                Size = new Size(110, 28),
                BackColor = Theme.ButtonBackground,
                ForeColor = Theme.AccentCyan,
                FlatStyle = FlatStyle.Flat,
                Font = Theme.FontRegular,
                Cursor = Cursors.Hand,
                Enabled = false
            };
            _saveButton.Click += SaveButton_Click;

            detailsContent.Controls.AddRange(new Control[] {
                nameLabel, _nameTextBox, descLabel, _descriptionTextBox, _modCountLabel, _lastUsedLabel, _saveButton
            });

            _detailsPanel.Controls.Add(detailsContent);
            _detailsPanel.Controls.Add(detailsLabel);

            _closeButton = CreateButton("Close", 80);
            _closeButton.Location = new Point(this.Width - 100, this.Height - 60);
            _closeButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            _closeButton.Click += (s, e) => this.Close();

            mainPanel.Controls.Add(leftPanel, 0, 0);
            mainPanel.Controls.Add(_detailsPanel, 1, 0);

            this.Controls.Add(mainPanel);
            this.Controls.Add(_closeButton);

            this.ResumeLayout(false);
        }

        private Button CreateButton(string text, int width)
        {
            return new Button
            {
                Text = text,
                Size = new Size(width, 28),
                BackColor = Theme.ButtonBackground,
                ForeColor = Theme.AccentCyan,
                FlatStyle = FlatStyle.Flat,
                Font = Theme.FontRegular,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 5, 0)
            };
        }

        private void LoadProfiles()
        {
            _profilesListBox.Items.Clear();
            var profiles = _profileService.GetProfiles();
            
            foreach (var profile in profiles)
            {
                _profilesListBox.Items.Add(profile);
            }

            if (_profilesListBox.Items.Count > 0)
            {
                _profilesListBox.SelectedIndex = 0;
            }
        }

        private void ProfilesListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedProfile = _profilesListBox.SelectedItem as ModProfile;
            UpdateDetailsPanel(selectedProfile);
        }

        private void ProfilesListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            var profile = _profilesListBox.Items[e.Index] as ModProfile;
            if (profile == null) return;

            e.DrawBackground();

            var textColor = profile.IsDefault ? Theme.AccentPink : Theme.TextPrimary;
            var font = profile.IsDefault ? Theme.FontBold : Theme.FontRegular;
            
            using (var brush = new SolidBrush(textColor))
            {
                var text = profile.IsDefault ? $"{profile.Name} (Default)" : profile.Name;
                e.Graphics.DrawString(text, font, brush, e.Bounds.X + 5, e.Bounds.Y + 2);
            }

            e.DrawFocusRectangle();
        }

        private void UpdateDetailsPanel(ModProfile profile)
        {
            if (profile == null)
            {
                _selectedProfile = null;
                _nameTextBox.Clear();
                _descriptionTextBox.Clear();
                _modCountLabel.Text = "";
                _lastUsedLabel.Text = "";
                _detailsPanel.Enabled = false;
                _saveButton.Enabled = false;
                return;
            }

            _selectedProfile = profile;
            _detailsPanel.Enabled = true;
            _nameTextBox.Text = profile.Name;
            _descriptionTextBox.Text = profile.Description;
            _modCountLabel.Text = $"Enabled Mods: {profile.EnabledMods.Count}";
            _lastUsedLabel.Text = $"Last Used: {profile.LastUsed:yyyy-MM-dd HH:mm}";

            _nameTextBox.ReadOnly = profile.IsDefault;
            _deleteButton.Enabled = !profile.IsDefault;
            _saveButton.Enabled = true;
        }

        private void CreateButton_Click(object sender, EventArgs e)
        {
            var dialog = new CreateProfileDialog();
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                _profileService.CreateProfile(dialog.ProfileName, dialog.ProfileDescription);
            }
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            var selectedProfile = _profilesListBox.SelectedItem as ModProfile;
            if (selectedProfile == null || selectedProfile.IsDefault) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete the profile '{selectedProfile.Name}'?",
                "Delete Profile",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                _profileService.DeleteProfile(selectedProfile.Id);
            }
        }

        private void DuplicateButton_Click(object sender, EventArgs e)
        {
            var selectedProfile = _profilesListBox.SelectedItem as ModProfile;
            if (selectedProfile == null) return;

            var dialog = new CreateProfileDialog($"{selectedProfile.Name} Copy", selectedProfile.Description, "Duplicate Profile", "Duplicate");
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                _profileService.DuplicateProfile(selectedProfile.Id, dialog.ProfileName);
            }
        }

        private void SetDefaultButton_Click(object sender, EventArgs e)
        {
            var selectedProfile = _profilesListBox.SelectedItem as ModProfile;
            if (selectedProfile == null) return;

            _profileService.SetDefaultProfile(selectedProfile.Id);
            _profilesListBox.Invalidate();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (_selectedProfile == null) return;

            var newName = _nameTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(newName))
            {
                MessageBox.Show("Profile name cannot be empty.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _nameTextBox.Focus();
                return;
            }

            _selectedProfile.Name = newName;
            _selectedProfile.Description = _descriptionTextBox.Text.Trim();
            _profileService.UpdateProfile(_selectedProfile);
        }

        private void OnProfilesChanged(object sender, Services.ProfileEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => LoadProfiles()));
            }
            else
            {
                LoadProfiles();
            }
        }

        private void ProfileManagerForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
                e.Handled = true;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _profileService.ProfilesChanged -= OnProfilesChanged;
            }
            base.Dispose(disposing);
        }
    }
}