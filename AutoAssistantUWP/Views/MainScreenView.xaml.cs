using AutoAssistantAppDataLibrary.DataLayer;
using AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen;
using InterfacesUWP;
using InterfacesUWP.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using ToolsPortable;
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
    public sealed partial class MainScreenView : ViewHostGeneric
    {
        public new MainScreenViewModel ViewModel
        {
            get { return base.ViewModel as MainScreenViewModel; }
            set { base.ViewModel = value; }
        }

        public class Blah
        {
            public string Title { get; set; }
            public string Glyph { get; set; }
        }

        public MainScreenView()
        {
            this.InitializeComponent();

            DataContextChanged += MainScreenView_DataContextChanged;
        }

        private void MainScreenView_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            // Have to do this in Loaded, since otherwise data binding hasn't taken effect yet and the list
            // doesn't have any items, so setting selected item does nothing
            UpdateSelectedItem();
        }

        public override void OnViewModelSetOverride()
        {
            base.OnViewModelSetOverride();

            ViewModel.PropertyChanged += new WeakEventHandler<PropertyChangedEventArgs>(ViewModel_PropertyChanged).Handler;

            UpdateSyncingAnimation();
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.IsIndeterminateSyncing):
                    UpdateSyncingAnimation();
                    break;

                case nameof(ViewModel.SelectedItem):
                    UpdateSelectedItem();
                    break;
            }
        }

        private void UpdateSelectedItem()
        {
            if (ViewModel.SelectedItem == null || ViewModel.SelectedItem == NavigationManager.MainMenuSelections.Settings)
            {
                ListViewMainMenuItems.SelectedItem = null;
            }
            else
            {
                ListViewMainMenuItems.SelectedItem = ViewModel.SelectedItem;
            }

            CloseMenuIfOverlay();
        }

        private void CloseMenuIfOverlay()
        {
            if ((SplitViewMenu.DisplayMode == SplitViewDisplayMode.Overlay || SplitViewMenu.DisplayMode == SplitViewDisplayMode.CompactOverlay)
                && SplitViewMenu.IsPaneOpen)
            {
                SplitViewMenu.IsPaneOpen = false;
            }
        }

        private bool _isSyncingAnimationRunning;
        private void UpdateSyncingAnimation()
        {
            if (ViewModel.IsIndeterminateSyncing)
            {
                if (!_isSyncingAnimationRunning)
                {
                    _isSyncingAnimationRunning = true;
                    StoryboardSyncing.Begin();
                }
            }
            else
            {
                _isSyncingAnimationRunning = false;
                StoryboardSyncing.Stop();
            }
        }

        public void ToggleMenu()
        {
            SplitViewMenu.IsPaneOpen = !SplitViewMenu.IsPaneOpen;
        }

        private void ButtonMenu_Click(object sender, RoutedEventArgs e)
        {
            ToggleMenu();
        }

        private void ListViewMainMenuItems_ItemClick(object sender, ItemClickEventArgs e)
        {
            ViewModel.SelectedItem = (NavigationManager.MainMenuSelections)e.ClickedItem;
            CloseMenuIfOverlay();
        }

        public class MainMenuItem
        {
            public string Glyph { get; set; }

            public string Title { get; set; }
        }

        private void ButtonSyncErrors_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ViewSyncErrors();
        }

        private void ButtonSync_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SyncCurrentAccount();
        }

        private void ButtonSettings_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ViewSettings();
            CloseMenuIfOverlay();
        }

        private void ThisView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            switch (DeviceInfo.GetCurrentDeviceFormFactor())
            {
                case DeviceFormFactor.Mobile:
                    SplitViewMenu.DisplayMode = SplitViewDisplayMode.Overlay;
                    break;

                default:
                    if (e.NewSize.Width > 1060)
                        SplitViewMenu.DisplayMode = SplitViewDisplayMode.CompactInline;
                    else
                        SplitViewMenu.DisplayMode = SplitViewDisplayMode.CompactOverlay;
                    break;
            }
        }
    }
}
