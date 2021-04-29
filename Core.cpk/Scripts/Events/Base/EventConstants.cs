namespace AtomicTorch.CBND.CoreMod.Events
{
    using System.Runtime.CompilerServices;
    using AtomicTorch.CBND.CoreMod.Helpers.Server;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Helpers;

    public static class EventConstants
    {
        public static readonly double ServerEventDelayMultiplier;

        static EventConstants()
        {
            if (Api.IsClient)
            {
                return;
            }

            ServerEventDelayMultiplier = ServerRates.Get(
                "WorldEventDelayMultiplier",
                defaultValue: ServerLocalModeHelper.IsLocalServer
                                  ? 0.2
                                  : 1.0,
                @"Determines the world event delay multiplier.
                  Most events have a configured delay to prevent them from starting until players could advance enough.
                  E.g. bosses will not spawn for the first 48 hours after the server wipe. 
                  With this multiplier it's possible to adjust the delay to make events starting earlier or later
                  (for example, if you want to get a boss spawn after 12 instead of 48 hours, set the multiplier to 0.25).
                  Please note: on PvP servers certain events like bosses will never start until
                  the T4 specialized tech time gate is unlocked (as there is no viable way
                  for players to defeat the boss until they can craft T4 weapons).");

            ServerEventDelayMultiplier = MathHelper.Clamp(ServerEventDelayMultiplier, 0, 10);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void EnsureInitialized()
        {
        }
    }
}