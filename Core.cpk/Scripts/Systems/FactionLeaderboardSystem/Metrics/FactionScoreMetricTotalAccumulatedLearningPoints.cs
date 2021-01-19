namespace AtomicTorch.CBND.CoreMod.Systems.FactionLeaderboardSystem.Metrics
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Scripting;

    /// <summary>
    /// This metric just sums up all the faction members accumulated learning points.
    /// </summary>
    public class FactionScoreMetricTotalAccumulatedLearningPoints : ProtoFactionScoreMetric
    {
        public override double FinalScoreCoefficient => 1;

        public override string Name => "Total accumulated LP";

        public override double PowerCoefficient => 0.9;

        public override double ServerGetCurrentValue(ILogicObject faction)
        {
            ulong result = 0;
            var characters = Api.Server.Characters;
            foreach (var entry in FactionSystem.ServerGetFactionMembersReadOnly(faction))
            {
                var playerCharacter = characters.GetPlayerCharacter(entry.Name);
                result += playerCharacter.SharedGetTechnologies().LearningPointsAccumulatedTotal;
            }

            return result;
        }

        public override void ServerInitialize()
        {
        }
    }
}