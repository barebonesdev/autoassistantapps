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
using Vx.Views;

namespace AutoAssistantAppDataLibrary.ViewModels.MainWindow.Settings
{
    public class ChangeUsernameViewModel : PopupComponentViewModel
    {
        protected override bool InitialAllowLightDismissValue => false;
        public override bool ImportantForAutofill => true;

        public event EventHandler<string> ActionError;

        public AccountDataItem Account { get; private set; }

        public ChangeUsernameViewModel(BaseViewModel parent, AccountDataItem account) : base(parent)
        {
            Account = account;
            Username = account.Username;
            Title = "Change username";
        }

        protected override View Render()
        {
            return RenderGenericPopupContent(
                new TextBox
                {
                    Header = "New username",
                    Text = VxValue.Create(Username, t => { Username = t; Error = null; }),
                    AutoFocus = true,
                    OnSubmit = Update,
                    IsEnabled = !IsUpdatingUsername,
                    ValidationState = Error != null ? InputValidationState.Invalid(Error) : null
                },

                new AccentButton
                {
                    Text = "Update username",
                    Click = Update,
                    Margin = new Thickness(0, 24, 0, 0)
                }
            );
        }

        public string Username { get => GetState<string>(); set => SetState(value); }

        public bool IsUpdatingUsername { get => GetState<bool>(); set => SetState(value); }

        public string Error { get => GetState<string>(); set => SetState(value); }

        public async void Update()
        {
            Username = Username.Trim();
            IsUpdatingUsername = true;
            SetError(null);

            try
            {
                try
                {
                    await AccountsManager.ValidateUsername(Username);
                }

                catch (AccountsManager.UsernameExistsLocallyException)
                {
                    SetError(AutoAssistantResources.GetString("Settings_ChangeUsernamePage_Errors_UsernameExists"));
                    return;
                }

                catch (AccountsManager.UsernameInvalidException)
                {
                    SetError(Credentials.USERNAME_ERROR);
                    return;
                }

                catch (AccountsManager.UsernameWasEmptyException)
                {
                    SetError(AutoAssistantResources.GetString("Settings_ChangeUsernamePage_Errors_NoUsername"));
                    return;
                }

                catch (Exception ex)
                {
                    SetError(ex.ToString());
                    return;
                }


                if (Account.IsOnlineAccount)
                {
                    try
                    {
                        ChangeUsernameResponse resp = await WebHelper.Download<ChangeUsernameRequest, ChangeUsernameResponse>(Website.URL + "changeusername", new ChangeUsernameRequest()
                        {
                            Credentials = Account.GenerateCredentials(),
                            NewUsername = Username
                        }, Website.ApiKey);

                        if (resp.Error != null)
                        {
                            SetError(resp.Error);
                            return;
                        }

                        await changeLocalUsername(Username);
                        GoBack();
                    }

                    catch
                    {
                        SetError(AutoAssistantResources.GetString("Settings_ChangeUsernamePage_Errors_FailedUpdateOnline"));
                    }
                }

                else
                {
                    await changeLocalUsername(Username);
                    GoBack();
                }
            }

            finally
            {
                IsUpdatingUsername = false;
            }
        }

        private async System.Threading.Tasks.Task changeLocalUsername(string newUsername)
        {
            Account.Username = newUsername;
            await AccountsManager.Save(Account);
        }

        private void SetError(string error)
        {
            ActionError?.Invoke(this, error);
        }
    }
}
