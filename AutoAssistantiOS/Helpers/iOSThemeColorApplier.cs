using System;
using System.Drawing;
using AutoAssistantAppDataLibrary.Helpers;
using InterfacesiOS.App;
using UIKit;

namespace AutoAssistantiOS.Helpers
{
    public static class iOSThemeColorApplier
    {
        /// <summary>
        /// Fires when theme colors have been updated. Views can subscribe to update their appearance.
        /// </summary>
        public static event EventHandler<ThemeColors> ThemeChanged;

        /// <summary>
        /// The currently applied theme colors, initialized to defaults.
        /// </summary>
        public static ThemeColors Current { get; private set; } =
            ThemeColorGenerator.Generate(ThemeColorGenerator.DefaultPrimary);

        public static void Apply(ThemeColors colors)
        {
            try
            {
                Current = colors;

                // Update global window tint color (accent)
                NativeiOSApplication.Current.Window.TintColor = ToUIColor(colors.DarkAccent);

                // Update snackbar button text color
                InterfacesiOS.Views.BareSnackbarPresenter.ButtonTextColor = ToUIColor(colors.Accent);

                ThemeChanged?.Invoke(null, colors);
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// Converts a System.Drawing.Color to UIKit.UIColor.
        /// </summary>
        public static UIColor ToUIColor(Color color)
        {
            return UIColor.FromRGB(color.R, color.G, color.B);
        }
    }
}
