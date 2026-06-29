using System.Drawing;
using System.Windows.Forms;

namespace ModManager.Utils
{
    public static class HelpFormUtils
    {
        private static readonly Font HeaderFont = new Font("Segoe UI", 20F, FontStyle.Bold);
        private static readonly Font SubtitleFont = new Font("Segoe UI", 10F, FontStyle.Italic);
        private static readonly Font SectionTitleFont = new Font("Segoe UI", 14F, FontStyle.Bold);
        private static readonly Font MonospaceFont = new Font("Consolas", 10F, FontStyle.Regular);
        private static readonly Font BodyFont = new Font("Segoe UI", 10F, FontStyle.Regular);

        public static int CreateHeaderSection(Panel contentPanel, int yPos, Color headerColor)
        {
            Label headerLabel = new Label();
            headerLabel.Text = "TEKKEN 8 MOD MANAGER HELP";
            headerLabel.Font = HeaderFont;
            headerLabel.ForeColor = headerColor;
            headerLabel.BackColor = Color.Transparent;
            headerLabel.AutoSize = true;
            headerLabel.Location = new Point(20, yPos);
            contentPanel.Controls.Add(headerLabel);
            yPos += headerLabel.Height + 2;

            Label subLabel = new Label();
            subLabel.Text = "Quick guide & keyboard shortcuts";
            subLabel.Font = SubtitleFont;
            subLabel.ForeColor = Theme.AccentPink;
            subLabel.BackColor = Color.Transparent;
            subLabel.AutoSize = true;
            subLabel.Location = new Point(22, yPos);
            contentPanel.Controls.Add(subLabel);
            yPos += subLabel.Height + 10;

            Panel divider = new Panel();
            divider.BackColor = Theme.AccentCyan;
            divider.Size = new Size(690, 2);
            divider.Location = new Point(20, yPos);
            contentPanel.Controls.Add(divider);
            return yPos + 2;
        }

        public static int CreateSection(Panel contentPanel, int yPos, string title, string[] lines, Color accentColor, Color textColor)
        {
            Label titleLabel = new Label();
            titleLabel.Text = title;
            titleLabel.Font = SectionTitleFont;
            titleLabel.ForeColor = accentColor;
            titleLabel.BackColor = Color.Transparent;
            titleLabel.AutoSize = true;
            titleLabel.Location = new Point(20, yPos);
            contentPanel.Controls.Add(titleLabel);
            yPos += titleLabel.Height + 4;

            Panel underline = new Panel();
            underline.BackColor = Color.FromArgb(60, accentColor);
            underline.Size = new Size(690, 1);
            underline.Location = new Point(20, yPos);
            contentPanel.Controls.Add(underline);
            yPos += 8;

            foreach (string line in lines)
            {
                if (string.IsNullOrEmpty(line))
                {
                    yPos += 8;
                    continue;
                }

                Label lineLabel = new Label();
                lineLabel.Text = line;
                lineLabel.Font = title == "KEYBOARD SHORTCUTS" && (line.Contains("F1") || line.Contains("F5") || line.Contains("Ctrl"))
                    ? MonospaceFont
                    : BodyFont;
                lineLabel.ForeColor = textColor;
                lineLabel.BackColor = Color.Transparent;
                lineLabel.AutoSize = true;
                lineLabel.Location = new Point(40, yPos);
                contentPanel.Controls.Add(lineLabel);
                yPos += lineLabel.Height + 4;
            }
            return yPos;
        }
    }
}
