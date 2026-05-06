using AutoAssistantAppDataLibrary.DataLayer;
using AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen;
using AutoAssistantAppDataLibrary.ViewModels.MainWindow.Welcome;
using BareMvvm.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoAssistantAppDataLibrary.ViewModels.MainWindow
{
    public class MainWindowViewModel : PagedViewModelWithPopups
    {
        public static event EventHandler<AccountDataItem> LoggedInFromNormalActivation;

        public AccountDataItem CurrentAccount { get; private set; }

        private bool _isEnabled = true;

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value, "IsEnabled"); }
        }

        public MainWindowViewModel(BaseViewModel parent) : base(parent)
        {
            AccountsManager.OnAccountDeleted += AccountsManager_OnAccountDeleted;
        }

        private void AccountsManager_OnAccountDeleted(object sender, Guid localAccountId)
        {
            if (CurrentAccount != null && CurrentAccount.LocalAccountId == localAccountId)
            {
                Dispatcher.Run(delegate
                {
                    var dontWait = SetCurrentAccount(null);
                });
            }
        }

        /// <summary>
        /// Assumes already on UI thread.
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public async Task SetCurrentAccount(AccountDataItem account, bool syncAccount = true)
        {
            base.Popups.Clear();

            CurrentAccount = account;

            if (account == null)
                base.Replace(new WelcomeViewModel(this));

            else
            {
                try
                {
                    IsEnabled = false;
                    base.ClearContentAndBackStack();
                    base.Replace(await MainScreenViewModel.LoadAsync(this, account, syncAccount: syncAccount));
                }

                finally
                {
                    IsEnabled = true;
                }
            }
        }

        public void ShowPopupUpdateCredentials(AccountDataItem account, UpdateCredentialsType updateType)
        {
            // TODO
        }

        public async Task HandleNormalLaunchActivation()
        {
            // Restore previous login
            AccountDataItem lastAccount = await AccountsManager.GetLastLogin();
            if (lastAccount != null && lastAccount.IsAutoLoginPossible && lastAccount.AutoLogin)
            {
                await this.SetCurrentAccount(lastAccount);
                LoggedInFromNormalActivation?.Invoke(this, lastAccount);
            }

            else
                await this.SetCurrentAccount(null);
        }

        private DateTime _timeLeftAt = DateTime.Now;
        public void HandleBeingLeft()
        {
            _timeLeftAt = DateTime.Now;
        }

        public async Task HandleBeingReturnedTo()
        {
            // If the day has changed (went from yesterday to today), reset the entire view model
            if (_timeLeftAt.Date != DateTime.Now.Date || AccountDataStore.RetrieveAndResetWasUpdatedByBackgroundTask())
            {
                await HandleNormalLaunchActivation();
                return;
            }

            // If it's been gone for more than a minute, do a sync
            if (DateTime.Now.AddMinutes(-1) > _timeLeftAt)
            {
                try
                {
                    if (CurrentAccount != null)
                    {
                        try
                        {
                            var dontWait = SyncLayer.Sync.SyncAccountAsync(CurrentAccount);
                        }
                        catch { }
                    }
                }

                catch { }
            }
        }

        public MainScreenViewModel GetMainScreenViewModel()
        {
            return Content as MainScreenViewModel;
        }
    }
}
