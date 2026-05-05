using AutoAssistantAppDataLibrary.SyncLayer;
using BareMvvm.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen
{
    public class SyncErrorsViewModel : BaseMainScreenViewModelChild
    {
        public SyncErrorsViewModel(BaseViewModel parent, LoggedError[] syncErrors) : base(parent)
        {
            SyncErrors = syncErrors;
        }

        public LoggedError[] SyncErrors { get; private set; }
    }
}
