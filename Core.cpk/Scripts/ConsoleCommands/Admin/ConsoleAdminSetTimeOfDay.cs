namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Admin
{
    using System;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.DayNightSystem;

    public class ConsoleAdminSetTimeOfDay : BaseConsoleCommand
    {
        public override string Description =>
            "Forces specified time of day on client or server."
            + Environment.NewLine
            + "To reset please execute this command without any arguments.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "admin.setTimeOfDay";

        public string Execute(byte? hour = null, byte minute = 0)
        {
            if (!hour.HasValue)
            {
                // reset
                DayNightSystem.ServerResetTimeOfDayOffset();
                return null;
            }

            DayNightSystem.ServerSetCurrentTimeOfDay(hour.Value, minute);
            return null;
        }
    }
}