using AutoAssistantAppDataLibrary.DataLayer;
using AutoAssistantAppDataLibrary.Extensions;
using AutoAssistantLibrary.Requests;
using AutoAssistantLibrary.Responses;
using BareMvvm.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using Vx.Views;

namespace AutoAssistantAppDataLibrary.ViewModels.MainWindow.Settings
{
    public class MyAccountViewModel : PopupComponentViewModel
    {
        private static DateTime _timeLastConfirmed = DateTime.MinValue;
        private static Guid _lastConfrimedAccountId;

        public AccountDataItem CurrentAccount { get; private set; }

        private MyAccountViewModel(BaseViewModel parent) : base(parent)
        {
            Title = "My Account";
        }

        public static MyAccountViewModel Load(BaseViewModel parent)
        {
            MainWindowViewModel windowViewModel = parent.FindAncestor<MainWindowViewModel>();

            if (windowViewModel == null)
            {
                throw new NullReferenceException("Could not find MainWindowViewModel ancestor");
            }

            if (windowViewModel.CurrentAccount == null)
            {
                throw new InvalidOperationException("There's no current account.");
            }

            return new MyAccountViewModel(parent)
            {
                CurrentAccount = windowViewModel.CurrentAccount,
                _rememberUsername = windowViewModel.CurrentAccount.RememberUsername,
                _rememberPassword = windowViewModel.CurrentAccount.RememberPassword,
                _autoLogin = windowViewModel.CurrentAccount.AutoLogin
            };
        }

        private bool _rememberUsername;
        public bool RememberUsername
        {
            get { return _rememberUsername; }
            set
            {
                if (_rememberUsername != value)
                {
                    _rememberUsername = value;
                    OnPropertyChanged(nameof(RememberUsername));
                }

                if (CurrentAccount.RememberUsername != value)
                {
                    CurrentAccount.RememberUsername = value;
                    SaveChanges();
                }
            }
        }

        private bool _rememberPassword;
        public bool RememberPassword
        {
            get { return _rememberPassword; }
            set
            {
                if (_rememberPassword != value)
                {
                    _rememberPassword = value;
                    OnPropertyChanged(nameof(RememberPassword));
                }

                if (CurrentAccount.RememberPassword != value)
                {
                    CurrentAccount.RememberPassword = value;
                    SaveChanges();
                }
            }
        }

        private bool _autoLogin;
        public bool AutoLogin
        {
            get { return _autoLogin; }
            set
            {
                if (_autoLogin != value)
                {
                    _autoLogin = value;
                    OnPropertyChanged(nameof(AutoLogin));
                }

                if (CurrentAccount.AutoLogin != value)
                {
                    CurrentAccount.AutoLogin = value;
                    SaveChanges();
                }
            }
        }

        private async void SaveChanges()
        {
            try
            {
                await AccountsManager.Save(CurrentAccount);
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex, Extensions.Telemetry.SeverityLevel.Critical);
            }
        }

        public async void LogOut()
        {
            AccountsManager.SetLastLoginIdentifier(Guid.Empty);
            await FindAncestor<MainWindowViewModel>().SetCurrentAccount(null);
        }

        public void ChangeUsername()
        {
            ConfirmIdentityAndThen(delegate
            {
                ShowPopup(new ChangeUsernameViewModel(GetPopupViewModelHost(), CurrentAccount));
            });
        }

        public void ChangePassword()
        {
            ConfirmIdentityAndThen(delegate
            {
                ShowPopup(new ChangePasswordViewModel(GetPopupViewModelHost(), CurrentAccount));
            });
        }

        public void ChangeEmail()
        {
            ConfirmIdentityAndThen(delegate
            {
                ShowPopup(new ChangeEmailViewModel(GetPopupViewModelHost(), CurrentAccount));
            });
        }

        public void PromptConfirmDelete()
        {
            ConfirmIdentityAndThen(delegate
            {
                ShowPopup(new DeleteAccountViewModel(GetPopupViewModelHost(), CurrentAccount));
            });
        }

        public void ConvertToOnline()
        {
            if (CurrentAccount.Username.Equals(Credentials.UpgradedFromSilverlightUsername, StringComparison.CurrentCultureIgnoreCase))
            {
                var dontWait = new PortableMessageDialog("You must first change your username to a unique username.", "No username").ShowAsync();
                return;
            }

            if (CurrentAccount.Password == Credentials.UpgradedFromSilverlightHashedPassword)
            {
                var dontWait = new PortableMessageDialog("You must first change your password to a unique password.", "No password").ShowAsync();
                return;
            }

            ConfirmIdentityAndThen(delegate
            {
                ShowPopup(new ConvertToOnlineViewModel(GetPopupViewModelHost(), CurrentAccount));
            });
        }

