using ModManager.Models;
using ModManager.Utils;

namespace ModManager.Views
{
    public partial class SelectModRootDialog : Form
    {
        private ComboBox _rootComboBox;
        private Button _okButton;
        private Button _cancelButton;

        public ModRootKind SelectedKind =>
            _rootComboBox.SelectedItem is ModRootItem item ? item.Kind : ModRootKind.Standard;

        public SelectModRootDialog(string modName, ModRootKind detectedKind)
        {
            InitializeComponent(modName, detectedKind);
        }

        private void InitializeComponent(string modName, ModRootKind detectedKind)
        {
            this.SuspendLayout();

            this.Text = "Choose Mod Folder";
            this.Size = new Size(420, 210);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowIcon = false;

            ThemeUtils.ApplyCyberpunkTheme(this);

            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            var infoLabel = new Label
            {
                Text = $"Where should '{modName}' be installed?",
                Location = new Point(0, 10),
                Size = new Size(360, 20),
                Font = Theme.FontRegular,
                ForeColor = Theme.TextSecondary
            };

            var rootLabel = new Label
            {
                Text = "Mod folder:",
                Location = new Point(0, 45),
                Size = new Size(100, 20),
                Font = Theme.FontRegular,
                ForeColor = Theme.TextSecondary
            };

            _rootComboBox = new ComboBox
            {
                Location = new Point(0, 70),
                Size = new Size(360, 25),
                Font = Theme.FontRegular,
                BackColor = Theme.InputBackground,
                ForeColor = Theme.TextPrimary,
                FlatStyle = FlatStyle.Flat,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            foreach (ModRootKind kind in Enum.GetValues(typeof(ModRootKind)))
            {
                int index = _rootComboBox.Items.Add(new ModRootItem(kind));
                if (kind == detectedKind)
                    _rootComboBox.SelectedIndex = index;
            }
            if (_rootComboBox.SelectedIndex < 0 && _rootComboBox.Items.Count > 0)
                _rootComboBox.SelectedIndex = 0;

            var buttonsPanel = new FlowLayoutPanel
            {
                Location = new Point(220, 120),
                Size = new Size(140, 35),
                FlowDirection = FlowDirection.LeftToRight,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };

            _okButton = new Button
            {
                Text = "Install",
                Size = new Size(65, 28),
                BackColor = Theme.ButtonBackground,
                ForeColor = Theme.AccentCyan,
                FlatStyle = FlatStyle.Flat,
                Font = Theme.FontRegular,
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.OK
            };

            _cancelButton = new Button
            {
                Text = "Cancel",
                Size = new Size(65, 28),
                BackColor = Theme.ButtonBackground,
                ForeColor = Theme.TextSecondary,
                FlatStyle = FlatStyle.Flat,
                Font = Theme.FontRegular,
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.Cancel
            };

            buttonsPanel.Controls.Add(_okButton);
            buttonsPanel.Controls.Add(_cancelButton);

            mainPanel.Controls.Add(infoLabel);
            mainPanel.Controls.Add(rootLabel);
            mainPanel.Controls.Add(_rootComboBox);
            mainPanel.Controls.Add(buttonsPanel);

            this.Controls.Add(mainPanel);
            this.AcceptButton = _okButton;
            this.CancelButton = _cancelButton;

            this.ResumeLayout(false);
        }

        private sealed class ModRootItem
        {
            public ModRootKind Kind { get; }
            public ModRootItem(ModRootKind kind) => Kind = kind;
            public override string ToString() => $"{ModRoots.DisplayName(Kind)} ({DescribeKind(Kind)})";

            private static string DescribeKind(ModRootKind kind) => kind switch
            {
                ModRootKind.Standard => "standard pak mods",
                ModRootKind.Legacy => "legacy pak mods",
                ModRootKind.Logic => "UE4SS script mods",
                _ => string.Empty
            };
        }
    }
}
