using AutoAssistantAppDataLibrary.DataLayer;
using AutoAssistantAppDataLibrary.Extensions;
using AutoAssistantAppDataLibrary.Helpers;
using AutoAssistantAppDataLibrary.SyncLayer;
using AutoAssistantLibrary.Requests;
using AutoAssistantLibrary.Responses;
using BareMvvm.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using Vx.Views;

namespace AutoAssistantAppDataLibrary.ViewModels.MainWindow.Welcome.Login
{
    public class LoginViewModel : PopupComponentViewModel
    {
        public override bool ImportantForAutofill => true;

        public LoginViewModel(BaseViewModel parent) : base(parent)
        {
            Title = "Log in";
            Initialize();
        }

        private const string STORED_PASS = "Stored password, hidden for security";

        public MyObservableList<AccountDataItem> Accounts
        {
            get { return GetValue<MyObservableList<AccountDataItem>>(); }
            private set { SetValue(value); }
        }

        public MyObservableList<AccountDataItem> AccountsWithRememberUsername
        {
            get { return GetValue<MyObservableList<AccountDataItem>>(); }
            private set { SetValue(value); }
        }

        public string Username
        {
            get { return GetValueOrDefault<string>(""); }
            set
            {
                if (!object.Equals(Username, value))
                {
                    SetValue(value);
                    OnUsernameChanged();
                }
            }
        }

        private string _password = "";
        public string Password
        {
            get { return _password; }
            set
            {
                if (!object.Equals(_password, value))
                {
                    SetProperty(ref _password, value, nameof(Password));
                    OnPasswordChanged();
                }
            }
        }

        public bool IsCheckingOnlinePassword
        {
            get { return GetValue<bool>(); }
            set { SetValue(value); }
        }

        public bool IsLoggingInOnline
        {
            get { return GetValue<bool>(); }
            set { SetValue(value); }
        }

        public bool IsSyncingAccount
        {
            get { return GetValue<bool>(); }
            set { SetValue(value); }
        }

        public Action AlertUserIncorrectPasswordAndOffline { get; set; }
        public Action AlertUserIncorrectPasswordAndLocalAccount { get; set; }
        public Action<string> AlertUserUpgradeAccountNeeded { get; set; }
        public Action AlertOfflineAndNoLocalAccountFound { get; set; }

        private new async void Initialize()
        {
            Accounts = new MyObservableList<AccountDataItem>(await AccountsManager.GetAllAccounts());

            // TODO: What if RememberUsername is edited? That's quite a minor case, not worth building something for, but it's potentially a flaw
            AccountsWithRememberUsername = Accounts.Sublist(i => i.RememberUsername);

            AccountsManager.OnAccountDeleted += AccountsManager_OnAccountDeleted;
            AccountsManager.OnAccountAdded += AccountsManager_OnAccountAdded;

            var lastLoginLocalId = AccountsManager.GetLastLoginLocalId();
            if (lastLoginLocalId != Guid.Empty)
            {
                var lastLogin = Accounts.FirstOrDefault(i => i.LocalAccountId == lastLoginLocalId);

                if (lastLogin != null && lastLogin.RememberUsername)
                    Username = lastLogin.Username;
            }
        }

        private void OnUsernameChanged()
        {
            FillInPassword(Username);
        }

        private void OnPasswordChanged()
        {
            // If we previously auto filled and the user is changing it, remove the auto fill so they have to type their correct password.
            // We check if it equals stored pass, since when we programmatically set the password to STORED_PASS, that'll trigger this event,
            // but since it will equal STORED_PASS, we won't immediately disable it
            if (_autoFilledHashedPassword != null && !Password.Equals(STORED_PASS))
            {
                _autoFilledHashedPassword = null;
            }
        }

        private void AccountsManager_OnAccountAdded(object sender, AccountDataItem e)
        {
            Dispatcher.Run(delegate
            {
                Accounts.Add(e);
            });
        }

        private void AccountsManager_OnAccountDeleted(object sender, Guid e)
        {
            Dispatcher.Run(delegate
            {
                Accounts.RemoveWhere(i => i.LocalAccountId == e);
            });
        }

        public bool CanLogin()
        {
            return _autoFilledHashedPassword != null;
        }

        public async System.Threading.Tasks.Task LoginAsync()
        {
            try
            {
                string username = getUsername();
                string password = getPassword();

                var matching = await FindAccountByUsername(username);

                if (matching == null)
                    localNotFound(username);

                else
                {
                    if ((_autoFilledHashedPassword != null && matching.Password.Equals(_autoFilledHashedPassword))
                        || matching.Password.Equals(password))
                    {
                        ToMainPage(matching);
                    }

                    else
                        incorrectLocalPassword(matching, password);
                }
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
                await new PortableMessageDialog("Error logging in. Your issue has been sent to the developer.").ShowAsync();
            }
        }

        private string getUsername()
        {
            return Username;
        }

        private string getPassword()
        {
            return Credentials.Encrypt(Password);
        }

        private string _autoFilledHashedPassword;

        private void FillInPassword(string username)
        {
            try
            {
                var matching = FindAccountByUsername(Accounts, username);

                // If matching, we potentially can fill in password if the user has selected to remember password
                if (matching != null && matching.RememberPassword)
                {
                    _autoFilledHashedPassword = matching.Password;
                    _password = STORED_PASS;
                    OnPropertyChanged(nameof(Password));
                }

                // Otherwise, we should clear the password box (if it's displaying our Stored Password message)
                else
                {
                    if (_autoFilledHashedPassword != null)
                    {
                        _password = "";
                        OnPropertyChanged(nameof(Password));
                    }
                }
            }

            catch { }
        }

