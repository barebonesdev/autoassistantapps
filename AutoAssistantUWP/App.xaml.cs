using AutoAssistantAppDataLibrary.App;
using AutoAssistantAppDataLibrary.DataLayer;
using AutoAssistantAppDataLibrary.Extensions;
using AutoAssistantAppDataLibrary.Extensions.Telemetry;
using AutoAssistantAppDataLibrary.Helpers;
using AutoAssistantAppDataLibrary.ViewModels;
using AutoAssistantAppDataLibrary.ViewModels.MainWindow;
using AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen;
using AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen.Fuel;
using AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen.Garage;
using AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen.Maintenance;
using AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen.Overview;
using AutoAssistantAppDataLibrary.ViewModels.MainWindow.Settings;
using AutoAssistantAppDataLibrary.ViewModels.MainWindow.Welcome;
using AutoAssistantAppDataLibrary.ViewModels.MainWindow.Welcome.CreateAccount;
using AutoAssistantAppDataLibrary.ViewModels.MainWindow.Welcome.Login;
using AutoAssistantAppDataLibrary.Windows;
using AutoAssistantUWP.Views;
using BareMvvm.Core.ViewModels;
using InterfacesUWP.App;
using InterfacesUWP.AppWindows;
using InterfacesUWP.ViewModelPresenters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Vx.Views;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using static AutoAssistantAppDataLibrary.Helpers.ArgumentsHelper;

