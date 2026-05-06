using AutoAssistantAppDataLibrary.Windows;
using AutoAssistantAppDataLibrary.ViewModels.MainWindow;
using AutoAssistantiOS.Helpers;
using BareMvvm.Core.App;
using InterfacesiOS.App;
using InterfacesiOS.Helpers;
using InterfacesiOS.Windows;

namespace AutoAssistantiOS
{
    [Register("SceneDelegate")]
    public class SceneDelegate : UIResponder, IUIWindowSceneDelegate
    {
        [Export("window")]
        public UIWindow Window { get; set; }

        private MainAppWindow _mainAppWindow;

        [Export("scene:willConnectToSession:options:")]
        public void WillConnect(UIScene scene, UISceneSession session, UISceneConnectionOptions connectionOptions)
        {
            if (scene is UIWindowScene windowScene)
            {
                var window = new UIWindow(windowScene);
                window.BackgroundColor = UIColorCompat.SystemBackgroundColor;
                window.TintColor = ColorResources.AccentColor;
                window.RootViewController = new UIViewController();
                window.MakeKeyAndVisible();

                Window = window;
                NativeiOSApplication.Current.Window = window;

                RegisterWindow();
            }
        }

        private async void RegisterWindow()
        {
            _mainAppWindow = new MainAppWindow();

            await PortableApp.Current.RegisterWindowAsync(_mainAppWindow, new NativeiOSAppWindow(Window));

            var mainWindowViewModel = _mainAppWindow.GetViewModel();
            await mainWindowViewModel.HandleNormalLaunchActivation();

            NativeiOSApplication.Current.ViewManager.RootViewModel = _mainAppWindow.ViewModel;
        }

        [Export("sceneDidBecomeActive:")]
        public void DidBecomeActive(UIScene scene)
        {
            try
            {
                foreach (var window in AutoAssistantAppDataLibrary.App.AutoAssistantApp.Current.Windows.OfType<MainAppWindow>())
                {
                    var dontWait = window.GetViewModel().HandleBeingReturnedTo();
                }
            }
            catch { }
        }

        [Export("sceneDidEnterBackground:")]
        public void DidEnterBackground(UIScene scene)
        {
            try
            {
                foreach (var window in AutoAssistantAppDataLibrary.App.AutoAssistantApp.Current.Windows.OfType<MainAppWindow>())
                {
                    window.GetViewModel().HandleBeingLeft();
                }
            }
            catch { }
        }
    }
}
