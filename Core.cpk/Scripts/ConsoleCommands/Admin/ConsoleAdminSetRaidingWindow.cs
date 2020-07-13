// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Admin
{
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.RaidingProtection;

    public class ConsoleAdminSetRaidingWindow : BaseConsoleCommand
    {
        public override string Description =>
            @"Set raiding window. Please ensure you're using a correct UTC offset!
              To disable raiding protection completely, please set 24 hours duration from any hour with any UTC offset.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "admin.setRaidingWindow";

        public string Execute(double UTC, double fromHour, double duration)
        {
            UTC %= 24;
            fromHour %= 24;
            if (duration > 24)
            {
                duration = 24;
            }

            var window = new DayTimeInterval(fromHour - UTC,
                                          fromHour - UTC + duration);
            RaidingProtectionSystem.ServerSetRaidingWindow(window);
            return "Server raiding window set. Please check Politics/Diplomacy menu!";
        }
    }
}