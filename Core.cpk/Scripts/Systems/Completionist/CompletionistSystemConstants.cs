namespace AtomicTorch.CBND.CoreMod.Systems.Completionist
{
    using System;
    using System.Runtime.CompilerServices;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Helpers;

    public static class CompletionistSystemConstants
    {
        static CompletionistSystemConstants()
        {
            if (Api.IsClient)
            {
                return;
            }

            var learningPointsRate = TechConstants.ServerLearningPointsGainMultiplier;

            var baseAmount = PveSystem.ServerIsPvE ? 10 : 5;

            ServerRewardLearningPointsPerEntry = (ushort)MathHelper.Clamp(
                Math.Round(baseAmount * learningPointsRate, MidpointRounding.AwayFromZero),
                0,
                ushort.MaxValue);
        }

        public static ushort ServerRewardLearningPointsPerEntry { get; }

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        private static void EnsureInitialized()
        {
        }

        private class Bootstrapper : BaseBootstrapper
        {
            public override void ServerInitialize(IServerConfiguration serverConfiguration)
            {
                EnsureInitialized();
            }
        }
    }
}