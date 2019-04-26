namespace AtomicTorch.CBND.CoreMod.Perks
{
    using System;
    using AtomicTorch.CBND.CoreMod.Perks.Base;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class PerkClaimDeposits : ProtoPerk
    {
        private static readonly Lazy<PerkClaimDeposits> LazyInstance
            = new Lazy<PerkClaimDeposits>(Api.GetProtoEntity<PerkClaimDeposits>);

        public static PerkClaimDeposits Instance => LazyInstance.Value;

        public override ITextureResource Icon { get; }
            = new TextureResource("Icons/IconConstructionSite");

        public override string Name => "Claim oil and lithium deposits";
    }
}