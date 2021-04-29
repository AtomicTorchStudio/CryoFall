namespace AtomicTorch.CBND.CoreMod.CharacterOrigins
{
    using AtomicTorch.CBND.CoreMod.Stats;

    public class CharacterOriginGalacticRepublic : ProtoCharacterOrigin
    {
        public override string Description =>
            "The Galactic Republic is the core of human civilization and the main power in the galaxy, with Earth at its center. Each of the worlds in the republic has equal voting rights and is legally obligated to follow galactic law; however outside of this each world is largely independent to pursue its own policies and interests.";

        public override string Name => "Galactic Republic";

        protected override void FillDefaultEffects(Effects effects)
        {
            effects
                .AddPercent(this, StatName.FarmingTasksSpeed, 25)
                .AddValue(this, StatName.FoodMax, 10)
                .AddValue(this, StatName.WaterMax, 10)
                .AddValue(this, StatName.StaminaMax, 10);
        }
    }
}