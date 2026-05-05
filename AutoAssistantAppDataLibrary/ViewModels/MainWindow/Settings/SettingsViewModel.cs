using BareMvvm.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoAssistantAppDataLibrary.ViewModels.MainWindow.Settings
{
    public class SettingsViewModel : PagedViewModel
    {
        public SettingsViewModel(BaseViewModel parent) : base(parent)
        {
            Navigate(new SettingsListViewModel(this));
        }
    }
}