namespace AutoAssistantUWP
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : NativeUwpApplication
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        public override Type GetPortableAppType()
        {
            return typeof(AutoAssistantUwpApp);
        }

        public override Dictionary<Type, Type> GetGenericViewModelToViewMappings()
        {
            return new Dictionary<Type, Type>
            {
                { typeof(PopupComponentViewModel), typeof(PopupComponentView) },
                { typeof(PagedViewModelWithPopups), typeof(PagedViewModelWithPopupsPresenter) },
                { typeof(PagedViewModel), typeof(PagedViewModelPresenter) },
                { typeof(BaseViewModel), typeof(BaseViewModelView) }
            };
        }

        public override Dictionary<Type, Type> GetViewModelToViewMappings()
        {
            return new Dictionary<Type, Type>()
            {
                //{ typeof(WelcomeViewModel), typeof(WelcomeView) },
                //{ typeof(LoginViewModel), typeof(LoginView) },
                //{ typeof(CreateAccountViewModel), typeof(CreateAccountView) },
                //{ typeof(MainScreenViewModel), typeof(MainScreenView) },
                //{ typeof(GarageViewModel), typeof(GarageView) },
                //{ typeof(AddVehicleViewModel), typeof(AddVehicleView) },
                //{ typeof(OverviewViewModel), typeof(OverviewView) },
                //{ typeof(FuelViewModel), typeof(FuelView) },
                //{ typeof(AddFuelViewModel), typeof(AddFuelView) },
                //{ typeof(ViewFuelViewModel), typeof(ViewFuelView) },
                //{ typeof(MaintenanceViewModel), typeof(MaintenanceView) },
                //{ typeof(AddScheduleItemViewModel), typeof(AddScheduleItemView) },
                //{ typeof(ViewScheduleItemViewModel), typeof(ViewScheduleItemView) },
                //{ typeof(AddMaintenanceRecordViewModel), typeof(AddMaintenanceRecordView) },
                //{ typeof(ViewMaintenanceRecordViewModel), typeof(ViewMaintenanceRecordView) },
                //{ typeof(SyncErrorsViewModel), typeof(SyncErrorsView) },
                //{ typeof(AboutViewModel), typeof(AboutView) },
                //{ typeof(ChangeEmailViewModel), typeof(ChangeEmailView) },
                //{ typeof(ChangePasswordViewModel), typeof(ChangePasswordView) },
                //{ typeof(ChangeUsernameViewModel), typeof(ChangeUsernameView) },
                //{ typeof(ConfirmIdentityViewModel), typeof(ConfirmIdentityView) },
                //{ typeof(ConvertToOnlineViewModel), typeof(ConvertToOnlineView) },
                //{ typeof(DeleteAccountViewModel), typeof(DeleteAccountView) },
                //{ typeof(MyAccountViewModel), typeof(MyAccountView) },
                //{ typeof(SettingsListViewModel), typeof(SettingsListView) },
                //{ typeof(ForgotUsernameViewModel), typeof(ForgotUsernameView) },
                //{ typeof(RecoveredUsernamesViewModel), typeof(RecoveredUsernamesView) },
                //{ typeof(ResetPasswordViewModel), typeof(ResetPasswordView) },
                //{ typeof(SearchMaintenanceRecordsViewModel), typeof(SearchMaintenanceRecordsView) },
                //{ typeof(ImportFuelIntroViewModel), typeof(ImportFuelIntroView) },
                //{ typeof(ImportFuelSelectCsvViewModel), typeof(ImportFuelSelectCsvView) },
                //{ typeof(ImportFuelPreviewImportViewModel), typeof(ImportFuelPreviewImportView) },
                //{ typeof(ExportFuelToCsvViewModel), typeof(ExportFuelToCsvView) }
            };
        }

        protected override async System.Threading.Tasks.Task OnLaunchedOrActivated(IActivatedEventArgs e)
        {
            try
            {
#if DEBUG
                //if (System.Diagnostics.Debugger.IsAttached)
                //{
                //    this.DebugSettings.EnableFrameRateCounter = true;
                //}
#endif

                // Wait for initialization to complete, to ensure we don't accidently add multiple windows
                // Although right now we don't even do any async tasks, so this will be useless
                await AutoAssistantApp.InitializeTask;

                MainAppWindow mainAppWindow;

                // If no windows, need to register window
                mainAppWindow = AutoAssistantApp.Current.Windows.OfType<MainAppWindow>().FirstOrDefault();
                NativeUwpAppWindow nativeWindow = null;
                if (mainAppWindow == null)
                {
                    // This configures the view models, does NOT call Activate yet
                    nativeWindow = new NativeUwpAppWindow();
                    mainAppWindow = new MainAppWindow();
                    await AutoAssistantApp.Current.RegisterWindowAsync(mainAppWindow, nativeWindow);

                    if (AutoAssistantApp.Current.Windows.Count > 1)
                    {
                        throw new Exception("There are more than 1 windows registered");
                    }
                }

                if (e is LaunchActivatedEventArgs)
                {
                    var launchEventArgs = e as LaunchActivatedEventArgs;
                    var launchContext = !object.Equals(launchEventArgs.TileId, "App") ? LaunchSurface.SecondaryTile : LaunchSurface.Normal;
                    if (launchContext == LaunchSurface.Normal)
                    {
                        // Track whether was launched from primary tile
                        if (ApiInformation.IsPropertyPresent(typeof(LaunchActivatedEventArgs).FullName, nameof(LaunchActivatedEventArgs.TileActivatedInfo)))
                        {
                            if (launchEventArgs.TileActivatedInfo != null)
                            {
                                launchContext = LaunchSurface.PrimaryTile;
                            }
                        }
                    }

                    await HandleArguments(mainAppWindow, launchEventArgs.Arguments, launchContext);
                }

                if (mainAppWindow.GetViewModel().Content == null)
                {
                    await mainAppWindow.GetViewModel().HandleNormalLaunchActivation();
                }

                // Show the window content and activate the window
                nativeWindow?.DisplayWindowContent();
                Window.Current.Activate();

                // Listen to window activation changes
                Window.Current.Activated += Current_Activated;

                // Set up the default window properties
                ConfigureWindowProperties();
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private void Current_Activated(object sender, WindowActivatedEventArgs e)
        {
            switch (e.WindowActivationState)
            {
                case CoreWindowActivationState.CodeActivated:
                case CoreWindowActivationState.PointerActivated:

                    try
                    {
                        foreach (var window in AutoAssistantApp.Current.Windows.OfType<MainAppWindow>())
                        {
                            var dontWait = window.GetViewModel().HandleBeingReturnedTo();
                        }
                    }

                    catch { }

                    break;

                case CoreWindowActivationState.Deactivated:

                    try
                    {
                        foreach (var window in AutoAssistantApp.Current.Windows.OfType<MainAppWindow>())
                        {
                            (window.ViewModel as MainWindowViewModel)?.HandleBeingLeft();
                        }
                    }
                    catch (Exception ex)
                    {
                        TelemetryExtension.Current?.TrackException(ex);
                    }

                    break;
            }
        }

        private static async System.Threading.Tasks.Task HandleArguments(MainAppWindow mainAppWindow, string arguments, LaunchSurface launchContext)
        {
            try
            {
                MainWindowViewModel viewModel = mainAppWindow.GetViewModel();

                var args = ArgumentsHelper.Parse(arguments);

                Guid desiredLocalAccountId = Guid.Empty;

                if (args is BaseArgumentsWithAccount)
                    desiredLocalAccountId = (args as BaseArgumentsWithAccount).LocalAccountId;
                else
                    desiredLocalAccountId = AccountsManager.GetLastLoginLocalId();

                Guid currentLocalAccountId = Guid.Empty;
                if (viewModel.CurrentAccount != null)
                    currentLocalAccountId = viewModel.CurrentAccount.LocalAccountId;

                if (false)
                {
                    // In future we'll have specific deep link actions
                }

                else
                {
                    TrackLaunch(args, launchContext, "Launch");
                    if (viewModel.Content == null)
                    {
                        await viewModel.HandleNormalLaunchActivation();
                    }
                }
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex, SeverityLevel.Error);
            }
        }

        private static void TrackLaunch(BaseArguments args, LaunchSurface launchSurface, string action)
        {
            if (launchSurface == LaunchSurface.Uri || launchSurface == LaunchSurface.Normal)
            {
                if (args != null)
                {
                    launchSurface = args.LaunchSurface;
                }
            }

            if (launchSurface != LaunchSurface.Normal)
            {
                TelemetryExtension.Current?.TrackEvent($"Launch_From{launchSurface}_{action}");
            }
        }

        private static void ConfigureWindowProperties()
        {
            try
            {
                var view = ApplicationView.GetForCurrentView();

                // Set up the title bar
                var titleBar = view.TitleBar;

                titleBar.BackgroundColor = Color.FromArgb(255, 26, 32, 74);
                titleBar.ForegroundColor = Colors.White;

                titleBar.InactiveBackgroundColor = Color.FromArgb(255, 73, 79, 117);
                titleBar.InactiveForegroundColor = Colors.LightGray;


                titleBar.ButtonBackgroundColor = titleBar.BackgroundColor;
                titleBar.ButtonForegroundColor = titleBar.ForegroundColor;

                titleBar.ButtonHoverBackgroundColor = Color.FromArgb(255, 75, 96, 179);
                titleBar.ButtonHoverForegroundColor = Colors.White;

                titleBar.ButtonInactiveBackgroundColor = titleBar.InactiveBackgroundColor;
                titleBar.ButtonInactiveForegroundColor = titleBar.InactiveForegroundColor;

                titleBar.ButtonPressedBackgroundColor = Color.FromArgb(255, 84, 107, 199);
                titleBar.ButtonPressedForegroundColor = Colors.White;


                // Set up the min window size
                view.SetPreferredMinSize(new Size(300, 300));



                // Set up status bar
                //if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
                //{
                //    var statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();

                //    statusBar.BackgroundColor = (Color)Application.Current.Resources["PowerPlannerBlueColor"];
                //    statusBar.BackgroundOpacity = 1;
                //    statusBar.ForegroundColor = Colors.White;
                //}
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex, SeverityLevel.Error);
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }

        internal static async Task<bool> ConfirmDelete(string message, string title)
        {
            MessageDialog dialog = new MessageDialog(message, title);

            var commandDelete = new UICommand("Delete");
            var commandCancel = new UICommand("Cancel");

            dialog.Commands.Add(commandDelete);
            dialog.Commands.Add(commandCancel);

            var response = await dialog.ShowAsync();

            if (response == commandDelete)
            {
                return true;
            }

            return false;
        }
    }
}
