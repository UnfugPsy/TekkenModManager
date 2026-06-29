using System.Drawing;
using System.Windows.Forms;

namespace ModManager.Utils
{
    public static class ThemeUtils
    {
        public static void ApplyCyberpunkTheme(Form form)
        {
            form.BackColor = Color.FromArgb(16, 16, 22);
            form.Font = new Font("Segoe UI", 9F);
            ApplyToControls(form.Controls);
        }

        private static void ApplyToControls(Control.ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                if (control is ListView listView)
                {
                    listView.BackColor = Color.FromArgb(20, 20, 28);
                    listView.ForeColor = Color.FromArgb(200, 200, 255);
                    listView.BorderStyle = BorderStyle.None;
                    listView.GridLines = false;
                }
                else if (control is Button button)
                {
                    button.FlatStyle = FlatStyle.Flat;
                    button.FlatAppearance.BorderColor = Color.FromArgb(0, 255, 255);
                    button.FlatAppearance.MouseOverBackColor = Color.FromArgb(60, 60, 80);
                    button.BackColor = Color.FromArgb(32, 32, 40);
                    button.ForeColor = Color.FromArgb(0, 255, 255);
                    button.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                    button.Cursor = Cursors.Hand;
                }
                else if (control is Label label)
                {
                    if (label.Name == "lblGameLocation")
                        label.ForeColor = Color.FromArgb(0, 255, 255);
                    else if (label.Name == "lblInstructions")
                        label.ForeColor = Color.FromArgb(180, 180, 255);
                }

                if (control.Controls.Count > 0)
                    ApplyToControls(control.Controls);
            }
        }
    }
}
