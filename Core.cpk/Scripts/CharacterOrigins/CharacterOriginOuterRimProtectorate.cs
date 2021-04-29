namespace AtomicTorch.CBND.CoreMod.CharacterOrigins
{
    using AtomicTorch.CBND.CoreMod.Stats;

    public class CharacterOriginOuterRimProtectorate : ProtoCharacterOrigin
    {
        public override string Description =>
            "The Outer Rim Protectorate forms the periphery outside of the core planets and mostly consists of less-developed industrial worlds. These worlds generally follow the policies of one of many large industrial corporations and are united in loosely formed coalitions driven by economic interests. As the Galactic Republic expands, these worlds are eventually absorbed as proper member states of the Republic.";

        public override string Name => "Outer Rim Protectorate";

        protected override void FillDefaultEffects(Effects effects)
        {
            effects
                .AddPercent(this, StatName.MiningSpeed,      10)
                .AddPercent(this, StatName.WoodcuttingSpeed, 20)
                .AddPercent(this, StatName.BuildingSpeed,    10)
                .AddPercent(this, StatName.HungerRate,       15); // hungry miners!
        }
    }
}