using AutoAssistantAppDataLibrary.DataLayer;
using AutoAssistantLibrary.Requests;
using AutoAssistantLibrary.Responses;
using BareMvvm.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace AutoAssistantAppDataLibrary.ViewModels.MainWindow.Settings
{
    public class ChangePasswordViewModel : BaseViewModel
    {
        public event EventHandler<string> ActionError;
        public event EventHandler<string> ActionPasswordsDidNotMatch;

        public AccountDataItem Account { get; private set; }

        public ChangePasswordViewModel(BaseViewModel parent, AccountDataItem account) : base(parent)
        {
            Account = account;
        }

        private string _password = "";

        public string Password
        {
            get { return _password; }
            set { SetProperty(ref _password, value, nameof(Password)); }
        }

        private string _confirmPassword = "";

        public string ConfirmPassword
        {
            get { return _confirmPassword; }
            set { SetProperty(ref _confirmPassword, value, nameof(ConfirmPassword)); }
        }

        private bool _isUpdatingPassword;

        public bool IsUpdatingPassword
        {
            get { return _isUpdatingPassword; }
            set { SetProperty(ref _isUpdatingPassword, value, nameof(IsUpdatingPassword)); }
        }

        public async void Update()
        {
            IsUpdatingPassword = true;

            try
            {
                if (string.IsNullOrWhiteSpace(Password))
                {
                    SetError(AutoAssistantResources.GetString("Settings_ChangePasswordPage_Errors_MustEnterPassword"));
                    return;
                }

                if (Password.Length < 5)
                {
                    SetError(AutoAssistantResources.GetString("String_InvalidPasswordTooShortExplanation"));
                    return;
                }

                if (!Password.Equals(ConfirmPassword))
                {
                    ActionPasswordsDidNotMatch?.Invoke(this, AutoAssistantResources.GetString("Settings_ChangePasswordPage_Errors_PasswordsDidNotMatch"));
                    return;
                }

                string encryptedPassword = Credentials.Encrypt(Password);

                if (Account.IsOnlineAccount)
                {
                    try
                    {
                        ChangePasswordResponse resp = await WebHelper.Download<ChangePasswordRequest, ChangePasswordResponse>(Website.URL + "changepassword", new ChangePasswordRequest()
                        {
                            Credentials = Account.GenerateCredentials(),
                            NewPassword = encryptedPassword
                        }, Website.ApiKey);

                        if (resp.Error != null)
                        {
                            SetError(resp.Error);
                            return;
                        }

                        await changeLocalPassword(Account, encryptedPassword);
                        GoBack();
                    }

                    catch { SetError(AutoAssistantResources.GetString("Settings_ChangePasswordPage_Errors_FailedUpdateOnline")); }

                }

                else
                {
                    await changeLocalPassword(Account, encryptedPassword);
                    GoBack();
                }
            }

            finally { IsUpdatingPassword = false; }
        }

        private void SetError(string error)
        {
            ActionError?.Invoke(this, error);
        }

        private async System.Threading.Tasks.Task changeLocalPassword(AccountDataItem account, string newEncryptedPassword)
        {
            account.Password = newEncryptedPassword;
            await AccountsManager.Save(account);
        }
    }
}
