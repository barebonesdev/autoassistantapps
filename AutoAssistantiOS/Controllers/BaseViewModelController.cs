using BareMvvm.Core.ViewModels;
using InterfacesiOS.Controllers;
using System;
using System.Collections.Generic;
using System.Text;
using Vx.iOS;
using Vx.iOS.Views;

namespace AutoAssistantiOS.Controllers
{
    internal class BaseViewModelController : BareMvvmUIViewController<BaseViewModel>
    {
        public override void OnViewModelSetOverride()
        {
            base.OnViewModelSetOverride();

            UpdateNookInsets();

            var renderedComponent = ViewModel.Render();
            renderedComponent.TranslatesAutoresizingMaskIntoConstraints = false;
            View.Add(renderedComponent);
            renderedComponent.StretchWidthAndHeight(View);
        }

        /// <summary>
        /// This method only exists on iOS 11+, not sure if this class will still work on iOS 10/9, but not sure how to dynamically overload a method...
        /// </summary>
        public override void ViewSafeAreaInsetsDidChange()
        {
            if (ViewModel != null)
            {
                UpdateNookInsets();
            }
        }

        private void UpdateNookInsets()
        {
            ViewModel.UpdateNookInsets(new Vx.Views.Thickness((float)View.SafeAreaInsets.Left, (float)View.SafeAreaInsets.Top, (float)View.SafeAreaInsets.Right, (float)View.SafeAreaInsets.Bottom));
        }
    }
}
