using AutoAssistantAppDataLibrary.ViewModels.MainWindow.Welcome.CreateAccount;
using AutoAssistantAppDataLibrary.ViewModels.MainWindow.Welcome.Login;
using BareMvvm.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vx.Views;

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

        protected override View Render()
        {
            return new LinearLayout
            {
                Margin = new Thickness(Theme.Current.PageMargin),
                Orientation = Orientation.Vertical,
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    new TextBlock
                    {
                        Text = "Auto Assistant",
                        TextAlignment = HorizontalAlignment.Center,
                        FontSize = 48,
                        FontWeight = FontWeights.SemiLight
                    },

                    new TextBlock
                    {
                        Text = "The ultimate vehicle maintenance tracker.",
                        TextAlignment = HorizontalAlignment.Center
                    },

                    new LinearLayout
                    {
                        Margin = new Thickness(0, 12, 0, 0),
                        Orientation = Orientation.Horizontal,
                        Children =
                        {
                            new Button
                            {
                                Text = "LOG IN",
                                Margin = new Thickness(0, 0, 6, 0),
                                Click = Login
                            }.LinearLayoutWeight(1),
                            new Button
                            {
                                Text = "CREATE ACCOUNT",
                                Margin = new Thickness(6, 0, 0, 0),
                                Click = CreateAccount
                            }.LinearLayoutWeight(1)
                        }
                    }
                }
            };
        }
    }
}
