namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Generators
{
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    [NotPersistent]
    public readonly struct PragmiumReactorStatsData : IRemoteCallParameter
    {
        public PragmiumReactorStatsData(
            ushort fuelLifetimePercent,
            ushort efficiencyPercent,
            double psiEmissionLevel,
            double outputValue,
            ushort startupShutdownTimePercent)
        {
            this.FuelLifetimePercent = fuelLifetimePercent;
            this.EfficiencyPercent = efficiencyPercent;
            this.PsiEmissionLevel = psiEmissionLevel;
            this.OutputValue = outputValue;
            this.StartupShutdownTimePercent = startupShutdownTimePercent;
        }

        [SyncToClient]
        public ushort EfficiencyPercent { get; }

        [SyncToClient]
        public ushort FuelLifetimePercent { get; }

        [SyncToClient(networkDataType: typeof(float))]
        public double OutputValue { get; }

        [SyncToClient(networkDataType: typeof(float))]
        public double PsiEmissionLevel { get; }

        [SyncToClient]
        public ushort StartupShutdownTimePercent { get; }
    }
}