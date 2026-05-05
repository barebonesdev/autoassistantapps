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

namespace AutoAssistantAppDataLibrary.ViewModels.MainWindow.Welcome.Login
{
    public class ResetPasswordViewModel : BaseViewModel
    {
        public ResetPasswordViewModel(BaseViewModel parent, string username) : base(parent)
        {
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
    }
}
