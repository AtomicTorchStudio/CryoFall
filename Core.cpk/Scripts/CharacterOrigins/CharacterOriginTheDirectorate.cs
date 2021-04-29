namespace AtomicTorch.CBND.CoreMod.CharacterOrigins
{
    using AtomicTorch.CBND.CoreMod.Stats;

    public class CharacterOriginTheDirectorate : ProtoCharacterOrigin
    {
        public override string Description =>
            "The Directorate is an isolationist faction composed of several worlds that seceded from the Republic due to an ideological dispute and later grew into an independent and quite substantial power. It's located on the outer edge of the Republic core and surrounded by Republic worlds on all sides. The Directorate has been in a state of cold war with the Republic for hundreds of years and has no trade and barely any diplomatic ties.";

        public override string Name => "The Directorate";

        protected override void FillDefaultEffects(Effects effects)
        {
            effects
                .AddValue(this, StatName.StaminaMax, 10)
                .AddPercent(this, StatName.CraftingSpeed, 10)
                .AddPercent(this, StatName.SearchingSpeed, 15)
                .AddPercent(this, StatName.ImplantDegradationSpeedMultiplier, -5);
        }
    }
}