namespace AtomicTorch.CBND.CoreMod.Systems.FactionLeaderboardSystem.Metrics
{
    using System;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.Systems.Technologies;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Logic;

    /// <summary>
    /// This metric accumulates the learning points gained by faction members while they're in the faction.
    /// </summary>
    public class FactionScoreMetricLearningPointsScore : ProtoFactionScoreMetric
    {
        public override string Description =>
            "Awarded for gaining LP while in the faction. Doesn't apply to LP gained from quests and consumable items.";

        public override double FinalScoreCoefficient => 0.5;

        public override string Name => "Learning points score";

        public override double PowerCoefficient => 0.9;

        public override double ServerGetCurrentValue(ILogicObject faction)
        {
            return Faction.GetPrivateState(faction).ServerMetricLearningPoints;
        }

        public override void ServerInitialize()
        {
            PlayerCharacterTechnologies.ServerCharacterGainedLearningPoints
                += this.ServerCharacterGainedLearningPointsHandler;
        }

        private void ServerCharacterGainedLearningPointsHandler(
            ICharacter character,
            int gainedLearningPoints,
            bool isModifiedByStat)
        {
            if (!isModifiedByStat)
            {
                // gained through the quest, by a consumable item, etc
                return;
            }

            var faction = FactionSystem.ServerGetFaction(character);
            if (faction is null)
            {
                return;
            }

            var factionPrivateState = Faction.GetPrivateState(faction);
            factionPrivateState.ServerMetricLearningPoints
                = (uint)Math.Min(factionPrivateState.ServerMetricLearningPoints + gainedLearningPoints,
                                 uint.MaxValue);

            /*Logger.Dev("Learning points metric updated: "
                       + gainedLearningPoints.ToString("0.###")
                       + " total metric: "
                       + factionPrivateState.ServerMetricLearningPoints.ToString("0.###"));*/
        }
    }
}