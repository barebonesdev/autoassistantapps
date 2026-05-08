using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Vx.Views
{
    public abstract class Theme
    {
        public static event EventHandler ThemeChanged;

        private static Theme _currentTheme;
        public static Theme Current
        {
            get
            {
#if DEBUG
                if (_currentTheme == null)
                {
                    System.Diagnostics.Debugger.Break();
                    throw new NotImplementedException("Native platform needs to initialize Theme");
                }
#endif

                return _currentTheme;
            }
            set => _currentTheme = value;
        }

        public abstract bool IsDarkTheme { get; }

        public static Color DefaultAccentColor { get; set; } = Color.FromArgb(46, 54, 109); // #2E366D

        /// <summary>
        /// Accent color for light theme so shows up on dark themes
        /// </summary>
        public static Color DefaultDarkAccentColor { get; set; } = Color.FromArgb(44, 50, 100);

        /// <summary>
        /// Background color of buttons, titlebars, etc
        /// </summary>
        public Color ChromeColor { get; set; } = Color.FromArgb(46, 54, 109);

        /// <summary>
        /// Lighter variant of <see cref="ChromeColor"/>, used for ripple/pressed states.
        /// </summary>
        public Color ChromeLightColor { get; set; } = Color.FromArgb(44, 50, 100);

        public abstract Color ForegroundColor { get; }

        public abstract Color SubtleForegroundColor { get; }

        public Color BackgroundColor => IsDarkTheme ? Color.Black : Color.White;

        public abstract Color PopupPageBackgroundColor { get; }

        public abstract Color PopupPageBackgroundAltColor { get; }

        public Color AccentColor => IsDarkTheme ? DefaultDarkAccentColor : DefaultAccentColor;

        public Color BackgroundAlt1Color => IsDarkTheme ? Color.FromArgb(51, 51, 51) : Color.FromArgb(233, 233, 233);

        public Color BackgroundAlt2Color => IsDarkTheme ? Color.FromArgb(22, 22, 22) : Color.FromArgb(245, 245, 245);

        /// <summary>
        /// A darker color than Alt1 or Alt2
        /// </summary>
        public Color BackgroundAlt3Color => IsDarkTheme ? Color.FromArgb(72, 72, 72) : Color.FromArgb(210, 210, 210);

        private static Lazy<float> _pageMargin = new Lazy<float>(() =>
        {
            if (VxPlatform.Current == Platform.Android)
            {
                return 16;
            }
            else if (VxPlatform.Current == Platform.iOS)
            {
                return 20;
            }
            else
            {
                return 24;
            }
        });

        /// <summary>
        /// Windows uses 24px for typical page margins, and then sometimes 16px margins between content? And sometimes 12.
        /// iOS seems to use 20px for typical page margins and seems to work in units of 10.
        /// Android typically works in units of 8dp, only 16dp for page margins, but otherwise straightforward with Windows?
        /// </summary>
        public float PageMargin => _pageMargin.Value;

        public virtual float CaptionFontSize => 12;

        public virtual float BodyFontSize => 14;

        public virtual float SubtitleFontSize => 20;

        public virtual float TitleFontSize => 24;

        public virtual float HeaderFontSize => 38;

        public static void NotifyThemeChanged()
        {
            ThemeChanged?.Invoke(null, EventArgs.Empty);
        }
    }
}
