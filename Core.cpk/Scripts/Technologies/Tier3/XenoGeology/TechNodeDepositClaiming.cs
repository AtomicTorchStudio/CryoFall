namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.XenoGeology
{
    using AtomicTorch.CBND.CoreMod.Perks;

    public class TechNodeDepositClaiming : TechNode<TechGroupXenogeology>
    {
        //public override ITextureResource Icon => IconPlaceholder;

        //public override string Name => "Deposit claiming";

        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddPerk<PerkClaimDeposits>();
        }
    }
}