using AutoAssistantAppDataLibrary.ViewItems;
using AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen.Maintenance;
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

namespace AutoAssistantUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SearchMaintenanceRecordsView : PopupViewHostGeneric
    {
        public SearchMaintenanceRecordsViewModel ViewModel
        {
            get { return base.ViewModel as SearchMaintenanceRecordsViewModel; }
            set { base.ViewModel = value; }
        }

        public SearchMaintenanceRecordsView()
        {
            this.InitializeComponent();
        }

        private void ListViewResults_ItemClick(object sender, ItemClickEventArgs e)
        {
            ViewModel.ViewMaintenanceRecord(e.ClickedItem as ViewItemMaintenanceRecordEntry);
        }

        private void TextBoxSearch_Loaded(object sender, RoutedEventArgs e)
        {
            TextBoxSearch.Focus(FocusState.Programmatic);
        }
    }
}
