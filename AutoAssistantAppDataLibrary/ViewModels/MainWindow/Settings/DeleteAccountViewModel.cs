using AutoAssistantAppDataLibrary.DataLayer;
using AutoAssistantLibrary.Requests;
using AutoAssistantLibrary.Responses;
using BareMvvm.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace AutoAssistantAppDataLibrary.ViewModels.MainWindow.Settings
{
    public class DeleteAccountViewModel : BaseViewModel
    {
        public AccountDataItem Account { get; private set; }

        public DeleteAccountViewModel(BaseViewModel parent, AccountDataItem account) : base(parent)
        {
            Account = account;
        }

        private bool _deleteOnlineAccountToo;
        public bool DeleteOnlineAccountToo
        {
            get { return _deleteOnlineAccountToo; }
            set { SetProperty(ref _deleteOnlineAccountToo, value, nameof(DeleteOnlineAccountToo)); }
        }

        /// <summary>
        /// This permanently deletes without any confirmation.
        /// </summary>
        /// <returns></returns>
        public async System.Threading.Tasks.Task DeleteAsync()
        {
            if (Account.IsOnlineAccount)
            {
                if (DeleteOnlineAccountToo)
                {
                    try
                    {
                        DeleteAccountResponse resp = await WebHelper.Download<DeleteAccountRequest, DeleteAccountResponse>(Website.URL + "deleteaccount", new DeleteAccountRequest() { Credentials = Account.GenerateCredentials() }, Website.ApiKey);

                        if (resp.Error != null)
                        {
                            await new PortableMessageDialog(resp.Error, AutoAssistantResources.GetString("Settings_DeleteAccountPage_Errors_ErrorDeletingHeader")).ShowAsync();
                        }

                        else
                        {
                            deleteAndFinish();
                        }
                    }

                    catch { await new PortableMessageDialog(AutoAssistantResources.GetString("Settings_DeleteAccountPage_Errors_UnknownErrorDeletingOnline"), AutoAssistantResources.GetString("Settings_DeleteAccountPage_Errors_ErrorDeletingHeader")).ShowAsync(); }
                }

                //otherwise just remove device
                else
                {
                    //no need to check whether delete device succeeded
                    try { var dontWait = WebHelper.Download<DeleteDevicesRequest, DeleteDevicesResponse>(Website.URL + "deletedevicesmodern", new DeleteDevicesRequest() { DeviceIdsToDelete = new List<int>() { Account.DeviceId }, Credentials = Account.GenerateCredentials() }, Website.ApiKey); }

                    catch { }

                    deleteAndFinish();
                }
            }

            else
            {
                deleteAndFinish();
            }
        }

        private async void deleteAndFinish()
        {
            await AccountsManager.Delete(Account.LocalAccountId);
            RemoveViewModel();
        }
    }
}
