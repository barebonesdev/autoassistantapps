using AutoAssistantAppDataLibrary.Extensions;
using AutoAssistantAppDataLibrary.ViewModels.MainWindow.Settings;
using BareMvvm.Core.ViewModels;
using InterfacesUWP.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace AutoAssistantUWP.Views.SettingsViews
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsListView : ViewHostGeneric
    {
        public new SettingsListViewModel ViewModel
        {
            get { return base.ViewModel as SettingsListViewModel; }
            set { base.ViewModel = value; }
        }

        public SettingsListView()
        {
            this.InitializeComponent();
        }

#if DEBUG
        ~SettingsListView()
        {
            System.Diagnostics.Debug.WriteLine("SettingsListView disposed");
        }
#endif

        public override void OnViewModelLoadedOverride()
        {
            base.OnViewModelLoadedOverride();

            try
            {
                if (!ViewModel.HasAccount)
                {
                    ButtonMyAccount.Visibility = Visibility.Collapsed;
                    //ButtonCalendarIntegration.Visibility = Visibility.Collapsed;
                    //ButtonReminders.Visibility = Visibility.Collapsed;
                    //ButtonLiveTiles.Visibility = Visibility.Collapsed;
                    //ButtonSyncOptions.Visibility = Visibility.Collapsed;
                    //ButtonTwoWeekSchedule.Visibility = Visibility.Collapsed;
                }

                UpdateUpgradeToPremiumVisibility();

                //Window.Current.Activated += Current_Activated;
            }

            catch (Exception ex)
            {
                base.IsEnabled = false;
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private async void UpdateUpgradeToPremiumVisibility()
        {
            //try
            //{
            //    if (await PowerPlannerApp.Current.IsFullVersionAsync())
            //    {
            //        ButtonUpgradeToPremium.Visibility = Visibility.Collapsed;
            //    }
            //    else
            //    {
            //        ButtonUpgradeToPremium.Visibility = Visibility.Visible;
            //    }
            //}

            //catch (Exception ex)
            //{
            //    TelemetryHelper.TrackException(ex);
            //}
        }

        private void Current_Activated(object sender, Windows.UI.Core.WindowActivatedEventArgs e)
        {
            UpdateUpgradeToPremiumVisibility();
        }

        private void ButtonMyAccount_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.OpenMyAccount();
        }

        private void ButtonAbout_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.OpenAbout();
        }

        //private void ButtonUpgradeToPremium_Click(object sender, RoutedEventArgs e)
        //{
        //    ViewModel.OpenPremiumVersion();
        //}

        //private void ButtonReminders_Click(object sender, RoutedEventArgs e)
        //{
        //    ViewModel.OpenReminderSettings();
        //}

        //private void ButtonTwoWeekSchedule_Click(object sender, RoutedEventArgs e)
        //{
        //    ViewModel.OpenTwoWeekScheduleSettings();
        //}

        //private void ButtonSyncOptions_Click(object sender, RoutedEventArgs e)
        //{
        //    ViewModel.OpenSyncOptions();
        //}

        //private void ButtonLiveTiles_Click(object sender, RoutedEventArgs e)
        //{
        //    var pagedViewModel = ViewModel.FindAncestor<PagedViewModel>();
        //    pagedViewModel.Navigate(new TileSettingsViewModel(pagedViewModel));
        //}

        //private void ButtonCalendarIntegration_Click(object sender, RoutedEventArgs e)
        //{
        //    ViewModel.OpenCalendarIntegration();
        //}
    }
}
