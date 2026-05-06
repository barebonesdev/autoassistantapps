using BareMvvm.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vx.Views;

namespace AutoAssistantAppDataLibrary.ViewModels.MainWindow.Welcome.Login
{
    public class RecoveredUsernamesViewModel : PopupComponentViewModel
    {
        public RecoveredUsernamesViewModel(BaseViewModel parent, string[] usernames) : base(parent)
        {
            Title = "Usernames";

            Usernames = usernames;

            var loginViewModel = parent.GetPopupViewModelHost()?.Popups.OfType<LoginViewModel>().FirstOrDefault();
            if (loginViewModel != null && usernames.Length > 0)
            {
                loginViewModel.Username = usernames[0];
            }
        }

        public string[] Usernames { get; private set; }

        protected override View Render()
        {
            var elements = new List<View>();
            elements.Add(new TextBlock
            {
                Text = "Your usernames are...",
                Margin = new Thickness(0, 0, 0, 12)
            });

            foreach (var username in Usernames)
            {
                elements.Add(new TextBlock
                {
                    Text = username
                });
            }

            elements.Add(new Button
            {
                Text = "Back to login",
                Margin = new Thickness(0, 12, 0, 0),
                Click = RemoveViewModel
            });

            return RenderGenericPopupContent(elements);
        }
    }
}
