using AutoAssistantAppDataLibrary.DataLayer;
using AutoAssistantAppDataLibrary.ViewModels.MainWindow.Welcome.CreateAccount;
using AutoAssistantLibrary.Requests;
using AutoAssistantLibrary.Responses;
using BareMvvm.Core;
using BareMvvm.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using Vx.Views;

namespace AutoAssistantAppDataLibrary.ViewModels.MainWindow.Settings
{
    public class ChangePasswordViewModel : PopupComponentViewModel
    {
        protected override bool InitialAllowLightDismissValue => false;
        public override bool ImportantForAutofill => true;

        public event EventHandler<string> ActionError;
        public event EventHandler<string> ActionPasswordsDidNotMatch;

        public AccountDataItem Account { get; private set; }

        public ChangePasswordViewModel(BaseViewModel parent, AccountDataItem account) : base(parent)
        {
            Account = account;
            Title = "Change password";

            Password = new TextField(required: true, maxLength: 50, minLength: 5);
            ConfirmPassword = new TextField(required: true, mustMatch: Password);
        }

        protected override View Render()
        {
            bool isEnabled = !IsUpdatingPassword;

            return RenderGenericPopupContent(
                new PasswordBox(Password)
                {
                    Header = "New password",
                    PlaceholderText = "Enter your new password",
                    AutoFocus = true,
                    IsEnabled = isEnabled,
                    AutoMoveToNextTextBox = true,
                    OnSubmit = Update
                },

                new PasswordBox(ConfirmPassword)
                {
                    Header = "Confirm new password",
                    PlaceholderText = "Re-enter your new password",
                    Margin = new Thickness(0, 18, 0, 0),
                    IsEnabled = isEnabled,
                    OnSubmit = Update
                },

                new AccentButton
                {
                    Text = "Update password",
                    Click = Update,
                    Margin = new Thickness(0, 24, 0, 0),
                    IsEnabled = isEnabled
                }
            );
        }

        [VxSubscribe]
        public TextField Password { get; private set; }

        [VxSubscribe]
        public TextField ConfirmPassword { get; private set; }

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
                if (!ValidateAllInputs())
                {
                    return;
                }

                string encryptedPassword = Credentials.Encrypt(Password.Text);

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
