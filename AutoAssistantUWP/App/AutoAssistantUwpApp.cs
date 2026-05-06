using AutoAssistantAppDataLibrary;
using AutoAssistantAppDataLibrary.App;
using AutoAssistantAppDataLibrary.Extensions;
using AutoAssistantUWP.Extensions;
using AutoAssistantUWP.Helpers;
using BareMvvm.Core.App;
using InterfacesUWP;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;

namespace AutoAssistantUWP
{
    public class AutoAssistantUwpApp : AutoAssistantApp
    {
        public static new AutoAssistantUwpApp Current
        {
            get { return PortableApp.Current as AutoAssistantUwpApp; }
        }

        protected override async Task InitializeAsyncOverride()
        {
            Initialize();

            await base.InitializeAsyncOverride();
        }

        private static bool _initialized;
        private static void Initialize()
        {
            if (_initialized)
                return;

            _initialized = true;

            Variables.VERSION = new Version(Package.Current.Id.Version.Major, Package.Current.Id.Version.Minor, Package.Current.Id.Version.Build, Package.Current.Id.Version.Revision);

            AutoAssistantAppDataLibrary.SyncLayer.SyncExtensions.GetAppName = delegate { return "Auto Assistant for Windows 10"; };
            AutoAssistantAppDataLibrary.SyncLayer.SyncExtensions.GetPlatform = delegate
            {
                if (DeviceInfo.DeviceFamily == DeviceFamily.Mobile)
                {
                    return "Windows 10 Mobile";
                }
                else
                {
                    return "Windows 10";
                }
            };
            //LocalizationExtension.Current = new UWPLocalizationExtension();
            TelemetryExtension.Current = new UWPTelemetryExtension();
            BrowserExtension.Current = new UWPBrowserExtension();
            EmailExtension.Current = new UWPEmailExtension();
            //InAppPurchaseExtension.Current = new UWPInAppPurchaseExtension();
            //PowerPlannerAppDataLibrary.Extensions.AppointmentsExtension.Current = new UWPAppointmentsExtension();
            //PowerPlannerAppDataLibrary.Extensions.NetworkInfoExtension.Current = new UWPNetworkInfoExtension();
            //PowerPlannerAppDataLibrary.Extensions.PushExtension.Current = new UWPPushExtension();
            //PowerPlannerAppDataLibrary.Extensions.RemindersExtension.Current = new UWPRemindersExtension();
            //NetworkInfoExtension.Current = new UWPNetworkInfoExtension();
            //PowerPlannerAppDataLibrary.Extensions.TilesExtension.Current = new UWPTilesExtension();
            //DateTimeFormatterExtension.Current = new UWPDateTimeFormatterExtension();
        }
    }
}
