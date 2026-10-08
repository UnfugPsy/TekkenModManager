using ModManager.Models;
using ModManager.Configuration;
using ModManager.Views;

namespace ModManager.Controls
{
    public class ModViewManager : UserControl
    {
        private Panel _headerPanel;
        private Label _titleLabel;
        private TextBox _searchBox;
        private DataGridView _dataGridView;
        private Label _conflictBanner;

        private List<ModInfo> _currentMods = new();
        private List<ModInfo> _filteredMods = new();
        private HashSet<string> _conflictingNames = new(StringComparer.OrdinalIgnoreCase);
        private bool _isLoading = false;

        public event EventHandler<ModToggleEventArgs> ModToggled;
        public event EventHandler<ModDeleteEventArgs> ModDeleteRequested;
        public event EventHandler<ModEditEventArgs> ModEditRequested;
        public event EventHandler<ModInfo> ModSelected;
        
        public ModViewManager()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            BackColor = Theme.Background;

            _headerPanel = new Panel
            {
                Height = 45,
                Dock = DockStyle.Top,
                BackColor = Theme.PanelDark,
                Padding = new Padding(15, 0, 15, 0)
            };

            _titleLabel = new Label
            {
                Text = "MOD MANAGER",
                Dock = DockStyle.Left,
                Font = Theme.FontLarge,
                ForeColor = Theme.AccentCyan,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = false,
                Width = 200
            };

            _searchBox = new TextBox
            {
                Dock = DockStyle.Right,
                Width = 220,
                Font = Theme.FontRegular,
                BackColor = Theme.InputBackground,
                ForeColor = Theme.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle,
                PlaceholderText = "Search mods..."
            };
            _searchBox.TextChanged += OnSearchTextChanged;

            _headerPanel.Controls.Add(_titleLabel);
            _headerPanel.Controls.Add(_searchBox);

            _conflictBanner = new Label
            {
                Dock = DockStyle.Top,
                Height = 0,
                BackColor = Color.FromArgb(80, 20, 0),
                ForeColor = Color.FromArgb(255, 160, 60),
                Font = Theme.FontBold,
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = false
            };

            CreateDataGridView();

            Controls.AddRange(new Control[] { _dataGridView, _conflictBanner, _headerPanel });
        }

        private void CreateDataGridView()
        {
            _dataGridView = new DataGridView();
            _dataGridView.SuspendLayout();

            _dataGridView.Dock = DockStyle.Fill;
            _dataGridView.BackgroundColor = Theme.Background;
            _dataGridView.ForeColor = Theme.TextPrimary;
            _dataGridView.GridColor = Color.FromArgb(32, 32, 42);
            _dataGridView.AllowUserToAddRows = false;
            _dataGridView.AllowUserToDeleteRows = false;
            _dataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _dataGridView.AutoGenerateColumns = false;
            _dataGridView.RowHeadersVisible = false;
            _dataGridView.ColumnHeadersVisible = true;
            _dataGridView.Font = Theme.FontRegular;
            _dataGridView.BorderStyle = BorderStyle.None;
            _dataGridView.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            _dataGridView.EnableHeadersVisualStyles = false;
            _dataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            _dataGridView.ColumnHeadersHeight = 42;
            _dataGridView.RowTemplate.Height = 38;
            _dataGridView.ScrollBars = ScrollBars.Both;
            _dataGridView.AllowUserToResizeColumns = true;
            _dataGridView.AllowUserToResizeRows = false;
            _dataGridView.MultiSelect = false;
            _dataGridView.ReadOnly = false;

            _dataGridView.DefaultCellStyle.BackColor = Theme.ListViewDark;
            _dataGridView.DefaultCellStyle.ForeColor = Theme.TextPrimary;
            _dataGridView.DefaultCellStyle.SelectionBackColor = Color.FromArgb(40, 40, 56);
            _dataGridView.DefaultCellStyle.SelectionForeColor = Theme.AccentCyan;
            _dataGridView.DefaultCellStyle.Font = Theme.FontRegular;
            _dataGridView.DefaultCellStyle.Padding = new Padding(8, 4, 8, 4);

            _dataGridView.ColumnHeadersDefaultCellStyle.BackColor = Theme.PanelDark;
            _dataGridView.ColumnHeadersDefaultCellStyle.ForeColor = Theme.AccentCyan;
            _dataGridView.ColumnHeadersDefaultCellStyle.Font = Theme.FontBold;
            _dataGridView.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            _dataGridView.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 8, 0);
            _dataGridView.ColumnHeadersDefaultCellStyle.SelectionBackColor = Theme.PanelDark;
            _dataGridView.ColumnHeadersDefaultCellStyle.SelectionForeColor = Theme.AccentCyan;

