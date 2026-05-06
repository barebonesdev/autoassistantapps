using AutoAssistantAppDataLibrary;
using AutoAssistantAppDataLibrary.App;
using AutoAssistantAppDataLibrary.Extensions;
using AutoAssistantAppDataLibrary.ViewModels;
using AutoAssistantAppDataLibrary.ViewModels.MainWindow;
using AutoAssistantAppDataLibrary.Windows;
using AutoAssistantiOS.App;
using AutoAssistantiOS.Controllers;
using AutoAssistantiOS.Extensions;
using AutoAssistantiOS.Helpers;
using BareMvvm.Core.App;
using BareMvvm.Core.ViewModels;
using InterfacesiOS.App;
using InterfacesiOS.Helpers;
using InterfacesiOS.ViewModelPresenters;
using InterfacesiOS.Windows;
using Vx.Extensions;

namespace AutoAssistantiOS
{
    public class AppDelegate : NativeiOSApplication
    {
        public override Type GetPortableAppType()
        {
            return typeof(AutoAssistantiOSApp);
        }

        public AppDelegate()
        {
            string versionName = NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString") as NSString;
            Variables.VERSION = Version.Parse(versionName);
        }

        public override Dictionary<Type, Type> GetGenericViewModelToViewMappings()
        {
            return new Dictionary<Type, Type>
            {
                { typeof(PopupComponentViewModel), typeof(PopupComponentViewController) },
                { typeof(PagedViewModelWithPopups), typeof(PagedViewModelWithPopupsPresenter) },
                { typeof(PagedViewModel), typeof(PagedViewModelPresenter) },
                { typeof(BaseViewModel), typeof(BaseViewModelController) }
            };
        }

        public override Dictionary<Type, Type> GetViewModelToViewMappings()
        {
            return new Dictionary<Type, Type>();
        }

        public override void OnActivated(UIApplication application)
        {
            base.OnActivated(application);

            try
            {
                foreach (var window in AutoAssistantApp.Current.Windows.OfType<MainAppWindow>())
                {
                    var dontWait = window.GetViewModel().HandleBeingReturnedTo();
                }
            }

            catch { }
        }

        public override void DidEnterBackground(UIApplication application)
        {
            base.DidEnterBackground(application);

            try
            {
                foreach (var window in AutoAssistantApp.Current.Windows.OfType<MainAppWindow>())
                {
                    window.GetViewModel().HandleBeingLeft();
                }
            }

            catch { }

            try
            {
                //TelemetryExtension.Current?.SuspendingApp();
            }
            catch { }
        }

        public override void WillTerminate(UIApplication application)
        {
            base.WillTerminate(application);

            try
            {
                //TelemetryExtension.Current?.SuspendingApp();
            }
            catch { }
        }

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            _hasActivatedWindow = false;

            TelemetryExtension.Current = new iOSTelemetryExtension();
            BrowserExtension.Current = new iOSBrowserExtension();
            EmailExtension.Current = new iOSEmailExtension();

            bool result = base.FinishedLaunching(application, launchOptions);

            return result;
        }

        [Export("application:configurationForConnectingSceneSession:options:")]
        public UISceneConfiguration GetConfiguration(UIApplication application, UISceneSession connectingSceneSession, UISceneConnectionOptions options)
        {
            return new UISceneConfiguration("Default Configuration", connectingSceneSession.Role);
        }

        private enum ShortcutAction
        {
        }

        public static bool _hasActivatedWindow;
        public static Func<MainWindowViewModel, Task> _handleLaunchAction;
    }
}
