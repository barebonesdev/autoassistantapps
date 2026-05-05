using AutoAssistantAppDataLibrary;
using AutoAssistantAppDataLibrary.Extensions;
using AutoAssistantAppDataLibrary.ViewModels.MainWindow.Welcome.Login;
using InterfacesUWP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using ToolsPortable;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace AutoAssistantUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginView : PopupViewHostGeneric
    {
        public LoginView()
        {
            this.InitializeComponent();
        }

        public new LoginViewModel ViewModel
        {
            get { return base.ViewModel as LoginViewModel; }
            set { base.ViewModel = value; }
        }

        public override void OnViewModelLoadedOverride()
        {
            base.OnViewModelLoadedOverride();

            ViewModel.AlertOfflineAndNoLocalAccountFound = AlertOfflineAndNoLocalAccountFound;
            ViewModel.AlertUserIncorrectPasswordAndLocalAccount = AlertUserIncorrectPasswordAndLocalAccount;
            ViewModel.AlertUserIncorrectPasswordAndOffline = AlertUserIncorrectPasswordAndOffline;

            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsCheckingOnlinePassword":
                    UpdateIsCheckingOnlinePassword();
                    break;

                case "IsLoggingInOnline":
                    UpdateIsLoggingInOnline();
                    break;

                case "IsSyncingAccount":
                    UpdateIsSyncingAccount();
                    break;
            }
        }

        private LoadingPopup _loadingCheckingOnlinePassword;
        private void UpdateIsCheckingOnlinePassword()
        {
            if (ViewModel.IsCheckingOnlinePassword)
            {
                if (_loadingCheckingOnlinePassword == null)
                {
                    _loadingCheckingOnlinePassword = new LoadingPopup()
                    {
                        Text = AutoAssistantResources.GetString("LoginPage_String_CheckingOnlinePassword")
                    };
                }

                _loadingCheckingOnlinePassword.Show();
            }

            else
            {
                _loadingCheckingOnlinePassword?.Close();
            }
        }

        private LoadingPopup _loadingPopupIsLoggingInOnline;
        private void UpdateIsLoggingInOnline()
        {
            if (ViewModel.IsLoggingInOnline)
            {
                if (_loadingPopupIsLoggingInOnline == null)
                {
                    _loadingPopupIsLoggingInOnline = new LoadingPopup()
                    {
                        Text = AutoAssistantResources.GetString("LoginPage_String_LoggingIn")
                    };
                }

                _loadingPopupIsLoggingInOnline.Show();
            }

            else
            {
                _loadingPopupIsLoggingInOnline?.Close();
            }
        }

        private LoadingPopup _loadingPopupIsSyncingAccount;
        private void UpdateIsSyncingAccount()
        {
            if (ViewModel.IsSyncingAccount)
            {
                if (_loadingPopupIsSyncingAccount == null)
                {
                    _loadingPopupIsSyncingAccount = new LoadingPopup()
                    {
                        Text = AutoAssistantResources.GetString("LoginPage_String_SyncingAccount")
                    };
                }

                _loadingPopupIsSyncingAccount.Show();
            }

            else
            {
                _loadingPopupIsSyncingAccount?.Close();
            }
        }

        private void AlertInvalidUsername()
        {
            ShowMessage(string.Format(AutoAssistantResources.GetString("String_InvalidUsernameExplanation"), string.Join(" ", StringTools.VALID_SPECIAL_URL_CHARS)), AutoAssistantResources.GetString("String_InvalidUsername"));
        }

        private void AlertOfflineAndNoLocalAccountFound()
        {
            MessageBox.Show(AutoAssistantResources.GetString("LoginPage_String_ExplanationOfflineAndNoLocalAccountFound"), AutoAssistantResources.GetString("LoginPage_String_NoAccountFoundHeader"));
        }

        private void AlertUserIncorrectPasswordAndLocalAccount()
        {
            MessageBox.Show(AutoAssistantResources.GetString("LoginPage_String_ExplanationIncorrectPasswordAndLocalAccount"), AutoAssistantResources.GetString("LoginPage_String_IncorrectPassword"));
        }

        private void AlertUserIncorrectPasswordAndOffline()
        {
            MessageBox.Show(AutoAssistantResources.GetString("LoginPage_String_ExplanationIncorrectPasswordAndOffline"), AutoAssistantResources.GetString("LoginPage_String_IncorrectPassword"));
        }

        private void AlertUsernameEmpty()
        {
            ShowMessage(AutoAssistantResources.GetString("LoginPage_String_UsernameEmptyExplanation"), AutoAssistantResources.GetString("LoginPage_String_UsernameEmpty"));
        }

        private void AlertUsernameExistsLocally()
        {
            ShowMessage(AutoAssistantResources.GetString("LoginPage_String_ExplanationUsernameExistsLocally"), AutoAssistantResources.GetString("LoginPage_String_UsernameExists"));
        }

        private async void tbPassword_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            try
            {
                if (e.Key == VirtualKey.Enter)
                {
                    e.Handled = true;
                    await ViewModel.LoginAsync();
                }
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private async void tbUsername_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                e.Handled = true;

                // If password has already been auto filled, might as well directly log in
                if (ViewModel.CanLogin())
                    await ViewModel.LoginAsync();

                // Otherwise switch to password box so user can type password
                else
                    tbPassword.Focus(FocusState.Programmatic);
            }
        }

        private async void buttonLogin_Click(object sender, RoutedEventArgs e)
        {
            await ViewModel.LoginAsync();
        }

        private static async void ShowMessage(string message, string title)
        {
            await new MessageDialog(message, title).ShowAsync();
        }

        private void showForgotPassword()
        {
            ViewModel.ForgotPassword();
        }

        private void thisPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width <= 450)
                VisualStateManager.GoToState(this, "CompactState", true);
            else
                VisualStateManager.GoToState(this, "NormalState", true);
        }

        private void tbUsername_Loaded(object sender, RoutedEventArgs e)
        {
            tbUsername.Focus(FocusState.Programmatic);
        }

        private void ButtonForgotUsername_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ForgotUsername();
        }

        private void ButtonForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            showForgotPassword();
        }
    }
}
