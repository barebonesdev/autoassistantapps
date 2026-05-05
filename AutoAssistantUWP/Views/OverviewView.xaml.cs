using AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen.Overview;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace AutoAssistantUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class OverviewView : MainScreenContentViewHostGeneric
    {
        public OverviewView()
        {
            this.InitializeComponent();
        }

        public new OverviewViewModel ViewModel
        {
            get { return base.ViewModel as OverviewViewModel; }
            set { base.ViewModel = value; }
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Edit();
        }

        private async void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            if (await App.ConfirmDelete("Are you sure you want to delete this vehicle and all of its contents? This action is permanent.", "Delete vehicle?"))
            {
                ViewModel.Delete();
            }
        }
    }
}
