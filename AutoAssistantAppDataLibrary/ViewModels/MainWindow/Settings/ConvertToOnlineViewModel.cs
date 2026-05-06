using AutoAssistantAppDataLibrary.DataLayer;
using AutoAssistantAppDataLibrary.Extensions;
using AutoAssistantAppDataLibrary.SyncLayer;
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
    public class ConvertToOnlineViewModel : PopupComponentViewModel
    {
        protected override bool InitialAllowLightDismissValue => false;
        public override bool ImportantForAutofill => true;
        public AccountDataItem Account { get; private set; }

        public ConvertToOnlineViewModel(BaseViewModel parent, AccountDataItem account) : base(parent)
        {
            Account = account;

            Title = "Convert to online account";
        }

        protected override View Render()
        {
            bool isEnabled = !IsConverting;

            return RenderGenericPopupContent(

                new TextBox(Email)
                {
                    Header = "Email address",
                    PlaceholderText = "For recovery purposes",
                    AutoFocus = true,
                    InputScope = InputScope.Email,
                    OnSubmit = () => _ = CreateOnlineAccountAsync(),
                    IsEnabled = isEnabled
                },

                new AccentButton
                {
                    Text = "Convert to online",
                    Margin = new Thickness(0, 24, 0, 0),
                    Click = () => _ = CreateOnlineAccountAsync(),
                    IsEnabled = isEnabled
                }

            );
        }

        [VxSubscribe]
        public TextField Email { get; private set; } = CreateAccountViewModel.GenerateEmailTextField();

        private bool _showConfirmMergeExisting;
        public bool ShowConfirmMergeExisting
        {
            get { return _showConfirmMergeExisting; }
            set { SetProperty(ref _showConfirmMergeExisting, value, nameof(ShowConfirmMergeExisting)); }
        }

        public bool IsConverting { get => GetState<bool>(); set => SetState(value); }

        private CreateAccountResponse _response;
        public async System.Threading.Tasks.Task CreateOnlineAccountAsync()
        {
            if (!ValidateAllInputs())
            {
                return;
            }

            var currAccount = Account;

            string email = Email.Text;

            try
            {
                _response = await WebHelper.Download<CreateAccountRequest, CreateAccountResponse>(
                    Website.URL + "createaccount",
                    new CreateAccountRequest()
                    {
                        Username = currAccount.Username,
                        Password = currAccount.Password,
                        Email = email,
                        AddDevice = true
                    }, Website.ApiKey);

                if (_response.Error != null)
                {
                    Email.SetError(_response.Error);
                }

                else
                {
                    if (_response.ExistsButCredentialsMatched)
                    {
                        ShowConfirmMergeExisting = true;
                        return;
                    }

                    await finishConvertingToOnline(currAccount);
                    RemoveViewModel();
                    return;
                }

                return;
            }

            catch { }

            finally
            {
                IsConverting = false;
            }

            Email.SetError("Failed to create online account.");
        }

        public async System.Threading.Tasks.Task MergeExisting()
        {
            if (_response == null)
            {
                return;
            }

            await finishConvertingToOnline(Account);
            RemoveViewModel();
        }

        public void CancelMergeExisting()
        {
            _response = null;
            ShowConfirmMergeExisting = false;
        }

        private async System.Threading.Tasks.Task finishConvertingToOnline(AccountDataItem account)
        {
            account.AccountId = _response.AccountId;
            account.DeviceId = _response.DeviceId;

            await AccountsManager.Save(account);

            // Transfer the settings
            //try
            //{
            //    await SavedGradeScalesManager.TransferToOnlineAccountAsync(account.LocalAccountId, account.AccountId);
            //}

            //catch (Exception ex)
            //{
            //    TelemetryExtension.Current?.TrackException(ex);
            //}

            //have it sync
            SyncWithoutBlocking(account);
        }

        private async void SyncWithoutBlocking(AccountDataItem account)
        {
            try
            {
                await Sync.SyncAccountAsync(account);
            }

            catch (OperationCanceledException) { }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex, Extensions.Telemetry.SeverityLevel.Error);
            }
        }
    }
}
