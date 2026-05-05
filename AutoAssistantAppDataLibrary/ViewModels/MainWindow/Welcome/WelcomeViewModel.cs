using AutoAssistantAppDataLibrary.ViewModels.MainWindow.Welcome.CreateAccount;
using AutoAssistantAppDataLibrary.ViewModels.MainWindow.Welcome.Login;
using BareMvvm.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoAssistantAppDataLibrary.ViewModels.MainWindow.Welcome
{
    public class WelcomeViewModel : BaseViewModel
    {
        public WelcomeViewModel(BaseViewModel parent) : base(parent) { }

        public void Login()
        {
            ShowPopup(new LoginViewModel(this));
        }

        public void CreateAccount()
        {
            ShowPopup(new CreateAccountViewModel(this));
        }

        public void OpenSettings()
        {
            //var mainWindowViewModel = this.FindAncestor<MainWindowViewModel>();
            //mainWindowViewModel.Navigate(new SettingsViewModel(mainWindowViewModel));
        }
    }
}
