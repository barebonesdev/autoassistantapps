using AutoAssistantAppDataLibrary.App;
using AutoAssistantAppDataLibrary.Extensions;
using AutoAssistantAppDataLibrary.SyncLayer;
using BareMvvm.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vx.Views;

namespace AutoAssistantAppDataLibrary.ViewModels.MainWindow.Settings
{
    public class AboutViewModel : PopupComponentViewModel
    {
        public AboutViewModel(BaseViewModel parent) : base(parent)
        {
            Title = "About";
        }

        protected override View Render()
        {
            return RenderGenericPopupContent(
                RenderHeader("Version", false),
                RenderDescription(Variables.VERSION.ToString(), isTextSelectionEnabled: true),

                RenderHeader("Developer"),
                RenderDescription("BareBones Dev, owned by Andrew Leader"),

                RenderHeader("About the app"),
                RenderDescription("This app is completely free. Primarily because I know I'm not going to have time to devote to the app. I built it because I needed a car maintenance tracking app for my own uses. I am focusing my main efforts on Power Planner and Roam Apps."),

                RenderHeader("Privacy policy"),
                new Button
                {
                    Text = "https://autoassistantapp.azurewebsites.net/privacy",
                    Click = OpenPrivacy,
                    Margin = new Thickness(0, 6, 0, 0)
                },

                RenderHeader("Contact"),
                new Button
                {
                    Text = "support@powerplanner.net",
                    Click = EmailDeveloper,
                    Margin = new Thickness(0, 6, 0, 0)
                }
            );
        }

        private TextBlock RenderHeader(string text, bool includeTopMargin = true)
        {
            return new TextBlock
            {
                Text = text,
                FontSize = Theme.Current.TitleFontSize,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, includeTopMargin ? 12 : 0, 0, 0)
            };
        }

        private TextBlock RenderDescription(string text, bool isTextSelectionEnabled = false)
        {
            return new TextBlock
            {
                Text = text,
                IsTextSelectionEnabled = isTextSelectionEnabled,
            };
        }

        private void OpenPrivacy()
        {
            _ = BrowserExtension.Current?.OpenUrlAsync(new Uri("https://autoassistantapp.azurewebsites.net/privacy"));
        }

        public static void EmailDeveloper()
        {
            try
            {
                string accountInfo = "";
                var account = AutoAssistantApp.Current.GetCurrentAccount();
                if (account != null)
                {
                    accountInfo = " - " + account.AccountId + " - " + account.DeviceId;
                }

                string subject = $"{SyncExtensions.GetAppName()} - Contact Developer - " + Variables.VERSION + accountInfo;

                _ = EmailExtension.Current.ComposeNewMailAsync("support@powerplanner.net", subject);
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }
    }
}
