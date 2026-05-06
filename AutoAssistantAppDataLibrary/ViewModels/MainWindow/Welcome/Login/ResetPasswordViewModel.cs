using AutoAssistantAppDataLibrary.Extensions;
using AutoAssistantLibrary.Requests;
using AutoAssistantLibrary.Responses;
using BareMvvm.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using Vx.Views;

namespace AutoAssistantAppDataLibrary.ViewModels.MainWindow.Welcome.Login
{
    public class ResetPasswordViewModel : PopupComponentViewModel
    {
        public ResetPasswordViewModel(BaseViewModel parent, string username) : base(parent)
        {
            Title = "Forgot Password";
            
            Username = username;
        }

        private string _username;
        public string Username
        {
            get { return _username; }
            set { SetProperty(ref _username, value, nameof(Username)); }
        }

        private string _email = ForgotUsernameViewModel.StoredEmail;
        public string Email
        {
            get { return _email; }
            set { SetProperty(ref _email, value, nameof(Email)); ForgotUsernameViewModel.StoredEmail = value; }
        }

        private bool _isResettingPassword;
        public bool IsResettingPassword
        {
            get { return _isResettingPassword; }
            set { SetProperty(ref _isResettingPassword, value, nameof(IsResettingPassword)); }
        }

        public async void ResetPassword()
        {
            IsResettingPassword = true;

            try
            {
                var email = Email.Trim();
                var username = Username.Trim();

                ResetPasswordResponse resp = await WebHelper.Download<ResetPasswordRequest, ResetPasswordResponse>(
                    Website.URL + "resetpassword",
                    new ResetPasswordRequest() { Username = username, Email = email },
                    Website.ApiKey);

                if (resp == null)
                    return;

                if (resp.Error != null)
                {
                    var dontWait = new PortableMessageDialog(resp.Error, AutoAssistantResources.GetString("ResetPassword_String_ErrorResettingPassword")).ShowAsync();
                }

                else
                {
                    IsResettingPassword = false;
                    var loginViewModel = Parent.GetPopupViewModelHost()?.Popups.OfType<LoginViewModel>().FirstOrDefault();
                    if (loginViewModel != null)
                    {
                        loginViewModel.Username = username;
                    }
                    await new PortableMessageDialog(resp.Message, AutoAssistantResources.GetString("ResetPassword_String_ResetSuccessHeader")).ShowAsync();
                    base.RemoveViewModel();
                }
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex, Extensions.Telemetry.SeverityLevel.Error);
                var dontWait = new PortableMessageDialog(AutoAssistantResources.GetStringOfflineExplanation(), AutoAssistantResources.GetString("ResetPassword_String_ErrorResettingPassword")).ShowAsync();
            }

            finally
            {
                IsResettingPassword = false;
            }
        }

        protected override View Render()
        {
            return RenderGenericPopupContent(

                new TextBox
                {
                    Header = "Username",
                    PlaceholderText = "Enter your username",
                    Text = VxValue.Create(Username, v => Username = v),
                    IsEnabled = !IsResettingPassword,
                    InputScope = InputScope.Username
                },

                new TextBox
                {
                    Header = "Email",
                    PlaceholderText = "Enter your email",
                    Text = VxValue.Create(Email, v => Email = v),
                    IsEnabled = !IsResettingPassword,
                    Margin = new Thickness(0, 12, 0, 0),
                    OnSubmit = ResetPassword
                },

                new AccentButton
                {
                    Text = IsResettingPassword ? "Resetting password..." : "Reset password",
                    Click = ResetPassword,
                    Margin = new Thickness(0, 12, 0, 0),
                    IsEnabled = !IsResettingPassword
                }

            );
        }
    }
}
