using AutoAssistantAppDataLibrary.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace AutoAssistantAppDataLibrary.DataLayer
{
    public static class Credentials
    {
        private const string Key = Secrets.AutoAssistantPrimaryKey;

        public static bool IsUsernameOkay(string username)
        {
            if (username == null || username.Length == 0)
                return false;

            if (username.Length > 50)
                return false;

            if (StringTools.IsStringFilenameSafe(username) && StringTools.IsStringUrlSafe(username))
                return true;

            return false;
        }

        public static readonly string USERNAME_ERROR = "Usernames must be 50 or fewer characters long, and can only contain letters, numbers, and the special symbols " + StringTools.ToString(StringTools.VALID_SPECIAL_URL_CHARS, ", ");

        public static string Encrypt(string password)
        {
            return EncryptionHelper.Sha256(Key + password + password.Length + EncryptionHelper.Sha256(password));
        }

        public const string UpgradedFromSilverlightUsername = "UpgradedFromSilverlight";
        public const string UpgradedFromSilverlightHashedPassword = "silverlight";
    }
}
