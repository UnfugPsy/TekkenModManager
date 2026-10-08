using ModManager.Utils;

namespace ModManager.Views
{
    public partial class CreateProfileDialog : Form
    {
        private TextBox _nameTextBox;
        private TextBox _descriptionTextBox;
        private Button _okButton;
        private Button _cancelButton;
        private readonly string _dialogTitle;
        private readonly string _confirmButtonText;

        public string ProfileName => _nameTextBox.Text.Trim();
        public string ProfileDescription => _descriptionTextBox.Text.Trim();

        public CreateProfileDialog(string defaultName = "", string defaultDescription = "", string title = "Create Profile", string confirmText = "Create")
        {
            _dialogTitle = title;
            _confirmButtonText = confirmText;
            InitializeComponent();
            
            _nameTextBox.Text = defaultName;
            _descriptionTextBox.Text = defaultDescription;
            
            if (!string.IsNullOrEmpty(defaultName))
            {
                _nameTextBox.SelectAll();
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form properties
            this.Text = _dialogTitle;
            this.Size = new Size(400, 250);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowIcon = false;

            // Apply theme
            ThemeUtils.ApplyCyberpunkTheme(this);

            // Main panel
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            // Name label
            var nameLabel = new Label
            {
                Text = "Profile Name:",
                Location = new Point(0, 10),
                Size = new Size(100, 20),
                Font = Theme.FontRegular,
                ForeColor = Theme.TextSecondary
            };

            // Name textbox
            _nameTextBox = new TextBox
            {
                Location = new Point(0, 35),
                Size = new Size(340, 25),
                Font = Theme.FontRegular,
                BackColor = Theme.InputBackground,
                ForeColor = Theme.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle
            };
            _nameTextBox.TextChanged += NameTextBox_TextChanged;

            // Description label
            var descLabel = new Label
            {
                Text = "Description (optional):",
                Location = new Point(0, 70),
                Size = new Size(150, 20),
                Font = Theme.FontRegular,
                ForeColor = Theme.TextSecondary
            };

            // Description textbox
            _descriptionTextBox = new TextBox
            {
                Location = new Point(0, 95),
                Size = new Size(340, 60),
                Font = Theme.FontRegular,
                BackColor = Theme.InputBackground,
                ForeColor = Theme.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };

            // Buttons panel
            var buttonsPanel = new FlowLayoutPanel
            {
                Location = new Point(200, 170),
                Size = new Size(140, 35),
                FlowDirection = FlowDirection.LeftToRight,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };

            _okButton = new Button
            {
                Text = _confirmButtonText,
                Size = new Size(65, 28),
                BackColor = Theme.ButtonBackground,
                ForeColor = Theme.AccentCyan,
                FlatStyle = FlatStyle.Flat,
                Font = Theme.FontRegular,
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.OK,
                Enabled = false
            };
            _okButton.Click += OkButton_Click;

            _cancelButton = new Button
            {
                Text = "Cancel",
                Size = new Size(65, 28),
                BackColor = Theme.ButtonBackground,
                ForeColor = Theme.TextSecondary,
                FlatStyle = FlatStyle.Flat,
                Font = Theme.FontRegular,
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.Cancel,
                Margin = new Padding(5, 0, 0, 0)
            };

            buttonsPanel.Controls.Add(_okButton);
            buttonsPanel.Controls.Add(_cancelButton);

            mainPanel.Controls.AddRange(new Control[] {
                nameLabel, _nameTextBox, descLabel, _descriptionTextBox, buttonsPanel
            });

            this.Controls.Add(mainPanel);

            this.AcceptButton = _okButton;
            this.CancelButton = _cancelButton;

            this.ResumeLayout(false);
        }

        private void NameTextBox_TextChanged(object sender, EventArgs e)
        {
            _okButton.Enabled = !string.IsNullOrWhiteSpace(_nameTextBox.Text);
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_nameTextBox.Text))
            {
                MessageBox.Show("Please enter a profile name.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _nameTextBox.Focus();
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}