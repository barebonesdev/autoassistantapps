using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;

namespace AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen.Fuel
{
    public class ImportFuelIntroViewModel : BaseMainScreenViewModelChild
    {
        public ImportFuelIntroViewModel(BaseViewModel parent) : base(parent)
        {
        }

        public void Next()
        {
            MainScreenViewModel.ShowPopup(new ImportFuelSelectCsvViewModel(MainScreenViewModel));
        }
    }
}
