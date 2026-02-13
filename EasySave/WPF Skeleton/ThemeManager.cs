using System.Windows;
using System.Windows.Media;

namespace WpfSkeleton
{
    public static class ThemeManager
    {
        public static void ApplyTheme(string theme)
        {
            var app = Application.Current;
            var resources = app.Resources;

            if (theme == "Dark")
            {
                // ── Backgrounds ──
                resources["BackgroundBrush"] = new SolidColorBrush(Color.FromRgb(15, 17, 23));     // #0F1117
                resources["SurfaceBrush"] = new SolidColorBrush(Color.FromRgb(22, 25, 34));     // #161922
                resources["NavbarBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(11, 13, 19));     // #0B0D13
                resources["ConsoleBrush"] = new SolidColorBrush(Color.FromRgb(10, 12, 16));     // #0A0C10
                resources["CardBrush"] = new SolidColorBrush(Color.FromRgb(26, 29, 39));     // #1A1D27
                resources["InputBrush"] = new SolidColorBrush(Color.FromRgb(30, 34, 48));     // #1E2230

                // ── Foregrounds ──
                resources["ForegroundBrush"] = new SolidColorBrush(Color.FromRgb(232, 234, 237));  // #E8EAED
                resources["ForegroundSecondaryBrush"] = new SolidColorBrush(Color.FromRgb(156, 163, 175));  // #9CA3AF
                resources["ConsoleForegroundBrush"] = new SolidColorBrush(Color.FromRgb(74, 222, 128));   // #4ADE80

                // ── Borders ──
                resources["BorderBrush"] = new SolidColorBrush(Color.FromRgb(42, 46, 58));     // #2A2E3A
                resources["BorderSubtleBrush"] = new SolidColorBrush(Color.FromRgb(31, 35, 48));     // #1F2330

                // ── Accents ──
                resources["AccentBrush"] = new SolidColorBrush(Color.FromRgb(52, 211, 153));   // #34D399
                resources["AccentHoverBrush"] = new SolidColorBrush(Color.FromRgb(110, 231, 183));  // #6EE7B7
                resources["AccentSubtleBrush"] = new SolidColorBrush(Color.FromRgb(26, 58, 46));     // #1A3A2E

                // ── Buttons ──
                resources["ButtonBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(30, 34, 48));     // #1E2230
                resources["ButtonHoverBrush"] = new SolidColorBrush(Color.FromRgb(40, 45, 62));     // #282D3E
                resources["ButtonPressedBrush"] = new SolidColorBrush(Color.FromRgb(50, 56, 72));     // #323848

                // ── Status ──
                resources["SuccessBrush"] = new SolidColorBrush(Color.FromRgb(52, 211, 153));   // #34D399
                resources["WarningBrush"] = new SolidColorBrush(Color.FromRgb(251, 191, 36));   // #FBBF24
                resources["ErrorBrush"] = new SolidColorBrush(Color.FromRgb(248, 113, 113));  // #F87171
                resources["InfoBrush"] = new SolidColorBrush(Color.FromRgb(96, 165, 250));   // #60A5FA

                // ── Nav ──
                resources["NavActiveBrush"] = new SolidColorBrush(Color.FromRgb(20, 24, 32));     // #141820
                resources["NavHoverBrush"] = new SolidColorBrush(Color.FromRgb(17, 21, 32));     // #111520
            }
            else // Light
            {
                // ── Backgrounds ──
                resources["BackgroundBrush"] = new SolidColorBrush(Color.FromRgb(248, 249, 251));  // #F8F9FB
                resources["SurfaceBrush"] = new SolidColorBrush(Colors.White);
                resources["NavbarBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(243, 244, 246));  // #F3F4F6
                resources["ConsoleBrush"] = new SolidColorBrush(Color.FromRgb(17, 24, 39));     // #111827 (stays dark)
                resources["CardBrush"] = new SolidColorBrush(Colors.White);
                resources["InputBrush"] = new SolidColorBrush(Colors.White);

                // ── Foregrounds ──
                resources["ForegroundBrush"] = new SolidColorBrush(Color.FromRgb(17, 24, 39));     // #111827
                resources["ForegroundSecondaryBrush"] = new SolidColorBrush(Color.FromRgb(107, 114, 128));  // #6B7280
                resources["ConsoleForegroundBrush"] = new SolidColorBrush(Color.FromRgb(74, 222, 128));   // #4ADE80

                // ── Borders ──
                resources["BorderBrush"] = new SolidColorBrush(Color.FromRgb(229, 231, 235));  // #E5E7EB
                resources["BorderSubtleBrush"] = new SolidColorBrush(Color.FromRgb(243, 244, 246));  // #F3F4F6

                // ── Accents ──
                resources["AccentBrush"] = new SolidColorBrush(Color.FromRgb(16, 185, 129));   // #10B981
                resources["AccentHoverBrush"] = new SolidColorBrush(Color.FromRgb(52, 211, 153));   // #34D399
                resources["AccentSubtleBrush"] = new SolidColorBrush(Color.FromRgb(209, 250, 229));  // #D1FAE5

                // ── Buttons ──
                resources["ButtonBackgroundBrush"] = new SolidColorBrush(Colors.White);
                resources["ButtonHoverBrush"] = new SolidColorBrush(Color.FromRgb(243, 244, 246));  // #F3F4F6
                resources["ButtonPressedBrush"] = new SolidColorBrush(Color.FromRgb(229, 231, 235));  // #E5E7EB

                // ── Status ──
                resources["SuccessBrush"] = new SolidColorBrush(Color.FromRgb(16, 185, 129));   // #10B981
                resources["WarningBrush"] = new SolidColorBrush(Color.FromRgb(245, 158, 11));   // #F59E0B
                resources["ErrorBrush"] = new SolidColorBrush(Color.FromRgb(239, 68, 68));    // #EF4444
                resources["InfoBrush"] = new SolidColorBrush(Color.FromRgb(59, 130, 246));   // #3B82F6

                // ── Nav ──
                resources["NavActiveBrush"] = new SolidColorBrush(Colors.White);
                resources["NavHoverBrush"] = new SolidColorBrush(Color.FromRgb(249, 250, 251));  // #F9FAFB
            }
        }
    }
}
