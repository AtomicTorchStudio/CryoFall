namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.World
{
    using System;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.DayNightSystem;

    public class ConsoleWorldSetTimeOfDay : BaseConsoleCommand
    {
        public override string Description =>
            "Forces specified time of day on client or server."
            + Environment.NewLine
            + "To reset please execute this command without any arguments.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "world.setTimeOfDay";

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