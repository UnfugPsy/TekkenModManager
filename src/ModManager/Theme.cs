using System.Drawing;

namespace ModManager
{
    public static class Theme
    {
        public static readonly Color Background = Color.FromArgb(16, 16, 22);
        public static readonly Color PanelDark = Color.FromArgb(18, 18, 24);
        public static readonly Color InstructionsPanelDark = Color.FromArgb(24, 24, 32);
        public static readonly Color ListViewDark = Color.FromArgb(20, 20, 28);
        public static readonly Color InputBackground = Color.FromArgb(32, 32, 40);
        public static readonly Color CardBackground = Color.FromArgb(22, 22, 30);
        public static readonly Color CardHover = Color.FromArgb(32, 32, 40);
        
        public static readonly Color ButtonBackground = Color.FromArgb(32, 32, 40);
        public static readonly Color ButtonHover = Color.FromArgb(60, 60, 80);
        
        public static readonly Color BorderColor = Color.FromArgb(60, 60, 80);
        public static readonly Color BorderAccent = Color.FromArgb(0, 255, 255);
        
        public static readonly Color AccentCyan = Color.FromArgb(0, 255, 255);
        public static readonly Color AccentPink = Color.FromArgb(255, 0, 102);
        public static readonly Color AccentGreen = Color.FromArgb(0, 255, 127);
        public static readonly Color AccentOrange = Color.FromArgb(255, 165, 0);
        
        public static readonly Color TextPrimary = Color.FromArgb(200, 200, 255);
        public static readonly Color TextSecondary = Color.FromArgb(180, 180, 255);
        public static readonly Color TextAccent = Color.FromArgb(0, 255, 255);
        public static readonly Color TextMuted = Color.FromArgb(120, 120, 160);
        
        public static readonly Font FontRegular = new Font("Segoe UI", 9F, FontStyle.Regular);
        public static readonly Font FontBold = new Font("Segoe UI", 9F, FontStyle.Bold);
        public static readonly Font FontSmall = new Font("Segoe UI", 8F, FontStyle.Regular);
        public static readonly Font FontSmallBold = new Font("Segoe UI", 7.5F, FontStyle.Bold);
        public static readonly Font FontMedium = new Font("Segoe UI", 10F, FontStyle.Regular);
        public static readonly Font FontLarge = new Font("Segoe UI", 12F, FontStyle.Bold);
        
        public const int ButtonHeight = 35;
        public const int ButtonMargin = 10;
        public const int BorderSize = 1;
    }
}