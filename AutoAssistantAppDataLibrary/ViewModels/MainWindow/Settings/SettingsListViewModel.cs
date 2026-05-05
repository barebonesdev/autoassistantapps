using AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen;
using BareMvvm.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoAssistantAppDataLibrary.ViewModels.MainWindow.Settings
{
    public class SettingsListViewModel : BaseViewModel
    {
        private PagedViewModel _pagedViewModel;

        public SettingsListViewModel(BaseViewModel parent) : base(parent)
        {
            HasAccount = FindAncestor<MainScreenViewModel>()?.CurrentAccount != null;
            _pagedViewModel = FindAncestor<PagedViewModel>();
        }

        public bool HasAccount { get; private set; }

        public void OpenMyAccount()
        {
            _pagedViewModel.Navigate(MyAccountViewModel.Load(_pagedViewModel));
        }

        public void OpenAbout()
        {
            _pagedViewModel.Navigate(new AboutViewModel(_pagedViewModel));
        }

        //public void OpenPremiumVersion()
        //{
        //    PowerPlannerApp.Current.PromptPurchase(null);
        //}

        //public void OpenReminderSettings()
        //{
        //    _pagedViewModel.Navigate(new ReminderSettingsViewModel(_pagedViewModel));
        //}

        //public void OpenSyncOptions()
        //{
        //    _pagedViewModel.Navigate(new SyncOptionsViewModel(_pagedViewModel));
        //}

        //public void OpenCalendarIntegration()
        //{
        //    _pagedViewModel.Navigate(new CalendarIntegrationViewModel(_pagedViewModel));
        //}

        //public void OpenTwoWeekScheduleSettings()
        //{
        //    _pagedViewModel.Navigate(new TwoWeekScheduleSettingsViewModel(_pagedViewModel));
        //}
    }
}
