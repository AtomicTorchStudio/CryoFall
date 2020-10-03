namespace AtomicTorch.CBND.CoreMod.Systems.ServerWelcomeMessage
{
    using System;
    using System.Linq;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Servers;

    public static class NextWipeDateServerTagHelper
    {
        private const string TagKey = "NextWipeDate=";

        public static DateTime? ClientGetServerNextWipeDateUtc(ServerInfo serverInfo)
        {
            var tag = serverInfo.ScriptingTags.FirstOrDefault(t => t.StartsWith(TagKey));
            try
            {
                if (tag is null)
                {
                    return null;
                }

                var ticksStr = tag.Substring(startIndex: tag.IndexOf('=') + 1);
                return new DateTime(ticks: long.Parse(ticksStr),
                                    DateTimeKind.Utc);
            }
            catch
            {
                return null;
            }
        }

        public static void ServerRefreshServerInfoTagForNextWipeDate()
        {
            var existingTag = Api.Server.Core.ServerInfoTags.FirstOrDefault(t => t.StartsWith(TagKey));
            if (existingTag is not null)
            {
                Api.Server.Core.RemoveServerInfoTag(existingTag);
            }

            var wipeDateUtc = WelcomeMessageSystem.ServerScheduledWipeDateUtc;
            if (wipeDateUtc.HasValue)
            {
                Api.Server.Core.AddServerInfoTag(TagKey + wipeDateUtc.Value.Ticks);
            }
        }
    }
}