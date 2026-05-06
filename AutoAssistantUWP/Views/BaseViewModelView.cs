using BareMvvm.Core.ViewModels;
using InterfacesUWP.Views;
using System;
using System.Collections.Generic;
using System.Text;
using Vx.Uwp;
using Windows.UI.Xaml.Media.Animation;

namespace AutoAssistantUWP.Views
{
    internal class BaseViewModelView : ViewHostGeneric
    {
        public BaseViewModelView()
        {
            Transitions.Add(new EntranceThemeTransition());
        }

        public new BaseViewModel ViewModel
        {
            get => base.ViewModel as BaseViewModel;
            set => base.ViewModel = value;
        }

        public override void OnViewModelSetOverride()
        {
            base.OnViewModelSetOverride();

            Content = ViewModel.Render();
        }
    }
}
