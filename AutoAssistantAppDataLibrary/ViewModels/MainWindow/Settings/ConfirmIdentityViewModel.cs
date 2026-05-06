using AutoAssistantAppDataLibrary.DataLayer;
using AutoAssistantAppDataLibrary.ViewModels.MainWindow.Welcome.Login;
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
    public class ConfirmIdentityViewModel : PopupComponentViewModel
    {
        protected override bool InitialAllowLightDismissValue => false;
        public override bool ImportantForAutofill => true;
        private AccountDataItem _currAccount;
        public event EventHandler OnIdentityConfirmed;
        public event EventHandler ActionIncorrectPassword;

        public bool ShowForgotPassword { get; private set; }

        public ConfirmIdentityViewModel(BaseViewModel parent, AccountDataItem account) : base(parent)
        {
            _currAccount = account;

            if (_currAccount == null)
            {
                throw new InvalidOperationException("There's no current account.");
            }

            ShowForgotPassword = account.IsOnlineAccount;
            Title = "Confirm identity";
        }

        public string Password { get => GetState<string>(); set => SetState(value); }


        public bool IncorrectPassword { get => GetState<bool>(); set => SetState(value); }

        public void Continue()
        {
            if (Credentials.Encrypt(Password).Equals(_currAccount.Password))
            {
                GoBack();

                OnIdentityConfirmed?.Invoke(this, new EventArgs());
            }

            else
            {
                ActionIncorrectPassword?.Invoke(this, new EventArgs());
            }
        }

        protected override View Render()
        {
            return RenderGenericPopupContent(
                new TextBlock
                {
                    Text = "Please re-enter your login credentials."
                },

                new PasswordBox
                {
                    Text = VxValue.Create(Password, t => { Password = t; IncorrectPassword = false; }),
                    Header = "Password",
                    PlaceholderText = "Your current password",
                    Margin = new Thickness(0, 12, 0, 0),
                    AutoFocus = true,
                    OnSubmit = Continue,
                    ValidationState = IncorrectPassword ? InputValidationState.Invalid("Incorrect password") : null
                },

                ShowForgotPassword ? new TextButton
                {
                    Text = "Forgot password?",
                    Margin = new Thickness(0, 6, 0, 0),
                    Click = ForgotPassword,
                    HorizontalAlignment = HorizontalAlignment.Left
                } : null,

                new AccentButton
                {
                    Text = "Continue",
                    Click = Continue,
                    Margin = new Thickness(0, 12, 0, 0)
                }
            );
        }

        public void ForgotPassword()
        {
            ShowPopup(new ResetPasswordViewModel(GetPopupViewModelHost(), _currAccount.Username));
        }
    }
}
