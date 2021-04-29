namespace AtomicTorch.CBND.CoreMod.CharacterOrigins
{
    using AtomicTorch.CBND.CoreMod.Stats;

    public class CharacterOriginIndependentWorlds : ProtoCharacterOrigin
    {
        public override string Description =>
            "Independent worlds are those that elected not to join the Republic, maintaining full control of their policies and military. However, due to political and economic pressure, these worlds are still largely intertwined with the Galactic Republic, similar to 21st-century small city-states. Some people also refer to these independent worlds as Fringe worlds.";

        public override string Name => "Independent worlds";

        protected override void FillDefaultEffects(Effects effects)
        {
            effects
                .AddPercent(this, StatName.ForagingSpeed, 25)
                .AddValue(this, StatName.FishingKnowledgeLevel, 10)
                .AddPercent(this, StatName.TinkerTableBonus, 10)
                .AddPercent(this, StatName.HealthRegenerationPerSecond, 15);
        }
    }
}