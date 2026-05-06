using AutoAssistantAppDataLibrary.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace AutoAssistantAppDataLibrary
{
    public static class Website
    {
#if DEBUG
        public const string URL = "https://autoassistantapp.azurewebsites.net/api/";
        //public const string URL = "https://autoassistantapp-staging.azurewebsites.net/api/";
        //public const string URL = "http://localhost:6384/api/";
#else
        public const string URL = "https://autoassistantapp.azurewebsites.net/api/";
#endif

        public static readonly ApiKeyCombo ApiKey = new ApiKeyCombo(Secrets.AutoAssistantApiKey, Secrets.AutoAssistantApiHashedKey);

        //public static async Task<ForgotUsernameResponse> ForgotUsername(string email)
        //{
        //    return await WebHelper.Download<ForgotUsernameRequest, ForgotUsernameResponse>(URL + "forgotusernamemodern", new ForgotUsernameRequest()
        //    {
        //        Email = email
        //    }, ApiKey);
        //}


        /// <summary>
        /// Returns something like https://powerplannerstorage.blob.core.windows.net/modern-91353/Images/635121668276610139-58978167.jpg
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="image"></param>
        /// <returns></returns>
        //public static string GetImageUrl(long accountId, string image)
        //{
        //    //api/getimagemodern/[AccountId]_[ImageName]
        //    //return IMAGE_URL + accountId + "_" + image;

        //    return "https://powerplannerstorage.blob.core.windows.net/modern-" + accountId + "/Images/" + image;
        //}
    }
}