        public void ConfirmIdentityAndThen(Action action)
        {
            if ((_lastConfrimedAccountId == CurrentAccount.LocalAccountId && _timeLastConfirmed.AddMinutes(10) > DateTime.Now)
                || CurrentAccount.Password == Credentials.UpgradedFromSilverlightHashedPassword)
            {
                if (CurrentAccount.Password == Credentials.UpgradedFromSilverlightHashedPassword)
                {
                    _lastConfrimedAccountId = CurrentAccount.LocalAccountId; _timeLastConfirmed = DateTime.Now;
                }

                action.Invoke();
                return;
            }

            var confirmViewModel = new ConfirmIdentityViewModel(GetPopupViewModelHost(), CurrentAccount);
            confirmViewModel.OnIdentityConfirmed += delegate { _lastConfrimedAccountId = CurrentAccount.LocalAccountId; _timeLastConfirmed = DateTime.Now; action.Invoke(); };
            ShowPopup(confirmViewModel);
        }

        public async Task DeleteAccount(bool deleteOnlineToo)
        {
            if (CurrentAccount.IsOnlineAccount)
            {
                if (deleteOnlineToo)
                {
                    try
                    {
                        DeleteAccountResponse resp = await WebHelper.Download<DeleteAccountRequest, DeleteAccountResponse>(Website.URL + "deleteaccount", new DeleteAccountRequest() { Credentials = CurrentAccount.GenerateCredentials() }, Website.ApiKey);

                        if (resp.Error != null)
                        {
                            await new PortableMessageDialog(resp.Error, AutoAssistantResources.GetString("Settings_DeleteAccountPage_Errors_ErrorDeletingHeader")).ShowAsync();
                        }

                        else
                        {
                            deleteAndFinish(CurrentAccount);
                        }
                    }

                    catch { await new PortableMessageDialog(AutoAssistantResources.GetString("Settings_DeleteAccountPage_Errors_UnknownErrorDeletingOnline"), AutoAssistantResources.GetString("Settings_DeleteAccountPage_Errors_ErrorDeletingHeader")).ShowAsync(); }

                    finally { }
                }

                //otherwise just remove device
                else
                {
                    //no need to check whether delete device succeeded
                    try { var dontWait = WebHelper.Download<DeleteDevicesRequest, DeleteDevicesResponse>(Website.URL + "deletedevices", new DeleteDevicesRequest() { DeviceIdsToDelete = new List<int>() { CurrentAccount.DeviceId }, Credentials = CurrentAccount.GenerateCredentials() }, Website.ApiKey); }

                    catch { }

                    deleteAndFinish(CurrentAccount);
                }
            }

            else
            {
                deleteAndFinish(CurrentAccount);
            }
        }

        private async void deleteAndFinish(AccountDataItem account)
        {
            await AccountsManager.Delete(account.LocalAccountId);
        }

        protected override View Render()
        {
            return RenderGenericPopupContent(

                new LinearLayout
                {
                    Orientation = Orientation.Horizontal,
                    Children =
                    {
                        new TextBlock
                        {
                            Text = "Username:",
                            FontWeight = FontWeights.Bold,
                            WrapText = false
                        },

                        new TextBlock
                        {
                            Text = CurrentAccount.Username,
                            IsTextSelectionEnabled = true,
                            Margin = new Thickness(4, 0, 0, 0)
                        }.LinearLayoutWeight(1),

                    }
                },

                new Button
                {
                    Text = "Log out",
                    Click = LogOut,
                    Margin = new Thickness(0, 24, 0, 0)
                },

                new Button
                {
                    Text = "Change username",
                    Click = ChangeUsername,
                    Margin = new Thickness(0, 12, 0, 0)
                },

                new Button
                {
                    Text = "Change password",
                    Click = ChangePassword,
                    Margin = new Thickness(0, 12, 0, 0)
                },

                CurrentAccount.IsOnlineAccount ? new Button
                {
                    Text = "Change email",
                    Click = ChangeEmail,
                    Margin = new Thickness(0, 12, 0, 0)
                } : null,

                new CheckBox
                {
                    Text = "Remember username",
                    IsChecked = VxValue.Create(RememberUsername, v => RememberUsername = v),
                    Margin = new Thickness(0, 24, 0, 0)
                },

                new CheckBox
                {
                    Text = "Remember password",
                    IsChecked = VxValue.Create(RememberPassword, v => RememberPassword = v),
                    IsEnabled = CurrentAccount.IsRememberPasswordPossible,
                    Margin = new Thickness(0, 6, 0, 0)
                },

                new CheckBox
                {
                    Text = "Automatically log in",
                    IsChecked = VxValue.Create(AutoLogin, v => AutoLogin = v),
                    IsEnabled = CurrentAccount.IsAutoLoginPossible,
                    Margin = new Thickness(0, 6, 0, 0)
                },

                CurrentAccount.IsOnlineAccount ? null : new AccentButton
                {
                    Text = "Convert to online account",
                    Margin = new Thickness(0, 24, 0, 0),
                    Click = ConvertToOnline
                },

                new AccentButton
                {
                    Text = "Delete account",
                    Margin = new Thickness(0, CurrentAccount.IsOnlineAccount ? 24 : 12, 0, 0),
                    Click = PromptConfirmDelete
                }
            );
        }
    }
}
