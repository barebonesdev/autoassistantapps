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

        private MainAppWindow _mainAppWindow;

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

            RegisterWindow(null);

            return result;
        }


        private enum ShortcutAction
        {
        }

        public static bool _hasActivatedWindow;
        public static Func<MainWindowViewModel, Task> _handleLaunchAction;
        private async void RegisterWindow(ShortcutAction? shortcutAction)
        {
#pragma warning disable CA1422 // UIWindow(CGRect) is obsoleted on iOS 26.0, but needed for pre-scene-based lifecycle
            this.Window = new UIWindow(UIScreen.MainScreen.Bounds);
#pragma warning restore CA1422

            Window.BackgroundColor = UIColorCompat.SystemBackgroundColor;
            Window.TintColor = ColorResources.AccentColor;
            this.Window.RootViewController = UIStoryboard.FromName("LaunchScreen", null).InstantiateInitialViewController();

            this.Window.MakeKeyAndVisible();

            _mainAppWindow = new MainAppWindow();
            await PortableApp.Current.RegisterWindowAsync(_mainAppWindow, new NativeiOSAppWindow(Window));

            // Launch the app
            var mainWindowViewModel = _mainAppWindow.GetViewModel();
            if (shortcutAction != null)
            {
                //HandleShortcutAction(shortcutAction.Value);

                //// We make sure to activate the normal launch, and then later the HandleLaunch kicks in
                //if (!_hasActivatedWindow)
                //{
                //    await mainWindowViewModel.HandleNormalLaunchActivation();
                //}
            }
            else
            {
                await mainWindowViewModel.HandleNormalLaunchActivation();
            }

            ViewManager.RootViewModel = _mainAppWindow.ViewModel;
        }
    }
}
