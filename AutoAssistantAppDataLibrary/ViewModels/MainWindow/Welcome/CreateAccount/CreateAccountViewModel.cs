using AutoAssistantAppDataLibrary.DataLayer;
using AutoAssistantAppDataLibrary.Extensions;
using AutoAssistantAppDataLibrary.Helpers;
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

namespace AutoAssistantAppDataLibrary.ViewModels.MainWindow.Welcome.CreateAccount
{
    public class CreateAccountViewModel : PopupComponentViewModel
    {
        protected override bool InitialAllowLightDismissValue => false;
        public override bool ImportantForAutofill => true;

        public Action AlertPasswordTooShort = delegate { ShowMessage("Your password is too short.", "Password too short"); };
        public Action AlertConfirmationPasswordDidNotMatch = delegate { ShowMessage("Your confirmation password didn't match.", "Invalid password"); };
        public Action AlertNoUsername = delegate { ShowMessage("You must provide a username!", "No username"); };
        public Action AlertNoEmail = delegate { ShowMessage("You must provide an email!", "No email"); };

        private List<AccountDataItem> _accounts;

        public static TextField GeneratePasswordTextField()
        {
            return new TextField(required: true, maxLength: 50, minLength: 5);
        }

        public static TextField GenerateEmailTextField()
        {
            return new TextField(required: true, maxLength: 150, inputValidator: EmailInputValidator.Instance, ignoreOuterSpaces: true);
        }

        public CreateAccountViewModel(BaseViewModel parent) : base(parent)
        {
            Title = "Create account";

            Username = new TextField(required: true, maxLength: 50, inputValidator: new CustomInputValidator(ValidateUsername), ignoreOuterSpaces: true, reportValidatorInvalidInstantly: true);
            Email = GenerateEmailTextField();
            Password = GeneratePasswordTextField();

            LoadAccounts();
        }

        [VxSubscribe]
        public TextField Username { get; private set; }

        private InputValidationState ValidateUsername(string username)
        {
            if (username.Contains(' '))
                return InputValidationState.Invalid("Cannot contain spaces");

            if (!StringTools.IsStringFilenameSafe(username) || !StringTools.IsStringUrlSafe(username))
            {
                var characters = username.ToCharArray().Distinct().ToArray();
                var validSpecialChars = StringTools.VALID_SPECIAL_FILENAME_CHARS.Intersect(StringTools.VALID_SPECIAL_URL_CHARS).ToArray();

                var validCharacters = characters.Where(i => Char.IsLetterOrDigit(i) || validSpecialChars.Contains(i)).ToArray();
                var invalidCharacters = characters.Except(validCharacters).ToArray();

                try
                {
                    return InputValidationState.Invalid(string.Format("Cannot contain characters {0}", string.Join(", ", invalidCharacters)));
                }
                catch
                {
                    return InputValidationState.Invalid("Invalid");
                }
            }

            if (_accounts == null)
            {
                return null;
            }

            if (_accounts.Any(i => i.Username.Equals(username, StringComparison.CurrentCultureIgnoreCase)))
            {
                return InputValidationState.Invalid("Username already exists locally");
            }

            return InputValidationState.Valid;
        }

        [VxSubscribe]
        public TextField Password { get; private set; }

        [VxSubscribe]
        public TextField Email { get; private set; }

        private bool isOkayToCreate()
        {
            return true;
        }

        private async void LoadAccounts()
        {
            try
            {
                _accounts = await AccountsManager.GetAllAccounts();
            }
            catch
            {
                _accounts = new List<AccountDataItem>();
            }
        }


        public async Task CreateLocalAccountAsync()
        {
            await FinishCreateAccount(Username.Text, getHashedPassword(), 0, 0);
        }

        private bool _isCreatingOnlineAccount;
        public bool IsCreatingOnlineAccount
        {
            get { return _isCreatingOnlineAccount; }
            set { SetProperty(ref _isCreatingOnlineAccount, value, nameof(IsCreatingOnlineAccount)); }
        }

        public async void CreateAccount()
        {
            if (!ValidateAllInputs())
            {
                return;
            }

            if (!isOkayToCreate())
                return;

            string username = Username.Text.Trim();
            string password = getHashedPassword();
            string email = Email.Text.Trim();

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
            return Credentials.Encrypt(Password.Text);
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



        public AccountDataItem DefaultAccountToUpgrade { get; private set; }

        /// <summary>
        /// Creating a local account should only be allowed when not upgrading the default account
        /// </summary>
        public bool IsCreateLocalAccountVisible => DefaultAccountToUpgrade == null;

        protected override View Render()
        {
            return new ScrollView
            {
                Content = new LinearLayout
                {
                    Margin = new Thickness(Theme.Current.PageMargin),
                    Children =
                    {
                        new TextBox(Username)
                        {
                            Header = "Username",
                            PlaceholderText = "Pick a username",
                            InputScope = InputScope.Username,
                            AutoFocus = true,
                            AutoMoveToNextTextBox = true,
                            OnSubmit = CreateAccount,
                            IsEnabled = !IsCreatingOnlineAccount
                        },

                        new TextBox(Email)
                        {
                            Header = "Email address",
                            PlaceholderText = "For recovery purposes",
                            InputScope = InputScope.Email,
                            Margin = new Thickness(0, 18, 0, 0),
                            AutoMoveToNextTextBox = true,
                            OnSubmit = CreateAccount,
                            IsEnabled = !IsCreatingOnlineAccount
                        },

                        new PasswordBox(Password)
                        {
                            Header = "Password",
                            PlaceholderText = "Create a password",
                            Margin = new Thickness(0, 18, 0, 0),
                            OnSubmit = CreateAccount,
                            IsEnabled = !IsCreatingOnlineAccount
                        },

                        new AccentButton
                        {
                            Text = IsCreatingOnlineAccount ? "Creating account..." : "Create account",
                            Click = CreateAccount,
                            Margin = new Thickness(0, 24, 0, 0),
                            IsEnabled = !IsCreatingOnlineAccount
                        },

                        IsCreateLocalAccountVisible ? new TextButton
                        {
                            Text = "No internet? Create local account",
                            HorizontalAlignment = HorizontalAlignment.Right,
                            Margin = new Thickness(0, 16, 0, 0),
                            Click = CreateLocalAccount,
                            IsEnabled = !IsCreatingOnlineAccount
                        } : null
                    }
                }
            };
        }
        public async void CreateLocalAccount()
        {
            if (DefaultAccountToUpgrade != null)
            {
                // This code should never be hit. If it does get hit, that implies the UI wasn't correctly hiding the option for
                // creating the local account (it should be hidden when upgrading a default account, only allowing online account).
                TelemetryExtension.Current?.TrackException(new Exception("Tried to create local account for default account"));
                return;
            }

            if (!ValidateAllInputs(customValidators: new Dictionary<string, Action<TextField>>()
            {
                // Email isn't required for local accounts
                { nameof(Email), f => f.Validate(overrideRequired: false) }
            }))
            {
                return;
            }

            bool shouldCreate = await new PortableMessageDialog(
                "This account will be created as a local account. You won't be able to access it from other devices.",
                "Warning: Offline Account",
                "Create",
                "Go Back")
                .ShowForResultAsync();

            if (shouldCreate)
            {
                await CreateLocalAccountAsync();
            }
        }
    }
}
