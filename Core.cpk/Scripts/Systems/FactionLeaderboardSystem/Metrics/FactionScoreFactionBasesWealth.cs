namespace AtomicTorch.CBND.CoreMod.Systems.FactionLeaderboardSystem.Metrics
{
    using AtomicTorch.CBND.GameApi.Data.Logic;

    /// <summary>
    /// TODO
    /// </summary>
    public class FactionScoreFactionBasesWealth : ProtoFactionScoreMetric
    {
        public override double FinalScoreCoefficient => 1;

        public override string Name => "Wealth of faction bases";

        public override double PowerCoefficient => 0.9;

        public override double ServerGetCurrentValue(ILogicObject faction)
        {
            ulong result = 0;
            // TODO: implement
            return result;
        }

        public override void ServerInitialize()
        {
        }
    }
}