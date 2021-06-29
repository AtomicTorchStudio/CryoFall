// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Mod
{
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.ServerPlayerAccess;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ConsoleModWhiteListEnabled : BaseConsoleCommand
    {
        public override string Description =>
            @"Enables or disables the whitelist.
              When the whitelist enabled, only players added to the whitelist
              can connect to the server (plus admins and moderators).
              Please note: there is also a blacklist that is intended
              to work in an opposite way by disallowing access only to those players
              that are listed in a blacklist while everyone else is allowed.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerModerator;

        public override string Name => "mod.whiteList.enabled";

        public string Execute(bool isEnabled)
        {
            ServerPlayerAccessSystem.SetWhitelistMode(isEnabled);
            return "Whitelist mode switched: " + (isEnabled ? "enabled" : "disabled") + " now";
        }

        public string Execute()
        {
            return "Whitelist is currently " + (Api.Server.Core.IsAccessWhiteListEnabled ? "enabled" : "disabled");
        }
    }
}