            _dataGridView.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(24, 24, 32);
            _dataGridView.AlternatingRowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(40, 40, 56);
            _dataGridView.AlternatingRowsDefaultCellStyle.SelectionForeColor = Theme.AccentCyan;

            _dataGridView.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewCheckBoxColumn
                {
                    Name = "Enabled", HeaderText = "ACTIVE", Width = 70,
                    ReadOnly = false, Resizable = DataGridViewTriState.False,
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Alignment = DataGridViewContentAlignment.MiddleCenter,
                        NullValue = false
                    }
                },
                new DataGridViewTextBoxColumn { Name = "Name",     HeaderText = "MOD NAME", Width = 260, ReadOnly = true, Resizable = DataGridViewTriState.True },
                new DataGridViewTextBoxColumn { Name = "Status",   HeaderText = "STATUS",   Width = 90,  ReadOnly = true, Resizable = DataGridViewTriState.False, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter, Font = Theme.FontSmallBold } },
                new DataGridViewTextBoxColumn { Name = "Version",  HeaderText = "VERSION",  Width = 80,  ReadOnly = true, Resizable = DataGridViewTriState.True, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } },
                new DataGridViewTextBoxColumn { Name = "Category", HeaderText = "CATEGORY", Width = 110, ReadOnly = true, Resizable = DataGridViewTriState.True },
                new DataGridViewTextBoxColumn { Name = "Type",     HeaderText = "TYPE",     Width = 90,  ReadOnly = true, Resizable = DataGridViewTriState.True, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } },
                new DataGridViewTextBoxColumn { Name = "Size",     HeaderText = "SIZE",     Width = 90,  ReadOnly = true, Resizable = DataGridViewTriState.True, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight } },
                new DataGridViewButtonColumn  { Name = "Actions",  HeaderText = "",         Width = 70,  ReadOnly = false, Resizable = DataGridViewTriState.False, Text = "\uD83D\uDDD1", UseColumnTextForButtonValue = true, FlatStyle = FlatStyle.Flat,
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Alignment = DataGridViewContentAlignment.MiddleCenter,
                        BackColor = Theme.ButtonBackground,
                        ForeColor = Theme.AccentPink,
                        SelectionBackColor = Theme.ButtonBackground,
                        SelectionForeColor = Theme.AccentPink,
                        Font = Theme.FontMedium
                    } }
            });

            _dataGridView.CellValueChanged += OnDataGridCellValueChanged;
            _dataGridView.CellContentClick += OnDataGridCellContentClick;
            _dataGridView.CellDoubleClick += OnDataGridCellDoubleClick;
            _dataGridView.SelectionChanged += OnDataGridSelectionChanged;
            _dataGridView.CellFormatting += OnDataGridCellFormatting;
            _dataGridView.CurrentCellDirtyStateChanged += OnCurrentCellDirtyStateChanged;
            _dataGridView.CellClick += OnDataGridCellClick;
            _dataGridView.CellMouseEnter += OnDataGridCellMouseEnter;
            _dataGridView.CellMouseLeave += OnDataGridCellMouseLeave;
            _dataGridView.CellMouseDown += OnDataGridCellMouseDown;
            _dataGridView.Columns["Name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            _dataGridView.ResumeLayout();
        }
        public void SetConflicts(HashSet<string> conflictingModNames)
        {
            _conflictingNames = conflictingModNames ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            bool any = _conflictingNames.Count > 0;
            _conflictBanner.Visible = any;
            _conflictBanner.Height  = any ? 24 : 0;
            if (any)
                _conflictBanner.Text = $"\u26a0  {_conflictingNames.Count} mod(s) have conflicting .pak filenames — they may override each other";

            if (!_isLoading)
                _dataGridView.InvalidateColumn(_dataGridView.Columns["Name"]?.Index ?? 1);
        }

        public ModInfo? SelectedMod => _dataGridView.CurrentRow?.Tag as ModInfo;

        public void LoadMods(List<ModInfo> mods)
        {
            _currentMods = mods ?? new List<ModInfo>();
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            string term = _searchBox?.Text?.Trim() ?? "";
            _filteredMods = string.IsNullOrEmpty(term)
                ? _currentMods.ToList()
                : _currentMods.Where(m => m.Name.Contains(term, StringComparison.OrdinalIgnoreCase)
                                       || m.Category.Contains(term, StringComparison.OrdinalIgnoreCase)
                                       || ModRoots.DisplayName(m.RootKind).Contains(term, StringComparison.OrdinalIgnoreCase)
                                       || m.Author.Contains(term, StringComparison.OrdinalIgnoreCase)).ToList();
            LoadDataGrid();
        }

        private void OnSearchTextChanged(object sender, EventArgs e)
        {
            ApplyFilter();
        }

        private void LoadDataGrid()
        {
            if (_dataGridView == null) return;

            _isLoading = true;
            try
            {
                if (TryUpdateRowsInPlace())
                    return;

                _dataGridView.SuspendLayout();
                _dataGridView.Rows.Clear();

                foreach (var mod in _filteredMods)
                {
                    var row = _dataGridView.Rows[_dataGridView.Rows.Add()];
                    row.Cells["Enabled"].Value  = mod.IsEnabled;
                    row.Cells["Name"].Value     = mod.Name;
                    row.Cells["Status"].Value   = mod.IsEnabled ? "ACTIVE" : "INACTIVE";
                    row.Cells["Version"].Value  = mod.Version;
                    row.Cells["Category"].Value = mod.Category;
                    row.Cells["Type"].Value     = ModRoots.DisplayName(mod.RootKind);
                    row.Cells["Size"].Value     = mod.FormattedSize;
                    row.Cells["Name"].ToolTipText = BuildModTooltip(mod);
                    row.Cells["Actions"].ToolTipText = "Delete mod";
                    row.Tag = mod;
                }

                _dataGridView.ClearSelection();
                _dataGridView.ResumeLayout();
            }
            finally
            {
                _isLoading = false;
            }
        }

        public void RefreshView()
        {
            ApplyFilter();
        }

        private bool TryUpdateRowsInPlace()
        {
            if (_dataGridView.RowCount != _filteredMods.Count)
                return false;

            for (int i = 0; i < _filteredMods.Count; i++)
            {
                if (!(_dataGridView.Rows[i].Tag is ModInfo existing)
                    || !string.Equals(existing.Key, _filteredMods[i].Key, StringComparison.Ordinal))
                    return false;
            }

            for (int i = 0; i < _filteredMods.Count; i++)
            {
                var mod = _filteredMods[i];
                var row = _dataGridView.Rows[i];
                row.Cells["Enabled"].Value  = mod.IsEnabled;
                row.Cells["Status"].Value   = mod.IsEnabled ? "ACTIVE" : "INACTIVE";
                row.Cells["Version"].Value  = mod.Version;
                row.Cells["Category"].Value = mod.Category;
                row.Cells["Type"].Value     = ModRoots.DisplayName(mod.RootKind);
                row.Cells["Size"].Value     = mod.FormattedSize;
                row.Cells["Name"].ToolTipText = BuildModTooltip(mod);
                row.Tag = mod;
            }
            return true;
        }

        private static string BuildModTooltip(ModInfo mod)
        {
            var lines = new List<string>
            {
                mod.Name,
                $"Author: {mod.Author}",
                $"Added: {mod.DateAdded:yyyy-MM-dd}"
            };
            if (!string.IsNullOrWhiteSpace(mod.Description))
                lines.Add($"\n{mod.Description}");
            return string.Join("\n", lines);
        }

        private void OnCurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (_dataGridView.IsCurrentCellDirty &&
                _dataGridView.CurrentCell is DataGridViewCheckBoxCell &&
                _dataGridView.CurrentCell.ColumnIndex == 0)
            {
                _dataGridView.CommitEdit(DataGridViewDataErrorContexts.Commit);
                _dataGridView.EndEdit();
                _dataGridView.InvalidateCell(_dataGridView.CurrentCell);
            }
        }

        private void OnDataGridCellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (_isLoading) return;

            if (e.RowIndex >= 0 && e.ColumnIndex == 0)
            {
                var row = _dataGridView.Rows[e.RowIndex];
                if (row.Tag is ModInfo mod)
                {
                    var isEnabled = (bool)(_dataGridView.Rows[e.RowIndex].Cells["Enabled"].Value ?? false);
                    row.Cells["Status"].Value = isEnabled ? "ACTIVE" : "INACTIVE";
                    _dataGridView.InvalidateCell(_dataGridView.Columns["Status"].Index, e.RowIndex);
                    _dataGridView.BeginInvoke(new Action(() =>
                        ModToggled?.Invoke(this, new ModToggleEventArgs(mod.Name, isEnabled, mod.Key))));
                }
            }
        }

        private void OnDataGridCellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var row = _dataGridView.Rows[e.RowIndex];
            var columnName = _dataGridView.Columns[e.ColumnIndex].Name;

            if (columnName == "Enabled") return;

            if (columnName == "Actions" && row.Tag is ModInfo actionMod)
            {
                ModDeleteRequested?.Invoke(this, new ModDeleteEventArgs(actionMod.Name, actionMod.Key));
            }
        }

        private void OnDataGridCellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var columnName = _dataGridView.Columns[e.ColumnIndex].Name;
            if (columnName == "Enabled" || columnName == "Actions") return;

            if (_dataGridView.Rows[e.RowIndex].Tag is ModInfo mod)
                ModEditRequested?.Invoke(this, new ModEditEventArgs(mod));
        }

        private void OnDataGridCellClick(object sender, DataGridViewCellEventArgs e)
        {
        }

        private void OnDataGridSelectionChanged(object sender, EventArgs e)
        {
            if (_dataGridView.CurrentRow?.Tag is ModInfo mod &&
                _dataGridView.SelectedRows.Count > 0 &&
                _dataGridView.CurrentCell?.ColumnIndex != 0)
            {
                ModSelected?.Invoke(this, mod);
            }
        }

        private void OnDataGridCellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = _dataGridView.Rows[e.RowIndex];
            if (row.Selected) return;
            row.DefaultCellStyle.BackColor = Theme.CardHover;
        }

        private void OnDataGridCellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            _dataGridView.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Empty;
        }

        private void OnDataGridCellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0 || e.Button != MouseButtons.Right) return;
            _dataGridView.ClearSelection();
            _dataGridView.Rows[e.RowIndex].Selected = true;
            _dataGridView.CurrentCell = _dataGridView.Rows[e.RowIndex].Cells["Name"];
        }

        private void OnDataGridCellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            var columnName = _dataGridView.Columns[e.ColumnIndex].Name;
            var row = _dataGridView.Rows[e.RowIndex];

            if (columnName == "Status")
            {
                var isEnabled = (bool)(row.Cells["Enabled"].Value ?? false);
                if (isEnabled)
                {
                    e.CellStyle.ForeColor = Theme.AccentGreen;
                    e.CellStyle.SelectionForeColor = Theme.AccentGreen;
                }
                else
                {
                    e.CellStyle.ForeColor = Theme.TextMuted;
                    e.CellStyle.SelectionForeColor = Theme.TextMuted;
                }
            }
            else if (columnName == "Name" && row.Tag is ModInfo mod)
            {
                if (_conflictingNames.Contains(mod.Name))
                {
                    e.CellStyle.ForeColor = Theme.AccentOrange;
                    e.CellStyle.Font = Theme.FontBold;
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _dataGridView?.Dispose();
            base.Dispose(disposing);
        }
    }
}
