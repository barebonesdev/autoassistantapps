using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;

namespace AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen.Fuel
{
    public class ImportFuelSelectCsvViewModel : BaseMainScreenViewModelChild
    {
        public ImportFuelSelectCsvViewModel(BaseViewModel parent) : base(parent)
        {
        }

        private string _csvText = "";
        public string CsvText
        {
            get { return _csvText; }
            set { SetProperty(ref _csvText, value, nameof(CsvText)); CanContinue = !string.IsNullOrWhiteSpace(value); }
        }

        private bool _canContinue = false;
        public bool CanContinue
        {
            get { return _canContinue; }
            private set { SetProperty(ref _canContinue, value, nameof(CanContinue)); }
        }

        public void Continue()
        {
            MainScreenViewModel.ShowPopup(new ImportFuelPreviewImportViewModel(MainScreenViewModel, CsvText));
        }
    }
}
