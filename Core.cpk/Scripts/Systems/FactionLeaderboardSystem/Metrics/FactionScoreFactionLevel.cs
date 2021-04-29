namespace AtomicTorch.CBND.CoreMod.Systems.FactionLeaderboardSystem.Metrics
{
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.GameApi.Data.Logic;

    /// <summary>
    /// This metric matches the faction level.
    /// </summary>
    public class FactionScoreFactionLevel : ProtoFactionScoreMetric
    {
        public override string Description => "The higher the faction level is, the more points are awarded!";

        public override double FinalScoreCoefficient => 3000;

        public override string Name => "Faction level";

        public override double PowerCoefficient => 1;

        public override double ServerGetCurrentValue(ILogicObject faction)
        {
            return Faction.GetPublicState(faction).Level - 1;
        }

        public override void ServerInitialize()
        {
        }
    }
}