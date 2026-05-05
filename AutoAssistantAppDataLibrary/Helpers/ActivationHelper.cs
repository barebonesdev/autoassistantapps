using Microsoft.QueryStringDotNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoAssistantAppDataLibrary.Helpers
{
    public enum LaunchSurface
    {
        // Order matters
        Normal,
        SecondaryTile,
        Calendar,
        Toast,
        Uri,
        JumpList,
        PrimaryTile
    }

    public static class ArgumentsHelper
    {
        internal const string KEY_ACTION = "action";
        internal const string KEY_LAUNCH_SURFACE = "launchSurface";

        public static BaseArguments Parse(string queryString)
        {
            QueryString qs = QueryString.Parse(queryString);

            string val;
            ArgumentsAction action = ArgumentsAction.Unknown;

            if (!(qs.TryGetValue(KEY_ACTION, out val) && Enum.TryParse(val, out action)))
                return null;

            BaseArguments answer = null;

            switch (action)
            {
            }

            if (answer != null)
            {
                if (answer.TryParse(qs))
                    return answer;
            }

            return null;
        }

        public abstract class BaseArguments
        {
            internal BaseArguments() { }

            public string SerializeToString()
            {
                QueryString qs = new QueryString();

                qs.Set(ArgumentsHelper.KEY_ACTION, Action.ToString());

                if (LaunchSurface != LaunchSurface.Normal)
                {
                    qs.Set(ArgumentsHelper.KEY_LAUNCH_SURFACE, ((int)LaunchSurface).ToString());
                }

                InjectValues(qs);

                return qs.ToString();
            }

            protected virtual void InjectValues(QueryString qs) { }

            internal abstract ArgumentsAction Action { get; }

            public LaunchSurface LaunchSurface { get; set; } = LaunchSurface.Normal;

            internal virtual bool TryParse(QueryString qs)
            {
                string str = null;
                if (qs.TryGetValue(ArgumentsHelper.KEY_LAUNCH_SURFACE, out str))
                {
                    LaunchSurface launchSurface;
                    if (Enum.TryParse<LaunchSurface>(str, out launchSurface))
                    {
                        LaunchSurface = launchSurface;
                    }
                }
                return true;
            }
        }

        public abstract class BaseArgumentsWithAccount : BaseArguments
        {
            public const string KEY_ACCOUNT = "account";

            public Guid LocalAccountId { get; set; }

            protected override void InjectValues(QueryString qs)
            {
                qs.Add(KEY_ACCOUNT, LocalAccountId.ToString());
            }

            internal override bool TryParse(QueryString qs)
            {
                string val;
                Guid id;

                if (!(qs.TryGetValue(KEY_ACCOUNT, out val) && Guid.TryParse(val, out id)))
                    return false;

                LocalAccountId = id;

                return base.TryParse(qs);
            }
        }
    }

    internal enum ArgumentsAction
    {
        Unknown
    }
}
