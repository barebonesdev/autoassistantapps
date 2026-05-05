using AutoAssistantAppDataLibrary;
using AutoAssistantAppDataLibrary.ViewModels.MainWindow.Welcome.Login;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace AutoAssistantUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ResetPasswordView : PopupViewHostGeneric
    {
        public new ResetPasswordViewModel ViewModel
        {
            get { return base.ViewModel as ResetPasswordViewModel; }
            set { base.ViewModel = value; }
        }

        public ResetPasswordView()
        {
            this.InitializeComponent();

            this.Title = AutoAssistantResources.GetString("ForgotPassword_ButtonHeader/Content");
        }

        public override void OnViewModelSetOverride()
        {
            base.OnViewModelSetOverride();

            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            UpdateStatus();
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.IsResettingPassword):
                    UpdateStatus();
                    break;
            }
        }

        private void UpdateStatus()
        {
            base.IsEnabled = !ViewModel.IsResettingPassword;
        }

        private void forgotPassword_Reset_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ResetPassword();
        }

        private void forgotPassword_Username_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                e.Handled = true;
                forgotPassword_Email.Focus(FocusState.Programmatic);
            }
        }

        private void forgotPassword_Email_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                e.Handled = true;
                ViewModel.ResetPassword();
            }
        }
    }
}
