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
                resources["BackgroundBrush"] = new SolidColorBrush(Color.FromRgb(30, 30, 30));
                resources["ForegroundBrush"] = new SolidColorBrush(Colors.White);
                resources["NavbarBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(45, 45, 45));
                resources["ButtonBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(60, 60, 60));
                resources["ButtonHoverBrush"] = new SolidColorBrush(Color.FromRgb(80, 80, 80));
            }
            else
            {
                resources["BackgroundBrush"] = new SolidColorBrush(Colors.White);
                resources["ForegroundBrush"] = new SolidColorBrush(Colors.Black);
                resources["NavbarBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(211, 211, 211));
                resources["ButtonBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(230, 230, 230));
                resources["ButtonHoverBrush"] = new SolidColorBrush(Color.FromRgb(200, 200, 200));
            }
        }
    }
}
