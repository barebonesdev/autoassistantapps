using BareMvvm.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen
{
    public class BaseMainScreenViewModelDescendant : BaseViewModel
    {
        public BaseMainScreenViewModelDescendant(BaseViewModel parent) : base(parent)
        {
        }

        public virtual MainScreenViewModel MainScreenViewModel
        {
            get
            {
                if (Parent is MainScreenViewModel)
                    return Parent as MainScreenViewModel;

                if (Parent is BaseMainScreenViewModelDescendant)
                    return (Parent as BaseMainScreenViewModelDescendant).MainScreenViewModel;

                throw new NullReferenceException("Couldn't find MainScreenViewModel");
            }
        }
    }
}
