using AutoAssistantAppDataLibrary.DataLayer;
using AutoAssistantAppDataLibrary.Helpers;
using AutoAssistantLibrary.Requests;
using AutoAssistantLibrary.Responses;
using BareMvvm.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace AutoAssistantAppDataLibrary.ViewModels.MainWindow.Welcome.CreateAccount
{
    public class CreateAccountViewModel : BaseViewModel
    {
        public Action AlertPasswordTooShort = delegate { ShowMessage("Your password is too short.", "Password too short"); };
        public Action AlertConfirmationPasswordDidNotMatch = delegate { ShowMessage("Your confirmation password didn't match.", "Invalid password"); };
        public Action AlertNoUsername = delegate { ShowMessage("You must provide a username!", "No username"); };
        public Action AlertNoEmail = delegate { ShowMessage("You must provide an email!", "No email"); };

        public CreateAccountViewModel(BaseViewModel parent) : base(parent) { }

        private string _username = "";
        public string Username
        {
            get { return _username; }
            set { SetProperty(ref _username, value, nameof(Username)); }
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

        private string _email = "";
        public string Email
        {
            get { return _email; }
            set { SetProperty(ref _email, value, nameof(Email)); }
        }

        private bool isPasswordOkay()
        {
            if (Password.Length < 5)
            {
                AlertPasswordTooShort?.Invoke();
                return false;
            }

            if (!ConfirmPassword.Equals(Password))
            {
                AlertConfirmationPasswordDidNotMatch?.Invoke();
                return false;
            }

            return true;
        }

        private bool isUsernameOkay()
        {
            if (string.IsNullOrWhiteSpace(Username))
            {
                AlertNoUsername?.Invoke();
                return false;
            }

            return true;
        }

        private bool isOkayToCreateLocal()
        {
            if (!isUsernameOkay())
                return false;

            if (!isPasswordOkay())
                return false;

            return true;
        }

        private bool isOkayToCreate()
        {
            if (!isOkayToCreateLocal())
                return false;

            if (string.IsNullOrWhiteSpace(Email))
            {
                AlertNoEmail?.Invoke();
                return false;
            }

            return true;
        }


        public async Task CreateLocalAccountAsync()
        {
            if (!isOkayToCreateLocal())
                return;

            await FinishCreateAccount(Username, getHashedPassword(), 0, 0);
        }

        private bool _isCreatingOnlineAccount;
        public bool IsCreatingOnlineAccount
        {
            get { return _isCreatingOnlineAccount; }
            set { SetProperty(ref _isCreatingOnlineAccount, value, nameof(IsCreatingOnlineAccount)); }
        }

        public async void CreateAccount()
        {
            if (!isOkayToCreate())
                return;

            string username = Username.Trim();
            string password = getHashedPassword();
            string email = Email.Trim();

            IsCreatingOnlineAccount = true;

            try
            {
                CreateAccountResponse resp = await WebHelper.Download<CreateAccountRequest, CreateAccountResponse>(
                    Website.URL + "createaccount",
                    new CreateAccountRequest()
                    {
                        Username = username,
                        Password = password,
                        Email = email,
                        AddDevice = true
                    }, Website.ApiKey);

                if (resp == null)
                    ShowMessage(AutoAssistantResources.GetStringOfflineExplanation(), "Error creating account");

                else if (resp.Error != null)
                    ShowMessage(resp.Error, "Error creating account");

                else
                {
                    await FinishCreateAccount(username, password, resp.AccountId, resp.DeviceId);
                }
            }

            catch
            {
                ShowMessage(AutoAssistantResources.GetStringOfflineExplanation(), "Error creating account");
            }

            finally
            {
                IsCreatingOnlineAccount = false;
            }
        }

        private string getHashedPassword()
        {
            return Credentials.Encrypt(Password);
        }

        private async System.Threading.Tasks.Task FinishCreateAccount(string username, string hashedPassword, long accountId, int deviceId)
        {
            var account = await CreateAccountHelper.CreateAccountLocally(username, hashedPassword, accountId, deviceId);

            if (account != null)
            {
                AccountsManager.SetLastLoginIdentifier(account.LocalAccountId);
                await FindAncestor<MainWindowViewModel>().SetCurrentAccount(account);
            }
        }

        private static async void ShowMessage(string message, string title)
        {
            await new PortableMessageDialog(message, title).ShowAsync();
        }
    }
}
