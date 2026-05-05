using AutoAssistantAppDataLibrary.ViewModels.MainWindow.Settings;
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
    public sealed partial class DeleteAccountView : PopupViewHostGeneric
    {
        public new DeleteAccountViewModel ViewModel
        {
            get { return base.ViewModel as DeleteAccountViewModel; }
            set { base.ViewModel = value; }
        }

        public DeleteAccountView()
        {
            this.InitializeComponent();
        }

        public override void OnViewModelLoadedOverride()
        {
            base.OnViewModelLoadedOverride();

            checkBoxDeleteOnlineToo.Visibility = ViewModel.Account.IsOnlineAccount ? Visibility.Visible : Visibility.Collapsed;
        }

        private async void buttonConfirmDelete_Click(object sender, RoutedEventArgs e)
        {
            IsEnabled = false;
            await ViewModel.DeleteAsync();
            IsEnabled = true;
        }
    }
}
