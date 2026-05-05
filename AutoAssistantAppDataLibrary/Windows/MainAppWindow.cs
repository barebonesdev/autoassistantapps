using AutoAssistantAppDataLibrary.DataLayer;
using AutoAssistantAppDataLibrary.ViewModels.MainWindow;
using AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen;
using BareMvvm.Core.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoAssistantAppDataLibrary.Windows
{
    public class MainAppWindow : PortableAppWindow
    {
        public MainAppWindow() : base()
        {
            ViewModel = new MainWindowViewModel(null);
        }

        public MainWindowViewModel GetViewModel()
        {
            return ViewModel as MainWindowViewModel;
        }

        public AccountDataItem GetCurrentAccount()
        {
            return GetViewModel().CurrentAccount;
        }

        public void ShowPopupUpdateCredentials(AccountDataItem account, UpdateCredentialsType updateType)
        {
            GetViewModel().ShowPopupUpdateCredentials(account, updateType);
        }

        public MainScreenViewModel GetMainScreenViewModel()
        {
            return GetViewModel()?.GetMainScreenViewModel();
        }
    }
}
