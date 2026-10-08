using ModManager.Models;
using ModManager.Utils;

namespace ModManager.Views
{
    public sealed class ModMetadataEditDialog : Form
    {
        private TextBox _versionBox;
        private TextBox _categoryBox;
        private TextBox _authorBox;
        private TextBox _descriptionBox;

        public string ResultVersion => _versionBox.Text.Trim();
        public string ResultCategory => _categoryBox.Text.Trim();
        public string ResultAuthor => _authorBox.Text.Trim();
        public string ResultDescription => _descriptionBox.Text.Trim();

        public ModMetadataEditDialog(ModInfo mod)
        {
            InitializeComponent(mod.Name);
            _versionBox.Text = mod.Version;
            _categoryBox.Text = mod.Category;
            _authorBox.Text = mod.Author;
            _descriptionBox.Text = mod.Description;
        }

        private void InitializeComponent(string modName)
        {
            SuspendLayout();

            Text = $"Edit Metadata — {modName}";
            Size = new Size(420, 320);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;

            ThemeUtils.ApplyCyberpunkTheme(this);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16),
                ColumnCount = 2,
                RowCount = 6
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 10));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

            layout.Controls.Add(MakeLabel("Version:"), 0, 0);
            _versionBox = MakeTextBox(); layout.Controls.Add(_versionBox, 1, 0);

            layout.Controls.Add(MakeLabel("Category:"), 0, 1);
            _categoryBox = MakeTextBox(); layout.Controls.Add(_categoryBox, 1, 1);

            layout.Controls.Add(MakeLabel("Author:"), 0, 2);
            _authorBox = MakeTextBox(); layout.Controls.Add(_authorBox, 1, 2);

            layout.Controls.Add(MakeLabel("Description:"), 0, 3);
            _descriptionBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = Theme.FontRegular,
                BackColor = Theme.InputBackground,
                ForeColor = Theme.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle
            };
            layout.Controls.Add(_descriptionBox, 1, 3);

            var btnPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0)
            };
            layout.SetColumnSpan(btnPanel, 2);
            layout.Controls.Add(btnPanel, 0, 5);

            var cancelBtn = MakeButton("Cancel", DialogResult.Cancel);
            var saveBtn = MakeButton("Save", DialogResult.OK);
            btnPanel.Controls.AddRange(new Control[] { cancelBtn, saveBtn });

            AcceptButton = saveBtn;
            CancelButton = cancelBtn;

            Controls.Add(layout);
            ResumeLayout();
        }

        private static Label MakeLabel(string text) => new Label
        {
            Text = text,
            Dock = DockStyle.Fill,
            Font = Theme.FontRegular,
            ForeColor = Theme.TextSecondary,
            TextAlign = ContentAlignment.MiddleLeft
        };

        private static TextBox MakeTextBox() => new TextBox
        {
            Dock = DockStyle.Fill,
            Font = Theme.FontRegular,
            BackColor = Theme.InputBackground,
            ForeColor = Theme.TextPrimary,
            BorderStyle = BorderStyle.FixedSingle
        };

        private static Button MakeButton(string text, DialogResult result) => new Button
        {
            Text = text,
            DialogResult = result,
            Width = 75,
            Height = 28,
            BackColor = Theme.ButtonBackground,
            ForeColor = Theme.AccentCyan,
            FlatStyle = FlatStyle.Flat,
            Font = Theme.FontRegular,
            Cursor = Cursors.Hand
        };
    }
}
