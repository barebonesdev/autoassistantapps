using AutoAssistantAppDataLibrary.ViewItems;
using AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen.Garage;
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
    public sealed partial class GarageView : MainScreenContentViewHostGeneric
    {
        public GarageView()
        {
            this.InitializeComponent();
        }

        public new GarageViewModel ViewModel
        {
            get { return base.ViewModel as GarageViewModel; }
            set { base.ViewModel = value; }
        }

        private void ButtonAddVehicle_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.AddVehicle();
        }

        private void ListViewVehicles_ItemClick(object sender, ItemClickEventArgs e)
        {
            ViewModel.OpenVehicle(e.ClickedItem as ViewItemVehicle);
        }
    }
}
