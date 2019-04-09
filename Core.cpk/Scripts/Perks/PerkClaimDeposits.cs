namespace AtomicTorch.CBND.CoreMod.Perks
{
    using AtomicTorch.CBND.CoreMod.Perks.Base;
    using AtomicTorch.CBND.GameApi.Resources;

    public class PerkClaimDeposits : ProtoPerk
    {
        public override ITextureResource Icon { get; }
            = new TextureResource("Icons/IconConstructionSite");

        public override string Name => "Claim oil and lithium deposits";
    }
}