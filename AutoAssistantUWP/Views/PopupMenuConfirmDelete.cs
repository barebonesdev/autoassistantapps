using AutoAssistantAppDataLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AutoAssistantUWP.Views
{
    public static class PopupMenuConfirmDelete
    {
        public static void Show(FrameworkElement el, Action onDelete)
        {
            var menuFlyout = new MenuFlyout();
            var itemYesDelete = new MenuFlyoutItem()
            {
                Text = AutoAssistantResources.GetString("String_YesDelete")
            };
            itemYesDelete.Click += delegate
            {
                onDelete();
            };
            menuFlyout.Items.Add(itemYesDelete);

            try
            {
                menuFlyout.ShowAt(el);
            }

            catch { }
        }
    }
}
