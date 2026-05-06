using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using Vx.Views;

namespace AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen.Fuel
{
    public class ImportFuelSelectCsvViewModel : PopupComponentViewModel
    {
        public ImportFuelSelectCsvViewModel(BaseViewModel parent) : base(parent)
        {
            Title = "Import fuel records";
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
            ShowPopup(new ImportFuelPreviewImportViewModel(Parent, CsvText));
        }

        protected override View Render()
        {
            return new LinearLayout
            {
                Children =
                {
                    new MultilineTextBox
                    {
                        Text = VxValue.Create(CsvText, v => CsvText = v),
                        Margin = new Thickness(Theme.Current.PageMargin + NookInsets.Left, Theme.Current.PageMargin, Theme.Current.PageMargin + NookInsets.Right, Theme.Current.PageMargin),
                        PlaceholderText = "Enter CSV text"
                    }.LinearLayoutWeight(1),

                    new AccentButton
                    {
                        Text = "Continue",
                        Margin = new Thickness(Theme.Current.PageMargin + NookInsets.Left, 0, Theme.Current.PageMargin + NookInsets.Right, Theme.Current.PageMargin + NookInsets.Bottom),
                        Click = Continue,
                        IsEnabled = CanContinue
                    }
                }
            };
        }
    }
}
