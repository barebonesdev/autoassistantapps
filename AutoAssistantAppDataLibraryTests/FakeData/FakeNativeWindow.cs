using BareMvvm.Core.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace AutoAssistantAppDataLibraryTests.FakeData
{
    public class FakeNativeWindow : INativeAppWindow
    {
        public event EventHandler<CancelEventArgs> BackPressed;

        public void Register(PortableAppWindow portableWindow)
        {

        }
    }
}
