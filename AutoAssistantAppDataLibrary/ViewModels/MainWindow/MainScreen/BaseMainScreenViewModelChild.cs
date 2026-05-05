using BareMvvm.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen
{
    public abstract class BaseMainScreenViewModelChild : BaseMainScreenViewModelDescendant
    {
        public BaseMainScreenViewModelChild(BaseViewModel parent) : base(parent) { }

        public override MainScreenViewModel MainScreenViewModel
        {
            get { return (MainScreenViewModel)Parent; }
        }

        private List<MainScreenViewModel.ChangedItemListener> _listeners = new List<MainScreenViewModel.ChangedItemListener>();
        protected MainScreenViewModel.ChangedItemListener ListenToItem(Guid itemIdentifier)
        {
            var listener = MainScreenViewModel.ListenToItem(itemIdentifier);

            // We add to an instance variable list, so that the reference won't get lost until the view model gets destroyed
            _listeners.Add(listener);

            return listener;
        }
    }
}
