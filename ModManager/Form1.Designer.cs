using System.Drawing;
using System.Windows.Forms;

namespace ModManager
{
    partial class Form1
    {
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.FlowLayoutPanel buttonPanel;
        private System.Windows.Forms.Label lblGameLocation;
        private System.Windows.Forms.Button btnSetGameLocation;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnOpenModFolder;
        private System.Windows.Forms.Button btnAddModZip;
        private System.Windows.Forms.Button btnStartGame;
        private System.Windows.Forms.Label lblInstructions;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.Panel sideAccent;
        private System.Windows.Forms.Button btnHelp;
        
        private System.Windows.Forms.Panel profilePanel;
        private System.Windows.Forms.Label lblProfile;
        private System.Windows.Forms.ComboBox cmbProfiles;
        private System.Windows.Forms.Button btnCreateProfile;
        private System.Windows.Forms.Button btnManageProfiles;
        private System.Windows.Forms.Button btnSaveProfile;
    private void InitializeComponent()
    {
      components = new System.ComponentModel.Container();
      listView1 = new ListView();
      panelTop = new Panel();
      buttonPanel = new FlowLayoutPanel();
      btnSetGameLocation = new Button();
      btnRefresh = new Button();
      btnOpenModFolder = new Button();
      btnAddModZip = new Button();
      btnStartGame = new Button();
      lblGameLocation = new Label();
      lblInstructions = new Label();
      btnHelp = new Button();
      sideAccent = new Panel();
      
      profilePanel = new Panel();
      lblProfile = new Label();
      cmbProfiles = new ComboBox();
      btnCreateProfile = new Button();
      btnManageProfiles = new Button();
      btnSaveProfile = new Button();
      
      panelTop.SuspendLayout();
      buttonPanel.SuspendLayout();
      profilePanel.SuspendLayout();
      SuspendLayout();
      
      listView1.BackColor = Theme.ListViewDark;
      listView1.BorderStyle = BorderStyle.None;
      listView1.Dock = DockStyle.Fill;
      listView1.ForeColor = Theme.TextPrimary;
      listView1.Location = new Point(4, 142);
      listView1.Name = "listView1";
      listView1.OwnerDraw = true;
      listView1.Size = new Size(796, 408);
      listView1.TabIndex = 0;
      listView1.UseCompatibleStateImageBehavior = false;
      panelTop.BackColor = Theme.PanelDark;
      panelTop.Controls.Add(profilePanel);
      panelTop.Controls.Add(buttonPanel);
      panelTop.Controls.Add(lblGameLocation);
      panelTop.Dock = DockStyle.Top;
      panelTop.Location = new Point(0, 32);
      panelTop.Name = "panelTop";
      panelTop.Padding = new Padding(15, 5, 15, 5);
      panelTop.Size = new Size(800, 110);
      panelTop.TabIndex = 2;
      
      buttonPanel.BackColor = Color.Transparent;
      buttonPanel.Controls.Add(btnStartGame);
      buttonPanel.Controls.Add(btnAddModZip);
      buttonPanel.Controls.Add(btnRefresh);
      buttonPanel.Controls.Add(btnOpenModFolder);
      buttonPanel.Controls.Add(btnSetGameLocation);
      buttonPanel.Dock = DockStyle.Top;
      buttonPanel.Location = new Point(15, 5);
      buttonPanel.Name = "buttonPanel";
      buttonPanel.Padding = new Padding(10, 5, 0, 0);
      buttonPanel.Size = new Size(770, 45);
      buttonPanel.TabIndex = 0;
      
      btnSetGameLocation.Name = "btnSetGameLocation";
      btnSetGameLocation.TabIndex = 0;
      btnSetGameLocation.UseVisualStyleBackColor = false;
      btnSetGameLocation.Click += btnSetGameLocation_Click;
      
      btnRefresh.Name = "btnRefresh";
      btnRefresh.TabIndex = 1;
      btnRefresh.UseVisualStyleBackColor = false;
      btnRefresh.Click += btnRefresh_Click;
      
      btnOpenModFolder.Name = "btnOpenModFolder";
      btnOpenModFolder.TabIndex = 2;
      btnOpenModFolder.UseVisualStyleBackColor = false;
      btnOpenModFolder.Click += btnOpenModFolder_Click;
      
      btnAddModZip.Name = "btnAddModZip";
      btnAddModZip.TabIndex = 3;
      btnAddModZip.UseVisualStyleBackColor = false;
      btnAddModZip.Click += btnAddModZip_Click;
      
      btnStartGame.Name = "btnStartGame";
      btnStartGame.TabIndex = 4;
      btnStartGame.UseVisualStyleBackColor = false;
      btnStartGame.Click += btnStartGame_Click;
      
      lblGameLocation.Dock = DockStyle.Bottom;
      lblGameLocation.Font = Theme.FontMedium;
      lblGameLocation.ForeColor = Theme.TextAccent;
      lblGameLocation.Location = new Point(15, 80);
      lblGameLocation.Name = "lblGameLocation";
      lblGameLocation.Padding = new Padding(15, 0, 0, 0);
      lblGameLocation.Size = new Size(770, 25);
      lblGameLocation.TabIndex = 1;
      lblGameLocation.Text = "Game Location: Not Set";
      lblGameLocation.TextAlign = ContentAlignment.MiddleLeft;
      
      profilePanel.BackColor = Color.Transparent;
      profilePanel.Controls.Add(lblProfile);
      profilePanel.Controls.Add(cmbProfiles);
      profilePanel.Controls.Add(btnCreateProfile);
      profilePanel.Controls.Add(btnManageProfiles);
      profilePanel.Controls.Add(btnSaveProfile);
      profilePanel.Dock = DockStyle.Top;
      profilePanel.Location = new Point(15, 50);
      profilePanel.Name = "profilePanel";
      profilePanel.Size = new Size(770, 30);
      profilePanel.TabIndex = 3;
      
      lblProfile.Font = Theme.FontBold;
      lblProfile.ForeColor = Theme.AccentPink;
      lblProfile.Location = new Point(10, 5);
      lblProfile.Name = "lblProfile";
      lblProfile.Size = new Size(60, 20);
      lblProfile.TabIndex = 0;
      lblProfile.Text = "Profile:";
      lblProfile.TextAlign = ContentAlignment.MiddleLeft;
      
      cmbProfiles.BackColor = Theme.InputBackground;
      cmbProfiles.ForeColor = Theme.TextPrimary;
      cmbProfiles.Font = Theme.FontRegular;
      cmbProfiles.FormattingEnabled = true;
      cmbProfiles.Location = new Point(75, 3);
      cmbProfiles.Name = "cmbProfiles";
      cmbProfiles.Size = new Size(180, 23);
      cmbProfiles.TabIndex = 1;
      cmbProfiles.DropDownStyle = ComboBoxStyle.DropDownList;
      cmbProfiles.SelectedIndexChanged += cmbProfiles_SelectedIndexChanged;
      
      btnCreateProfile.BackColor = Theme.ButtonBackground;
      btnCreateProfile.ForeColor = Theme.AccentCyan;
      btnCreateProfile.FlatStyle = FlatStyle.Flat;
      btnCreateProfile.Font = Theme.FontSmall;
      btnCreateProfile.Location = new Point(265, 3);
      btnCreateProfile.Name = "btnCreateProfile";
      btnCreateProfile.Size = new Size(60, 23);
      btnCreateProfile.TabIndex = 2;
      btnCreateProfile.Text = "Create";
      btnCreateProfile.UseVisualStyleBackColor = false;
      btnCreateProfile.Click += btnCreateProfile_Click;
      
      btnSaveProfile.BackColor = Theme.ButtonBackground;
      btnSaveProfile.ForeColor = Theme.AccentPink;
      btnSaveProfile.FlatStyle = FlatStyle.Flat;
      btnSaveProfile.Font = Theme.FontSmall;
      btnSaveProfile.Location = new Point(335, 3);
      btnSaveProfile.Name = "btnSaveProfile";
      btnSaveProfile.Size = new Size(50, 23);
      btnSaveProfile.TabIndex = 3;
      btnSaveProfile.Text = "Save";
      btnSaveProfile.UseVisualStyleBackColor = false;
      btnSaveProfile.Click += btnSaveProfile_Click;
      
      btnManageProfiles.BackColor = Theme.ButtonBackground;
      btnManageProfiles.ForeColor = Theme.TextSecondary;
      btnManageProfiles.FlatStyle = FlatStyle.Flat;
      btnManageProfiles.Font = Theme.FontSmall;
      btnManageProfiles.Location = new Point(395, 3);
      btnManageProfiles.Name = "btnManageProfiles";
      btnManageProfiles.Size = new Size(65, 23);
      btnManageProfiles.TabIndex = 4;
      btnManageProfiles.Text = "Manage";
      btnManageProfiles.UseVisualStyleBackColor = false;
      btnManageProfiles.Click += btnManageProfiles_Click;
      
      lblInstructions.BackColor = Theme.InstructionsPanelDark;
      lblInstructions.Dock = DockStyle.Fill;
      lblInstructions.Font = Theme.FontRegular;
      lblInstructions.ForeColor = Theme.TextSecondary;
      lblInstructions.Location = new Point(0, 0);
      lblInstructions.Name = "lblInstructions";
      lblInstructions.Padding = new Padding(15, 0, 80, 0);
      lblInstructions.Size = new Size(800, 32);
      lblInstructions.TabIndex = 3;
      lblInstructions.Text = "Double-click mod rows to enable/disable them • F5: Refresh • Ctrl+G: Set Game Location • F1: Help";
      lblInstructions.TextAlign = ContentAlignment.MiddleLeft;
      
      btnHelp.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      btnHelp.Location = new Point(733, 5);
      btnHelp.Name = "btnHelp";
      btnHelp.Size = new Size(60, 22);
      btnHelp.TabIndex = 4;
      btnHelp.Text = "HELP";
      btnHelp.TextAlign = ContentAlignment.MiddleCenter;
      btnHelp.UseVisualStyleBackColor = false;
      btnHelp.Click += btnHelp_Click;
      
      sideAccent.BackColor = Theme.AccentPink;
      sideAccent.Dock = DockStyle.Left;
      sideAccent.Location = new Point(0, 142);
      sideAccent.Name = "sideAccent";
      sideAccent.Size = new Size(4, 408);
      sideAccent.TabIndex = 1;
      
      AutoScaleDimensions = new SizeF(7F, 15F);
      AutoScaleMode = AutoScaleMode.Font;
      BackColor = Theme.Background;
      ClientSize = new Size(800, 550);
      Controls.Add(btnHelp);
      Controls.Add(listView1);
      Controls.Add(sideAccent);
      Controls.Add(panelTop);
      Controls.Add(lblInstructions);
      Name = "Form1";
      Text = "Tekken Mod Manager";
      Load += MainForm_Load;
      panelTop.ResumeLayout(false);
      buttonPanel.ResumeLayout(false);
      profilePanel.ResumeLayout(false);
      ResumeLayout(false);
    }
    
    
    private System.ComponentModel.IContainer components;
  }
}
