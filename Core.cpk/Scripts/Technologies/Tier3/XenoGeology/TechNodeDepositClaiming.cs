﻿namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.XenoGeology
{
    using AtomicTorch.CBND.CoreMod.Perks;

    public class TechNodeDepositClaiming : TechNode<TechGroupXenogeologyT3>
    {
        public override FeatureAvailability AvailableIn => FeatureAvailability.OnlyPvP;

        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddPerk<PerkClaimDeposits>();
        }
    }
}