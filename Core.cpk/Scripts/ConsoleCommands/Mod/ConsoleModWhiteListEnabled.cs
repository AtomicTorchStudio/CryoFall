// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Mod
{
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.ServerPlayerAccess;

    public class ConsoleModWhiteListEnabled : BaseConsoleCommand
    {
        public override string Description =>
            "Enables or disables the whitelist";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerModerator;

        public override string Name => "mod.whiteList.enabled";

        public string Execute(bool isEnabled = true)
        {
            ServerPlayerAccessSystem.SetWhitelistModeEnabled(isEnabled);
            return "Whitelist is " + (isEnabled ? "enabled" : "disabled");
        }
    }
}