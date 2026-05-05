// Helpers/Settings.cs
using Plugin.Settings;
using Plugin.Settings.Abstractions;
using System;
using static AutoAssistantAppDataLibrary.DataLayer.NavigationManager;

namespace AutoAssistantAppDataLibrary.Helpers
{
    /// <summary>
    /// This is the Settings static class that can be used in your Core solution or in any
    /// of your client applications. All settings are laid out the same exact way with getters
    /// and setters. 
    /// </summary>
    public static class Settings
    {
        public static TEnum GetEnumOrDefault<TEnum>(this ISettings settings, string key, TEnum defaultValue = default(TEnum)) where TEnum : struct
        {
            // Try/catch since this used to be stored as an integer, and now we store it as a string
            string val;
            try
            {
                val = settings.GetValueOrDefault<string>(key, null);
                if (val == null)
                {
                    return defaultValue;
                }
            }
            catch
            {
                return defaultValue;
            }

            TEnum answer;
            if (Enum.TryParse<TEnum>(val, out answer))
            {
                return answer;
            }

            return defaultValue;
        }

        public static bool AddOrUpdateEnum<TEnum>(this ISettings settings, string key, TEnum value) where TEnum : struct
        {
            string val = value.ToString();
            return settings.AddOrUpdateValue(key, val);
        }

        private static ISettings AppSettings
        {
            get
            {
                return CrossSettings.Current;
            }
        }



        #region Constants

        private const string WAS_UPDATED_BY_BACKGROUND_TASK = "WasUpdatedByBackground";
        private const string LAST_LOGIN_LOCAL_ID = "LastLogin";
        private const string LAST_SELECTED_TIME_OPTION_FOR_TASK_WITH_CLASS = "LastTimeOptionTaskClass";
        private const string LAST_SELECTED_TIME_OPTION_FOR_TASK_WITHOUT_CLASS = "LastTimeOptionTaskNoClass";
        private const string LAST_SELECTED_TIME_OPTION_FOR_EVENT_WITH_CLASS = "LastTimeOptionEventClass";
        private const string LAST_SELECTED_TIME_OPTION_FOR_EVENT_WIHTOUT_CLASS = "LastTimeOptionEventNoClass";

        #endregion

        public static bool WasUpdatedByBackgroundTask
        {
            get
            {
                return AppSettings.GetValueOrDefault<bool>(WAS_UPDATED_BY_BACKGROUND_TASK, false);
            }

            set
            {
                if (value)
                    AppSettings.AddOrUpdateValue(WAS_UPDATED_BY_BACKGROUND_TASK, true);
                else
                    AppSettings.Remove(WAS_UPDATED_BY_BACKGROUND_TASK);
            }
        }

        public static Guid LastLoginLocalId
        {
            get
            {
                return AppSettings.GetValueOrDefault<Guid>(LAST_LOGIN_LOCAL_ID, Guid.Empty);
            }

            set
            {
                AppSettings.AddOrUpdateValue(LAST_LOGIN_LOCAL_ID, value);
            }
        }

        public static string LastSelectedTimeOptionForTaskWithClass
        {
            get
            {
                return AppSettings.GetValueOrDefault<string>(LAST_SELECTED_TIME_OPTION_FOR_TASK_WITH_CLASS, null);
            }

            set
            {
                AppSettings.AddOrUpdateValue(LAST_SELECTED_TIME_OPTION_FOR_TASK_WITH_CLASS, value);
            }
        }

        public static string LastSelectedTimeOptionForTaskWithoutClass
        {
            get
            {
                return AppSettings.GetValueOrDefault<string>(LAST_SELECTED_TIME_OPTION_FOR_TASK_WITHOUT_CLASS, null);
            }

            set
            {
                AppSettings.AddOrUpdateValue(LAST_SELECTED_TIME_OPTION_FOR_TASK_WITHOUT_CLASS, value);
            }
        }

        public static string LastSelectedTimeOptionForEventWithClass
        {
            get
            {
                return AppSettings.GetValueOrDefault<string>(LAST_SELECTED_TIME_OPTION_FOR_EVENT_WITH_CLASS, null);
            }

            set
            {
                AppSettings.AddOrUpdateValue(LAST_SELECTED_TIME_OPTION_FOR_EVENT_WITH_CLASS, value);
            }
        }

        public static string LastSelectedTimeOptionForEventWithoutClass
        {
            get
            {
                return AppSettings.GetValueOrDefault<string>(LAST_SELECTED_TIME_OPTION_FOR_EVENT_WIHTOUT_CLASS, null);
            }

            set
            {
                AppSettings.AddOrUpdateValue(LAST_SELECTED_TIME_OPTION_FOR_EVENT_WIHTOUT_CLASS, value);
            }
        }

        public static class NavigationManagerSettings
        {
            private const string MAIN_MENU_SELECTION = "NavManager_MainMenuSelection";
            private const string CLASS_SELECTION = "NavManager_ClassSelection";

            static NavigationManagerSettings()
            {
            }

            public static MainMenuSelections MainMenuSelection
            {
                get { return AppSettings.GetEnumOrDefault<MainMenuSelections>(MAIN_MENU_SELECTION, MainMenuSelections.Fuel); }
                set
                {
                    // Don't remember settings page
                    if (value == MainMenuSelections.Settings)
                    {
                        return;
                    }

                    AppSettings.AddOrUpdateEnum(MAIN_MENU_SELECTION, value);
                }
            }

            public static Guid ClassSelection
            {
                get { return AppSettings.GetValueOrDefault<Guid>(CLASS_SELECTION, Guid.Empty); }
                set { AppSettings.AddOrUpdateValue(CLASS_SELECTION, value); }
            }

            public static void Clear()
            {
                AppSettings.Remove(MAIN_MENU_SELECTION);
                AppSettings.Remove(CLASS_SELECTION);
            }
        }
    }
}