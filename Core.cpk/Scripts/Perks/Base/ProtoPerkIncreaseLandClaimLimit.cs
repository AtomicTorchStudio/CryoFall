namespace AtomicTorch.CBND.CoreMod.Perks.Base
{
    using AtomicTorch.CBND.CoreMod.Stats;

    public abstract class ProtoPerkIncreaseLandClaimLimit : ProtoPerk
    {
        public abstract byte LimitIncrease { get; }

        protected sealed override void PrepareEffects(Effects effects)
        {
            effects.AddValue(this, StatName.LandClaimsMaxNumber, this.LimitIncrease);
        }
    }
}