        /// <summary>
        /// Loads all accounts, and then finds
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        private async Task<AccountDataItem> FindAccountByUsername(string username)
        {
            var allAccounts = await GetAllAccounts();

            return FindAccountByUsername(allAccounts, username);
        }

        private static AccountDataItem FindAccountByUsername(IEnumerable<AccountDataItem> allAccounts, string username)
        {
            return allAccounts.FirstOrDefault(i => i.Username.Equals(username, StringComparison.CurrentCultureIgnoreCase));
        }

        private async void incorrectLocalPassword(AccountDataItem account, string password)
        {
            if (account.IsOnlineAccount)
            {
                try
                {
                    IsCheckingOnlinePassword = true;
                    CheckPasswordResponse resp = await WebHelper.Download<CheckPasswordRequest, CheckPasswordResponse>(
                        Website.URL + "checkpassword",
                        new CheckPasswordRequest()
                        {
                            AccountId = account.AccountId,
                            Username = account.Username,
                            PasswordToTry = password
                        }, Website.ApiKey);

                    if (resp.Error != null)
                        ShowMessage(resp.Error, "Password error");

                    else
                    {
                        //update to new password
                        account.Password = password;
                        await AccountsManager.Save(account);

                        //then log them in
                        ToMainPage(account);
                    }
                }

                catch
                {
                    AlertUserIncorrectPasswordAndOffline?.Invoke();
                }

                finally
                {
                    IsCheckingOnlinePassword = false;
                }
            }

            else
                AlertUserIncorrectPasswordAndLocalAccount?.Invoke();
        }

        public async void ToMainPage(AccountDataItem account, bool existingAccount = true)
        {
            AccountsManager.SetLastLoginIdentifier(account.LocalAccountId);

            // We only sync if it's an existing account. For new accounts, we already performed a sync when logging in for first time.
            await base.FindAncestor<MainWindowViewModel>().SetCurrentAccount(account, syncAccount: existingAccount);

            if (existingAccount)
            {
                account.ExecuteOnLoginTasks();
            }
        }

        private async void localNotFound(string username)
        {
            string password = getPassword();

            try
            {
                IsLoggingInOnline = true;

                AddDeviceResponse resp;

                try
                {
                    resp = await ToolsPortable.WebHelper.Download<AddDeviceRequest, AddDeviceResponse>(
                        Website.URL + "adddevice",
                        new AddDeviceRequest()
                        {
                            Username = username,
                            Password = password
                        }, Website.ApiKey);
                }

                catch
                {
                    IsLoggingInOnline = false;
                    throw;
                }

                IsLoggingInOnline = false;

                if (resp.Error != null)
                {
                    ShowMessage(resp.Error, "Error logging in");
                }

                else
                {
                    AccountDataItem account = await CreateAccount(username, password, resp.AccountId, resp.DeviceId);
                    AccountsManager.SetLastLoginIdentifier(account.LocalAccountId);

                    if (account != null)
                    {
                        IsSyncingAccount = true;

                        try
                        {
                            var result = await Sync.SyncAccountAsync(account);
                        }

                        catch (OperationCanceledException) { }

                        catch { }

                        IsSyncingAccount = false;

                        ToMainPage(account, false); // don't sync since we just did
                    }
                }
            }

            catch (Exception ex)
            {
                Debug.WriteLine("Failed logging into online account: " + ex.ToString());

                AlertOfflineAndNoLocalAccountFound?.Invoke();
            }

            finally
            {
                IsLoggingInOnline = false;
            }
        }
        public System.Threading.Tasks.Task<AccountDataItem> CreateAccount(string username, string password, long accountId, int deviceId)
        {
            return CreateAccountHelper.CreateAccountLocally(username, password, accountId, deviceId);
        }

        private static async void ShowMessage(string message, string title)
        {
            await new PortableMessageDialog(message, title).ShowAsync();
        }

        private async Task<List<AccountDataItem>> GetAllAccounts()
        {
            return await Task.Run(async delegate
            {
                return await AccountsManager.GetAllAccounts();
            });
        }

        public void ForgotUsername()
        {
            ShowPopup(new ForgotUsernameViewModel(Parent));
        }

        public void ForgotPassword()
        {
            ShowPopup(new ResetPasswordViewModel(Parent, Username));
        }

        protected override View Render()
        {
            return RenderGenericPopupContent(
                new TextBox
                {
                    Header = "Username",
                    Text = VxValue.Create(Username, v => Username = v),
                    IsEnabled = !IsLoggingInOnline
                },

                new PasswordBox
                {
                    Header = "Password",
                    Margin = new Thickness(0, 12, 0, 0),
                    Text = VxValue.Create(Password, v => Password = v),
                    OnSubmit = () => _ = LoginAsync(),
                    IsEnabled = !IsLoggingInOnline
                },

                new LinearLayout
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Margin = new Thickness(0, 12, 0, 0),
                    Children =
                    {
                        new TextButton
                        {
                            Text = "Forgot Username",
                            Click = ForgotUsername
                        },

                        new TextBlock
                        {
                            Text = "|",
                            WrapText = false,
                            Margin = new Thickness(6, 0, 6, 0)
                        },

                        new TextButton
                        {
                            Text = "Forgot Password",
                            Click = ForgotPassword
                        }
                    }
                },

                new AccentButton
                {
                    Text = IsLoggingInOnline ? "Logging in..." : "Log in",
                    IsEnabled = !IsLoggingInOnline,
                    Margin = new Thickness(0, 12, 0, 0),
                    Click = () => _ = LoginAsync()
                }
            );
        }
    }
}
